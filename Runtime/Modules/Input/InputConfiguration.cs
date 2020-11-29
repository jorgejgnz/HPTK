using HPTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Settings
{
    public enum MovingAverage
    {
        None,
        Simple,
        Weighted,
        Exponential
    }

    [CreateAssetMenu(menuName = "HPTK/InputConfiguration Asset", order = 2)]
    public class InputConfiguration : ScriptableObject
    {
        [Header("Control")]
        public bool updateWrist = true;
        public bool updateForearm = true;
        public bool ignoreTrackingLoss = false;

        [Header("Hand tracking loss")]
        [Range(0.0f, 1.0f)]
        public float handTrackingLostUnderConfidence = 0.5f;
        [Range(0.0f, 1.0f)]
        [Tooltip("Save hand historic (for prediction) when confidence is greater than...")]
        public float saveHandHistoricOverConfidence = 0.5f;
        public bool hideMasterWhenLost = false;
        public bool hideSlaveWhenLost = false;

        [Header("Fingers tracking loss")]
        [Range(0.0f, 1.0f)]
        public float fingersTrackingLostUnderConfidence = 0.75f;

        [Header("Predictive tracking")]
        public bool usePredictiveTrackingWhenLost = false;
        [Range(1.0f, 10.0f)]
        public float maxPredictionTime = 2.0f;
        [Range(0.05f, 0.5f)]
        public float maxPredictionDisplacement = 0.4f;

        [Header("Noise reduction")]
        public bool recordTracking = true;
        public MovingAverage movingAverage = MovingAverage.Weighted;
        [Tooltip("Can't be changed during gameplay")]
        [Range(5, 60)]
        public int windowSize = 3;
        public bool applyToWristPosition = true;
        public bool applyToWristRotation = true;
        public bool applyToFingersPosition = false;
        public bool applyToFingersRotation = true;

        [Header("Hand clamping")]
        public bool useHandClamping = true;
        public bool gradualHandClamping = true;
        [Range(0.0f, 1.0f)]
        public float startDecreasingHandClampUnderConfidence = 0.25f;
        [Tooltip("Lower or equal 0 disables clamping")]
        public float lowestHandLinearSpeed = 1.0f;
        [Tooltip("Lower or equal 0 disables clamping")]
        public float lowestHandAngularSpeed = 10.0f;

        [Header("Finger clamping")]
        public bool useFingerClamping = false;
        public bool gradualFingerClamping = true;
        [Range(0.0f, 1.0f)]
        public float startDecreasingFingerClampUnderConfidence = 0.75f;
        //[Tooltip("Lower or equal 0 disables clamping")]
        //public float lowestMasterBoneLinearSpeed = 0.0f;
        [Tooltip("Lower or equal 0 disables clamping")]
        public float lowestFingerAngularSpeed = 10.0f;
    }
}
