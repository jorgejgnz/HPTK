using HPTK.Helpers;
using HPTK.Models.Interaction;
using HPTK.Views.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class InteractableHandler : HPTKHandler
    {
        public sealed class InteractableViewModel
        {
            InteractableModel model;
            public Rigidbody rigidbodyRef { get { return model.rigidbodyRef; } }
            public HPTKHandler[] relatedHandlers { get { return model.relatedHandlers.ToArray(); } }

            public InteractableViewModel(InteractableModel model)
            {
                this.model = model;
            }
        }
        public InteractableViewModel viewModel;

        public UnityEvent onFirstHover = new UnityEvent();
        public InteractionEvent onHover = new InteractionEvent();
        public UnityEvent onFirstTouch = new UnityEvent();
        public InteractionEvent onTouch = new InteractionEvent();
        public UnityEvent onFirstGrab = new UnityEvent();
        public InteractionEvent onGrab = new InteractionEvent();
        public InteractionEvent onUngrab = new InteractionEvent();
        public UnityEvent onLastUngrab = new UnityEvent();
        public InteractionEvent onUntouch = new InteractionEvent();
        public UnityEvent onLastUntouch = new UnityEvent();
        public InteractionEvent onUnhover = new InteractionEvent();
        public UnityEvent onLastUnhover = new UnityEvent();
    }
}
