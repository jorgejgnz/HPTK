using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Input
{
    public class PoseSkeletonTracker : InputDataProvider
    {
        [Header("Poses")]
        public HandPoseAsset startPose;
        public HandPoseAsset endPose;

        [Header("Control")]
        [Range(0.0f, 2.0f)]
        public float animationSpeed = 0.5f;
        [Range(0.0f,1.0f)]
        public float poseLerp;

        /*
        * 0 - wrist
        * 1 - forearm
        * 
        * 2 - thumb0
        * 3 - thumb1
        * 4 - thumb2
        * 5 - thumb3
        * 
        * 6 - index1
        * 7 - index2
        * 8 - index3
        * 
        * 9 - middle1
        * 10 - middle2
        * 11 - middle3
        * 
        * 12 - ring1
        * 13 - ring2
        * 14 - ring3
        * 
        * 15 - pinky0
        * 16 - pinky1
        * 17 - pinky2
        * 18 - pinky3
        */

        private void Update()
        {
            if (animationSpeed > 0.0f) poseLerp = 0.5f + (Mathf.Sin(Time.time * animationSpeed - 1.0f) / 2.0f);
        }

        public override void UpdateData()
        {
            base.UpdateData();

            UpdateBone(0, Space.World, transform.rotation, transform.position);
            UpdateBone(1, Space.Self, Quaternion.identity, Vector3.zero);

            // All poses will have 5 fingers but they can have more or less fingers

            // THUMB
            UpdateFingerFromPose(2, 4, startPose.thumb, endPose.thumb);
            // INDEX
            UpdateFingerFromPose(6, 3, startPose.index, endPose.index);
            // MIDDLE
            UpdateFingerFromPose(9, 3, startPose.middle, endPose.middle);
            // RING
            UpdateFingerFromPose(12, 3, startPose.ring, endPose.ring);
            // PINKY
            UpdateFingerFromPose(15, 4, startPose.pinky, endPose.pinky);

            confidence = 1.0f;

            UpdateFingerPosesFromBones();

            log = "Updating hands from poses\n";
        }

        void UpdateBone(int i, Space space, Quaternion rotation, Vector3 position)
        {
            bones[i].space = space;
            bones[i].rotation = rotation;
            bones[i].position = position;
        }

        void UpdateFingerFromPose(int idpFirstBoneIndex, int idpFingerBonesLength, FingerPose startPose, FingerPose endPose)
        {
            int idpFingerBoneIndex, startPoseIndex, endPoseIndex;
            Vector3 lerpedPos;
            Quaternion lerpedRot;

            for (int b = 1; b <= idpFingerBonesLength; b++)
            {
                idpFingerBoneIndex = idpFingerBonesLength - b;
                startPoseIndex = startPose.bones.Count - b;
                endPoseIndex = endPose.bones.Count - b;

                if (startPoseIndex >= 0 && endPoseIndex < 0)
                {
                    lerpedPos = startPose.bones[startPoseIndex].position;
                    lerpedRot = startPose.bones[startPoseIndex].rotation;
                }
                else if (startPoseIndex < 0 && endPoseIndex >= 0)
                {
                    lerpedPos = endPose.bones[endPoseIndex].position;
                    lerpedRot = endPose.bones[endPoseIndex].rotation;
                }
                else if (startPoseIndex >= 0 && endPoseIndex >= 0)
                {
                    lerpedPos = Vector3.Lerp(startPose.bones[startPoseIndex].position, endPose.bones[endPoseIndex].position, poseLerp);
                    lerpedRot = Quaternion.Lerp(startPose.bones[startPoseIndex].rotation, endPose.bones[endPoseIndex].rotation, poseLerp);
                }
                else
                {
                    lerpedPos = Vector3.zero;
                    lerpedRot = Quaternion.identity;
                }
        
                UpdateBone(idpFirstBoneIndex + idpFingerBoneIndex, Space.Self, lerpedRot, lerpedPos);
            }
        }
    }
}
