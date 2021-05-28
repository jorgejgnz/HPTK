using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Input
{
    [RequireComponent(typeof(InputModel))]
    public sealed class InputView : HPTKView
    {
        InputModel model;

        public HandView hand { get { return model.hand.specificView; } }

        public InputDataProvider inputDataProvider
        {
            get { return model.inputDataProvider; }
            set
            {
                model.inputDataProvider = value;
                model.inputDataProvider.InitData();
                model.controller.InitRecordingArrays();
            }
        }
        public float confidence { get { return model.inputDataProvider.confidence; } }
        public bool handIsTracked { get { return model.handIsTracked; } }
        public bool fingersAreTracked { get { return model.fingersAreTracked; } }
        public bool isPredicting { get { return model.isPredicting; } }

        // Events
        [Header("Hand tracking loss")]
        public UnityEvent onHandTrackingLost = new UnityEvent();
        public UnityEvent onHandTrackingRecovered = new UnityEvent();

        [Header("Fingers tracking loss")]
        public UnityEvent onFingersTrackingLost = new UnityEvent();
        public UnityEvent onFingersTrackingRecovered = new UnityEvent();

        [Header("Predictive tracking")]
        public UnityEvent onPredictionStart = new UnityEvent();
        public UnityEvent onPredictionInterrupted = new UnityEvent();
        public UnityEvent onPredictionTimeLimitReached = new UnityEvent();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<InputModel>();
        }
    }
}
