using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Assets;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.Input;
using HandPhysicsToolkit.Helpers;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    public class AvatarAnimRecorder : MonoBehaviour
    {
        [ReadOnly]
        public bool recording = false;

        public bool recordOnStart = false;
        public AvatarRecordingAsset overwriteThis;

        public AvatarView avatar;

        public float fps = 25.0f;
        public int maxSamples = 3600;

        float timeSinceLastSample;

        Transform _head;
        Transform head
        {
            get
            {
                if (_head == null) _head = avatar.body.torso.head.master.transformRef;
                return _head;
            }
        }

        InputDataProvider _leftIdp;
        InputDataProvider leftIdp
        {
            get
            {
                if (_leftIdp == null)
                {
                    InputView input = avatar.body.leftHand.GetRegisteredView<InputView>();
                    if (input && input.inputDataProvider) _leftIdp = input.inputDataProvider;
                }

                return _leftIdp;
            }
        }

        InputDataProvider _rightIdp;
        InputDataProvider rightIdp
        {
            get
            {
                if (_rightIdp == null)
                {
                    InputView input = avatar.body.rightHand.GetRegisteredView<InputView>();
                    if (input && input.inputDataProvider) _rightIdp = input.inputDataProvider;
                }

                return _rightIdp;
            }
        }

        private void Start()
        {
#if !UNITY_EDITOR
            if (recordOnStart) StartRecording();
#endif
        }

        private void OnApplicationPause(bool pause)
        {
            if (recording) StopRecording();
        }

        private void Update()
        {
            if (recording)
            {
                timeSinceLastSample += Time.deltaTime;

                if (timeSinceLastSample >= (1 / fps) && head && leftIdp && rightIdp)
                {
                    overwriteThis.frames.Add(new AvatarSample(avatar.body.referenceTsf, avatar.body.torso.head.master.transformRef, avatar.body.leftHand, avatar.body.rightHand));
                    timeSinceLastSample = 0.0f;
                    Debug.Log("Sample recorded!");
                }
            }

            if (recording && overwriteThis.frames.Count >= maxSamples) StopRecording();
        }

        public void StartRecording()
        {
            if (avatar.body.referenceTsf == null)
            {
                Debug.LogError("Body is missing its reference Transform (BodyModel.referenceTsf)");
                return;
            }

            timeSinceLastSample = (1 / fps);

            overwriteThis.frames.Clear();
            overwriteThis.fps = fps;

            recording = true;         
        }

        public void StopRecording()
        {
            recording = false;
            // Export(recordTitle, avatarRecording);

#if UNITY_EDITOR
            EditorUtility.SetDirty(overwriteThis);
            AssetDatabase.SaveAssets();
#endif
        }

        public void Export(string filename, AvatarRecordingAsset avatarRecording)
        {
            FileHelpers.ReplaceJson<AvatarRecordingAsset>(filename, avatarRecording);
            Debug.Log("Exported!");
        }

        private void OnDrawGizmos()
        {
            if (recording && overwriteThis.frames.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(overwriteThis.frames[overwriteThis.frames.Count - 1].head.position, 0.01f);
                Gizmos.DrawSphere(overwriteThis.frames[overwriteThis.frames.Count - 1].leftHand.wrist.position, 0.01f);
                Gizmos.DrawSphere(overwriteThis.frames[overwriteThis.frames.Count - 1].rightHand.wrist.position, 0.01f);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AvatarAnimRecorder))]
    public class AvatarRecorderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AvatarAnimRecorder myScript = (AvatarAnimRecorder)target;

            GUI.enabled = !myScript.recording;
            if (GUILayout.Button("START (!)"))
            {
                myScript.StartRecording();
            }
            GUI.enabled = myScript.recording;
            if (GUILayout.Button("STOP"))
            {
                myScript.StopRecording();
            }
            GUI.enabled = true;
        }
    }
#endif
}
