using HPTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class InputModel : HPTKModel
    {
        public ProxyHandModel proxyHand;

        public InputDataProvider inputDataProvider;

        public bool updateBonesOnValidate = true;

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

        [Header("Master rig mapping")]
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

        [HideInInspector]
        public MasterBoneModel[] bonesToUpdate;

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
                    bone = proxyHand.master.thumb.bones[0] as MasterBoneModel;
                    if (bone)
                        thumb0 = bone;
                    bone = proxyHand.master.thumb.bones[1] as MasterBoneModel;
                    if (bone)
                        thumb1 = bone;
                    bone = proxyHand.master.thumb.bones[2] as MasterBoneModel;
                    if (bone)
                        thumb2 = bone;
                    bone = proxyHand.master.thumb.bones[3] as MasterBoneModel;
                    if (bone)
                        thumb3 = bone;
                }

                if (proxyHand.master.index)
                {
                    bone = proxyHand.master.index.bones[0] as MasterBoneModel;
                    if (bone)
                        index1 = bone;
                    bone = proxyHand.master.index.bones[1] as MasterBoneModel;
                    if (bone)
                        index2 = bone;
                    bone = proxyHand.master.index.bones[2] as MasterBoneModel;
                    if (bone)
                        index3 = bone;
                }

                if (proxyHand.master.middle)
                {
                    bone = proxyHand.master.middle.bones[0] as MasterBoneModel;
                    if (bone)
                        middle1 = bone;
                    bone = proxyHand.master.middle.bones[1] as MasterBoneModel;
                    if (bone)
                        middle2 = bone;
                    bone = proxyHand.master.middle.bones[2] as MasterBoneModel;
                    if (bone)
                        middle3 = bone;
                }

                if (proxyHand.master.ring)
                {
                    bone = proxyHand.master.ring.bones[0] as MasterBoneModel;
                    if (bone)
                        ring1 = bone;
                    bone = proxyHand.master.ring.bones[1] as MasterBoneModel;
                    if (bone)
                        ring2 = bone;
                    bone = proxyHand.master.ring.bones[2] as MasterBoneModel;
                    if (bone)
                        ring3 = bone;
                }

                if (proxyHand.master.pinky)
                {
                    bone = proxyHand.master.pinky.bones[0] as MasterBoneModel;
                    if (bone)
                        pinky0 = bone;
                    bone = proxyHand.master.pinky.bones[1] as MasterBoneModel;
                    if (bone)
                        pinky1 = bone;
                    bone = proxyHand.master.pinky.bones[2] as MasterBoneModel;
                    if (bone)
                        pinky2 = bone;
                    bone = proxyHand.master.pinky.bones[3] as MasterBoneModel;
                    if (bone)
                        pinky3 = bone;
                }
            }
            else
            {
                Debug.LogError("ProxyHand field or ProxyHand.master are NULL");
            }
        }
    }
}

