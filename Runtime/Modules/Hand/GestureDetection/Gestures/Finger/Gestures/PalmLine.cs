using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Helpers;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class PalmLine : FingerGesture
    {
        float minDist;
        float maxDist;
        Vector3 nearestPointToLine;
        float dist;

        public override sealed void InitFingerGesture()
        {
            base.InitFingerGesture();
        }

        public override sealed void FingerLerpUpdate()
        {
            base.FingerLerpUpdate();

            // Lerp
            minDist = conf.minPalmRelDistance * hand.totalScale;
            maxDist = conf.maxPalmRelDistance * hand.totalScale;
            nearestPointToLine = BasicHelpers.NearestPointOnFiniteLine(hand.palmExterior.transformRef.position, hand.palmInterior.transformRef.position, finger.tip.transformRef.position);
            dist = Vector3.Distance(nearestPointToLine, finger.tip.transformRef.position);
            _lerp = 1.0f - Mathf.InverseLerp(minDist, maxDist, dist);

            // Intention
            _providesIntention = false;
        }
    }
}
