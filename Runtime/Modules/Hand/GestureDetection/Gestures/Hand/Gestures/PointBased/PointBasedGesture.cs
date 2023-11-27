using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class PointBasedGesture : HandGesture
    {
        [Header("Point-based Gesture")]
        public PointBasedGestureAsset points;
        public float threshold = 0.05f;

        [Space]
        public bool thumb = true;
        public bool index = true;
        public bool middle = true;
        public bool ring = true;
        public bool pinky = true;

        private List<float> distances = new List<float>();

        private float Distance(HandModel hand, FingerModel finger, Vector3 expectedLocalPos)
        {
            // Gesture should work even if some finger is missing
            if (!finger) return 0.0f;

            Vector3 actualLocalPos = hand.wrist.transformRef.InverseTransformPoint(finger.tip.transformRef.position);
            if (hand.side != points.handSide) actualLocalPos *= -1.0f;
            
            return Vector3.Distance(expectedLocalPos, actualLocalPos);
        }

        public override sealed void InitHandGesture()
        {
            base.InitHandGesture();
        }

        public override sealed void HandLerpUpdate()
        {
            base.HandLerpUpdate();

            distances.Clear();
            if (thumb) distances.Add(Distance(hand, hand.thumb, points.thumbTip));
            if (index) distances.Add(Distance(hand, hand.index, points.indexTip));
            if (middle) distances.Add(Distance(hand, hand.middle, points.middleTip));
            if (ring) distances.Add(Distance(hand, hand.ring, points.ringTip));
            if (pinky) distances.Add(Distance(hand, hand.pinky, points.pinkyTip));

            // Lerp
            if (distances.Any(d => d > threshold))
            {
                _lerp = 0.0f;
            }
            else
            {
                _lerp = 1.0f - Mathf.InverseLerp(0.0f, threshold, distances.Average());
            }
        }

        public override sealed void LateGestureUpdate()
        {
            base.LateGestureUpdate();
        }
    }
}
