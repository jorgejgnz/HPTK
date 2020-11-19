using HPTK.Models.Avatar;
using HPTK.Views.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HPTK.Helpers
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
        Pinky
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

    public static class AvatarHelpers
    {
        public static void HandModelInit(HandModel hand)
        {
            List<FingerModel> fingerList = new List<FingerModel>();

            if (hand.thumb) fingerList.Add(hand.thumb);
            if (hand.index) fingerList.Add(hand.index);
            if (hand.middle) fingerList.Add(hand.middle);
            if (hand.ring) fingerList.Add(hand.ring);
            if (hand.pinky) fingerList.Add(hand.pinky);

            hand.fingers = fingerList.ToArray();

            for (int i = 0; i < hand.fingers.Length; i++)
            {
                hand.fingers[i].hand = hand;
            }
        }

        public static BoneModel[] GetHandBones(HandModel hand)
        {
            // Same order as OVRSkeleton.Bones
            List<BoneModel> handBones = new List<BoneModel>();

            // Wrist
            handBones.Add(hand.wrist);

            // Forearm
            if (hand.forearm)
                handBones.Add(hand.forearm);

            // Finger bones
            for (int i = 0; i < hand.fingers.Length; i++)
            {
                handBones.AddRange(hand.fingers[i].bones);
            }

            return handBones.ToArray();
        }

        public static SlaveBoneModel[] GetSlaveHandBones(HandModel hand)
        {
            List<SlaveBoneModel> slaveBones = new List<SlaveBoneModel>();

            for (int i = 0; i < hand.bones.Length; i++)
            {
                if (hand.bones[i] is SlaveBoneModel)
                    slaveBones.Add(hand.bones[i] as SlaveBoneModel);
            }

            return slaveBones.ToArray();
        }

        public static Transform[] GetAllTransforms(HandModel hand)
        {
            List<Transform> handTransforms = new List<Transform>();

            // Wrist, Forearm and Finger bones
            for (int i = 0; i < hand.bones.Length; i++)
            {
                handTransforms.Add(hand.bones[i].transformRef);
            }

            // Figner tips
            for (int i = 0; i < hand.fingers.Length; i++)
            {
                handTransforms.Add(hand.fingers[i].fingerTip);
            }

            return handTransforms.ToArray();
        }

        public static Transform[] GetFingerTransforms(FingerModel finger)
        {
            Transform[] boneTransforms = new Transform[finger.bones.Length];

            for (int i = 0; i < boneTransforms.Length; i++)
            {
                boneTransforms[i] = finger.bones[i].transformRef;
            }

            return boneTransforms;
        }

        public static int GetBonesCount(HandModel hand)
        {
            int n = 2; // wrist + forearm

            for (int i = 0; i < hand.fingers.Length; i++)
            {
                n += hand.fingers[i].bones.Length;
            }

            return n;
        }

        public static void UpdateFingerLengths(HandModel hand, float scale)
        {
            for (int i = 0; i < hand.fingers.Length; i++)
            {
                hand.fingers[i].length = GetFingerLength(hand.fingers[i], scale);
            }
        }

        public static float GetFingerLength(FingerModel finger, float scale)
        {
            float length = 0.0f;

            bool ignore = true;
            for (int i = 0; i < finger.bones.Length; i++)
            {
                if (finger.bones[i].transformRef == finger.fingerBase)
                    ignore = false;

                if (!ignore)
                {
                    if (i != finger.bones.Length - 1)
                        length += Vector3.Distance(finger.bones[i].transformRef.position,finger.bones[i + 1].transformRef.position);
                    else
                        length += Vector3.Distance(finger.bones[i].transformRef.position, finger.fingerTip.position);
                }
            }

            return length / scale;
        }

        public static float GetFingerFlexion(FingerModel finger, float minFlexRelDistance, float scale)
        {
            float distance = Vector3.Distance(finger.fingerBase.position,finger.fingerTip.position);

            return 1.0f - Mathf.InverseLerp(minFlexRelDistance * scale, finger.length * scale, distance);
        }

        // If bone1 -> Finger rotation. If bone 2 -> Finger strength
        public static float GetBoneRotLerp(Transform boneTsfRef, float maxLocalRotZ, float minLocalRotZ)
        {
            float localRotZ = boneTsfRef.localRotation.eulerAngles.z;

            if (localRotZ <= maxLocalRotZ && localRotZ >= minLocalRotZ)
                return Mathf.InverseLerp(maxLocalRotZ, minLocalRotZ, localRotZ);
            else if (localRotZ < 0.0f || localRotZ > maxLocalRotZ || (localRotZ >= 0.0f && localRotZ < 180.0f))
                return 0.0f;
            else
                return 1.0f;
        }

        public static float GetHandFist(HandModel hand)
        {
            float sum = 0.0f;

            for (int i = 0; i < hand.fingers.Length; i++)
            {
                if (hand.fingers[i] != hand.thumb)
                    sum += hand.fingers[i].palmLineLerp; 
            }

            return sum / (hand.fingers.Length - 1);
        }

        public static float GetHandGrasp(HandModel hand)
        {
            float sum = 0.0f;

            for (int i = 0; i < hand.fingers.Length; i++)
            {
                if (hand.fingers[i] != hand.thumb)
                    sum += hand.fingers[i].baseRotationLerp;
            }

            return sum / (hand.fingers.Length - 1);
        }

        public static float GetFingerPinch(FingerModel finger, float maxRelDistance, float minRelDistance, float scale)
        {
            if (finger == finger.hand.thumb)
            {
                return new List<FingerModel>(finger.hand.fingers).FindAll(x => x != finger.hand.thumb).Max(x => x.pinchLerp);
            }
            else
            {
                float minAbsDistance = minRelDistance * scale;
                float maxAbsDistance = maxRelDistance * scale;

                float distance = Vector3.Distance(finger.fingerTip.position, finger.hand.thumb.fingerTip.position);

                return 1.0f - Mathf.InverseLerp(minAbsDistance, maxAbsDistance, distance);
            }
          
        }

        public static float GetPalmLineLerp(FingerModel finger, float maxRelDistance, float minRelDistance, float scale)
        {
            float maxAbsDistance = maxRelDistance * scale;
            float minAbsDistance = minRelDistance * scale;

            Vector3 nearestPointToLine = NearestPointOnFiniteLine(finger.hand.palmExterior.position, finger.hand.palmInterior.position, finger.fingerTip.position);
            float distance = Vector3.Distance(nearestPointToLine, finger.fingerTip.position);

            return 1.0f - Mathf.InverseLerp(minAbsDistance, maxAbsDistance, distance);
        }

        public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
        {
            Vector3 line = (end - start);
            float len = line.magnitude;
            line.Normalize();

            Vector3 v = pnt - start;
            float d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return start + line * d;
        }

        public static Vector3 GetHandRayDirection(HandModel hand, Transform shoulderTip)
        {
            return (hand.ray.position - shoulderTip.position).normalized;
        }

        public static void CopyValues(HandModel from, HandModel to)
        {
            to.thumb = from.thumb;
            to.index = from.index;
            to.middle = from.middle;
            to.ring = from.ring;
            to.pinky = from.pinky;
            to.wrist = from.wrist;
            to.forearm = from.forearm;

            to.pinchCenter = from.pinchCenter;
            to.throatCenter = from.throatCenter;
            to.palmCenter = from.palmCenter;
            to.palmExterior = from.palmExterior;
            to.palmInterior = from.palmInterior;
            to.ray = from.ray;

            to.skinnedMR = from.skinnedMR;
        }

        public static InteractorHandler GetFirstInteractor(ProxyHandModel proxyHand)
        {
            for (int i = 0; i < proxyHand.relatedHandlers.Count; i++)
            {
                if (proxyHand.relatedHandlers[i] is InteractorHandler)
                {
                    // Return the first Interactor found
                    return proxyHand.relatedHandlers[i] as InteractorHandler;
                }
            }

            return null;
        }
    }
}
