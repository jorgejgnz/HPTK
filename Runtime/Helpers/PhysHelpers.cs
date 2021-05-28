using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
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
                    UnityEngine.Physics.IgnoreCollision(rbAcolliders[i], rbBcolliders[j], ignore);
                }
            }
        }

        public static void GetColliders(Rigidbody rb, List<Collider> _result)
        {
            rb.GetComponents<Collider>().ToList(_result);

            _result.AddRange(rb.GetComponentsInChildren<Collider>());
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

        public static void ClosestHitFromPoint(ref RaycastHit closestHit, RaycastHit[] hits, Ray ray, float rayLength, Rigidbody rb, bool collideWithOtherRbs, bool collideWithTriggers, int layerMask)
        {
            hits = UnityEngine.Physics.RaycastAll(ray, rayLength, layerMask);

            float minDistance = Mathf.Infinity;
            float distance;

            for (int h = 0; h < hits.Length; h++)
            {
                if (rb == null) continue;

                if (!collideWithOtherRbs && hits[h].rigidbody != rb) continue;

                if (!collideWithTriggers && hits[h].collider.isTrigger) continue;

                distance = Vector3.Distance(ray.origin, hits[h].point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHit = hits[h];
                }
            }
        }

        public static void ClosestHitFromLine(ref RaycastHit closestHit, ref RaycastHit tempHit, RaycastHit[] hits, Vector3 rayDir, float rayLength, Vector3 lineStartWorldPos, Vector3 lineEndWorldPos, int resolution, Rigidbody rb, bool ignoreOtherRbs, bool ignoreTriggers, int layerMask)
        {
            Vector3 slice = (lineEndWorldPos - lineStartWorldPos) / resolution;

            float minDistance = Mathf.Infinity;
            float tempDistance;

            Vector3 rayOrigin;
            Ray ray;
            for (int i = 1; i < resolution; i++)
            {
                rayOrigin = lineStartWorldPos + slice * i;
                ray = new Ray(rayOrigin, rayDir);
                ClosestHitFromPoint(ref tempHit, hits, ray, rayLength, rb, ignoreOtherRbs, ignoreTriggers, layerMask);

                tempDistance = Vector3.Distance(rayOrigin, tempHit.point);

                if (tempDistance < minDistance)
                {
                    minDistance = tempDistance;
                    closestHit = tempHit;
                }
            }
        }

        public static void SetSolverIterations(int solverIterations, int solverVelocityIterations)
        {
            UnityEngine.Physics.defaultSolverIterations = solverIterations;
            Pheasy.registry.ForEach(p =>
            {
                p.rb.solverIterations = solverIterations;
                p.rb.solverVelocityIterations = solverVelocityIterations;
            });
        }
    }
}
