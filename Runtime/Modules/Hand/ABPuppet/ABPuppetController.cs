using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Utils;
using HandPhysicsToolkit.Modules.Avatar;
using static HandPhysicsToolkit.Modules.Hand.ABPuppet.ABPuppetReprModel;
using static HandPhysicsToolkit.Utils.ABHierarchyWizard;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Helpers;

namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    [RequireComponent(typeof(ABPuppetModel))]
    public class CustomController : HPTKController
    {
        [ReadOnly]
        public ABPuppetModel model;

        private Collider[] handColliders;
        private bool resetInProcess;
        private Vector3 prevWristLocalScale = Vector3.one;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<ABPuppetModel>();
            SetGeneric(model.view, model);

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = BasicHelpers.FindScriptableObject<ArticulationBodiesConfiguration>(HPTK.core.defaultConfAssets);

            if (model.configuration == null)
            {
                Debug.LogError("Any PuppetConfiguration found in PuppetModel or HPTK.core.defaultConfAssets. The module cannot continue");
                return;
            }
        }

        public void OnEnable()
        {
            model.hand.registry.Add(this);
            model.configuration.OnConfigurationChange.AddListener(ChangeConfiguration);
        }

        public void OnDisable()
        {
            model.hand.registry.Remove(this);
            model.configuration.OnConfigurationChange.RemoveListener(ChangeConfiguration);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();

            if (model.hand.body.avatar.ready) SetupHand();
            else model.hand.body.avatar.view.onStarted.AddListener(() => SetupHand());
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            //
        }

        void FixedUpdate()
        {
            StabilizeHand();

            DetectScaleChange();
        }

        void SetupHand()
        {
            for (int bc = 0; bc < model.hierarchyWizard.boneConfigs.Count; bc++)
            {
                ABHierarchyWizard.BoneConfig config = model.hierarchyWizard.boneConfigs[bc];
                if (!config.reprModel) continue;

                BoneModel bone = config.reprModel.point.bone;

                if (bone.part is FingerModel)
                {
                    config.reprModel.zOnly = GetFingerBoneIndex(bone) >= bone.part.bones.Count - 2;
                    config.reprModel.isStrong = IsStrongFingerBone(GetFingerBoneIndex(bone), bone.part.bones.Count);
                }

                for (int p = 0; p < config.parents.Count; p++)
                {
                    ParentConfig c = config.parents[p];
                    ConfigureNode(config.reprModel, c.tsf, c.suffix);
                }

                if (bone.part is FingerModel)
                {
                    AddFingerBoneABFollower(bone, config.reprModel.zOnly);
                }

                if (bone == model.hand.wrist)
                {
                    AddPalmABFollower(bone);
                }

                Pheasy pheasy = config.reprModel.transformRef.GetComponent<Pheasy>();
                if (!pheasy)
                {
                    pheasy = config.reprModel.transformRef.gameObject.AddComponent<Pheasy>();
                }
                config.reprModel.pheasy = pheasy;
            }

            SetupColliderProperties();
        }

        void ConfigureNode(ABPuppetReprModel reprModel, Transform tsf, string suffix)
        {
            bool root = suffix.ToLower().Contains("root");
            bool position = suffix.ToLower().Contains("position");
            bool rotation = suffix.ToLower().Contains("rotation");

            char axis = '-';
            if (suffix.ToLower().EndsWith("x")) axis = 'x';
            else if (suffix.ToLower().EndsWith("y")) axis = 'y';
            else if (suffix.ToLower().EndsWith("z")) axis = 'z';

            if (root)
            {
                tsf.position = model.hand.body.replicatedTsf.position;
                tsf.rotation = model.hand.body.replicatedTsf.rotation;
            }

            ArticulationBody ab = tsf.GetComponent<ArticulationBody>();
            if (!ab) ab = tsf.gameObject.AddComponent<ArticulationBody>();

            if (root) reprModel.root = ab;
            else if (position && axis == 'x') reprModel.posX = ab;
            else if (position && axis == 'y') reprModel.posY = ab;
            else if (position && axis == 'z') reprModel.posZ = ab;
            else if (rotation && axis == 'x') reprModel.rotX = ab;
            else if (rotation && axis == 'y') reprModel.rotY = ab;
            else if (rotation && axis == 'z') reprModel.rotZ = ab;
            else if (rotation && axis == '-') reprModel.rotation = ab;
            else { Debug.LogError("Suffix not supported!"); return; }

            ArticulationNode node = new ArticulationNode(ab);
            node.axis = axis;

            reprModel.articulationNodes.Add(node);

            if (root)
            {
                // Root
                SetupRoot(ab, model.configuration.Root);
                ab.immovable = true;
            }
            else if (reprModel.point.bone == model.hand.wrist)
            {
                // Palm
                if (position)
                {
                    SetupPalmPositionArticulationBody(ab, axis);
                }
                else if (rotation && axis == '-' && model.configuration.UseWristAnchorRotation)
                {
                    SetupArticulationBodyConfiguration(ab, model.configuration.Palm);
                    SetupFixedArticulationJoint(ab);
                }
                else if (rotation)
                {
                    SetupPalmRevoluteRotationArticulationBody(ab, axis);
                }
            }
            else if (reprModel.point.bone.part is FingerModel)
            {
                // Fingers
                SetupFingerNode(node, reprModel.isStrong, reprModel.zOnly);
            }
        }

        void SetupColliderProperties()
        {
            Collider palmCollider = (model.hand.wrist.reprs[ABPuppetModel.key] as ABPuppetReprModel).colliderRef;

            if (palmCollider == null)
            {
                Debug.LogError("Palm collider not found!");
                return;
            }

            palmCollider.material = model.configuration.HandPhysicMaterial;

            Transform palmRoot = model.hand.wrist.reprs[ABPuppetModel.key].transformRef;

            handColliders = palmRoot.parent.GetComponentsInChildren<Collider>();

            if (model.configuration.DisableFingerToPalmCollision)
            {
                foreach (Collider collider in handColliders)
                {
                    collider.material = model.configuration.HandPhysicMaterial;
                    UnityEngine.Physics.IgnoreCollision(palmCollider, collider, true);
                }
            }

            if (model.configuration.DisableFingerToFingerCollision)
            {
                for (int i = 0; i < handColliders.Length - 1; i++)
                {
                    for (int j = i + 1; j < handColliders.Length; j++)
                    {
                        Collider colliderA = handColliders[i];
                        Collider colliderB = handColliders[j];
                        UnityEngine.Physics.IgnoreCollision(colliderA, colliderB, true);
                    }
                }
            }

            if (model.configuration.AddFakeColliders)
            {
                foreach (BoneModel bone in model.hand.bones)
                {
                    ABPuppetReprModel reprModel = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;

                    foreach (ArticulationNode node in reprModel.articulationNodes)
                    {
                        if (node.ab == reprModel.rotZ || node.ab == reprModel.root) continue;

                        SphereCollider sc = node.ab.gameObject.AddComponent<SphereCollider>();
                        sc.radius = 0.005f;
                        node.ab.gameObject.layer = (int)Mathf.Log(model.configuration.NoCollisionLayer.value, 2);
                    }
                }
            }
        }

        void SetupPalmPositionArticulationBody(ArticulationBody articulationBody, char axis)
        {
            SetupArticulationBodyConfiguration(articulationBody, model.configuration.Palm);
            SetupRoot(articulationBody, model.configuration.Root);
            SetupPrismaticArticulationJoint(articulationBody, axis, model.configuration.Palm.PositionDrives);
        }

        void SetupPalmRevoluteRotationArticulationBody(ArticulationBody articulationBody, char axis)
        {
            SetupArticulationBodyConfiguration(articulationBody, model.configuration.Palm);
            SetupRoot(articulationBody, model.configuration.Root);
            SetupAllAxesRevoluteArticulationJoint(articulationBody, axis, model.configuration.Palm.RotationDrives);

            if (axis == 'z') articulationBody.mass = model.configuration.Palm.Mass;
        }

        void SetupPrismaticArticulationJoint(ArticulationBody ab, char axis, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            ab.jointType = ArticulationJointType.PrismaticJoint;
            ab.anchorRotation = Quaternion.identity;

            switch (axis)
            {
                case 'x':
                    ab.linearLockX = ArticulationDofLock.FreeMotion;
                    ab.linearLockY = ArticulationDofLock.LockedMotion;
                    ab.linearLockZ = ArticulationDofLock.LockedMotion;
                    ab.xDrive = SetupArticulationDrive(ab.xDrive, articulationDriveConfiguration);
                    break;
                case 'y':
                    ab.linearLockX = ArticulationDofLock.LockedMotion;
                    ab.linearLockY = ArticulationDofLock.FreeMotion;
                    ab.linearLockZ = ArticulationDofLock.LockedMotion;
                    ab.yDrive = SetupArticulationDrive(ab.yDrive, articulationDriveConfiguration);
                    break;
                case 'z':
                    ab.linearLockX = ArticulationDofLock.LockedMotion;
                    ab.linearLockY = ArticulationDofLock.LockedMotion;
                    ab.linearLockZ = ArticulationDofLock.FreeMotion;
                    ab.zDrive = SetupArticulationDrive(ab.zDrive, articulationDriveConfiguration);
                    break;
                default:
                    Debug.LogError("Axis not recognized!");
                    break;
            }
        }

        void SetupRevoluteArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            articulationBody.jointType = ArticulationJointType.RevoluteJoint;
            articulationBody.twistLock = ArticulationDofLock.FreeMotion;

            var xDrive = articulationBody.xDrive;
            articulationBody.xDrive = SetupArticulationDrive(xDrive, articulationDriveConfiguration);
        }

        void SetupSingleAxisRevoluteArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            SetupRevoluteArticulationJoint(articulationBody, articulationDriveConfiguration);

            articulationBody.anchorRotation = Quaternion.Euler(0, 90, 180);
        }

        void SetupAllAxesRevoluteArticulationJoint(ArticulationBody ab, char axis, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            SetupRevoluteArticulationJoint(ab, articulationDriveConfiguration);

            switch (axis)
            {
                case 'x':
                    ab.anchorRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 'y':
                    ab.anchorRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case 'z':
                    ab.anchorRotation = Quaternion.Euler(0, 270, 0);
                    break;
                default:
                    break;
            }
        }

        void SetupFixedArticulationJoint(ArticulationBody articulationBody)
        {
            articulationBody.jointType = ArticulationJointType.FixedJoint;
            articulationBody.matchAnchors = false;
            articulationBody.anchorRotation = Quaternion.identity;
        }

        void SetupFingerNode(ArticulationNode node, bool isStrong, bool zOnly)
        {
            if (zOnly && node.axis != 'z')
            {
                Debug.LogError("Bug!");
                return;
            }

            if (node.axis == '-')
            {
                Debug.LogError("Bug!");
                return;
            }

            SetupArticulationBodyConfiguration(node.ab, model.configuration.FingerBone);

            if (zOnly)
            {
                SetupSingleAxisRevoluteArticulationJoint(node.ab, model.configuration.FingerBone.RotationDrives);
            }
            else
            {
                if (isStrong)
                {
                    SetupAllAxesRevoluteArticulationJoint(node.ab, node.axis, model.configuration.FingerBone.StrongRotationDrives);
                }
                else
                {
                    SetupAllAxesRevoluteArticulationJoint(node.ab, node.axis, model.configuration.FingerBone.RotationDrives);
                }
            }

            if (node.axis != 'z') node.ab.mass = model.configuration.Root.Mass;
        }

        void AddPalmABFollower(BoneModel palm)
        {
            ABPuppetReprModel puppetRepr = palm.reprs[ABPuppetModel.key] as ABPuppetReprModel;
            ReprModel masterRepr = palm.reprs[model.followThis];

            if (model.configuration.UseWristAnchorRotation)
            {
                puppetRepr.abFollower = puppetRepr.rotation.gameObject.AddComponent<ArticulationBodyFollower>();

                puppetRepr.abFollower.ArticulationBody = puppetRepr.rotation;
                puppetRepr.abFollower.Root = puppetRepr.root;

                puppetRepr.abFollower.AllAxesRotationBody = puppetRepr.rotation;
            }
            else
            {
                puppetRepr.abFollower = puppetRepr.rotZ.gameObject.AddComponent<ArticulationBodyFollower>();

                puppetRepr.abFollower.YAxisRotationBody = puppetRepr.rotY;
                puppetRepr.abFollower.XAxisRotationBody = puppetRepr.rotX;
                puppetRepr.abFollower.ZAxisRotationBody = puppetRepr.rotZ;

                puppetRepr.abFollower.ArticulationBody = puppetRepr.rotZ;
            }

            puppetRepr.abFollower.UseWorldRotation = true;

            puppetRepr.abFollower.Target = masterRepr.transformRef;

            // prismatic joint
            puppetRepr.abFollower.XAxisPositionBody = puppetRepr.posX;
            puppetRepr.abFollower.YAxisPositionBody = puppetRepr.posY;
            puppetRepr.abFollower.ZAxisPositionBody = puppetRepr.posZ;
        }

        void AddFingerBoneABFollower(BoneModel bone, bool zOnly)
        {
            ABPuppetReprModel puppetRepr = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;
            ReprModel masterRepr = bone.reprs[model.followThis];

            puppetRepr.abFollower = puppetRepr.rotZ.gameObject.AddComponent<ArticulationBodyFollower>();
            puppetRepr.abFollower.Target = masterRepr.transformRef;

            // revolute joint
            if (!zOnly)
            {
                puppetRepr.abFollower.YAxisRotationBody = puppetRepr.rotY;
                puppetRepr.abFollower.XAxisRotationBody = puppetRepr.rotX;
            }

            puppetRepr.abFollower.ZAxisRotationBody = puppetRepr.rotZ;

            // set the articulation body
            puppetRepr.abFollower.ArticulationBody = puppetRepr.rotZ;
        }

        void SetupRoot(ArticulationBody articulationBody, ArticulationBodyBaseConfiguration articulationBodyBaseConfiguration)
        {
            articulationBody.mass = articulationBodyBaseConfiguration.Mass;
            articulationBody.useGravity = articulationBodyBaseConfiguration.UseGravity;
            articulationBody.collisionDetectionMode = articulationBodyBaseConfiguration.CollisionDetectionMode;

            SetupStabilitySettings(articulationBody);
        }

        void SetupStabilitySettings(ArticulationBody articulationBody)
        {
            articulationBody.maxLinearVelocity = model.configuration.MaxLinearVelocity;
            articulationBody.maxAngularVelocity = model.configuration.MaxAngularVelocity;
            articulationBody.maxDepenetrationVelocity = model.configuration.MaxDepenetrationVelocity;
            articulationBody.maxJointVelocity = model.configuration.MaxJointVelocity;
        }

        void SetupArticulationBodyConfiguration(ArticulationBody articulationBody, ArticulationBodyConfiguration articulationBodyConfiguration)
        {
            SetupRoot(articulationBody, articulationBodyConfiguration);

            articulationBody.linearDamping = articulationBodyConfiguration.LinearDamping;
            articulationBody.angularDamping = articulationBodyConfiguration.AngularDamping;
            articulationBody.jointFriction = articulationBodyConfiguration.JointFriction;
        }

        ArticulationDrive SetupArticulationDrive(ArticulationDrive articulationDrive, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            articulationDrive.stiffness = articulationDriveConfiguration.Stiffness;
            articulationDrive.damping = articulationDriveConfiguration.Damping;
            articulationDrive.forceLimit = articulationDriveConfiguration.ForceLimit;

            return articulationDrive;
        }

        void ChangeConfiguration()
        {
            ABPuppetReprModel palmRepr = model.hand.wrist.reprs[ABPuppetModel.key] as ABPuppetReprModel;

            foreach (BoneModel bone in model.hand.bones)
            {
                ABPuppetReprModel repr = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;

                foreach (ArticulationNode node in repr.articulationNodes)
                {
                    if (node.ab == palmRepr.root)
                    {
                        SetupRoot(node.ab, model.configuration.Root);
                    }
                    else if (node.ab == palmRepr.posX || node.ab == palmRepr.posY || node.ab == palmRepr.posZ)
                    {
                        SetupArticulationBodyConfiguration(node.ab, model.configuration.Palm);
                        SetupRoot(node.ab, model.configuration.Root);
                        SetupPrismaticArticulationJoint(node.ab, node.axis, model.configuration.Palm.PositionDrives);
                    }
                    else if (node.ab == palmRepr.rotX || node.ab == palmRepr.rotY || node.ab == palmRepr.rotZ)
                    {
                        SetupArticulationBodyConfiguration(node.ab, model.configuration.Palm);
                        SetupRoot(node.ab, model.configuration.Root);
                        SetupAllAxesRevoluteArticulationJoint(node.ab, node.axis, model.configuration.Palm.RotationDrives);

                        if (node.ab == palmRepr.rotZ)
                        {
                            node.ab.mass = model.configuration.Palm.Mass;
                        }
                    }
                    else
                    {
                        SetupArticulationBodyConfiguration(node.ab, model.configuration.FingerBone);

                        if (repr.isStrong)
                        {
                            SetupRevoluteArticulationJoint(node.ab, model.configuration.FingerBone.StrongRotationDrives);
                        }
                        else
                        {
                            SetupRevoluteArticulationJoint(node.ab, model.configuration.FingerBone.RotationDrives);
                        }

                        if (node.axis == 'y' || node.axis == 'x')
                        {
                            node.ab.mass = model.configuration.Root.Mass;
                        }
                    }
                }
            }
        }

        bool IsStrongFingerBone(int boneIndex, int boneCount)
        {
            return boneCount - boneIndex > 2;
        }

        void ResetAllVelocities()
        {
            foreach (BoneModel bone in model.hand.bones)
            {
                ABPuppetReprModel repr = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;

                foreach (ArticulationNode node in repr.articulationNodes)
                {
                    node.ab.velocity = Vector3.zero;
                    node.ab.angularVelocity = Vector3.zero;
                }
            }
        }

        void StabilizeHand()
        {
            if (resetInProcess)
            {
                resetInProcess = false;
                return;
            }

            foreach (BoneModel bone in model.hand.bones)
            {
                ABPuppetReprModel repr = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;

                foreach (ArticulationNode node in repr.articulationNodes)
                {
                    if (node.ab.velocity.magnitude > model.configuration.MaxLinearVelocity ||
                        node.ab.angularVelocity.magnitude > model.configuration.MaxAngularVelocity)
                    {
                        PerformBodyReset(node.ab);
                    }
                }
            }
        }

        void DetectScaleChange()
        {
            ABPuppetReprModel puppetHandRepr = model.hand.wrist.reprs[ABPuppetModel.key] as ABPuppetReprModel;
            Transform puppetHandTsf = puppetHandRepr.transformRef;

            if (puppetHandTsf.localScale != prevWristLocalScale)
            {
                puppetHandRepr.abFollower.OnScaleChange();

                for (int f = 0; f < model.hand.fingers.Count; f++)
                {
                    for (int b = 0; b < model.hand.fingers[f].bonesFromRootToTip.Count; b++)
                    {
                        BoneModel bone = model.hand.fingers[f].bonesFromRootToTip[b];
                        ABPuppetReprModel boneRepr = bone.reprs[ABPuppetModel.key] as ABPuppetReprModel;
                        boneRepr.abFollower.OnScaleChange();
                    }

                }

                prevWristLocalScale = puppetHandTsf.localScale;
            }
        }

        void PerformBodyReset(ArticulationBody body)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            resetInProcess = true;
        }

        int GetFingerBoneIndex(BoneModel bone)
        {
            if (bone.part is FingerModel)
            {
                return (bone.part as FingerModel).bonesFromRootToTip.IndexOf(bone);
            }
            else
            {
                return -1;
            }
        }
    }
}
