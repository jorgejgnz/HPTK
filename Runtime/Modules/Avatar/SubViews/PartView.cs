using HandPhysicsToolkit.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(PartModel))]
    public class PartView : HPTKView
    {
        protected PartModel model;

        public string partName { get { return model.name; } }

        protected AvatarController controller { get { return model.body.avatar.controller; } }

        // Parents
        public PartView parent { get { if (model.parent) return model.parent.view; else return null; } }
        public BodyView body { get { return model.body.view; } }
        public BoneView root { get { return model.root.view; } }

        // Parts
        List<PartView> _parts = new List<PartView>();
        public List<PartView> parts { get { model.parts.ConvertAll(p => p.view, _parts); return _parts; } }

        // Bones
        List<BoneView> _bones = new List<BoneView>();
        public List<BoneView> bones { get { model.bones.ConvertAll(b => b.view, _bones); return _bones; } }

        // Info
        public Side side { get { return model.side; } }
        public HumanBodyPart part { get { return model.part; } }

        // Scaling
        public float totalScale { get { return model.totalScale; } }
        public float realScale
        {
            get { return model.realScale; }
            set { model.realScale = value; }
        }
        public float extraScale
        {
            get { return model.extraScale; }
            set { model.extraScale = value; }
        }

        // Related modules
        List<HPTKView> _registry = new List<HPTKView>();
        public List<HPTKView> registry { get { model.registry.ConvertAll(m => m.genericView, _registry); return _registry; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<PartModel>();
        }

        public void UpdateScale()
        {
            model.body.avatar.controller.UpdateScale(model);
        }

        public Tout GetRegisteredView<Tout>() where Tout : HPTKView
        {
            return BasicHelpers.FindFirst<HPTKView, Tout>(registry);
        }
    }
}
