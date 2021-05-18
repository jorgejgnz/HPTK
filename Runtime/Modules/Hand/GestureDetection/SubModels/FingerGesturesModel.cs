using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public class FingerGesturesModel : HPTKModel
    {
        public GestureDetectionModel parent;

        [Header("Gestures")]
        public Pinch pinch;
        public Flex flex;
        public Strength strength;
        public PalmLine palmLine;
        public BaseRotation baseRotation;

        public List<FingerGesture> extra = new List<FingerGesture>();

        [Header("Read Only")]
        [ReadOnly]
        public FingerModel finger;

        FingerGesturesView _view;
        public FingerGesturesView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<FingerGesturesView>();
                    if (!_view) _view = gameObject.AddComponent<FingerGesturesView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();

            if (!parent.fingers.Contains(this)) parent.fingers.Add(this);
        }
    }
}
