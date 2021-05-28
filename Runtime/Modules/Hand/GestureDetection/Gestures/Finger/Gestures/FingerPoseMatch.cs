using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class FingerPoseMatch : FingerGesture
    {
        [HideInInspector]
        public FingerPose pose;

        public float maxAngle = 30.0f;

        Quaternion expectedRot, realRot;

        public override sealed void InitFingerGesture()
        {
            base.InitFingerGesture();
        }

        public override sealed void FingerLerpUpdate()
        {
            base.FingerLerpUpdate();

            if (pose == null || maxAngle <= 0)
                return;

            // Lerp

            int poseBone = pose.bones.Count - 1; // From tip to root
            int count = 0;
            _lerp = 0.0f;

            for (int b = _model.finger.bonesFromRootToTip.Count - 1; b >= 0; b--)
            {
                if (poseBone < 0)
                    break;

                expectedRot = pose.bones[poseBone].rotation;

                if (pose.bones[poseBone].space == Space.Self)
                {
                    realRot = _model.finger.bonesFromRootToTip[b].point.master.localRotation;

                    // If missing thumb0 or pinky0 in hand
                    if (poseBone > 0 && b == 0)
                    {
                        expectedRot = pose.bones[poseBone - 1].rotation * expectedRot;
                    }
                    // If missing thumb0 or pinky0 in pose
                    else if (poseBone == 0 && b > 0)
                    {
                        expectedRot = realRot;
                    }
                }
                else
                {
                    realRot = _model.finger.bonesFromRootToTip[b].transformRef.rotation;
                }

                _lerp += Mathf.InverseLerp(maxAngle, 0.0f, Quaternion.Angle(expectedRot, realRot));

                count++;

                poseBone--;
            }

            if (count > 0) _lerp = _lerp / count;

            // Intention
            _providesIntention = false;
        }
    }
}
