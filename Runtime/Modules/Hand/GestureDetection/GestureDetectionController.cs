using HandPhysicsToolkit;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    [RequireComponent(typeof(GestureDetectionModel))]
    public class GestureDetectionController : HPTKController
    {
        [ReadOnly]
        public GestureDetectionModel model;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<GestureDetectionModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.hand.registry.Add(this);
        }

        private void OnDisable()
        {
            model.hand.registry.Remove(this);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = BasicHelpers.FindScriptableObject<GestureDetectionConfiguration>(HPTK.core.defaultConfAssets);

            if (model.configuration == null)
            {
                Debug.LogError("Any GestureDetectionConfiguration found in GestureDetectionModel or HPTK.core.defaultConfAssets. The module cannot continue");
                gameObject.SetActive(false);
                return;
            }

            // Init gestures
            InitHandGesture(model.grasp);
            InitHandGesture(model.fist);
            model.extra.ForEach(g => InitHandGesture(g));

            InitFingerGestureGroup(model.thumb, model.hand.thumb);
            InitFingerGestureGroup(model.index, model.hand.index);
            InitFingerGestureGroup(model.middle, model.hand.middle);
            InitFingerGestureGroup(model.ring, model.hand.ring);
            InitFingerGestureGroup(model.pinky, model.hand.pinky);
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!gameObject.activeSelf)
                return;

            // Update hand gestures
            UpdateGesture(model.grasp);
            UpdateGesture(model.fist);
            model.extra.ForEach(g => UpdateGesture(g));

            // Update finger gestures
            model.fingers.ForEach(f => UpdateFingerGestureGroup(f));
        }

        void UpdateFingerGestureGroup(FingerGesturesModel gestures)
        {
            UpdateGesture(gestures.pinch);
            UpdateGesture(gestures.flex);
            UpdateGesture(gestures.strength);
            UpdateGesture(gestures.palmLine);
            UpdateGesture(gestures.baseRotation);

            gestures.extra.ForEach(g => UpdateGesture(g));
        }

        void InitFingerGestureGroup(FingerGesturesModel gestures, FingerModel model)
        {
            if (gestures && model)
            {
                gestures.finger = model;

                InitFingerGesture(gestures.pinch, gestures);
                InitFingerGesture(gestures.flex, gestures);
                InitFingerGesture(gestures.strength, gestures);
                InitFingerGesture(gestures.palmLine, gestures);
                InitFingerGesture(gestures.baseRotation, gestures);

                gestures.extra.ForEach(g => InitFingerGesture(g, gestures));
            }
        }

        void InitFingerGesture(FingerGesture gesture, FingerGesturesModel model)
        {
            if (!gesture || !model)
                return;

            gesture.model = model;
            gesture.InitGesture();
        }

        void InitHandGesture(HandGesture gesture)
        {
            if (!gesture)
                return;

            gesture.model = model;
            gesture.InitGesture();
        }

        void UpdateGesture(Gesture gesture)
        {
            if (gesture) gesture.UpdateGesture();
        }
    }
}