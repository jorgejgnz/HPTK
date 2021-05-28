using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class GestureDetectionModel : HPTKModel
    {
        public HandModel hand;

        public GestureDetectionConfiguration configuration;

        [Header("Gestures")]
        public Grasp grasp;
        public Fist fist;

        public List<HandGesture> extra = new List<HandGesture>();

        [Header("Fingers")]
        public FingerGesturesModel thumb;
        public FingerGesturesModel index;
        public FingerGesturesModel middle;
        public FingerGesturesModel ring;
        public FingerGesturesModel pinky;

        [ReadOnly]
        public List<FingerGesturesModel> fingers = new List<FingerGesturesModel>();

        GestureDetectionController _controller;
        public GestureDetectionController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<GestureDetectionController>();
                    if (!_controller) _controller = gameObject.AddComponent<GestureDetectionController>();
                }

                return _controller;
            }
        }

        GestureDetectionView _view;
        public GestureDetectionView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<GestureDetectionView>();
                    if (!_view) _view = gameObject.AddComponent<GestureDetectionView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();
        }
    }
}
