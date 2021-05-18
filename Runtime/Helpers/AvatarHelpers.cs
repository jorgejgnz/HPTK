using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public enum Side
    {
        None,
        Left,
        Right,
        Both
    }

    public enum HumanFinger
    {
        None,
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky,
        All
    }

    public enum HumanFingerBone
    {
        None = 0,
        Metacarpal = 1,
        Proximal = 2,
        Intermediate = 3,
        Distal = 4,
    }

    public enum HandGesture
    {
        None,
        IndexPinch,
        FullPinch,
        PrecisionGrip,
        PowerGrip,
        Custom
    }

    public enum HandRepresentation
    {
        None = 0,
        Master = 1,
        Slave = 2,
        Ghost = 3,
        Other = 4
    }

    public enum HumanBodyPart
    {
        None,
        Head,
        Eye,
        Jaw,
        Neck,
        Spine,
        Chest,
        Shoulder,
        Arm,
        Hand,
        Finger,
        Hips,
        Leg,
        Foot,
        Toe,
        All
    }

    public static class AvatarHelpers
    {
        public static void GetBonesFromRootToTip(FingerModel finger, List<BoneModel> _orderedBones)
        {
            _orderedBones.Clear();

            BoneModel bone = finger.tip.bone;

            for (int i = 0; i < 50; i++)
            {
                _orderedBones.Add(bone);

                if (!bone.parent || bone == finger.root) break;
                else bone = bone.parent;
            }

            _orderedBones.Reverse();
        }

        public static HumanBodyBones GetHumanFingerBone(Side s, HumanFinger finger, int distanceFromDitalBone)
        {
            switch (s)
            {
                case Side.Left:
                    switch (finger)
                    {
                        case HumanFinger.Thumb:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.LeftThumbDistal;
                                case 1:
                                    return HumanBodyBones.LeftThumbIntermediate;
                                case 2:
                                    return HumanBodyBones.LeftThumbProximal;
                            }
                            break;
                        case HumanFinger.Index:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.LeftIndexDistal;
                                case 1:
                                    return HumanBodyBones.LeftIndexIntermediate;
                                case 2:
                                    return HumanBodyBones.LeftIndexProximal;
                            }
                            break;
                        case HumanFinger.Middle:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.LeftMiddleDistal;
                                case 1:
                                    return HumanBodyBones.LeftMiddleIntermediate;
                                case 2:
                                    return HumanBodyBones.LeftMiddleProximal;
                            }
                            break;
                        case HumanFinger.Ring:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.LeftRingDistal;
                                case 1:
                                    return HumanBodyBones.LeftRingIntermediate;
                                case 2:
                                    return HumanBodyBones.LeftRingProximal;
                            }
                            break;
                        case HumanFinger.Pinky:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.LeftLittleDistal;
                                case 1:
                                    return HumanBodyBones.LeftLittleIntermediate;
                                case 2:
                                    return HumanBodyBones.LeftLittleProximal;
                            }
                            break;
                    }
                    break;
                case Side.Right:
                    switch (finger)
                    {
                        case HumanFinger.Thumb:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.RightThumbDistal;
                                case 1:
                                    return HumanBodyBones.RightThumbIntermediate;
                                case 2:
                                    return HumanBodyBones.RightThumbProximal;
                            }
                            break;
                        case HumanFinger.Index:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.RightIndexDistal;
                                case 1:
                                    return HumanBodyBones.RightIndexIntermediate;
                                case 2:
                                    return HumanBodyBones.RightIndexProximal;
                            }
                            break;
                        case HumanFinger.Middle:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.RightMiddleDistal;
                                case 1:
                                    return HumanBodyBones.RightMiddleIntermediate;
                                case 2:
                                    return HumanBodyBones.RightMiddleProximal;
                            }
                            break;
                        case HumanFinger.Ring:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.RightRingDistal;
                                case 1:
                                    return HumanBodyBones.RightRingIntermediate;
                                case 2:
                                    return HumanBodyBones.RightRingProximal;
                            }
                            break;
                        case HumanFinger.Pinky:
                            switch (distanceFromDitalBone)
                            {
                                case 0:
                                    return HumanBodyBones.RightLittleDistal;
                                case 1:
                                    return HumanBodyBones.RightLittleIntermediate;
                                case 2:
                                    return HumanBodyBones.RightLittleProximal;
                            }
                            break;
                    }
                    break;
            }

            return HumanBodyBones.LastBone;
        }
    }
}
