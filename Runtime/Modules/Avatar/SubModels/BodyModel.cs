using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HandPhysicsToolkit.Utils;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class BodyModel : HPTKModel
    {
        [Header("Human groups")]
        public HumanTorsoModel torso;
        public HumanArmModel leftArm;
        public HumanArmModel rightArm;
        public HumanLegModel leftLeg;
        public HumanLegModel rightLeg;

        [Header("Camera")]
        public bool followCamera = true;
        public Transform moveThisAsHead;
        public Transform referenceTsf;
        public Transform replicatedTsf;

        [Header("Read Only")]
        [ReadOnly]
        public AvatarModel avatar;
        [ReadOnly]
        public PartModel root;
        [ReadOnly]
        public List<PartModel> parts = new List<PartModel>();
        [SerializeField]
        [ReadOnly]
        List<HPTKController> _registry;
        public HPTKRegistry registry = new HPTKRegistry();

        public BoneModel head { get { return torso.head; } }
        public HandModel leftHand { get { return leftArm.hand; } }
        public HandModel rightHand { get { return rightArm.hand; } }

        protected AvatarController controller { get { return avatar.controller; } }

        BodyView _view;
        public BodyView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<BodyView>();
                    if (!_view) _view = gameObject.AddComponent<BodyView>();
                }

                return _view;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();

            if (!avatar) avatar = GetComponent<AvatarModel>();
            if (!avatar) avatar = transform.parent.GetComponent<AvatarModel>();

            if (!avatar.bodies.Contains(this)) avatar.bodies.Add(this);
            if (!avatar.body) avatar.body = this;

            if (!leftHand) Debug.LogWarning("Missing reference of left hand for body " + transform.name);
            if (!rightHand) Debug.LogWarning("Missing reference of right hand for body " + transform.name);
        }
    }
}

