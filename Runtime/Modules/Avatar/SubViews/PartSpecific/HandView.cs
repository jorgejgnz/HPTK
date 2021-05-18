using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Hand.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public sealed class HandView : PartView
    {
        HandModel hand { get { return model as HandModel; } }

        // Wrist
        public BoneView wrist { get { return hand.wrist.view; } }

        // Fingers
        public FingerView thumb { get { if (hand.thumb) return hand.thumb.specificView; else return null; } }
        public FingerView index { get { if (hand.index) return hand.index.specificView; else return null; } }
        public FingerView middle { get { if (hand.middle) return hand.middle.specificView; else return null; } }
        public FingerView ring { get { if (hand.ring) return hand.ring.specificView; else return null; } }
        public FingerView pinky { get { if (hand.pinky) return hand.pinky.specificView; else return null; } }

        List<FingerView> _fingers = new List<FingerView>();
        public List<FingerView> fingers { get { hand.fingers.ConvertAll(f => f.specificView, _fingers); return _fingers; } }

        // Transforms
        public PointView pinchCenter { get { return hand.pinchCenter.view; } }
        public PointView throatCenter { get { return hand.throatCenter.view; } }
        public PointView palmCenter { get { return hand.palmCenter.view; } }
        public PointView palmNormal { get { return hand.palmNormal.view; } }
        public PointView palmExterior { get { return hand.palmExterior.view; } }
        public PointView palmInterior { get { return hand.palmInterior.view; } }
        public PointView ray { get { return hand.ray.view; } }

        public void SetHandVisuals(bool enabled, string key)
        {
            model.body.avatar.controller.SetPartVisuals(hand, key, enabled);
        }

        public void SetHandPhysics(bool enabled)
        {
            model.body.avatar.controller.SetHandPhysics(hand, enabled);
        }
    }
}
