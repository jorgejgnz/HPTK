using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(PointModel))]
    public sealed class PointView : HPTKView
    {
        PointModel model;

        public string pointName { get { return model.name; } }

        // Parent
        public BoneView bone { get { return model.bone.view; } }

        // Representations
        public ReprView master { get { return model.master.view; } }

        Dictionary<string,ReprView> _reprs;
        public Dictionary<string, ReprView> reprs
        {
            get
            {
                if (_reprs == null) _reprs = model.reprs.ToDictionary(r => r.Key, r => r.Value.view);
                return _reprs;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<PointModel>();
        }
    }
}
