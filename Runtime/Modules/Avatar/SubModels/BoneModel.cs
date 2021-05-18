using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class BoneModel : HPTKModel
    {
        public ReprModel master { get { return point.master; } }
        public Dictionary<string, ReprModel> reprs { get { return point.reprs; } }
        
        public HumanBodyBones humanBodyBone;

        [Header("Read Only")]
        [ReadOnly]
        public List<BoneModel> children = new List<BoneModel>();
        [ReadOnly]
        public List<PointModel> points = new List<PointModel>();
        [ReadOnly]
        public PointModel point;
        [ReadOnly]
        public BoneModel parent;
        [ReadOnly]
        public PartModel part;

        public Transform transformRef
        {
            get { return master.transformRef; }
            set { master.transformRef = value; }
        }

        public MeshRenderer meshRenderer
        {
            get { return master.meshRenderer; }
            set { master.meshRenderer = value; }
        }

        public SkinnedMeshRenderer skinnedMeshRenderer
        {
            get { return master.skinnedMeshRenderer; }
            set { master.skinnedMeshRenderer = value; }
        }

        BoneView _view;
        public BoneView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<BoneView>();
                    if (!_view) _view = gameObject.AddComponent<BoneView>();
                }

                return _view;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();

            children.RemoveAll(c => c == null);
            points.RemoveAll(p => p == null);

            if (!part) part = GetComponent<PartModel>();
            if (!part) part = transform.parent.GetComponent<PartModel>();

            if (!parent)
            {
                int siblingIndex = transform.GetSiblingIndex();
                if (siblingIndex > 0)
                {
                    parent = transform.parent.GetChild(siblingIndex - 1).GetComponent<BoneModel>();
                    if (parent != null && !parent.children.Contains(this)) parent.children.Add(this);
                }
                else if (!part.root) part.root = this;
                else if (part.root != this) Debug.LogWarning("Multiple roots found for part " + part.name + ": bones " + transform.name + " and " + part.root.name);
            }

            if (!part.bones.Contains(this)) part.bones.Add(this);

            BoneAwake();
        }

        protected virtual void BoneAwake() { }
    }
}