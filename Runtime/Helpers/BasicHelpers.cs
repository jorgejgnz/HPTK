using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Helpers
{
    public static class BasicHelpers
    {
        public static void SetLocalRotForHierarchy(Transform t, Vector3 newLocalRot)
        {
            t.localRotation = Quaternion.Euler(newLocalRot);
            for (int i = 0; i < t.childCount; i++)
            {
                SetLocalRotForHierarchy(t.GetChild(i), newLocalRot);
            }
        }

        public static bool IsCleanBranch(Transform t)
        {
            if (t.childCount > 1)
                return false;
            else if (t.childCount == 0)
                return true;
            else
                return IsCleanBranch(t.GetChild(0));
        }

        public static GameObject InstantiateEmptyChild(GameObject parent)
        {
            GameObject newObj = new GameObject();
            newObj.transform.parent = parent.transform;
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;

            return newObj;
        }

        public static GameObject InstantiateEmptyChild(GameObject parent, string name)
        {
            GameObject newObj = new GameObject();
            newObj.transform.parent = parent.transform;
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.transform.name = name;

            return newObj;
        }

        public static Vector3 FurthestPoint(Vector3 from, Vector3[] points)
        {
            Vector3 furthestPoint = from;
            float maxDistance = 0.0f;
            float d;
            for (int i = 0; i < points.Length; i++)
            {
                d = Vector3.Distance(from, points[i]);

                if (d > maxDistance)
                {
                    maxDistance = d;
                    furthestPoint = points[i];
                }
            }

            return furthestPoint;
        }

        public static Vector3 ClosestPoint(Vector3 to, Vector3[] points)
        {
            Vector3 closestPoint = to;
            float minDistance = Mathf.Infinity;
            float d;
            for (int i = 0; i < points.Length; i++)
            {
                d = Vector3.Distance(to, points[i]);

                if (d < minDistance)
                {
                    minDistance = d;
                    closestPoint = points[i];
                }
            }

            return closestPoint;
        }

        public static Quaternion[] GetRotations(Transform[] transforms, Space space)
        {
            List<Quaternion> rotations = new List<Quaternion>();

            for (int i = 0; i < transforms.Length; i++)
            {
                if (space == Space.World)
                    rotations.Add(transforms[i].rotation);
                else
                    rotations.Add(transforms[i].localRotation);
            }

            return rotations.ToArray();
        }

        public static Quaternion[] GetRelativeRotations(Transform[] transforms, Transform parent)
        {
            List<Quaternion> rotations = new List<Quaternion>();

            for (int i = 0; i < transforms.Length; i++)
            {
                rotations.Add(Quaternion.Inverse(transforms[i].rotation) * parent.rotation);
            }

            return rotations.ToArray();
        }

        public static Transform GetClosestTransform(Transform[] tsfs, Vector3 point)
        {
            float d;
            float minDistance = Mathf.Infinity;
            Transform candidate = null;

            for (int i = 0; i < tsfs.Length; i++)
            {
                d = Vector3.Distance(tsfs[i].position, point);
                if (d < minDistance)
                {
                    minDistance = d;
                    candidate = tsfs[i];
                }
            }

            return candidate;
        }

        public static T FindHandler<T>(HPTKHandler[] handlers) where T : HPTKHandler
        {
            for (int i = 0; i < handlers.Length; i++)
            {
                if (handlers[i] is T)
                    return handlers[i] as T;
            }

            return null;
        }

        public static T FindScriptableObject<T>(ScriptableObject[] scriptableObjects) where T : ScriptableObject
        {
            for (int i = 0; i < scriptableObjects.Length; i++)
            {
                if (scriptableObjects[i] is T)
                    return scriptableObjects[i] as T;
            }

            return null;
        }

        public static Quaternion ClampQuaternion(Quaternion origin, Quaternion destination, float maxAngle)
        {
            float angle = Quaternion.Angle(origin, destination);

            float lerp = Mathf.Clamp(angle, 0.0f, maxAngle);

            return Quaternion.Lerp(origin, destination, lerp);
        }
    }
}
