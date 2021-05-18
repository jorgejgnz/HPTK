using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    [RequireComponent(typeof(FingerGesturesModel))]
    public sealed class FingerGesturesView : HPTKView
    {
        FingerGesturesModel model;

        public GestureDetectionView parent { get { return model.parent.view; } }

        public FingerView finger { get { return model.finger.specificView; } }

        public Pinch pinch { get { return model.pinch; } }
        public Flex flex { get { return model.flex; } }
        public Strength strength { get { return model.strength; } }
        public PalmLine palmLine { get { return model.palmLine; } }
        public BaseRotation baseRotation { get { return model.baseRotation; } }

        public List<FingerGesture> extra { get { return model.extra; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<FingerGesturesModel>();
        }
    }
}
