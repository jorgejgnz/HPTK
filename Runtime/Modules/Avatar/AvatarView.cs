using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HandPhysicsToolkit.Helpers;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(AvatarModel))]
    public sealed class AvatarView : HPTKView
    {
        AvatarModel model;

        public string avatarName { get { return model.name; } }

        public bool awaked { get { return model.awaked; } }

        public bool started { get { return model.controller.started; } }

        public BodyView body { get { return model.body.view; } }

        // Representations
        List<BodyView> _bodies = new List<BodyView>();
        public List<BodyView> bodies
        {
            get
            {
                model.bodies.ConvertAll(b => b.view, _bodies);
                return _bodies;
            }
        }

        // Related modules
        List<HPTKView> _registry = new List<HPTKView>();
        public List<HPTKView> registry { get { model.registry.ConvertAll(m => m.genericView, _registry); return _registry; } }

        public bool ready { get { return model.ready; } }

        // Events
        public UnityEvent onStarted = new UnityEvent();

        public sealed override void Awake()
        {
            base.Awake();

            model = GetComponent<AvatarModel>();
        }

        public Tout GetRegisteredView<Tout>() where Tout : HPTKView
        {
            return BasicHelpers.FindFirst<HPTKView, Tout>(registry);
        }
    }
}
