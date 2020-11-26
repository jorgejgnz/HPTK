using HPTK.Models.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HPTK.Views.Handlers.ProxyHandHandler;

namespace HPTK.Input
{
    [Serializable]
    public class AbstractTsf
    {
        public string name;

        public Vector3 position;
        public Quaternion rotation;
        public Space space;

        public Vector3 localScale;

        public AbstractTsf(Vector3 position, Quaternion rotation, Space space, Vector3 localScale, string name)
        {
            this.position = position;
            this.rotation = rotation;
            this.space = space;

            this.localScale = localScale;
            this.name = name;
        }

        public AbstractTsf(string name, Space space)
        {
            this.space = space;
            this.name = name;
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

        public AbstractTsf(Transform tsf, Space space)
        {
            this.name = tsf.name;
            this.space = space;

            if (space == Space.World)
            {
                this.position = tsf.position;
                this.rotation = tsf.rotation;
            }
            else
            {
                this.position = tsf.localPosition;
                this.rotation = tsf.localRotation;
            }

            this.localScale = tsf.localScale;
        }

        public AbstractTsf(AbstractTsf abstractTsf)
        {
            this.name = abstractTsf.name;
            this.space = abstractTsf.space;
            this.position = abstractTsf.position;
            this.rotation = abstractTsf.rotation;
        }

        public static void ApplyTransform(AbstractTsf bonePose, Transform receiverTsf)
        {
            if (bonePose.space == Space.World)
            {
                receiverTsf.position = bonePose.position;
                receiverTsf.rotation = bonePose.rotation;
            }
            else
            {
                receiverTsf.localPosition = bonePose.position;
                receiverTsf.localRotation = bonePose.rotation;
            }

            receiverTsf.localScale = bonePose.localScale;
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

    public class InputDataProvider : MonoBehaviour
    {
        // InputDataProvider.bones will always be 24 items length
        protected int numOfBones = 24;

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
        public virtual void UpdateData() { }

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

        public void UpdateFingers()
        {
            List<AbstractTsf> tmpBones = new List<AbstractTsf>(bones);

            UpdateFinger(thumb, tmpBones.GetRange(2, 4).ToArray(), tmpBones[tmpBones.Count - 5]);
            UpdateFinger(index, tmpBones.GetRange(6, 3).ToArray(), tmpBones[tmpBones.Count - 4]);
            UpdateFinger(middle, tmpBones.GetRange(9, 3).ToArray(), tmpBones[tmpBones.Count - 3]);
            UpdateFinger(ring, tmpBones.GetRange(12, 3).ToArray(), tmpBones[tmpBones.Count - 2]);
            UpdateFinger(pinky, tmpBones.GetRange(15, 4).ToArray(), tmpBones[tmpBones.Count - 1]);
        }

        void UpdateFinger(FingerPose finger, AbstractTsf[] bones, AbstractTsf tip)
        {
            finger.bones = bones;
            finger.tip = tip;
        }
    }
}
