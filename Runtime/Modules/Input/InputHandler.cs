using HPTK.Input;
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
            public InputDataProvider inputDataProvider { get { return model.inputDataProvider; }
                set {
                    model.inputDataProvider = value;
                    model.inputDataProvider.InitData();
                    model.initialized = true;
                    InitRecordingArrays();
                } }
            public float confidence { get { return model.inputDataProvider.confidence; } }
            public bool handIsTracked { get { return model.handIsTracked; } }
            public bool fingersAreTracked { get { return model.fingersAreTracked; } }
            public bool isPredicting { get { return model.isPredicting; } }

            // Constructor
            public InputViewModel(InputModel model)
            {
                this.model = model;
            }

            void InitRecordingArrays()
            {
                // Initialize recording arrays
                model.boneRecords = new AbstractTsf[model.bonesToUpdate.Length][];
                for (int i = 0; i < model.boneRecords.Length; i++)
                {
                    model.boneRecords[i] = new AbstractTsf[model.configuration.windowSize];

                    for (int j = 0; j < model.boneRecords[i].Length; j++)
                    {
                        // Initial state of records is the same as in IDP
                        model.boneRecords[i][j] = new AbstractTsf(model.inputDataProvider.bones[i]);
                    }
                }
            }
        }

        public InputViewModel viewModel;

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
    }
}