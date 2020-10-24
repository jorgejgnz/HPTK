using HPTK.Views.Notifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Models.Avatar
{
    public class FingerModel : HPTKModel
    {
        [HideInInspector]
        public HandModel hand;

        public BoneModel[] bones;

        public Transform fingerBase;
        public Transform fingerTip;

        public BoneModel distal;

        public CollisionNotifier fingerTipCollisionNotifier;
        public LineRenderer linerenderer;

        [HideInInspector]
        public float length;

        public float flexLerp;
        public float strengthLerp;

        public float baseRotationLerp;
        public bool isClosed;

        public float pinchLerp;
        public float pinchSpeed;
        public bool isPinching;

        public float palmLineLerp;

        private void Awake()
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].finger = this;
            }
        }
    }

 
}
