using HPTK.Controllers.Avatar;
using HPTK.Models.Avatar;
using HPTK.Views.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class ProxyHandHandler : HPTKHandler
    {
        public sealed class ProxyHandViewModel
        {
            ProxyHandModel model;
            public float indexPinchLerp { get { return model.master.index.pinchLerp; } }
            public float graspLerp { get { return model.master.graspLerp; } }
            public bool isIndexPinching { get { return model.master.index.isPinching; } }
            public bool isGrasping { get { return model.master.isGrasping; } }
            public HPTKHandler[] relatedHandlers { get { return model.relatedHandlers.ToArray(); } }
            public ProxyHandViewModel(ProxyHandModel model)
            {
                this.model = model;
            }
        }
        public ProxyHandViewModel viewModel;

        public UnityEvent onInitialized;

        public UnityEvent onIndexPinch;
        public UnityEvent onIndexUnpinch;

        public UnityEvent onGrasp;
        public UnityEvent onUngrasp;
    }
}
