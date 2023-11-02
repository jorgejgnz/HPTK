using HandPhysicsToolkit;
using HandPhysicsToolkit.UI;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Constraint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Hand.Interactor;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    public class PlaneConstraint : Constraint
    {
        public PointInteractorView interactor;

        public override void OnMimicCompleted(PartModel part)
        {
            if (interactor)
            {
                if (part is HandModel)
                {
                    HandModel hand = part as HandModel;

                    // Move constrained wrist to fix UI penetration
                    float maxDist = 0.0f;
                    Vector3 wristOffset = Vector3.zero;
                    for (int i = 0; i < interactor.tracks.Count; i++)
                    {
                        if (interactor.tracks[i].state == PointTrackState.Pressing)
                        {
                            Vector3 offset = interactor.tracks[i].worldPlanePos - interactor.tracks[i].worldPos;
                            if (offset.magnitude > maxDist)
                            {
                                maxDist = offset.magnitude;
                                wristOffset = offset;
                            }
                        }
                    }

                    hand.wrist.reprs[ConstraintModel.key].transformRef.position = hand.wrist.reprs[ConstraintModel.key].transformRef.position + wristOffset;
                }
            }
            else
            {
                Debug.LogWarning("Point interactor view was not defined!");
            }
        }
    }
}
