using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Input
{
    public class InputDataProvider : MonoBehaviour
    {
        public Side side = Side.Left;

        // InputDataProvider.bones will always be 24 items length
        protected int numOfBones = 19;

        [HideInInspector]
        public AbstractTsf[] bones;

        [HideInInspector]
        public AbstractTsf wrist;
        [HideInInspector]
        public AbstractTsf forearm;
        [HideInInspector]
        public FingerPose thumb = new FingerPose(HumanFinger.Thumb);
        [HideInInspector]
        public FingerPose index = new FingerPose(HumanFinger.Index);
        [HideInInspector]
        public FingerPose middle = new FingerPose(HumanFinger.Middle);
        [HideInInspector]
        public FingerPose ring = new FingerPose(HumanFinger.Ring);
        [HideInInspector]
        public FingerPose pinky = new FingerPose(HumanFinger.Pinky);

        [Header("Input Data Provider")]

        [Range(0.0f, 1.0f)]
        public float confidence = 0.0f;

        [Range(0.5f, 1.5f)]
        public float scale = 1.0f;
  
        [Header("Debug")]
        public string log;

        List<AbstractTsf> tmpBones = new List<AbstractTsf>();
        List<AbstractTsf> bonesList = new List<AbstractTsf>();

        // Replaceable by inherited classes
        public virtual void InitData()
        {
            tmpBones.Clear();

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
         */

        public void UpdateBones()
        {
            tmpBones.Clear();

            tmpBones.Add(wrist);
            tmpBones.Add(forearm);

            tmpBones.AddRange(thumb.bones);
            tmpBones.AddRange(index.bones);
            tmpBones.AddRange(middle.bones);
            tmpBones.AddRange(ring.bones);
            tmpBones.AddRange(pinky.bones);

            bones = tmpBones.ToArray();
        }

        public void UpdateFingerPosesFromBones()
        {     
            bones.ToList(bonesList); // Enable .GetRange by turning the array into a list

            FromBonesToFingerPose(thumb, bonesList.GetRange(2, 4));
            FromBonesToFingerPose(index, bonesList.GetRange(6, 3));
            FromBonesToFingerPose(middle, bonesList.GetRange(9, 3));
            FromBonesToFingerPose(ring, bonesList.GetRange(12, 3));
            FromBonesToFingerPose(pinky, bonesList.GetRange(15, 4));
        }

        void FromBonesToFingerPose(FingerPose finger, List<AbstractTsf> bonesList)
        {
            // We can overwrite the item's references. The splitted array will remain splitted in fingers
            finger.bones = bonesList;
        }

        public void UpdateBonesFromFingerPoses()
        {
            bones.ToList(bonesList); // Enable .GetRange by turning the array into a list

            FromFingerPoseToBones(thumb, bonesList.GetRange(2, 4));
            FromFingerPoseToBones(index, bonesList.GetRange(6, 3));
            FromFingerPoseToBones(middle, bonesList.GetRange(9, 3));
            FromFingerPoseToBones(ring, bonesList.GetRange(12, 3));
            FromFingerPoseToBones(pinky, bonesList.GetRange(15, 4));
        }

        public void FromFingerPoseToBones(FingerPose finger, List<AbstractTsf> bonesList)
        {
            // We have to copy the content as we can't overwrite the item's references. It will be merged again from the splitted bones array to InputDataProvider.bones
            for (int b = 0; b < bonesList.Count; b++)
            {
                if (b < finger.bones.Count) AbstractTsf.Copy(finger.bones[b], bonesList[b]);
                else bonesList[b] = new AbstractTsf("Missing bone pose", Space.Self);
            }
        }
    }
}
