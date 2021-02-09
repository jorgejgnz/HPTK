using HPTK.Models.Avatar;
using HPTK.Views.Events;
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
            public bool isActive { get { return model.isActive; } set { model.isActive = value; } }

            public float confidence { get { return model.inputDataProvider.confidence; } }
            public bool handIsTracked { get { return model.handIsTracked; } }
            public bool fingersAreTracked { get { return model.fingersAreTracked; } }
            public bool isPredicting { get { return model.isPredicting; } }

            public InputViewModel(InputModel model)
            {
                this.model = model;
            }
        }

        public InputViewModel viewModel;

        [Header("Hand tracking loss")]
        public UnityEvent onHandTrackingLost;
        public UnityEvent onHandTrackingRecovered;

        [Header("Fingers tracking loss")]
        public UnityEvent onFingersTrackingLost;
        public UnityEvent onFingersTrackingRecovered;

        [Header("Predictive tracking")]
        public UnityEvent onPredictionStart;
        public UnityEvent onPredictionInterrupted;
        public UnityEvent onPredictionTimeLimitReached;
    }
}