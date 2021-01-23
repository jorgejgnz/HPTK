using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
    public class RelativeSkeletonTracker : InputDataProvider
    {
        [Header("References")]
        public Transform referenceHead;
        public Transform replicatedHead;

        public HandModel referenceHand;

        [Header("Copy scale")]
        public bool copyScale = true;
        [Range(-1.5f, 1.5f)]
        public float scaleOffset = 0.0f;

        Vector3 relPos;
        Quaternion relRot;

        public override void InitData()
        {
            base.InitData();
        }

        public override void UpdateData()
        {
            base.UpdateData();

            if (bones.Length != referenceHand.allTransforms.Length)
            {
                Debug.Log("Holy s*! " + bones.Length + " bones vs " + referenceHand.allTransforms.Length + " transforms");
                return;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                relRot = Quaternion.Inverse(referenceHead.transform.rotation) * referenceHand.allTransforms[i].rotation;
                relPos = referenceHead.InverseTransformPoint(referenceHand.allTransforms[i].position);

                bones[i].space = Space.World;
                bones[i].rotation = replicatedHead.rotation * relRot;
                bones[i].position = replicatedHead.TransformPoint(relPos);
            }

            confidence = 1.0f;

            if (copyScale)
            {
                scale = referenceHand.extraScale;
                scale += scaleOffset;
            }

            UpdateFingerPosesFromBones();
        }
    }
}
