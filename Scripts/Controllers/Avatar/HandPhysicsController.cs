using HPTK.Helpers;
using HPTK.Models.Avatar;
using HPTK.Settings;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Avatar
{
    public class HandPhysicsController : HandPhysicsHandler
    {
        public HandPhysicsModel model;

        SlaveBoneModel[] slaveBones;
        SlaveBoneModel slaveBone;

        bool decoupled = false;

        private void Awake()
        {
            viewModel = new HandPhysicsViewModel(model);
        }

        private void Start()
        {
            model.proxyHand.relatedHandlers.Add(this);


            slaveBones = AvatarHelpers.GetSlaveHandBones(model.proxyHand.slave);

            // Ignore palm-finger collisions
            for (int i = 0; i < slaveBones.Length; i++)
            {
                if (!slaveBones[i].colliderRef)
                    continue;

                Physics.IgnoreCollision((model.proxyHand.slave.wrist as SlaveBoneModel).colliderRef, slaveBones[i].colliderRef, true);
            }

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = core.model.defaultHandPhysicsConfiguration;

            for (int i = 0; i < slaveBones.Length; i++)
            {
                if (!slaveBones[i].jointRef)
                    continue;

                // Get initial connected body local rotations
                if (slaveBones[i].jointRef.connectedBody != null) slaveBones[i].initialConnectedBodyLocalRotation = slaveBones[i].jointRef.connectedBody.transform.localRotation;
                else slaveBones[i].initialConnectedBodyLocalRotation = slaveBones[i].jointRef.transform.rotation;

                // Set initial joint configurations
                if (slaveBones[i] == model.proxyHand.slave.wrist)
                    PhysHelpers.SetSlaveBoneConfiguration(slaveBones[i].jointRef, model.configuration.wrist);
                else if (slaveBones[i].isSpecial)
                    PhysHelpers.SetSlaveBoneConfiguration(slaveBones[i].jointRef, model.configuration.specials);
                else
                    PhysHelpers.SetSlaveBoneConfiguration(slaveBones[i].jointRef, model.configuration.fingers);

                slaveBones[i].rigidbodyRef.maxDepenetrationVelocity = model.configuration.fingers.maxLinearVelocity;
                slaveBones[i].rigidbodyRef.maxAngularVelocity = model.configuration.fingers.maxAngularVelocity;
            }
        }
        private void Update()
        {
            if (model.proxyHand.error > 0.5f && !decoupled)
                StartCoroutine(RecoverFromError());
        }

        private void FixedUpdate()
        {
            UpdateSlaveBone(model.proxyHand.slave.wrist as SlaveBoneModel, model.proxyHand.master.wrist.transformRef, Space.World, model.configuration.wrist);

            for (int i = 0; i < model.proxyHand.slave.fingers.Length; i++)
            {
                for (int j = 0; j < model.proxyHand.slave.fingers[i].bones.Length; j++)
                {
                    slaveBone = model.proxyHand.slave.fingers[i].bones[j] as SlaveBoneModel;

                    if (slaveBone.isSpecial)
                        UpdateSlaveBone(slaveBone, model.proxyHand.master.fingers[i].bones[j].transformRef, Space.Self, model.configuration.specials);
                    else
                        UpdateSlaveBone(slaveBone, model.proxyHand.master.fingers[i].bones[j].transformRef, Space.Self, model.configuration.fingers);
                }
            }
        }

        private void UpdateSlaveBone(SlaveBoneModel slaveBone, Transform master, Space space, SlaveBoneConfiguration boneConf)
        {
            if (!slaveBone.jointRef)
                return;

            if (boneConf.followsPosition)
            {
                if (boneConf.useTargetPos)
                {
                    if (space == Space.Self && slaveBone.jointRef.configuredInWorldSpace) Debug.LogError(slaveBone.name + " is configured in world space. You can't use local position as target position here");
                    if (space == Space.World && !slaveBone.jointRef.configuredInWorldSpace) Debug.LogError(slaveBone.name + " is configured in local space. You can't use world position as target position here");

                    Vector3 desiredWorldPosition = PhysHelpers.GetDesiredWorldPos(slaveBone.transformRef, master, space);

                    if (slaveBone.jointRef.configuredInWorldSpace)
                    {
                        slaveBone.jointRef.connectedAnchor = desiredWorldPosition;
                    }
                    else
                    {
                        slaveBone.jointRef.connectedAnchor = slaveBone.jointRef.connectedBody.transform.InverseTransformPoint(desiredWorldPosition);
                    }
                }
                else
                {
                    // Position
                    Vector3 desiredPosition = PhysHelpers.GetDesiredWorldPos(slaveBone.transformRef, master, space);

                    // Velocity
                    Vector3 newVelocity = (desiredPosition - slaveBone.transformRef.position) / Time.fixedDeltaTime;

                    slaveBone.rigidbodyRef.velocity = newVelocity;
                }
            }

            if (boneConf.followsRotation)
            {
                // Error logs
                if (space == Space.Self && slaveBone.jointRef.configuredInWorldSpace) Debug.LogError(slaveBone.name + " is configured in world space. You can't use local rotation as target rotation here");
                if (space == Space.World && !slaveBone.jointRef.configuredInWorldSpace) Debug.LogError(slaveBone.name + " is configured in local space. You can't use world rotation as target rotation here");

                if (space == Space.Self)
                {
                    Quaternion clampedLocalRot = master.localRotation;

                    if (slaveBone.minLocalRot.eulerAngles.z > 180.0f/* &&
                        master.localRotation.eulerAngles.z < slaveBone.minLocalRot.eulerAngles.z*/)
                    {
                        clampedLocalRot = slaveBone.minLocalRot;
                    }
                    ConfigurableJointExtensions.SetTargetRotationLocal(slaveBone.jointRef, clampedLocalRot, slaveBone.initialConnectedBodyLocalRotation);
                }
                else
                {
                    Quaternion worldToJointSpace = ConfigurableJointExtensions.GetWorldToJointSpace(slaveBone.jointRef);
                    Quaternion jointSpaceToWorld = Quaternion.Inverse(worldToJointSpace);
                    Quaternion resultRotation = ConfigurableJointExtensions.GetWorldResultRotation(slaveBone.jointRef, master.rotation, slaveBone.initialConnectedBodyLocalRotation, space, jointSpaceToWorld);

                    // Transform back into joint space
                    resultRotation *= worldToJointSpace;

                    // Set target rotation to our newly calculated rotation
                    slaveBone.jointRef.targetRotation = resultRotation;
                }

                // Local rotation offset
                slaveBone.jointRef.targetRotation *= Quaternion.Euler(slaveBone.targetEulerOffsetRot);

                if (boneConf.useDynamicStrength)
                    PhysHelpers.UpdateBoneStrength(slaveBone, boneConf.minDynamicRotDrive.toJointDrive(), boneConf.maxDynamicRotDrive.toJointDrive(), slaveBone.finger.strengthLerp);

            }
            
            if (boneConf.clampLinearVelocity)
                slaveBone.rigidbodyRef.velocity = Vector3.ClampMagnitude(slaveBone.rigidbodyRef.velocity, boneConf.maxLinearVelocity);

            if (boneConf.clampAngularVelocity)
                slaveBone.rigidbodyRef.angularVelocity = Vector3.ClampMagnitude(slaveBone.rigidbodyRef.angularVelocity, boneConf.maxAngularVelocity);
        }

        public void SetDecoupledMode(bool newDecoupled)
        {
            if (decoupled == newDecoupled)
                return;

            SlaveBoneConfiguration newConf;

            if (decoupled)
            {
                newConf = new SlaveBoneConfiguration(model.configuration.wrist);
                newConf.followsPosition = false;
                newConf.followsRotation = false;
                newConf.useGravity = true;
            }
            else
            {
                newConf = model.configuration.wrist;
            }

            for (int i = 0; i < slaveBones.Length; i++)
            {
                if (!slaveBones[i].jointRef)
                    continue;

                if (slaveBones[i] == model.proxyHand.slave.wrist)
                    PhysHelpers.SetSlaveBoneConfiguration(slaveBones[i].jointRef, newConf);
            }
        }

        IEnumerator RecoverFromError()
        {
            PhysHelpers.SetCollisionDetectionForBones(model.proxyHand.slave, false);

            yield return new WaitForSeconds(1.0f);

            PhysHelpers.SetCollisionDetectionForBones(model.proxyHand.slave, true);
        }
    }
}
