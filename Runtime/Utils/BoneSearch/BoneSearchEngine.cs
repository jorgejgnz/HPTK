using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class BoneSearchEngine : MonoBehaviour
    {
        public bool drawLines = true;

        [HideInInspector]
        public bool canSearch = true;

        [Header("Bones")]
        public List<Bone> bones = new List<Bone>();

        public virtual void Search(){ }

        public List<Point> GetPointsFromBones()
        {
            List<Point> allPoints = bones.ConvertAll<Point>(b => b.parent).Concat(bones.ConvertAll<Point>(b => b.child)).ToList();

            List<Point> uniquePoints = new List<Point>();
            for (int p = 0; p < allPoints.Count; p++)
            {
                if (uniquePoints.Find(up => up.original == allPoints[p].original) == null) uniquePoints.Add(allPoints[p]);
            }

            uniquePoints.RemoveAll(p => p == null || p.original == null);

            return uniquePoints;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            bones.ForEach(b => DrawBone(b));
        }

        protected void DrawBone(Bone b)
        {
            if (b.parent.tsf) Gizmos.DrawSphere(b.parent.tsf.position, 0.01f);
            if (b.parent.tsf && b.child.tsf) Gizmos.DrawLine(b.parent.tsf.position, b.child.tsf.position);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoneSearchEngine))]
public class BoneSearchEngineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoneSearchEngine myScript = (BoneSearchEngine)target;

        GUI.enabled = myScript.canSearch;
        if (GUILayout.Button("SEARCH BONES"))
        {
            myScript.Search();
        }
        GUI.enabled = true;
    }

    protected void OriginalField(Point p, string label)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(label);

        p.original = EditorGUILayout.ObjectField(p.original, typeof(Transform), true) as Transform;

        EditorGUILayout.EndHorizontal();
    }
}
#endif
