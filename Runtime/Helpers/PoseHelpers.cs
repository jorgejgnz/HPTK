using HandPhysicsToolkit.Assets;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class PoseHelpers
    {
        public static void ApplyFingerPose(FingerPose pose, FingerModel finger, string toReprKey, bool pos, bool rot, bool scale, bool inverted, bool useRotLimits)
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
                    Quaternion localRot = boneRepr.transformRef.localRotation;

                    Quaternion poseLocalRot;
                    if (fingerBoneIndex == 0 && poseBoneIndex > 0) poseLocalRot = pose.bones[poseBoneIndex - 1].rotation * pose.bones[poseBoneIndex].rotation;
                    else poseLocalRot = pose.bones[poseBoneIndex].rotation;

                    // Apply mobility limits
                    if (useRotLimits)
                    {
                        if (boneRepr.localRotZ < boneRepr.maxLocalRotZ && boneRepr.localRotZ > boneRepr.minLocalRotZ)
                        {
                            boneRepr.localRotation = localRot;
                        }
                        else
                        {
                            boneRepr.localRotation = poseLocalRot;
                        }
                    }
                    else
                    {
                        boneRepr.localRotation = poseLocalRot;
                    }
                }
                if (scale)
                {
                    boneRepr.transformRef.localScale = pose.bones[poseBoneIndex].localScale;
                }
            }
        }

        public static void ApplyHandPose(HandPoseAsset handPose, HandModel handDestination, string destinationReprKey, bool pos, bool rot, bool scale, bool inverted, bool useRotLimits)
        {
            for (int f = 0; f < handDestination.fingers.Count; f++)
            {
                FingerModel finger = handDestination.fingers[f];
                FingerPose pose = handPose.fingers.Find(fng => fng.finger == finger.finger);

                if (pose != null) ApplyFingerPose(pose, finger, destinationReprKey, pos, rot, scale, inverted, useRotLimits);
            }
        }

        public static void ApplyHandPose(HandModel fromHand, string fromKey, HandModel toHand, string toKey, bool pos, bool rot, bool scale, bool inverted, bool useRotLimits)
        {
            ApplyFingerPose(new FingerPose(fromHand.thumb, fromKey), toHand.thumb, toKey, pos, rot, scale, inverted, useRotLimits);
            ApplyFingerPose(new FingerPose(fromHand.index, fromKey), toHand.index, toKey, pos, rot, scale, inverted, useRotLimits);
            ApplyFingerPose(new FingerPose(fromHand.middle, fromKey), toHand.middle, toKey, pos, rot, scale, inverted, useRotLimits);
            ApplyFingerPose(new FingerPose(fromHand.ring, fromKey), toHand.ring, toKey, pos, rot, scale, inverted, useRotLimits);
            ApplyFingerPose(new FingerPose(fromHand.pinky, fromKey), toHand.pinky, toKey, pos, rot, scale, inverted, useRotLimits);
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

        public static FingerPose SaveFinger(FingerModel fingerModel, string repr)
        {
            FingerPose fingerPose;

            if (!fingerModel)
            {
                fingerPose = new FingerPose();
                fingerPose.bones = new List<AbstractTsf>();

                return fingerPose;
            }

            List<AbstractTsf> bones = new List<AbstractTsf>();

            for (int b = 0; b < fingerModel.bonesFromRootToTip.Count; b++)
            {
                bones.Add(new AbstractTsf(
                    fingerModel.bonesFromRootToTip[b].reprs[repr].localPosition,
                    fingerModel.bonesFromRootToTip[b].reprs[repr].localRotation,
                    Space.Self,
                    fingerModel.bonesFromRootToTip[b].reprs[repr].transformRef.localScale,
                    fingerModel.bonesFromRootToTip[b].reprs[repr].transformRef.name));
            }

            fingerPose = new FingerPose(fingerModel.finger);
            fingerPose.bones = bones;

            return fingerPose;
        }

        public static void SaveHand(HandModel hand, string repr, HandPoseAsset overwriteThis, string alias)
        {
            overwriteThis.alias = alias;

            overwriteThis.wrist = new AbstractTsf(
                        hand.wrist.reprs[repr].transformRef.localPosition,
                        hand.wrist.reprs[repr].transformRef.localRotation,
                        Space.Self,
                        hand.wrist.reprs[repr].transformRef.localScale,
                        hand.wrist.reprs[repr].transformRef.name);

            List<FingerPose> fingers = new List<FingerPose>();

            overwriteThis.thumb = SaveFinger(hand.thumb, repr);
            fingers.Add(overwriteThis.thumb);

            overwriteThis.index = SaveFinger(hand.index, repr);
            fingers.Add(overwriteThis.index);

            overwriteThis.middle = SaveFinger(hand.middle, repr);
            fingers.Add(overwriteThis.middle);

            overwriteThis.ring = SaveFinger(hand.ring, repr);
            fingers.Add(overwriteThis.ring);

            overwriteThis.pinky = SaveFinger(hand.pinky, repr);
            fingers.Add(overwriteThis.pinky);

            overwriteThis.fingers = fingers;
        }

        public static void UpdateZRotLimits(HandModel hand, string repr, HandPoseAsset pose, HandMobilityAsset mobility)
        {
            if (!hand || !hand.wrist.reprs.ContainsKey(repr)) return;

            if (!pose || !mobility)
            {
                RemoveZRotLimits(hand, repr);
            }
            else
            {
                UpdateZRotLimits(hand.thumb, repr, pose.thumb, mobility.thumb);
                UpdateZRotLimits(hand.index, repr, pose.index, mobility.index);
                UpdateZRotLimits(hand.middle, repr, pose.middle, mobility.middle);
                UpdateZRotLimits(hand.ring, repr, pose.ring, mobility.ring);
                UpdateZRotLimits(hand.pinky, repr, pose.pinky, mobility.pinky);
            }
        }

        public static void UpdateZRotLimits(FingerModel finger, string repr, FingerPose pose, HandMobilityAsset.FingerMobility mobility)
        {
            if (!finger || !finger.last.reprs.ContainsKey(repr)) return;

            if (pose == null || pose.bones.Count == 0 || mobility == null)
            {
                RemoveZRotLimits(finger, repr);
            }
            else
            {
                BoneModel bone;
                float desiredLocalRotZ;
                for (int b = 0; b < finger.bonesFromRootToTip.Count; b++)
                {
                    if (pose.bones[b].space == Space.World)
                    {
                        Debug.LogError("Poses with rotations in world space are not supported!");
                        return;
                    }

                    bone = finger.bonesFromRootToTip[b];

                    HandMobilityAsset.MobilityLimit mobilityLimit;
                    if (bone == finger.last) mobilityLimit = mobility.distal;
                    else if (bone == finger.oneUnderLast) mobilityLimit = mobility.middle;
                    else if (bone == finger.twoUnderLast) mobilityLimit = mobility.proximal;
                    else mobilityLimit = mobility.others;

                    bool limitOver = mobilityLimit == HandMobilityAsset.MobilityLimit.Locked || mobilityLimit == HandMobilityAsset.MobilityLimit.CanBeLower;
                    bool limitUnder = mobilityLimit == HandMobilityAsset.MobilityLimit.Locked || mobilityLimit == HandMobilityAsset.MobilityLimit.CanBeHigher;

                    desiredLocalRotZ = MathHelpers.GetProcessedAngleZ(pose.bones[b].rotation);
                    UpdateZRotLimits(bone.reprs[repr], pose.bones[b].rotation, desiredLocalRotZ, limitOver, limitUnder);
                }
            }
        }

        public static void UpdateZRotLimits(ReprModel boneRepr, Quaternion idealLocalRot, float idealLocalRotZ, bool limitOver, bool limitUnder)
        {
            if (!limitOver && !limitUnder) boneRepr.desiredLocalRot = Quaternion.identity;
            else boneRepr.desiredLocalRot = idealLocalRot;

            if (limitOver) boneRepr.maxLocalRotZ = idealLocalRotZ;
            else boneRepr.maxLocalRotZ = AvatarModel.maxLocalRotZ;

            if (limitUnder) boneRepr.minLocalRotZ = idealLocalRotZ;
            else boneRepr.minLocalRotZ = AvatarModel.minLocalRotZ;
        }

        public static void RemoveZRotLimits(HandModel hand, string repr)
        {
            RemoveZRotLimits(hand.thumb, repr);
            RemoveZRotLimits(hand.index, repr);
            RemoveZRotLimits(hand.middle, repr);
            RemoveZRotLimits(hand.ring, repr);
            RemoveZRotLimits(hand.pinky, repr);
        }

        public static void RemoveZRotLimits(FingerModel finger, string repr)
        {
            foreach(BoneModel b in finger.bones)
            {
                if (!b.reprs.ContainsKey(repr)) continue;

                b.reprs[repr].minLocalRotZ = AvatarModel.minLocalRotZ;
                b.reprs[repr].maxLocalRotZ = AvatarModel.maxLocalRotZ;
                b.reprs[repr].desiredLocalRot = Quaternion.identity;
            }
        }

        public static Vector3 GetRelativePoint(HandModel hand, HumanFinger finger, string repr)
        {
            FingerModel fingerModel = null;
            switch (finger)
            {
                case HumanFinger.Thumb: fingerModel = hand.thumb; break;
                case HumanFinger.Index: fingerModel = hand.index; break;
                case HumanFinger.Middle: fingerModel = hand.middle; break;
                case HumanFinger.Ring: fingerModel = hand.ring; break;
                case HumanFinger.Pinky: fingerModel = hand.pinky; break;
            }

            if (fingerModel != null)
            {
                return hand.wrist.reprs[repr].transformRef.InverseTransformPoint(fingerModel.tip.reprs[repr].transformRef.position);
            }
            else
            {
                return Vector3.zero;
            }
        }

        public static void WritePointBasedGesture(HandModel hand, string repr, PointBasedGestureAsset asset)
        {
            asset.handSide = hand.side;
            asset.thumbTip = GetRelativePoint(hand, HumanFinger.Thumb, repr);
            asset.indexTip = GetRelativePoint(hand, HumanFinger.Index, repr);
            asset.middleTip = GetRelativePoint(hand, HumanFinger.Middle, repr);
            asset.ringTip = GetRelativePoint(hand, HumanFinger.Ring, repr);
            asset.pinkyTip = GetRelativePoint(hand, HumanFinger.Pinky, repr);
        }
    }
}
