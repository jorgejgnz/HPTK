using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
    public class OVRSkeletonTracker : InputDataProvider
    {
        public OVRHand handData;
        public OVRSkeleton boneData;

        IList<OVRBone> providedBones;

        public override void UpdateData()
        {
            base.UpdateData();

            if (!handData || !boneData)
                return;

            if (handData.IsTracked)
            {
                providedBones = boneData.Bones;

                for (int i = 0; i < providedBones.Count; i++)
                {
                    bones[i].space = Space.World;
                    bones[i].position = providedBones[i].Transform.position;
                    bones[i].rotation = providedBones[i].Transform.rotation;
                }

                UpdateFingers();
            }
        }

    }
}
