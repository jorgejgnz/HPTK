using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    [RequireComponent(typeof(PuppetModel))]
    public sealed class PuppetView : HPTKView
    {
        PuppetModel model;

        PuppetController controller { get { return model.controller; } }

        public PartView part { get { return model.part.view; } }
        public bool ready { get { return model.ready; } }
        public bool kinematic { get { return model.kinematic; } set { controller.SetPhysics(!value); } }
        public PuppetConfiguration configuration { get { return model.configuration; } }

        // Events
        public UnityEvent onPhysicsReady = new UnityEvent();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<PuppetModel>();
        }

        public void SetPhysics(bool enabled)
        {
            controller.SetPhysics(enabled);
        }
    }
}
