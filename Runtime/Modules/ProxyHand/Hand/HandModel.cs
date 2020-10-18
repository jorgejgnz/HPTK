using HPTK.Helpers;
using HPTK.Models.Interaction;
using HPTK.Views.Handlers;
using HPTK.Views.Notifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Models.Avatar;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HPTK.Models.Avatar
{
    public class HandModel : HPTKModel
    {
        [HideInInspector]
        public ProxyHandModel proxyHand;

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

        [Header("(Updated by controller)")]
        public float fistLerp;
        public bool isFist;

        public float graspLerp;
        public float graspSpeed;
        public bool isGrasping;

        [HideInInspector]
        public Transform[] allTransforms;

        [HideInInspector]
        public BoneModel[] bones;

        protected void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            // Fingers
            AvatarHelpers.HandModelInit(this);

            // Bones
            bones = AvatarHelpers.GetHandBones(this);

            // Transforms (depends on .bones)
            allTransforms = AvatarHelpers.GetAllTransforms(this);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HandModel), true)]
public class HandModelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HandModel myScript = (HandModel)target;
        if (GUILayout.Button("INITIALIZE"))
        {
            myScript.Initialize();
        }
    }
}
#endif

