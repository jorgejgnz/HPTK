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

        [Header("Models")]
        public BoneModel[] bones;

        [Header("Refs")]
        public Transform fingerBase;
        public Transform fingerTip;

        public BoneModel distal;

        public CollisionNotifier fingerTipCollisionNotifier;
        public LineRenderer lineRenderer;

        [Header("Updated by Controller")]

        public float flexLerp;
        public float strengthLerp;

        [HideInInspector]
        public float length;

        public float baseRotationLerp;
        public bool isClosed;

        public float pinchLerp;
        public float pinchSpeed;
        public bool isPinching;
        public float timePinching;
        public float pinchIntentionTime;
        [Range(0.0f,1.0f)]
        public float pinchIntentionLerp;
        public bool isIntentionallyPinching;

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
