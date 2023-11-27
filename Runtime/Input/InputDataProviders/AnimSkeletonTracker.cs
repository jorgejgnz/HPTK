using HandPhysicsToolkit.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Input
{
    public class AnimSkeletonTracker : InputDataProvider
    {
        [Header("Animation")]
        public AvatarRecordingAsset playThis;
        public bool moveWrist = true;
        public bool playing = true;

        [Header("Control")]
        [Range(0.0f, 1.0f)]
        public float lerp = 0.0f;
        public float stopAtLerp = -1.0f;
        public float replayAtLerp = -1.0f;

        [Header("Events")]
        public UnityEvent animationCompleted = new UnityEvent();

        int frameIndex = 1;
        float timeSinceLastFrame;

        /*
        * 0 - wrist
        * 1 - forearm
        * 
        * 2 - thumb0
        * 3 - thumb1
        * 4 - thumb2
        * 5 - thumb3
        * 
        * 6 - index1
        * 7 - index2
        * 8 - index3
        * 
        * 9 - middle1
        * 10 - middle2
        * 11 - middle3
        * 
        * 12 - ring1
        * 13 - ring2
        * 14 - ring3
        * 
        * 15 - pinky0
        * 16 - pinky1
        * 17 - pinky2
        * 18 - pinky3
        */

        private void Update()
        {
            if (playThis == null || playThis.frames.Count == 0)
            {
                Debug.LogWarning("Null or empty avatar recording!");
                return;
            }

            if (playing)
            {
                timeSinceLastFrame += Time.deltaTime;

                if (timeSinceLastFrame >= (1 / playThis.fps))
                {
                    timeSinceLastFrame = 0.0f;

                    frameIndex++;

                    if (frameIndex >= playThis.frames.Count || (replayAtLerp > 0.0f && lerp >= replayAtLerp))
                    {
                        frameIndex = 0;
                        animationCompleted.Invoke();
                    }

                    lerp = Mathf.InverseLerp(0.0f, (float)(playThis.frames.Count - 1), (float)frameIndex);

                    if (stopAtLerp >= 0.0f && lerp >= stopAtLerp) Stop();
                }
            }
            else
            {
                frameIndex = (int)Mathf.Lerp(0.0f, (float)(playThis.frames.Count - 1), lerp);
            }
        }

        public override void UpdateData()
        {
            base.UpdateData();

            if (playThis == null || playThis.frames.Count == 0)
            {
                log = "Null or empty avatar recording!";
                return;
            }

            IDPSample frame;
            if (this.side == Helpers.Side.Right) frame = playThis.frames[frameIndex].rightHand;
            else if (this.side == Helpers.Side.Left) frame = playThis.frames[frameIndex].leftHand;
            else { Debug.LogError("Side not recognized!"); return; }

            Vector3 wristPos = transform.position;
            Quaternion wristRot = transform.rotation;
            if (moveWrist)
            {
                wristPos = frame.wrist.position;
                wristRot = frame.wrist.rotation;
            }

            UpdateBone(0, Space.World, wristRot, wristPos);
            UpdateBone(1, Space.Self, Quaternion.identity, Vector3.zero);

            UpdateBone(2, Space.Self, frame.thumb0.rotation, frame.thumb0.position);
            UpdateBone(3, Space.Self, frame.thumb1.rotation, frame.thumb1.position);
            UpdateBone(4, Space.Self, frame.thumb2.rotation, frame.thumb2.position);
            UpdateBone(5, Space.Self, frame.thumb3.rotation, frame.thumb3.position);

            UpdateBone(6, Space.Self, frame.index1.rotation, frame.index1.position);
            UpdateBone(7, Space.Self, frame.index2.rotation, frame.index2.position);
            UpdateBone(8, Space.Self, frame.index3.rotation, frame.index3.position);

            UpdateBone(9, Space.Self, frame.middle1.rotation, frame.middle1.position);
            UpdateBone(10, Space.Self, frame.middle2.rotation, frame.middle2.position);
            UpdateBone(11, Space.Self, frame.middle3.rotation, frame.middle3.position);

            UpdateBone(12, Space.Self, frame.ring1.rotation, frame.ring1.position);
            UpdateBone(13, Space.Self, frame.ring2.rotation, frame.ring2.position);
            UpdateBone(14, Space.Self, frame.ring3.rotation, frame.ring3.position);

            UpdateBone(15, Space.Self, frame.pinky0.rotation, frame.pinky0.position);
            UpdateBone(16, Space.Self, frame.pinky1.rotation, frame.pinky1.position);
            UpdateBone(17, Space.Self, frame.pinky2.rotation, frame.pinky2.position);
            UpdateBone(18, Space.Self, frame.pinky3.rotation, frame.pinky3.position);

            confidence = 1.0f;

            UpdateFingerPosesFromBones();

            log = $"Applying frame {frameIndex}";
        }

        private void UpdateBone(int i, Space space, Quaternion rotation, Vector3 position)
        {
            bones[i].space = space;
            bones[i].rotation = rotation;
            bones[i].position = position;
        }

        public void Play()
        {
            if (playThis == null)
            {
                Debug.LogError("Missing avatar recording!");
                return;
            }

            playing = true;
            timeSinceLastFrame = 0.0f;
        }

        public void Stop()
        {
            playing = false;
        }
    }
}
