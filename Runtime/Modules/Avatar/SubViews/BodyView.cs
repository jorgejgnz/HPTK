using HandPhysicsToolkit.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(BodyModel))]
    public sealed class BodyView : HPTKView
    {
        BodyModel model;

        public string bodyName { get { return model.name; } }

        // Tracking
        public Transform referenceTsf { get { return model.referenceTsf; } }
        public Transform replicatedTsf { get { return model.replicatedTsf; } }

        // Parent
        public AvatarView avatar { get { return model.avatar.view; } }

        // Hands
        public HandView leftHand { get { if (model.leftArm.hand) return model.leftArm.hand.specificView; else return null; } }
        public HandView rightHand { get { if (model.rightArm.hand) return model.rightArm.hand.specificView; else return null; } }

        // Parts
        public PartView root { get { return model.root.view; } }

        List<PartView> _parts = new List<PartView>();
        public List<PartView> parts { get { model.parts.ConvertAll(p => p.view, _parts); return _parts; } }

        // Related modules
        List<HPTKView> _registry = new List<HPTKView>();
        public List<HPTKView> registry { get { model.registry.ConvertAll(m => m.genericView, _registry); return _registry; } }

        // Groups
        HumanTorsoView _torso;
        public HumanTorsoView torso
        {
            get
            {
                if (_torso == null) _torso = new HumanTorsoView(model.torso);
                return _torso;
            }
        }

        HumanArmView _leftArm;
        public HumanArmView leftArm
        {
            get
            {
                if (_leftArm == null) _leftArm = new HumanArmView(model.leftArm);
                return _leftArm;
            }
        }

        HumanArmView _rightArm;
        public HumanArmView rightArm
        {
            get
            {
                if (_rightArm == null) _rightArm = new HumanArmView(model.rightArm);
                return _rightArm;
            }
        }

        HumanLegView _leftLeg;
        public HumanLegView leftLeg
        {
            get
            {
                if (_leftLeg == null) _leftLeg = new HumanLegView(model.leftLeg);
                return _leftLeg;
            }
        }

        HumanLegView _rightLeg;
        public HumanLegView rightLeg
        {
            get
            {
                if (_rightLeg == null) _rightLeg = new HumanLegView(model.rightLeg);
                return _rightLeg;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<BodyModel>();
        }

        public Tout GetRegisteredView<Tout>() where Tout : HPTKView
        {
            return BasicHelpers.FindFirst<HPTKView, Tout>(registry);
        }
    }
}
