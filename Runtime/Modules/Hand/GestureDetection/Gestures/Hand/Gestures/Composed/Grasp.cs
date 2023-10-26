using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class Grasp : HandGesture
    {
        float sum;
        int total;

        public override sealed void InitHandGesture()
        {
            base.InitHandGesture();
        }

        public override sealed void HandLerpUpdate()
        {
            base.HandLerpUpdate();

            // Lerp
            sum = 0.0f;
            total = 0;
            for (int f = 0; f < _model.fingers.Count; f++)
            {
                if (_model.fingers[f] == _model.thumb || _model.fingers[f].baseRotation == null)
                    continue;

                sum += _model.fingers[f].baseRotation.lerp;
                total++;
            }
            _lerp = sum / (float)total;

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
    }
}
