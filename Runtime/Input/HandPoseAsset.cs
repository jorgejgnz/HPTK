using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Input
{
    [CreateAssetMenu(menuName = "HPTK/HandPose", order = 2)]
    public class HandPoseAsset : ScriptableObject
    {
        public string alias;

        public AbstractTsf wrist;

        public FingerPose thumb = new FingerPose();
        public FingerPose index = new FingerPose();
        public FingerPose middle = new FingerPose();
        public FingerPose ring = new FingerPose();
        public FingerPose pinky = new FingerPose();

        public List<FingerPose> fingers = new List<FingerPose>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int f = 0; f < fingers.Count; f++)
            {
                if (fingers[f].finger == HumanFinger.Thumb) thumb = fingers[f];
                else if (fingers[f].finger == HumanFinger.Index) index = fingers[f];
                else if (fingers[f].finger == HumanFinger.Middle) middle = fingers[f];
                else if (fingers[f].finger == HumanFinger.Ring) ring = fingers[f];
                else if (fingers[f].finger == HumanFinger.Pinky) pinky = fingers[f];
            }
        }
#endif
    }

    [Serializable]
    public class FingerPose
    {
        public HumanFinger finger;

        [Header("From root to tip")]
        public List<AbstractTsf> bones = new List<AbstractTsf>();

        public FingerPose() { finger = HumanFinger.None; }

        public FingerPose(HumanFinger finger) { this.finger = finger; }

        public FingerPose(FingerModel finger, string key)
        {
            this.finger = finger.finger;

            for (int b = 0; b < finger.bonesFromRootToTip.Count; b++)
            {
                if (!finger.bonesFromRootToTip[b].reprs.ContainsKey(key))
                {
                    Debug.LogError("Finger " + finger.name + " does not have a " + key + " representation of bone " + finger.bonesFromRootToTip[b].name + ". FingerPose cannot be created completely");
                    break;
                }

                bones.Add(new AbstractTsf(finger.bonesFromRootToTip[b].reprs[key].transformRef, Space.Self));
            }
        }
    }
}