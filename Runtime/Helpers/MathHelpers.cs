using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class MathHelpers
    {
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
    }
}
