using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Input;
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
            pose.alias = alias;

            pose.wrist = new AbstractTsf(
                        hand.wrist.transformRef.localPosition,
                        hand.wrist.transformRef.localRotation,
                        Space.Self,
                        hand.wrist.transformRef.localScale,
                        hand.wrist.transformRef.name);

            List<FingerPose> fingers = new List<FingerPose>();

            pose.thumb = SaveFinger(hand.thumb);
            fingers.Add(pose.thumb);

            pose.index = SaveFinger(hand.index);
            fingers.Add(pose.index);

            pose.middle = SaveFinger(hand.middle);
            fingers.Add(pose.middle);

            pose.ring = SaveFinger(hand.ring);
            fingers.Add(pose.ring);

            pose.pinky = SaveFinger(hand.pinky);
            fingers.Add(pose.pinky);

            pose.fingers = fingers;
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
            if (finger) PoseHelpers.ApplyFingerPose(pose, finger, representation, applyPos, applyRot, applyScale, applyInverted);
        }

        FingerPose SaveFinger(FingerModel fingerModel)
        {
            if (!fingerModel) return SaveFinger();

            List<AbstractTsf> bones = new List<AbstractTsf>();

            for (int b = 0; b < fingerModel.bonesFromRootToTip.Count; b++)
            {
                bones.Add(new AbstractTsf(
                    fingerModel.bonesFromRootToTip[b].master.localPosition,
                    fingerModel.bonesFromRootToTip[b].master.localRotation,
                    Space.Self,
                    fingerModel.bonesFromRootToTip[b].master.transformRef.localScale,
                    fingerModel.bonesFromRootToTip[b].master.transformRef.name));
            }

            FingerPose fingerPose = new FingerPose(fingerModel.finger);
            fingerPose.bones = bones;

            return fingerPose;
        }

        FingerPose SaveFinger()
        {
            FingerPose fingerPose = new FingerPose();
            fingerPose.bones = new List<AbstractTsf>();

            return fingerPose;
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