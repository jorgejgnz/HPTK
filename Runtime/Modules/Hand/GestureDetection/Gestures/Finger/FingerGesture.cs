using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class FingerGesture : Gesture
    {
        [ReadOnly]
        [SerializeField]
        protected FingerGesturesModel _model;
        public FingerGesturesModel model { set { _model = value; } }

        [ReadOnly]
        [SerializeField]
        protected HandModel hand;
        [ReadOnly]
        [SerializeField]
        protected FingerModel finger;
        [ReadOnly]
        [SerializeField]
        protected GestureDetectionConfiguration conf;

        private void Awake()
        {
            FingerGesturesModel parentModel = transform.parent.GetComponent<FingerGesturesModel>();
            if (parentModel && !DirectlyReferenced(parentModel) && !parentModel.extra.Contains(this)) parentModel.extra.Add(this);
        }

        public override sealed void InitGesture()
        {
            base.InitGesture();

            hand = _model.finger.hand;
            finger = _model.finger;
            conf = _model.parent.configuration;

            InitFingerGesture();
        }

        public override sealed void LerpUpdate()
        {
            base.LerpUpdate();

            if (!_model)
                return;

            FingerLerpUpdate();
        }

        public virtual void InitFingerGesture() { }

        public virtual void FingerLerpUpdate() { }

        bool DirectlyReferenced(FingerGesturesModel parentModel)
        {
            if (this == parentModel.pinch ||
                this == parentModel.flex ||
                this == parentModel.strength ||
                this == parentModel.palmLine ||
                this == parentModel.baseRotation) return true;

            return false;
        }
    }
}
