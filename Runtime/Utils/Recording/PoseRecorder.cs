using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Assets;
using HandPhysicsToolkit.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    [ExecuteInEditMode]
    public class PoseRecorder : MonoBehaviour
    {
        public HandModel hand;
        public string representation = "master";

        [Header("Apply")]
        public bool applyInverted = false;
        public bool applyPos = false;
        public bool applyRot = true;
        public bool applyScale = false;
        public bool limitZRot = false;

        [Header("Apply on fingers")]
        public bool thumb = true;
        public bool index = true;
        public bool middle = true;
        public bool ring = true;
        public bool pinky = true;

        [Header("Pose to Apply or Overwrite")]
        public HandPoseAsset pose;

        [Header("Overwrite")]
        public string alias;

        bool dirty = false;

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && dirty)
            {
                AssetDatabase.SaveAssets();
                dirty = false;
            }
#endif
        }

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(pose);
            dirty = true;
#endif
            PoseHelpers.SaveHand(hand, representation, pose, alias);
        }

        public void Apply()
        {
            if (thumb) ApplyFinger(pose.thumb, hand.thumb);
            if (index) ApplyFinger(pose.index, hand.index);
            if (middle) ApplyFinger(pose.middle, hand.middle);
            if (ring) ApplyFinger(pose.ring, hand.ring);
            if (pinky) ApplyFinger(pose.pinky, hand.pinky);
        }

        void ApplyFinger(FingerPose pose, FingerModel finger)
        {
            if (finger) PoseHelpers.ApplyFingerPose(pose, finger, representation, applyPos, applyRot, applyScale, applyInverted, limitZRot);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PoseRecorder))]
    public class HandPoserEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PoseRecorder myScript = (PoseRecorder)target;

            if (GUILayout.Button("APPLY"))
            {
                myScript.Apply();
            }

            if (GUILayout.Button("OVERWRITE (!)"))
            {
                myScript.Save();
            }
        }
    }
#endif
}