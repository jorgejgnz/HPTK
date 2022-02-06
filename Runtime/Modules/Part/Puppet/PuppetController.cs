using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    [RequireComponent(typeof(PuppetModel))]
    public class PuppetController : HPTKController
    {
        [ReadOnly]
        public PuppetModel model;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<PuppetModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.part.registry.Add(this);
        }

        private void OnDisable()
        {
            model.part.registry.Remove(this);
        }

        public override sealed void ControllerStart()
        {
            base.ControllerStart();

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = BasicHelpers.FindScriptableObject<PuppetConfiguration>(HPTK.core.defaultConfAssets);

            if (model.configuration == null)
            {
                Debug.LogError("Any PuppetConfiguration found in PuppetModel or HPTK.core.defaultConfAssets. The module cannot continue");
                return;
            }

            // Start parts and bones recursively
            PartStart(model.part);

            // Start bones
            model.bones.ForEach(b => BoneStart(b, model.part.root));
            model.bones.ForEach(b => LateBoneStart(b));

            model.view.onPhysicsReady.Invoke();

            model.ready = true;
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!gameObject.activeSelf)
                return;

            if (!model.ready)
                return;

            // For each goal, make it copy localRotation of its master
            model.bones.ForEach(p => BoneUpdate(p));

            // Dynamic strength (...)
        }

        void PartStart(PartModel part)
        {
            if (model.parts.Contains(part))
                return;

            model.parts.Add(part);

            // Start children parts first
            part.parts.ForEach(p => { if (!p.registry.Find(c => c is PuppetController)) PartStart(p); });

            // For each bone. BoneStart can call BoneStart() for bones that shouldn't be part of model.bones
            part.bones.ForEach(b => { if (!model.bones.Contains(b)) model.bones.Add(b); });
        }

        void BoneStart(BoneModel bone, BoneModel mainRoot)
        {
            if (!bone.reprs.ContainsKey(PuppetModel.key))
            {
                Debug.LogWarning("Bone " + bone.name + " does not have any " + PuppetModel.key + " representation. PuppetController cannot start this bone");
                return;
            }

            PuppetReprModel slave = bone.reprs[PuppetModel.key] as PuppetReprModel;

            // Set puppet model for each puppet representation
            slave.puppet = model;

            // Set thumb bones as special
            if (bone.part is FingerModel)
            {
                FingerModel finger = bone.part as FingerModel;
                if (finger == finger.hand.thumb) slave.isSpecial = true;
            }

            if (!slave.usePhysics)
            {
                Debug.LogWarning("Bone " + bone.name + " has slave representation but it won't use physics");
                return;
            }
            
            // Set pheasy if needed
            if (slave.pheasy == null)
            {
                Pheasy pheasy = slave.transformRef.GetComponent<Pheasy>();

                if (pheasy == null) pheasy = slave.transformRef.gameObject.AddComponent<Pheasy>();

                pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                pheasy.rb.isKinematic = true;

                pheasy.axis = model.axisPrefab;

                slave.pheasy = pheasy;
            }

            // Get connected body
            Rigidbody connectedBody = null;
            if (bone.parent != null)
            {
                if (bone.parent.reprs.ContainsKey(PuppetModel.key))
                {
                    PuppetReprModel parentSlave = bone.parent.reprs[PuppetModel.key] as PuppetReprModel;

                    if (parentSlave.usePhysics && !parentSlave.pheasy)
                    {
                        // Debug.LogWarning("Bone " + bone.parent.name + " has a " + PuppetModel.key + " representation and it use physics but it has no Pheasy component. Forcing initialization");
                        BoneStart(bone.parent, mainRoot);
                    }

                    if (parentSlave.pheasy) connectedBody = parentSlave.pheasy.rb;
                }
                else
                {
                    Debug.LogWarning("Parent of bone " + bone.name + ", " + bone.parent.name + ", does not have a " + PuppetModel.key + " representation");
                }
            }

            // Add and save pheasy.constraint
            if (slave.constraint == null || slave.constraint.pheasy == null)
                slave.constraint = slave.pheasy.AddTargetConstraint("PuppetBone", connectedBody, false, null);

            // Replace or instantiate goal
            if (slave.goal == null) slave.goal = slave.constraint.connectedAnchor;
            else slave.constraint.connectedAnchor = slave.goal;

            // Set freedom
            slave.constraint.settings.angularMotion = ConfigurableJointMotion.Free;

            if (connectedBody != null) slave.constraint.settings.linearMotion = ConfigurableJointMotion.Locked;
            else slave.constraint.settings.linearMotion = ConfigurableJointMotion.Free;

            // Replace connected body by parent
            if (bone.parent == null || slave.point.bone == mainRoot || slave.constraint.connectedBody == null) // Wrist
            {
                UpdatePhysicsSettings(slave, model.configuration.root);
            }
            else if (slave.isSpecial) // Special bones
            {
                UpdatePhysicsSettings(slave, model.configuration.special);
            }
            else // Common bones
            {
                UpdatePhysicsSettings(slave, model.configuration.standard);
            }

            if (slave.constraint != null)
            {
                // Stability
                slave.constraint.settings.collideWithConnectedRb = false;
                slave.pheasy.safeMode = true;
                slave.pheasy.gradualMode = false;

                // Forced values
                slave.constraint.enabled = true;
                slave.constraint.keepAxisRelativeToObject = true;

                slave.pheasy.rb.isKinematic = false;
                slave.pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Performance
                slave.pheasy.editMode = true;
                AsyncHelpers.DoAfterFrames(this, 1, () => slave.pheasy.editMode = false);
            }

            // Find colliders and triggers. Children colliders are updated before their parents
            slave.pheasy.UpdateCollidersAndTriggerNotifiers();
        }

        void LateBoneStart(BoneModel bone)
        {
            if (!bone.reprs.ContainsKey(PuppetModel.key))
            {
                Debug.Log("Bone " + bone + " does not have a " + PuppetModel.key + " representation");
                return;
            }

            PuppetReprModel slave = bone.reprs[PuppetModel.key] as PuppetReprModel;

            if (!slave.usePhysics) return;

            if (bone.parent != null)
            {
                // Ignore part-parent collisions
                for (int b = 0; b < bone.part.parent.bones.Count; b++)
                {
                    BoneModel parentBone = bone.part.parent.bones[b];

                    if (parentBone.reprs.ContainsKey(PuppetModel.key))
                    {
                        PuppetReprModel parentSlave = parentBone.reprs[PuppetModel.key] as PuppetReprModel;
                        slave.pheasy.IgnoreCollisions(parentSlave.pheasy, true, true);
                    }
                }
            }

            slave.specificView.onPhysicsReady.Invoke();

            slave.ready = true;
        }

        void BoneUpdate(BoneModel bone)
        {
            if (bone.point.master == null || !bone.point.reprs.ContainsKey(PuppetModel.key)) return;

            ReprModel master;
            PuppetReprModel slave = bone.point.reprs[PuppetModel.key] as PuppetReprModel;

            if (!slave.usePhysics) return;

            if (!bone.reprs.ContainsKey(model.mimicRepr))
            {
                Debug.LogWarning("Bone " + bone.name + " does not have a " + model.mimicRepr + " representation. Puppet goal cannot be updated for this bone");
                return;
            }

            if (slave.constraint.connectedBody)
            {
                master = bone.point.reprs[model.mimicRepr];

                if (master.localRotZ < slave.maxLocalRotZ && master.localRotZ > slave.minLocalRotZ)
                {
                    slave.goal.rotation = slave.goal.parent.rotation * master.localRotation;
                }
                else
                {
                    slave.goal.localRotation = slave.fixedLocalRot;
                }
            }
            else
            {
                master = bone.point.reprs[model.mimicRepr];

                slave.goal.rotation = master.transformRef.rotation;
                slave.goal.position = master.transformRef.position;

                if (model.forceUnconnected) slave.pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                else slave.pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                slave.pheasy.rb.isKinematic = model.forceUnconnected;
                slave.constraint.force = model.forceUnconnected;
            }
        }

        void UpdatePhysicsSettings(PuppetReprModel slave, PuppetBoneConfiguration conf)
        {
            if (!slave.usePhysics) return;

            // Joint drives
            slave.constraint.settings.motionDrive = conf.positionDrive;
            slave.constraint.settings.angularDrive = conf.rotationDrive;

            // Joint mass scales
            slave.constraint.joint.massScale = conf.jointMassScale;
            slave.constraint.joint.connectedMassScale = conf.jointConnectedMassScale;

            // Rb
            slave.pheasy.rb.mass = conf.rigidbodyMass;
            slave.pheasy.rb.drag = conf.rigidbodyDrag;
            slave.pheasy.rb.angularDrag = conf.rigidbodyAngularDrag;
            slave.pheasy.rb.useGravity = conf.useGravity;

            // Stability
            slave.pheasy.safeMode = conf.safeMode;
            slave.pheasy.maxVelocity = conf.maxLinearVelocity;
            slave.pheasy.maxAngularVelocity = conf.maxAngularVelocity;
            slave.pheasy.maxDepenetrationVelocity = conf.maxDepenetrationVelocity;
            slave.pheasy.maxErrorAllowed = conf.maxErrorAllowed;

            // Debug
            slave.constraint.force = conf.force;
    }

        public void SetPhysics(bool enabled)
        {
            if (!model.part.root) return;

            PuppetReprModel root = model.part.root.reprs[PuppetModel.key] as PuppetReprModel;

            if (!root.usePhysics) return;

            model.kinematic = !enabled;

            if (enabled)
            {
                // Teleport
                root.pheasy.TeleportToDestination(root.constraint);
            }

            // Physics
            SetPartPhysics(model.part, enabled, new List<PartModel>());
        }

        public void SetPartPhysics(PartModel part, bool enabled, List<PartModel> processedParts)
        {
            if (processedParts.Contains(part)) return;

            processedParts.Add(part);

            // Bones
            part.bones.ForEach(b => SetSlavePhysics(b.reprs[PuppetModel.key] as PuppetReprModel, enabled) );

            // Parts
            part.parts.ForEach(p => SetPartPhysics(p, enabled, processedParts));
        }

        public void SetSlavePhysics(PuppetReprModel slave, bool enabled)
        {
            if (!slave.usePhysics) return;

            if (enabled)
            {
                slave.pheasy.rb.isKinematic = false;
                slave.pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            else
            {
                slave.pheasy.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                slave.pheasy.rb.isKinematic = true;
            }

            // Enable/Disable colliders
            slave.pheasy.colliders.ForEach(c => c.enabled = enabled);          
        }
    }
}
