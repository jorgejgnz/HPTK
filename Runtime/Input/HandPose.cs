using HPTK.Models.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
    [CreateAssetMenu(menuName = "HPTK/HandPose Asset", order = 2)]
    public class HandPose : ScriptableObject
    {
        public string alias;
        public Input.FingerPose[] fingers;
        public AbstractTsf wrist;
        public AbstractTsf forearm;

        public AbstractTsf[] GetBones()
        {
            List<AbstractTsf> bones = new List<AbstractTsf>();

            bones.Add(wrist);
            bones.Add(forearm);

            for (int i = 0; i < fingers.Length; i++)
            {
                bones.AddRange(fingers[i].bones);
            }

            return bones.ToArray();
        }

        public void ApplyPose(HandModel hand, bool pos, bool rot, bool scale, bool inverted)
        {
            // Wrist won't be applied
            for (int f = 0; f < Mathf.Min(hand.fingers.Length, fingers.Length); f++)
            {
                for (int b = 0; b < Mathf.Min(hand.fingers[f].bones.Length, fingers[f].bones.Length); b++)
                {
                    if (fingers[f].bones[b].space == Space.World)
                        Debug.LogWarning("poseBones[" + f + "] is not configured in local space!");

                    if (pos) hand.fingers[f].bones[b].transformRef.localPosition = inverted ? fingers[f].bones[b].position * -1.0f : fingers[f].bones[b].position;
                    if (rot) hand.fingers[f].bones[b].transformRef.localRotation = fingers[f].bones[b].rotation;
                    if (scale) hand.fingers[f].bones[b].transformRef.localScale = fingers[f].bones[b].localScale;
                }
            }
        }
    }

    [Serializable]
    public class FingerPose
    {
        public string name;
        public AbstractTsf[] bones;
        public AbstractTsf tip;

        public FingerPose(string name)
        {
            this.name = name;
            this.bones = new AbstractTsf[0];
        }
    }
}