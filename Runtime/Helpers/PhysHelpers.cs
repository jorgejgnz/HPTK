using HPTK.Components;
using HPTK.Models.Avatar;
using HPTK.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Helpers
{
    public static class PhysHelpers
    {
        public static Vector3 GetDesiredWorldPos(Transform slave, Transform master, Space space)
        {
            Vector3 desiredPosition;

            if (space == Space.Self)
                desiredPosition = slave.parent.TransformPoint(master.localPosition);
            else
                desiredPosition = master.position;
            // desiredPosition += worldPosOffset;

            return desiredPosition;
        }

        public static Quaternion GetDesiredWorldRot(Transform slave, Transform master, Space space)
        {
            Quaternion desiredRotation;

            if (space == Space.Self)
                desiredRotation = slave.parent.rotation * master.localRotation;
            else
                desiredRotation = master.transform.rotation;
            // desiredRotation *= Quaternion.Euler(worldRotOffset.x, worldRotOffset.y, worldRotOffset.z);

            return desiredRotation;
        }
        public static bool IsValid(Vector3 v)
        {
            if (float.IsInfinity(v.x)
                 || float.IsNegativeInfinity(v.x)
                 || float.IsNaN(v.x)) return false;

            if (float.IsInfinity(v.y)
                 || float.IsNegativeInfinity(v.y)
                 || float.IsNaN(v.y)) return false;

            if (float.IsInfinity(v.z)
                 || float.IsNegativeInfinity(v.z)
                 || float.IsNaN(v.z)) return false;

            return true;
        }

        public static void SetSlaveBoneConfiguration(ConfigurableJoint joint, SlaveBoneConfiguration conf)
        {
            Rigidbody rb = joint.GetComponent<Rigidbody>();
            rb.mass = conf.rigidbodyMass;
            rb.drag = conf.rigidbodyDrag;
            rb.angularDrag = conf.rigidbodyAngularDrag;
            rb.useGravity = conf.useGravity;

            if (conf.followsRotation)
            {
                joint.angularXDrive = conf.rotationDrive.toJointDrive();
                joint.angularYZDrive = conf.rotationDrive.toJointDrive();
                joint.slerpDrive = conf.rotationDrive.toJointDrive();
            }
            else
            {
                joint.angularXDrive = CustomJointDrive.zero.toJointDrive();
                joint.angularYZDrive = CustomJointDrive.zero.toJointDrive();
                joint.slerpDrive = CustomJointDrive.zero.toJointDrive();
            }

            if (conf.useTargetPos)
            {
                joint.xDrive = conf.positionDrive.toJointDrive();
                joint.yDrive = conf.positionDrive.toJointDrive();
                joint.zDrive = conf.positionDrive.toJointDrive();
            }
            else
            {
                joint.xDrive = CustomJointDrive.zero.toJointDrive();
                joint.yDrive = CustomJointDrive.zero.toJointDrive();
                joint.zDrive = CustomJointDrive.zero.toJointDrive();
            }

            joint.massScale = conf.jointMassScale;
            joint.connectedMassScale = conf.jointConnectedMassScale;
        }

        public static void UpdateBoneStrength(SlaveBoneModel bone, JointDrive minJointDrive, JointDrive maxJointDrive, float strength)
        {
            JointDrive updatedJointDrive = new JointDrive();
            updatedJointDrive.positionSpring = Mathf.Lerp(minJointDrive.positionSpring, maxJointDrive.positionSpring, strength);
            updatedJointDrive.positionDamper = Mathf.Lerp(minJointDrive.positionDamper, maxJointDrive.positionDamper, strength);
            updatedJointDrive.maximumForce = Mathf.Lerp(minJointDrive.maximumForce, maxJointDrive.maximumForce, strength);

            bone.jointRef.angularXDrive = updatedJointDrive;
            bone.jointRef.angularYZDrive = updatedJointDrive;
            bone.jointRef.slerpDrive = updatedJointDrive;
        }

        public static bool ToggleLockJoints(SlaveBoneModel[] bones, bool toggleValue)
        {
            toggleValue = !toggleValue;

            for (int i = 1; i < bones.Length; i++)
            {
                // Wrist, forearm and pinky0 are ignored
                if (!bones[i].jointRef || bones[i].jointRef.configuredInWorldSpace)
                    continue;

                if (toggleValue)
                {
                    bones[i].jointRef.xMotion = ConfigurableJointMotion.Locked;
                    bones[i].jointRef.yMotion = ConfigurableJointMotion.Locked;
                    bones[i].jointRef.zMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    bones[i].jointRef.xMotion = ConfigurableJointMotion.Free;
                    bones[i].jointRef.yMotion = ConfigurableJointMotion.Free;
                    bones[i].jointRef.zMotion = ConfigurableJointMotion.Free;
                }
            }

            return toggleValue;
        }

        public static ConfigurableJoint CreateSnapJoint(Rigidbody body, Rigidbody connectedBody, Vector3 pointPosition, Quaternion pointRotation, Vector3 destinationPosition, Quaternion destinationRotation, bool matchRotation, bool enableCollisions)
        {
            // Initial configuration
            ConfigurableJoint joint = body.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectedBody;
            joint.autoConfigureConnectedAnchor = false;
            joint.configuredInWorldSpace = false;

            joint.anchor = joint.transform.InverseTransformPoint(pointPosition);
            joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(destinationPosition);

            joint.massScale = 1.6f;

            joint.enableCollision = enableCollisions;

            // Linear limits
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;

            joint.targetPosition = Vector3.zero;

            // Target rotation
            if (matchRotation)
            {
                Quaternion initialRotation = joint.transform.rotation;

                Quaternion worldToJointSpace = ConfigurableJointExtensions.GetWorldToJointSpace(joint);
                Quaternion jointSpaceToWorld = Quaternion.Inverse(worldToJointSpace);

                Quaternion desiredWorldRot = (destinationRotation * Quaternion.Inverse(pointRotation)) * joint.transform.rotation;
                Quaternion resultRotation = jointSpaceToWorld * Quaternion.Inverse(desiredWorldRot) * initialRotation;

                // Transform back into joint space
                resultRotation *= worldToJointSpace;

                // Set target rotation to our newly calculated rotation
                joint.targetRotation = resultRotation;

                JointDrive rotDrive = new JointDrive();
                rotDrive.positionDamper = 0.0f;
                rotDrive.maximumForce = 0.0f;
                rotDrive.positionSpring = 0.0f;

                joint.rotationDriveMode = RotationDriveMode.Slerp;
                joint.slerpDrive = rotDrive;
            }

            return joint;
        }

        public static void FreeJoint(ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.xDrive = new JointDrive();
            joint.yDrive = new JointDrive();
            joint.zDrive = new JointDrive();
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.angularXDrive = new JointDrive();
            joint.angularYZDrive = new JointDrive();
            joint.slerpDrive = new JointDrive();
        }

        public static void UpdateJointMotionDrive(ConfigurableJoint joint, JointDrive motionDrive)
        {
            joint.xDrive = motionDrive;
            joint.yDrive = motionDrive;
            joint.zDrive = motionDrive;
        }

        public static void UpdateJointAngularDrive(ConfigurableJoint joint, JointDrive angularDrive)
        {
            joint.angularXDrive = angularDrive;
            joint.angularYZDrive = angularDrive;
            joint.slerpDrive = angularDrive;
        }

        public static void IgnoreChildrenCollisions(Rigidbody rbA, Rigidbody rbB, bool ignore)
        {
            Collider[] rbAcolliders = rbA.GetComponentsInChildren<Collider>();
            Collider[] rbBcolliders = rbB.GetComponentsInChildren<Collider>();
            for (int i = 0; i < rbAcolliders.Length; i++)
            {
                for (int j = 0; j < rbBcolliders.Length; j++)
                {
                    Physics.IgnoreCollision(rbAcolliders[i], rbBcolliders[j], ignore);
                }
            }
        }

        public static void SetCollisionDetectionForBones(SlaveHandModel hand, bool detectCollisions)
        {
            SlaveBoneModel slaveBone;
            for (int i = 0; i < hand.bones.Length; i++)
            {
                if (hand.bones[i] is SlaveBoneModel)
                {
                    slaveBone = hand.bones[i] as SlaveBoneModel;
                    if (slaveBone.rigidbodyRef != null)
                    {
                        slaveBone.rigidbodyRef.detectCollisions = detectCollisions;
                    }
                }
            }
        }

        public static void IgnoreEveryBoneCollisions(Rigidbody rb, SlaveHandModel hand, bool ignore, bool includeRbGroup)
        {
            SlaveBoneModel bone;
            List<Collider> rbColliders = new List<Collider>(rb.GetComponentsInChildren<Collider>());

            RigidbodyGroup rbGroup = rb.GetComponent<RigidbodyGroup>();
            if (rbGroup && includeRbGroup)
            {
                for (int i = 0; i < rbGroup.rigidbodies.Length; i++)
                {
                    rbColliders.AddRange(GetColliders(rbGroup.rigidbodies[i]));
                }
            }

            for (int i = 0; i < rbColliders.Count; i++)
            {
                for (int j = 0; j < hand.bones.Length; j++)
                {
                    if (hand.bones[j] is SlaveBoneModel) {

                        bone = hand.bones[j] as SlaveBoneModel;

                        if (bone.colliderRef != null)
                        {
                            Physics.IgnoreCollision(rbColliders[i], bone.colliderRef, ignore);
                        }
                    }
                }
            }
        }

        public static void IgnoreBoneCollisions(Rigidbody rb, SlaveBoneModel bone, bool ignore, bool includeRbGroup)
        {
            if (!bone.colliderRef)
                return;

            List<Collider> rbColliders = new List<Collider>(rb.GetComponentsInChildren<Collider>());

            RigidbodyGroup rbGroup = rb.GetComponent<RigidbodyGroup>();
            if (rbGroup && includeRbGroup)
            {
                for (int i = 0; i < rbGroup.rigidbodies.Length; i++)
                {
                    rbColliders.AddRange(GetColliders(rbGroup.rigidbodies[i]));
                }
            }

            for (int i = 0; i < rbColliders.Count; i++)
            {
                Physics.IgnoreCollision(rbColliders[i], bone.colliderRef, ignore);
            }
        }

        public static Collider[] GetColliders(Rigidbody rb)
        {
            List<Collider> colliders = new List<Collider>(rb.GetComponents<Collider>());

            colliders.AddRange(rb.GetComponentsInChildren<Collider>());

            return colliders.ToArray();
        }

        public static void IgnoreFingerTipsCollisions(Rigidbody rb, SlaveHandModel hand, bool ignore)
        {
            SlaveBoneModel bone;
            Collider[] rbColliders = rb.GetComponentsInChildren<Collider>();
            for (int i = 0; i < rbColliders.Length; i++)
            {
                for (int f = 0; f < hand.fingers.Length; f++)
                {
                    if (hand.fingers[f].distal is SlaveBoneModel)
                    {
                        bone = hand.fingers[f].distal as SlaveBoneModel;
                        if (bone.colliderRef != null)
                        {
                            Physics.IgnoreCollision(rbColliders[i], bone.colliderRef, ignore);
                        }
                    }
                }
            }
        }

        public static void JointLookAt(ConfigurableJoint joint, Transform pivot, Vector3 destinationWorldPos, Quaternion initialConnectedBodyLocalRotation)
        {
            Rigidbody rb = joint.GetComponent<Rigidbody>();

            Quaternion desiredWorldRot = Quaternion.LookRotation(destinationWorldPos - pivot.position);
            Quaternion deltaPivotRot = Quaternion.Inverse(pivot.rotation) * desiredWorldRot;
            Quaternion rbWorldRot = rb.rotation * deltaPivotRot;

            if (joint.configuredInWorldSpace)
            {
                Quaternion worldToJointSpace = ConfigurableJointExtensions.GetWorldToJointSpace(joint);
                Quaternion jointSpaceToWorld = Quaternion.Inverse(worldToJointSpace);
                Quaternion resultRotation = ConfigurableJointExtensions.GetWorldResultRotation(joint, rbWorldRot, initialConnectedBodyLocalRotation, Space.World, jointSpaceToWorld);

                // Transform back into joint space
                resultRotation *= worldToJointSpace;

                // Set target rotation to our newly calculated rotation
                joint.targetRotation = resultRotation;   
            }
            else
            {
                Quaternion connectedBodyDesiredLocalRot = Quaternion.Inverse(joint.connectedBody.rotation) * rbWorldRot;
                ConfigurableJointExtensions.SetTargetRotationLocal(joint, connectedBodyDesiredLocalRot, initialConnectedBodyLocalRotation);
            }
        }

        public static IEnumerator SmoothJoint(ConfigurableJoint joint, JointDrive startMotionDrive, JointDrive endMotionDrive, JointDrive startSlerpDrive, JointDrive endSlerpDrive, float duration)
        {
            float startTime = Time.time;

            float distCovered, fraction;

            while (Time.time <= startTime + duration)
            {
                if (joint == null)
                {
                    yield break;
                }

                distCovered = Time.time - startTime;
                fraction = Mathf.SmoothStep(0.0f, 1.0f, distCovered / duration);

                joint.xDrive = JointDriveLerp(startMotionDrive, endMotionDrive, fraction);
                joint.yDrive = JointDriveLerp(startMotionDrive, endMotionDrive, fraction);
                joint.zDrive = JointDriveLerp(startMotionDrive, endMotionDrive, fraction);

                joint.slerpDrive = JointDriveLerp(startSlerpDrive, endSlerpDrive, fraction);

                yield return new WaitForEndOfFrame();
            }
        }

        public static JointDrive JointDriveLerp (JointDrive min, JointDrive max, float lerp)
        {
            JointDrive jd = new JointDrive();
            jd.positionSpring = Mathf.SmoothStep(min.positionSpring, max.positionSpring, lerp);
            jd.positionDamper = Mathf.SmoothStep(min.positionDamper, max.positionDamper, lerp);
            jd.maximumForce = Mathf.SmoothStep(min.maximumForce, max.maximumForce, lerp);

            return jd;
        }

        public static IEnumerator SmoothLerpPosition(Transform item, Vector3 origin, Vector3 destination, float duration)
        {
            float startTime = Time.time;
            float journeyLength = Vector3.Distance(origin, destination);

            float distCovered, fractionOfJourney;

            while (Time.time <= startTime + duration)
            {
                distCovered = (Time.time - startTime) * (journeyLength / duration);
                fractionOfJourney = distCovered / journeyLength;

                item.localPosition = Vector3.Lerp(destination, origin, Mathf.SmoothStep(0.0f, 1.0f, fractionOfJourney));
                yield return new WaitForEndOfFrame();
            }
        }

        public static void SetHandPhysics(ProxyHandModel phModel, bool enabled)
        {
            SlaveBoneModel wristBone = phModel.slave.wrist as SlaveBoneModel;
            SetBonePhysics(wristBone, enabled);

            for (int f = 0; f < phModel.slave.fingers.Length; f++)
            {
                for (int b = 0; b < phModel.slave.fingers[f].bones.Length; b++)
                {
                    SlaveBoneModel slaveBone = phModel.slave.fingers[f].bones[b] as SlaveBoneModel;
                    SetBonePhysics(slaveBone, enabled);
                }
            }
        }

        public static void SetBonePhysics(SlaveBoneModel bone, bool enabled)
        {
            if (bone.rigidbodyRef)
                bone.rigidbodyRef.isKinematic = !enabled;

            if (bone.colliderRef)
                bone.colliderRef.enabled = enabled;
        }

        public static IEnumerator DoAfterFixedUpdate(Action toDo)
        {
            yield return new WaitForFixedUpdate();

            toDo.Invoke();
        }

        public static IEnumerator DoAfterUpdate(Action toDo)
        {
            yield return new WaitForEndOfFrame();

            toDo.Invoke();
        }

        public static IEnumerator DoAfter(float seconds, Action toDo)
        {
            yield return new WaitForSeconds(seconds);

            toDo.Invoke();
        }

    }
}
