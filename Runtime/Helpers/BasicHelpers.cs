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
    }
}
