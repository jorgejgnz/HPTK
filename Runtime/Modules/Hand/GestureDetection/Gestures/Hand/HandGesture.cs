using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class HandGesture : Gesture
    {
        [Header("Hand Gesture")]
        [SerializeField]
        [ReadOnly]
        protected GestureDetectionModel _model;
        public GestureDetectionModel model { set { _model = value; } }

        [SerializeField]
        [ReadOnly]
        protected HandModel hand;
        [SerializeField]
        [ReadOnly]
        protected GestureDetectionConfiguration conf;

        private void Awake()
        {
            GestureDetectionModel parentModel = transform.parent.GetComponent<GestureDetectionModel>();
            if (parentModel && !DirectlyReferenced(parentModel) && !parentModel.extra.Contains(this)) parentModel.extra.Add(this);
        }

        public override sealed void InitGesture()
        {
            base.InitGesture();

            hand = _model.hand;
            conf = _model.configuration;

            InitHandGesture();
        }

        public override sealed void LerpUpdate()
        {
            base.LerpUpdate();

            if (!_model)
                return;

            HandLerpUpdate();
        }

        public virtual void InitHandGesture() { }

        public virtual void HandLerpUpdate() { }

        bool DirectlyReferenced(GestureDetectionModel parentModel)
        {
            if (this == parentModel.fist ||
                this == parentModel.grasp) return true;

            return false;
        }
    }
}
