using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HandPhysicsToolkit.Modules.Avatar
{
    public class HandModel : PartModel
    {
        [Header("Models")]
        public BoneModel wrist;
        public FingerModel thumb;
        public FingerModel index;
        public FingerModel middle;
        public FingerModel ring;
        public FingerModel pinky;

        [Header("Transforms")]
        public PointModel pinchCenter;
        public PointModel throatCenter;
        public PointModel palmCenter;
        public PointModel palmNormal;
        public PointModel palmExterior;
        public PointModel palmInterior;
        public PointModel ray;

        [Header("(Hidden)")]
        List<FingerModel> _fingers;
        public List<FingerModel> fingers
        {
            get
            {
                if (_fingers == null)
                {
                    _fingers = new List<FingerModel>();
                    BasicHelpers.FindAll<PartModel, FingerModel>(parts, _fingers);
                }
                return _fingers;
            }
        }

        public HandView specificView { get { return view as HandView; } }

        protected override sealed void PartAwake()
        {
            base.PartAwake();

            if (thumb) thumb.hand = this;
            if (index) index.hand = this;
            if (middle) middle.hand = this;
            if (ring) ring.hand = this;
            if (pinky) pinky.hand = this;
        }

        protected sealed override PartView GetView()
        {
            PartView view = GetComponent<HandView>();
            if (!view) view = gameObject.AddComponent<HandView>();

            return view;
        }
    }
}
