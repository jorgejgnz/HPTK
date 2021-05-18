using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    [RequireComponent(typeof(GestureDetectionModel))]
    public sealed class GestureDetectionView : HPTKView
    {
        GestureDetectionModel model;

        public HandView hand { get { return model.hand.specificView; } }

        public Grasp grasp { get { return model.grasp; } }
        public Fist fist { get { return model.fist; } }

        public List<HandGesture> extra { get { return model.extra; } }

        public FingerGesturesView thumb { get { if (model.thumb) return model.thumb.view; else return null; } }

        public FingerGesturesView index { get { if (model.index) return model.index.view; else return null; } }

        public FingerGesturesView middle { get { if (model.middle) return model.middle.view; else return null; } }

        public FingerGesturesView ring { get { if (model.ring) return model.ring.view; else return null; } }

        public FingerGesturesView pinky { get { if (model.pinky) return model.pinky.view; else return null; } }

        List<FingerGesturesView> _fingers = new List<FingerGesturesView>();
        public List<FingerGesturesView> fingers { get { model.fingers.ConvertAll(f => f.view, _fingers); return _fingers; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<GestureDetectionModel>();
        }
    }
}
