using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class Pinch : FingerGesture
    {
        float minAbsDistance, maxAbsDistance;
        float distance;

        public override sealed void InitFingerGesture()
        {
            base.InitFingerGesture();
        }

        public override sealed void FingerLerpUpdate()
        {
            base.FingerLerpUpdate();

            // Lerp
            if (_model.flex.lerp < conf.minFlexLerpToDisablePinch)
            {
                if (_model == _model.parent.thumb)
                {
                    _lerp = 0.0f;

                    for (int f = 0; f < _model.parent.fingers.Count; f++)
                    {
                        FingerGesturesModel finger = _model.parent.fingers[f];

                        if (finger == _model.parent.thumb || finger.pinch == null)
                            continue;

                        if (finger.pinch.lerp > _lerp) _lerp = finger.pinch.lerp;
                    }
                }
                else
                {
                    minAbsDistance = conf.minPinchRelDistance * _model.finger.hand.totalScale;
                    maxAbsDistance = conf.maxPinchRelDistance * _model.finger.hand.totalScale;

                    distance = Vector3.Distance(_model.finger.tip.transformRef.position, _model.finger.hand.thumb.tip.transformRef.position);

                    _lerp = 1.0f - Mathf.InverseLerp(minAbsDistance, maxAbsDistance, distance);
                }
            }         
            else
                _lerp = 0.0f;

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
