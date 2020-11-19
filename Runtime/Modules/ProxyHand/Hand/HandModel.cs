using HPTK.Helpers;
using HPTK.Models.Interaction;
using HPTK.Views.Handlers;
using HPTK.Views.Notifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HPTK.Views.Handlers.ProxyHandHandler;

namespace HPTK.Models.Avatar
{
    public class HandModel : HPTKModel
    {
        [HideInInspector]
        public ProxyHandModel proxyHand;

        [HideInInspector]
        public HandViewModel viewModel;

        [Header("Models")]
        public FingerModel thumb;
        public FingerModel index;
        public FingerModel middle;
        public FingerModel ring;
        public FingerModel pinky;

        [HideInInspector]
        public FingerModel[] fingers;

        public BoneModel wrist;
        public BoneModel forearm;

        [Header("Transforms")]
        public Transform pinchCenter;
        public Transform throatCenter;
        public Transform palmCenter;
        public Transform palmNormal;
        public Transform palmExterior;
        public Transform palmInterior;

        public Transform ray;

        [Header("Components")]
        public SkinnedMeshRenderer skinnedMR;

        [Header("Updated by Controller")]
        public float fistLerp;
        public bool isFist;

        public float graspLerp;
        public float graspSpeed;
        public bool isGrasping;
        public float timeGrasping;
        public float graspIntentionTime;
        [Range(0.0f,1.0f)]
        public float graspIntentionLerp;
        public bool isIntentionallyGrasping;

        [HideInInspector]
        public Transform[] allTransforms;

        [HideInInspector]
        public BoneModel[] bones;

        protected void Awake()
        {
            // Fingers

            if (fingers == null || fingers.Length == 0)
            {
                AvatarHelpers.HandModelInit(this);
            }

            // Bones

            bones = AvatarHelpers.GetHandBones(this);

            // Transforms (depends on .bones)

            allTransforms = AvatarHelpers.GetAllTransforms(this);
            
        }
    }
}
