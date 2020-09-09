using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers.Input
{
    public class InputHandler : HPTKHandler
    {
        public sealed class InputViewModel
        {
            InputModel model;
            public ProxyHandHandler proxyHand { get { return model.proxyHand.handler; } }
            public InputViewModel(InputModel model)
            {
                this.model = model;
            }
        }

        public InputViewModel viewModel;

        // public UnityEvent onEvent;
    }
}