using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class HandPhysicsHandler : HPTKHandler
    {
        public sealed class HandPhysicsViewModel
        {
            HandPhysicsModel model;
            public ProxyHandHandler proxyHand { get { return model.proxyHand.handler; } }
            public bool isActive { get { return model.isActive; } set { model.isActive = value; } }
            public HandPhysicsViewModel(HandPhysicsModel model)
            {
                this.model = model;
            }
        }
        public HandPhysicsViewModel viewModel;

        // public UnityEvent onEvent;
    }
}
