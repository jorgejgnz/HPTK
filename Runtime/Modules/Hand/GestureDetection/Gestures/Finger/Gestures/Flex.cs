using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class Flex : FingerGesture
    {
        float minDist;
        float maxDist;
        float dist;

        public override sealed void InitFingerGesture()
        {
            base.InitFingerGesture();
        }

        public override sealed void FingerLerpUpdate()
        {
            base.FingerLerpUpdate();

            // Lerp
            dist = Vector3.Distance(finger.knuckle.transformRef.position, finger.tip.transformRef.position);
            minDist = conf.minFlexRelDistance * hand.totalScale;
            maxDist = finger.length;
            _lerp = 1.0f - Mathf.InverseLerp(minDist, maxDist, dist);

            // Intention
            _providesIntention = false;
        }
    }
}
