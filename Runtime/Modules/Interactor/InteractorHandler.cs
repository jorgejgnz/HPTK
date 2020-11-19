using HPTK.Models.Interaction;
using HPTK.Views.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class InteractorHandler : HPTKHandler
    {
        public sealed class InteractorViewModel
        {
            InteractorModel model;
            public ProxyHandHandler proxyHand { get { return model.proxyHand.handler; } }
            public InteractionModel[] interactions { get { return model.interactions.ToArray(); } }

            public int totalHovering { get { return model.totalHovering; } }
            public int totalTouching { get { return model.totalTouching; } }
            public int totalGrabbing { get { return model.totalGrabbing; } }

            public InteractorViewModel(InteractorModel model)
            {
                this.model = model;
            }
        }
        public InteractorViewModel viewModel;

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
