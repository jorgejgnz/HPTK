using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(BoneModel))]
    public class BoneView : HPTKView
    {
        protected BoneModel model;

        public string boneName { get { return model.name; } }

        // Part
        public PartView part { get { return model.part.view; } }

        // Bones
        public BoneView parent { get { if (model.parent != null) return model.parent.view; else return null; } }

        List<BoneView> _children = new List<BoneView>();
        public List<BoneView> children { get { model.children.ConvertAll(c => c.view, _children); return _children; } }

        // Points
        public PointView point { get { return model.point.view; } }

        List<PointView> _points = new List<PointView>();
        public List<PointView> points { get { _points = model.points.ConvertAll(p => p.view); return _points; } }

        // Shortcuts
        public ReprView master { get { return model.master.view; } }
        public Dictionary<string, ReprView> reprs { get { return model.point.view.reprs; } }
        public Transform transformRef { get { return model.transformRef; } }
        public MeshRenderer meshRenderer { get { return model.meshRenderer; } }
        public SkinnedMeshRenderer skinnedMeshRenderer { get { return model.skinnedMeshRenderer; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<BoneModel>();
        }
    }
}
