using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class Fist : HandGesture
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
                if (_model.fingers[f].palmLine == null)
                    continue;

                sum += _model.fingers[f].palmLine.lerp;
                total++;
            }
            _lerp = sum / (float)total;

            // Intention
            _providesIntention = false;
        }
    }
}
