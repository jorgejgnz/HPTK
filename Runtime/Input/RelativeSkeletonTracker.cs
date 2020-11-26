using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
    public class RelativeSkeletonTracker : InputDataProvider
    {
        public Transform referenceHead;
        public Transform replicatedHead;

        public HandModel referenceHand;

        Vector3 relPos;
        Quaternion relRot;

        public override void UpdateData()
        {
            base.UpdateData();

            for (int i = 0; i < bones.Length; i++)
            {
                relRot = Quaternion.Inverse(referenceHead.transform.rotation) * referenceHand.allTransforms[i].rotation;
                relPos = referenceHead.InverseTransformPoint(referenceHand.allTransforms[i].position);

                bones[i].space = Space.World;
                bones[i].rotation = replicatedHead.rotation * relRot;
                bones[i].position = replicatedHead.TransformPoint(relPos);
            }

            confidence = 1.0f;

            UpdateFingers();
        }
    }
}
