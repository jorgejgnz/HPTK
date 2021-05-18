using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class PoseHelpers
    {
        public static void ApplyFingerPose(FingerPose pose, FingerModel finger, string toReprKey, bool pos, bool rot, bool scale, bool inverted)
        {
            if (!finger.tip || !finger.tip.bone)
            {
                Debug.LogWarning("It was not possible to find distal bone for finger " + finger.name + ". Finger pose cannot be applied");
                return;
            }

            if (pose == null || !finger)
            {
                Debug.LogWarning("Missing finger pose or finger destination. Finger pose cannot be applied");
                return;
            }
            
            for (int pb = 1; pb <= pose.bones.Count; pb++)
            {
                // From tip to root (full length)
                int poseBoneIndex = pose.bones.Count - pb;
                int fingerBoneIndex = finger.bonesFromRootToTip.Count - pb;

                if (poseBoneIndex < 0) break;
                if (fingerBoneIndex < 0) break;

                if (pose.bones[poseBoneIndex].space != Space.Self)
                {
                    Debug.LogError("Pose for finger " + pose.finger.ToString() + " and bone " + pose.bones[poseBoneIndex].name + " does not have its AbstractTsf configured in local space. FingerPose cannot be applied completely");
                    continue;
                }

                ReprModel boneRepr = finger.bonesFromRootToTip[fingerBoneIndex].reprs[toReprKey];

                if (pos)
                {
                    boneRepr.localPosition = pose.bones[poseBoneIndex].position;
                    if (inverted) boneRepr.localPosition *= -1.0f;
                }
                if (rot)
                {
                    Quaternion localRot;
                    if (fingerBoneIndex == 0 && poseBoneIndex > 0) localRot = pose.bones[poseBoneIndex - 1].rotation * pose.bones[poseBoneIndex].rotation;
                    else localRot = pose.bones[poseBoneIndex].rotation;

                    boneRepr.localRotation = localRot;
                }
                if (scale)
                {
                    boneRepr.transformRef.localScale = pose.bones[poseBoneIndex].localScale;
                }
            }
        }

        public static void ApplyHandPose(HandPoseAsset handPose, HandModel handDestination, string destinationReprKey, bool pos, bool rot, bool scale, bool inverted)
        {
            for (int f = 0; f < handDestination.fingers.Count; f++)
            {
                FingerModel finger = handDestination.fingers[f];
                FingerPose pose = handPose.fingers.Find(fng => fng.finger == finger.finger);

                if (pose != null) ApplyFingerPose(pose, finger, destinationReprKey, pos, rot, scale, inverted);
            }
        }

        public static void ApplyHandPose(HandModel fromHand, string fromKey, HandModel toHand, string toKey, bool pos, bool rot, bool scale, bool inverted)
        {
            ApplyFingerPose(new FingerPose(fromHand.thumb, fromKey), toHand.thumb, toKey, pos, rot, scale, inverted);
            ApplyFingerPose(new FingerPose(fromHand.index, fromKey), toHand.index, toKey, pos, rot, scale, inverted);
            ApplyFingerPose(new FingerPose(fromHand.middle, fromKey), toHand.middle, toKey, pos, rot, scale, inverted);
            ApplyFingerPose(new FingerPose(fromHand.ring, fromKey), toHand.ring, toKey, pos, rot, scale, inverted);
            ApplyFingerPose(new FingerPose(fromHand.pinky, fromKey), toHand.pinky, toKey, pos, rot, scale, inverted);
        }

        public static FingerPose LerpFingerPose(FingerPose start, FingerPose end, float lerp)
        {
            if (start.bones.Count != end.bones.Count)
            {
                Debug.LogError("Numer of bones is not the same for the given finger poses. Pose cannot be lerped for this finger");
                return null;
            }

            FingerPose lerpFingerPose = new FingerPose(start.finger);

            for (int b = 0; b < start.bones.Count; b++)
            {
                AbstractTsf lerpBonePose;

                if (start.bones[b].space == end.bones[b].space)
                {
                    lerpBonePose = new AbstractTsf(start.bones[b].name, start.bones[b].space);
                    lerpBonePose.position = Vector3.Lerp(start.bones[b].position, end.bones[b].position, lerp);
                    lerpBonePose.rotation = Quaternion.Lerp(start.bones[b].rotation, end.bones[b].rotation, lerp);
                }
                else
                {
                    Debug.LogWarning("Space is not the same in start and end poses for bone " + b + ". Using local space and identity rotation for this bone");
                    lerpBonePose = new AbstractTsf(start.bones[b].name, Space.Self);
                }

                lerpFingerPose.bones.Add(lerpBonePose);
            }

            return lerpFingerPose;
        }
    }
}
