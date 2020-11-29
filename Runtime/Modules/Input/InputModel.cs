using HPTK.Input;
using HPTK.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class InputModel : HPTKModel
    {
        public ProxyHandModel proxyHand;

        public InputConfiguration configuration;

        public InputDataProvider inputDataProvider;

        public bool isActive = true;

        [HideInInspector]
        public MasterBoneModel[] bonesToUpdate;

        [HideInInspector]
        public float highestLinearSpeed = 1000.0f;
        [HideInInspector]
        public float highestAngularSpeed = 1000.0f;

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

        [Header("Master rig mapping")]
        public bool updateBonesOnValidate = true;
        public MasterBoneModel wrist;
        public MasterBoneModel forearm;
        public MasterBoneModel thumb0;
        public MasterBoneModel thumb1;
        public MasterBoneModel thumb2;
        public MasterBoneModel thumb3;
        public MasterBoneModel index1;
        public MasterBoneModel index2;
        public MasterBoneModel index3;
        public MasterBoneModel middle1;
        public MasterBoneModel middle2;
        public MasterBoneModel middle3;
        public MasterBoneModel ring1;
        public MasterBoneModel ring2;
        public MasterBoneModel ring3;
        public MasterBoneModel pinky0;
        public MasterBoneModel pinky1;
        public MasterBoneModel pinky2;
        public MasterBoneModel pinky3;

        [Header("Updated by Controller")]
        public bool handIsTracked = false;
        public bool fingersAreTracked = false;
        public bool isPredicting = false;

        // Noise reduction
        public float[] wmaWeights; // Assuming that window size won't change
        public AbstractTsf[][] boneRecords; // Assuming that bonesToUpdate and window size won't change. [boneToUpdate][record]

        public void OnValidate()
        {
            if (updateBonesOnValidate)
                MasterRigMapping();
        }

        void MasterRigMapping ()
        {
            if (proxyHand != null && proxyHand.master != null)
            {
                if (proxyHand.master.wrist) wrist = proxyHand.master.wrist as MasterBoneModel;
                if (proxyHand.master.forearm) forearm = proxyHand.master.forearm as MasterBoneModel;

                MasterBoneModel bone;

                if (proxyHand.master.thumb)
                {
                    for (int b = 0; b < proxyHand.master.thumb.bones.Length; b++)
                    {
                        bone = proxyHand.master.thumb.bones[b] as MasterBoneModel;
                        if (bone != null)
                        {
                            switch (b)
                            {
                                case 0:
                                    thumb0 = bone;
                                    break;
                                case 1:
                                    thumb1 = bone;
                                    break;
                                case 2:
                                    thumb2 = bone;
                                    break;
                                case 3:
                                    thumb3 = bone;
                                    break;
                            }
                        }
                    }
                }

                if (proxyHand.master.index)
                {
                    for (int b = 0; b < proxyHand.master.index.bones.Length; b++)
                    {
                        bone = proxyHand.master.index.bones[b] as MasterBoneModel;
                        if (bone != null)
                        {
                            switch (b)
                            {
                                case 0:
                                    index1 = bone;
                                    break;
                                case 1:
                                    index2 = bone;
                                    break;
                                case 2:
                                    index3 = bone;
                                    break;
                            }
                        }
                    }
                }

                if (proxyHand.master.middle)
                {
                    for (int b = 0; b < proxyHand.master.middle.bones.Length; b++)
                    {
                        bone = proxyHand.master.middle.bones[b] as MasterBoneModel;
                        if (bone != null)
                        {
                            switch (b)
                            {
                                case 0:
                                    middle1 = bone;
                                    break;
                                case 1:
                                    middle2 = bone;
                                    break;
                                case 2:
                                    middle3 = bone;
                                    break;
                            }
                        }
                    }
                }

                if (proxyHand.master.ring)
                {
                    for (int b = 0; b < proxyHand.master.ring.bones.Length; b++)
                    {
                        bone = proxyHand.master.ring.bones[b] as MasterBoneModel;
                        if (bone != null)
                        {
                            switch (b)
                            {
                                case 0:
                                    ring1 = bone;
                                    break;
                                case 1:
                                    ring2 = bone;
                                    break;
                                case 2:
                                    ring3 = bone;
                                    break;
                            }
                        }
                    }
                }

                if (proxyHand.master.pinky)
                {
                    for (int b = 0; b < proxyHand.master.pinky.bones.Length; b++)
                    {
                        bone = proxyHand.master.pinky.bones[b] as MasterBoneModel;
                        if (bone != null)
                        {
                            switch (b)
                            {
                                case 0:
                                    pinky0 = bone;
                                    break;
                                case 1:
                                    pinky1 = bone;
                                    break;
                                case 2:
                                    pinky2 = bone;
                                    break;
                                case 3:
                                    pinky3 = bone;
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("ProxyHand field or ProxyHand.master are NULL");
            }
        }
    }
}

