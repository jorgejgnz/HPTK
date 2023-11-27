using HandPhysicsToolkit.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HandPhysicsToolkit.Helpers
{
    public static class MathHelpers
    {
        public static Vector3 InverseTransformPoint(Vector3 parentWorldPosition, Quaternion parentWorldRotation, Vector3 childWorldPosition)
        {
            Quaternion inverseParentRotation = Quaternion.Inverse(parentWorldRotation);
            Vector3 relativeWorldPosition = childWorldPosition - parentWorldPosition;
            Vector3 relativeLocalPosition = inverseParentRotation * relativeWorldPosition;
            return relativeLocalPosition;
        }

        public static Vector3 GetRelativePosForScaleOne(this Transform t, Vector3 worldPos)
        {
            return Vector3.Scale(Vector3.one, Quaternion.Inverse(t.rotation) * (worldPos - t.position));
        }

        public static Vector3 GetWorldPosForScaleOne(this Transform t, Vector3 localPos)
        {
            return t.position + t.rotation * Vector3.Scale(Vector3.one, localPos);
        }

        public static float UnclampedInverseLerp(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        public static bool IsNorm(this float value)
        {
            return value >= 0.0f && value <= 1.0f;
        }

        public static bool IsNorm(this Vector2 value)
        {
            return value.x.IsNorm() && value.y.IsNorm();
        }

        public static bool IsNorm(this Vector3 value)
        {
            return value.x.IsNorm() && value.y.IsNorm() && value.z.IsNorm();
        }

        public static float GetProcessedAngleZ(Quaternion rotation)
        {
            float angle = rotation.eulerAngles.z;

            if (angle >= 180.0f)
                angle -= 180.0f;
            else
                angle += 180.0f;

            return angle;
        }

        public static Vector3 InverseTransformPoint(Vector3 worldPos, Quaternion worldRot, Vector3 localScale, Vector3 worldPoint)
        {
            Vector3 inverseScale = new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
            Quaternion inverseRotation = Quaternion.Inverse(worldRot);
            Vector3 inversePosition = -worldPos;

            Vector3 localPoint = worldPoint;
            localPoint += inversePosition;
            localPoint = inverseRotation * localPoint;
            localPoint = Vector3.Scale(localPoint, inverseScale);

            return localPoint;
        }

        public static Vector3 TransformPoint(Vector3 worldPos, Quaternion worldRot, Vector3 localScale, Vector3 localPoint)
        {
            Vector3 worldPoint = Vector3.Scale(localPoint, localScale);
            worldPoint = worldRot * worldPoint;
            worldPoint += worldPos;

            return worldPoint;
        }

        public static Quaternion AverageQuaternions(List<Quaternion> quaternions)
        {
            Assert.IsTrue(quaternions != null && quaternions.Count > 0);

            float weight = 1.0f / quaternions.Count;
            Vector3 forwardAvg = Vector3.zero;
            Vector3 upwardAvg = Vector3.zero;
            for (int i = 0; i < quaternions.Count; i++)
            {
                forwardAvg += weight * (quaternions[i] * Vector3.forward);
                upwardAvg += weight * (quaternions[i] * Vector3.up);
            }

            return Quaternion.LookRotation(forwardAvg, upwardAvg);
        }

        public static Quaternion WeightedAverageQuaternions(List<Quaternion> quaternions, List<float> weights)
        {
            Assert.IsTrue(quaternions != null && quaternions.Count > 0);
            Assert.IsTrue(weights != null && weights.Count >= quaternions.Count);

            int count = quaternions.Count;
            Vector3 forwardSum = Vector3.zero;
            Vector3 upwardSum = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                forwardSum += weights[i] * (quaternions[i] * Vector3.forward);
                upwardSum += weights[i] * (quaternions[i] * Vector3.up);
            }
            forwardSum /= (float)count;
            upwardSum /= (float)count;

            return Quaternion.LookRotation(forwardSum, upwardSum);
        }

        public static Vector3 GetClosestPointToInfiniteLine(Vector3 pnt, Vector3 a, Vector3 b)
        {
            return a + Vector3.Project(pnt - a, b - a);
        }

        public static float FindNearestLerpSection(float lerp, int sections)
        {
            if (sections < 1)
            {
                throw new ArgumentException("Use at least 1 section.");
            }

            float sectionSize = 1f / sections;
            int nearestSectionIndex = Mathf.RoundToInt(lerp / sectionSize);
            float nearestSectionValue = nearestSectionIndex * sectionSize;

            return nearestSectionValue;
        }
    }
}
