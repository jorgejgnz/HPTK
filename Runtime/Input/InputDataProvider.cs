using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
    public class InputDataProvider : MonoBehaviour
    {
        // InputDataProvider.bones will always be 24 items length
        protected int numOfBones = 24;

        [HideInInspector]
        public AbstractTsf[] bones;

        [HideInInspector]
        public AbstractTsf wrist;
        [HideInInspector]
        public AbstractTsf forearm;
        [HideInInspector]
        public FingerPose thumb = new FingerPose("Thumb");
        [HideInInspector]
        public FingerPose index = new FingerPose("Index");
        [HideInInspector]
        public FingerPose middle = new FingerPose("Middle");
        [HideInInspector]
        public FingerPose ring = new FingerPose("Ring");
        [HideInInspector]
        public FingerPose pinky = new FingerPose("Pinky");

        [Range(0.0f, 1.0f)]
        public float confidence = 0.0f;

        [Range(0.5f, 1.5f)]
        public float scale = 1.0f;

        [Header("Debug")]
        [TextArea]
        public string log;

        // Replaceable by inherited classes
        public virtual void InitData()
        {
            List<AbstractTsf> tmpBones = new List<AbstractTsf>();

            for (int b = 0; b < numOfBones; b++)
            {
                // Initialize .bones with empty AbstractTsfs
                tmpBones.Add(new AbstractTsf(b.ToString(), Space.World));
            }

            bones = tmpBones.ToArray();
        }

        // Replaceable by inherited classes
        public virtual void UpdateData() {
            log = "Base class UpdateData method!";
        }

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
         * 
         * 19 - thumbTip
         * 20 - indexTip
         * 21 - middleTip
         * 22 - ringTip
         * 23 - pinkyTip
         */

        public void UpdateBones()
        {
            List<AbstractTsf> tmpBones = new List<AbstractTsf>();

            tmpBones.Add(wrist);
            tmpBones.Add(forearm);

            tmpBones.AddRange(thumb.bones);
            tmpBones.AddRange(index.bones);
            tmpBones.AddRange(middle.bones);
            tmpBones.AddRange(ring.bones);
            tmpBones.AddRange(pinky.bones);

            tmpBones.Add(thumb.tip);
            tmpBones.Add(index.tip);
            tmpBones.Add(middle.tip);
            tmpBones.Add(ring.tip);
            tmpBones.Add(pinky.tip);

            bones = tmpBones.ToArray();
        }

        public void UpdateFingerPosesFromBones()
        {     
            List<AbstractTsf> bonesList = new List<AbstractTsf>(bones); // Enable .GetRange by turning the array into a list

            FromBonesToFingerPose(thumb, bonesList.GetRange(2, 4).ToArray(), bonesList[bonesList.Count - 5]);
            FromBonesToFingerPose(index, bonesList.GetRange(6, 3).ToArray(), bonesList[bonesList.Count - 4]);
            FromBonesToFingerPose(middle, bonesList.GetRange(9, 3).ToArray(), bonesList[bonesList.Count - 3]);
            FromBonesToFingerPose(ring, bonesList.GetRange(12, 3).ToArray(), bonesList[bonesList.Count - 2]);
            FromBonesToFingerPose(pinky, bonesList.GetRange(15, 4).ToArray(), bonesList[bonesList.Count - 1]);
        }

        void FromBonesToFingerPose(FingerPose finger, AbstractTsf[] bonesSubArray, AbstractTsf tip)
        {
            // We can overwrite the item's references. The splitted array will remain splitted in fingers
            finger.bones = bonesSubArray;
            finger.tip = tip;
        }

        public void UpdateBonesFromFingerPoses()
        {
            List<AbstractTsf> bonesList = new List<AbstractTsf>(bones); // Enable .GetRange by turning the array into a list

            FromFingerPoseToBones(thumb, bonesList.GetRange(2, 4).ToArray(), bonesList[bonesList.Count - 5]);
            FromFingerPoseToBones(index, bonesList.GetRange(6, 3).ToArray(), bonesList[bonesList.Count - 4]);
            FromFingerPoseToBones(middle, bonesList.GetRange(9, 3).ToArray(), bonesList[bonesList.Count - 3]);
            FromFingerPoseToBones(ring, bonesList.GetRange(12, 3).ToArray(), bonesList[bonesList.Count - 2]);
            FromFingerPoseToBones(pinky, bonesList.GetRange(15, 4).ToArray(), bonesList[bonesList.Count - 1]);
        }

        public void FromFingerPoseToBones(FingerPose finger, AbstractTsf[] bonesSubArray, AbstractTsf tip)
        {
            // We have to copy the content as we can't overwrite the item's references. It will be merged again from the splitted bones array to InputDataProvider.bones
            for (int b = 0; b < bonesSubArray.Length; b++)
            {
                AbstractTsf.Copy(finger.bones[b], bonesSubArray[b]);
            }

            // We can't overwite this reference as finger tips are also included in InputDataProviders.bones
            AbstractTsf.Copy(finger.tip, tip);
        }
    }
}
