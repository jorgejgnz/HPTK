using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace HandPhysicsToolkit.Helpers
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

        public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
        {
            Vector3 line = (end - start);
            float len = line.magnitude;
            line.Normalize();

            Vector3 v = pnt - start;
            float d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return start + line * d;
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

        public static Tout Get<Tin, Tout>(Dictionary<string,Tin> dict) where Tout : UnityEngine.Object where Tin : UnityEngine.Object
        {
            return dict.FirstOrDefault(x => x.Value is Tout).Value as Tout;
        }

        public static T GetComponentInParents<T>(Transform tsf)
        {
            Transform cursor = tsf;
            T component;
            while (cursor.parent != null)
            {
                component = cursor.GetComponent<T>();

                if (component != null) return component;

                cursor = cursor.parent;
            }
            return default(T);
        }

        public static Tout FindFirst<Tin,Tout>(List<Tin> list) where Tout : UnityEngine.Object where Tin : UnityEngine.Object
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Tout)
                    return list[i] as Tout;
            }

            return null;
        }

        public static void FindAll<Tin, Tout>(List<Tin> inList, List<Tout> _outList) where Tout : UnityEngine.Object where Tin : UnityEngine.Object
        {
            _outList.Clear();

            for (int i = 0; i < inList.Count; i++)
            {
                if (inList[i] is Tout)
                    _outList.Add(inList[i] as Tout);
            }
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

        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        public static float From360To180(float angle)
        {
            if (angle >= 180.0f)
                angle -= 180.0f;
            else
                angle += 180.0f;

            return angle;
        }

        public static void ApplyLayerRecursively(Transform root, int layer)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>();

            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.layer = layer;
            }
        }

        public static T GetComponentFromDirectChildren<T>(this Transform tsf) where T  : MonoBehaviour
        {
            T component = null;

            foreach (Transform transform in tsf)
            {
                if ((component = transform.GetComponent<T>()) != null) return component;
            }

            return component;
        }

        public static void GetComponentsFromDirectChildren<T>(this Transform t, List<T> _components) where T : MonoBehaviour
        {
            _components.Clear();

            foreach (Transform transform in t)
            {
                T component;
                if ((component = transform.GetComponent<T>()) != null) _components.Add(component);
            }
        }

        public static void GetDirectChildren(this Transform t, List<Transform> _children)
        {
            _children.Clear();

            foreach (Transform child in t)
            {
                _children.Add(child);
            }
        }

        public static void ExtractFromHierarchy(this Transform t, List<Transform> _previousChildren, out Transform previousParent)
        {
            t.GetDirectChildren(_previousChildren);

            previousParent = t.parent;

            t.parent = t.root;

            for (int c = 0; c < _previousChildren.Count; c++)
            {
                _previousChildren[c].parent = previousParent;
            }
        }

        public static void RotateIndependently(this Transform t, Quaternion newWorldRot, List<Transform> _previousChildren)
        {
            Transform previousParent;

            ExtractFromHierarchy(t, _previousChildren, out previousParent);

            t.rotation = newWorldRot;

            t.parent = previousParent;
            _previousChildren.ForEach(c => c.parent = t);
        }

        public static void ParentIndependently(this Transform t, Transform newParent, bool asIntermediary, List<Transform> _children)
        {
            List<Transform> previousChildren = new List<Transform>();

            Transform previousParent;

            t.ExtractFromHierarchy(previousChildren, out previousParent);

            if (asIntermediary)
            {
                newParent.GetDirectChildren(_children);
                _children.ForEach(c => c.parent = t);
            }

            t.parent = newParent;
        }

        public static Transform GetFirstChild(this Transform parent)
        {
            if (!parent) return null;
            if (parent.childCount < 1) return null;
            return parent.GetChild(0);
        }

        public static void FindAll<T>(this T[] array, Predicate<T> p, List<T> _result)
        {
            _result.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                if (p(array[i])) _result.Add(array[i]);
            }
        }

        public static void FindAll<T>(this List<T> list, Predicate<T> p, List<T> _result)
        {
            _result.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (p(list[i])) _result.Add(list[i]);
            }
        }

        public static void ConvertAll<Tin,Tout>(this List<Tin> list, Converter<Tin,Tout> converter, List<Tout> _result)
        {
            _result.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                _result.Add(converter(list[i]));
            }
        }

        public static void Except<T>(this List<T> list, List<T> toExclude, List<T> _result)
        {
            _result.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (!toExclude.Contains(list[i])) _result.Add(list[i]);
            }
        }

        public static void ToList<T>(this T[] array, List<T> _result)
        {
            _result.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                _result.Add(array[i]);
            }
        }

        public static void ToList<T>(this List<T> list, List<T> _result)
        {
            _result.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                _result.Add(list[i]);
            }
        }
    }
}
