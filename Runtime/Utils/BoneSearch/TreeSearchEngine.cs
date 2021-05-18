using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Utils;
using System;
using System.Linq;
using HandPhysicsToolkit.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    [ExecuteInEditMode]
    public class TreeSearchEngine : BoneSearchEngine
    {
        [Header("Tree root")]
        public Transform rootBone;

        public sealed override void Search()
        {
            bones.Clear();

            if (!rootBone) rootBone = transform;
            FindBones(rootBone);

            bool valid = true;
            bones.ForEach(b => { if (!b.IsValid()) valid = false; });
            if (!valid) Debug.LogError("Some bones were badly generated");
        }

        void FindBones(Transform tsf)
        {
            foreach (Transform child in tsf)
            {
                Point parentPoint = GetPointFromOriginal(tsf);
                if (parentPoint == null) parentPoint = new Point(tsf);

                Point childPoint = GetPointFromOriginal(child);
                if (childPoint == null) childPoint = new Point(child);

                bones.Add(new Bone(parentPoint, childPoint, 1.0f));

                FindBones(child);
            }
        }

        Point GetPointFromOriginal(Transform t)
        {
            for (int b = 0; b < bones.Count; b++)
            {
                if (bones[b].parent.original == t) return bones[b].parent;
                if (bones[b].child.original == t) return bones[b].child;
            }

            return null;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TreeSearchEngine))]
public class TreeBoneSearchEngineEditor : BoneSearchEngineEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif
