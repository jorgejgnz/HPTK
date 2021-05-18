using HandPhysicsToolkit;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class PointReprsEditor : MonoBehaviour
    {
        [Serializable]
        public class RelationPointReprs
        {
            [HideInInspector]
            public string name;

            public string endsWith;

            public PointModel point;
            public List<ReprModel> representations = new List<ReprModel>();

            [HideInInspector]
            public List<ReprModel> previousRepresentations = new List<ReprModel>();

            public RelationPointReprs() { }

            public RelationPointReprs(PointModel point) { this.point = point; }
        }

        public HPTKModel modelRoot;
        public List<Transform> searchForReprsUnderThese = new List<Transform>();

        public List<RelationPointReprs> points = new List<RelationPointReprs>();

        public void Assign()
        {
            for (int p = 0; p < points.Count; p++)
            {
                if (!points[p].point) continue;

                points[p].representations.ForEach(r => r.point = points[p].point);
                points[p].previousRepresentations.ForEach(r => { if (!points[p].representations.Contains(r) && r != null) r.point = null; });
                points[p].previousRepresentations = new List<ReprModel>(points[p].representations);
            }
        }

        public void FindPoints()
        {
            points.Clear();

            PointModel[] pointModels = modelRoot.GetComponentsInChildren<PointModel>();

            for (int p = 0; p < pointModels.Length; p++)
            {
                points.Add(new RelationPointReprs(pointModels[p]));
            }
        }

        public void FindComparingPoints()
        {
            List<ReprModel> reprs = new List<ReprModel>();

            searchForReprsUnderThese.ForEach(x => reprs.AddRange(x.GetComponentsInChildren<ReprModel>(true)));

            List<ReprModel> reprsOfPoint;
            for (int p = 0; p < points.Count; p++)
            {
                reprsOfPoint = reprs.FindAll(r => r.point == points[p].point);

                for (int r = 0; r < reprsOfPoint.Count; r++)
                {
                    if (!points[p].representations.Contains(reprsOfPoint[r])) points[p].representations.Add(reprsOfPoint[r]);
                }
            }
        }

        public void FindComparingNames()
        {
            List<Transform> reprs = new List<Transform>();

            searchForReprsUnderThese.ForEach(x => reprs.AddRange(x.GetComponentsInChildren<Transform>(true)));

            List<Transform> reprsOfPoint;
            for (int p = 0; p < points.Count; p++)
            {
                if (points[p].endsWith == "") continue;

                reprsOfPoint = reprs.FindAll(r => r.name.ToLower().EndsWith(points[p].endsWith.ToLower()));

                for (int r = 0; r < reprsOfPoint.Count; r++)
                {
                    ReprModel reprModel = reprsOfPoint[r].GetComponent<ReprModel>();
                    if (reprModel != null && !points[p].representations.Contains(reprModel)) points[p].representations.Add(reprModel);
                    else Debug.LogWarning(reprsOfPoint[r].name + " does not have a ReprModel attached!");
                }
            }
        }

        public void ClearRepresentations()
        {
            points.ForEach(p => p.representations.Clear());
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            points.ForEach(p => { if (p.point) p.name = p.point.name; });
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PointReprsEditor), editorForChildClasses: true), CanEditMultipleObjects]
public class PointReprsEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PointReprsEditor myScript = (PointReprsEditor)target;

        if (GUILayout.Button("FIND POINTS (FROM MODEL ROOT)"))
        {
            myScript.FindPoints();
        }

        if (GUILayout.Button("FIND REPRS COMPARING POINTS (FROM REPR ROOT)"))
        {
            myScript.FindComparingPoints();
        }

        if (GUILayout.Button("FIND REPRS COMPARING NAME ENDINGS (FROM REPR ROOT)"))
        {
            myScript.FindComparingNames();
        }

        if (GUILayout.Button("ASSIGN POINTS TO REPRS"))
        {
            myScript.Assign();
        }

        if (GUILayout.Button("CLEAR REPRS"))
        {
            myScript.ClearRepresentations();
        }
    }
}
#endif
