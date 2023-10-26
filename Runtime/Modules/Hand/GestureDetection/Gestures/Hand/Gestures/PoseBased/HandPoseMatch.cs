using HandPhysicsToolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class HandPoseMatch : HandGesture
    {
        [Header("Pose")]
        public HandPoseAsset pose;
        public float maxAngle = 30.0f;

        [Header("Related finger gestures")]
        public FingerPoseMatch thumb;
        public FingerPoseMatch index;
        public FingerPoseMatch middle;
        public FingerPoseMatch ring;
        public FingerPoseMatch pinky;

        [Header("Control")]
        public bool manualMode = false;

        List<FingerPoseMatch> fingers = new List<FingerPoseMatch>();

        public override sealed void InitHandGesture()
        {
            base.InitHandGesture();

            StartFinger(thumb);
            StartFinger(index);
            StartFinger(middle);
            StartFinger(ring);
            StartFinger(pinky);
        }

        public override sealed void HandLerpUpdate()
        {
            base.HandLerpUpdate();

            if (pose != null)
            {
                UpdateFinger(thumb, pose.thumb);
                UpdateFinger(index, pose.index);
                UpdateFinger(middle, pose.middle);
                UpdateFinger(ring, pose.ring);
                UpdateFinger(pinky, pose.pinky);

                // Lerp
                if (!manualMode)
                {
                    _lerp = 0.0f;
                    for (int f = 0; f < fingers.Count; f++)
                    {
                        _lerp += fingers[f].lerp;
                    }
                    if (fingers.Count > 0) _lerp = _lerp / fingers.Count;
                }
            }
            else
            {
                _lerp = 0.0f;
            }

            // Gesture intention  
            if (providesIntention)
            {
                // Backup previous values
                wasIntentionallyActive = isIntentionallyActive;

                if (isActive)
                {
                    intentionTime = Mathf.Clamp(timeActive, 0.0f, conf.minTimeToIntention);
                }
                else if (intentionTime > 0.0f)
                {
                    intentionTime = Mathf.Clamp(intentionTime - Time.deltaTime, 0.0f, conf.minTimeToIntention);
                }

                _intentionLerp = Mathf.InverseLerp(0.0f, conf.minTimeToIntention, intentionTime);

                _isIntentionallyActive = intentionLerp >= 1.0f;
            }
        }

        public override sealed void LateGestureUpdate()
        {
            base.LateGestureUpdate();

            // Intention events
            if (isIntentionallyActive && !wasIntentionallyActive)
                onIntendedActivation.Invoke();
            else if (!isIntentionallyActive && wasIntentionallyActive)
                onIntendedDeactivation.Invoke();
        }

        void StartFinger(FingerPoseMatch finger)
        {
            if (finger) fingers.Add(finger);
        }

        void UpdateFinger(FingerPoseMatch finger, FingerPose pose)
        {
            if (!finger || pose == null) return;

            finger.pose = pose;
            finger.maxAngle = maxAngle;
        }
    }
}
