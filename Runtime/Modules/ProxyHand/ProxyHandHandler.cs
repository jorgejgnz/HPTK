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

            // Most used properties are easily accessible
            public float indexPinchLerp { get { return model.master.index.pinchLerp; } }
            public float graspLerp { get { return model.master.graspLerp; } }
            public float fistLerp { get { return model.master.fistLerp; } }
            public float indexBaseRotationLerp { get { return model.master.index.baseRotationLerp; } }
            public float indexFlexLerp { get { return model.master.index.flexLerp; } }
            public float indexStrengthLerp { get { return model.master.index.strengthLerp; } }
            public bool isIndexPinching { get { return model.master.index.isPinching; } }
            public bool isGrasping { get { return model.master.isGrasping; } }
            public float errorLerp { get { return model.errorLerp; } }

            // All properties are accessible through HandViewModel and FingerviewModel
            public HandViewModel master { get { return new HandViewModel(model.master); } }
            public HandViewModel slave { get { return new HandViewModel(model.slave); } }
            public HandViewModel ghost { get { return new HandViewModel(model.ghost); } }

            public HPTKHandler[] relatedHandlers { get { return model.relatedHandlers.ToArray(); } }

            public ProxyHandViewModel(ProxyHandModel model)
            {
                this.model = model;
            }
        }

        public sealed class HandViewModel
        {
            HandModel model;

            public float graspLerp { get { return model.graspLerp; } }
            public float graspSpeed { get { return model.graspSpeed; } }
            public bool isGrasping { get { return model.isGrasping; } }
            public float fistLerp { get { return model.fistLerp; } }
            public bool isFist { get { return model.isFist; } }
            public FingerViewModel thumb { get { return new FingerViewModel(model.thumb); } }
            public FingerViewModel index { get { return new FingerViewModel(model.index); } }
            public FingerViewModel middle { get { return new FingerViewModel(model.middle); } }
            public FingerViewModel ring { get { return new FingerViewModel(model.ring); } }
            public FingerViewModel pinky { get { return new FingerViewModel(model.pinky); } }

            public HandViewModel(HandModel model)
            {
                this.model = model;
            }
        }

        public sealed class FingerViewModel
        {
            FingerModel model;

            public float pinchSpeed { get { return model.pinchSpeed; } }
            public float pinchLerp { get { return model.pinchLerp; } }
            public float baseRotationLerp { get { return model.baseRotationLerp; } }
            public float flexLerp { get { return model.flexLerp; } }
            public float strengthLerp { get { return model.strengthLerp; } }
            public float palmLineLerp { get { return model.palmLineLerp; } }
            public bool isPinching { get { return model.isPinching; } }
            public bool isClosed { get { return model.isClosed; } }

            public FingerViewModel(FingerModel model)
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
