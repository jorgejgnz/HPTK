using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class FingerGesturesPanel : MonoBehaviour
    {
        public FingerGesturesView finger;

        public ScaleLerper pinch;
        public ScaleLerper flex;
        public ScaleLerper palm;
        public ScaleLerper strength;
        public ScaleLerper rotation;
        public ScaleLerper gesture;

        Gesture extra;

        private void Update()
        {
            if (finger != null && finger.pinch) pinch.UpdateSize(finger.pinch.lerp); else pinch.UpdateSize(0.0f);
            if (finger != null && finger.flex) flex.UpdateSize(finger.flex.lerp); else flex.UpdateSize(0.0f);
            if (finger != null && finger.palmLine) palm.UpdateSize(finger.palmLine.lerp); else palm.UpdateSize(0.0f);
            if (finger != null && finger.strength) strength.UpdateSize(finger.strength.lerp); else strength.UpdateSize(0.0f);
            if (finger != null && finger.baseRotation) rotation.UpdateSize(finger.baseRotation.lerp); else rotation.UpdateSize(0.0f);
            if (extra) gesture.UpdateSize(extra.lerp); else gesture.UpdateSize(0.0f);
        }

        public void SearchExtraGesture()
        {
            if (finger != null && finger.extra.Count > 0) extra = finger.extra[0];
        }

        public void SearchInChildren()
        {
            ScaleLerper scaleLerper;
            for (int i = 0; i < transform.childCount; i++)
            {
                scaleLerper = transform.GetChild(i).GetComponent<ScaleLerper>();

                if (transform.GetChild(i).name.ToLower().Contains("palm")) palm = scaleLerper;
                else if (transform.GetChild(i).name.ToLower().Contains("strength")) strength = scaleLerper;
                else if (transform.GetChild(i).name.ToLower().Contains("flex")) flex = scaleLerper;
                else if (transform.GetChild(i).name.ToLower().Contains("rotation")) rotation = scaleLerper;
                else if (transform.GetChild(i).name.ToLower().Contains("pinch")) pinch = scaleLerper;
                else if (transform.GetChild(i).name.ToLower().Contains("gesture")) gesture = scaleLerper;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FingerGesturesPanel), editorForChildClasses: true), CanEditMultipleObjects]
    public class FingerGesturesPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FingerGesturesPanel myScript = (FingerGesturesPanel)target;

            if (GUILayout.Button("SEARCH IN CHILDREN"))
            {
                myScript.SearchInChildren();
            }
        }
    }
#endif
}
