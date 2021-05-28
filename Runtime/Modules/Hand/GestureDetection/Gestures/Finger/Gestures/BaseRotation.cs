using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class BaseRotation : FingerGesture
    {
        float localRotZ;

        public override sealed void InitFingerGesture()
        {
            base.InitFingerGesture();
        }

        public override sealed void FingerLerpUpdate()
        {
            base.FingerLerpUpdate();

            // Lerp
            localRotZ = finger.knuckle.master.localRotation.eulerAngles.z;

            if (localRotZ <= conf.maxLocalRotZ && localRotZ >= conf.minLocalRotZ)
                _lerp = Mathf.InverseLerp(conf.maxLocalRotZ, conf.minLocalRotZ, localRotZ);
            else if (localRotZ < 0.0f || localRotZ > conf.maxLocalRotZ || (localRotZ >= 0.0f && localRotZ < 180.0f))
                _lerp = 0.0f;
            else
                _lerp = 1.0f;

            // Intention
            _providesIntention = false;
        }
    }
}
