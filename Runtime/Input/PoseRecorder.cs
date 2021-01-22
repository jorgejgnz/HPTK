using System.Collections.Generic;
using UnityEngine;
using HPTK.Models.Avatar;
using HPTK.Input;
using HPTK.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HPTK.Utils
{
    public class PoseRecorder : MonoBehaviour
    {
        public HandModel hand;
        public HPTK.Input.HandPose pose;
        public string alias;

        [Header("On apply")]
        public bool applyInverted = false;
        public bool applyPos = false;
        public bool applyRot = true;
        public bool applyScale = false;

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(pose);
#endif

            // Check valid hand model initialization
            if (hand.fingers == null || hand.fingers.Length == 0)
            {
                Debug.LogWarning("HandModel was not initialized. Initializing...");
                AvatarHelpers.HandModelInit(hand);
            }

            // Save abstractTsfs
            List<FingerPose> fingers = new List<FingerPose>();

            // Poses always have 5 fingers in the same order for convention between hand models: thumb, index, middle, ring, pinky

            if (hand.thumb)
                fingers.Add(SaveFinger(hand.thumb));
            else
                fingers.Add(SaveFinger());

            if (hand.index)
                fingers.Add(SaveFinger(hand.index));
            else
                fingers.Add(SaveFinger());

            if (hand.middle)
                fingers.Add(SaveFinger(hand.middle));
            else
                fingers.Add(SaveFinger());

            if (hand.ring)
                fingers.Add(SaveFinger(hand.ring));
            else
                fingers.Add(SaveFinger());

            if (hand.pinky)
                fingers.Add(SaveFinger(hand.pinky));
            else
                fingers.Add(SaveFinger());

            pose.alias = alias;
            pose.fingers = fingers.ToArray();

            pose.wrist = new AbstractTsf(
                        hand.wrist.transformRef.localPosition,
                        hand.wrist.transformRef.localRotation,
                        Space.Self,
                        hand.wrist.transformRef.localScale,
                        hand.wrist.transformRef.name);

            if (hand.forearm)
            {
                pose.forearm = new AbstractTsf(
                            hand.forearm.transformRef.localPosition,
                            hand.forearm.transformRef.localRotation,
                            Space.Self,
                            hand.forearm.transformRef.localScale,
                            hand.forearm.transformRef.name);
            }
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public void Apply()
        {
            if (hand.fingers == null || hand.fingers.Length == 0)
            {
                AvatarHelpers.HandModelInit(hand);
            }

            pose.ApplyPose(hand, applyPos, applyRot, applyScale, applyInverted);
        }

        FingerPose SaveFinger(FingerModel fingerModel)
        {
            List<AbstractTsf> bones = new List<AbstractTsf>();

            for (int b = 0; b < fingerModel.bones.Length; b++)
            {
                bones.Add(new AbstractTsf(
                    fingerModel.bones[b].transformRef.localPosition,
                    fingerModel.bones[b].transformRef.localRotation,
                    Space.Self,
                    fingerModel.bones[b].transformRef.localScale,
                    fingerModel.bones[b].transformRef.name));
            }

            FingerPose fingerPose = new FingerPose(fingerModel.name);
            fingerPose.bones = bones.ToArray();

            return fingerPose;
        }

        FingerPose SaveFinger()
        {
            FingerPose fingerPose = new FingerPose("NULL");
            fingerPose.bones = new AbstractTsf[0];

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

            if (GUILayout.Button("OVERWRITE"))
            {
                myScript.Save();
            }
            if (GUILayout.Button("APPLY"))
            {
                myScript.Apply();
            }
        }
    }
#endif
}