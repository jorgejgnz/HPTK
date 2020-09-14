using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Settings
{
    [CreateAssetMenu(menuName = "HPTK/CoreConfiguration Asset", order = 2)]
    public class CoreConfiguration : ScriptableObject
    {
        public string alias;

        [Header("Finger theresolds")]
        public float minFlexRelDistance = 0.03f;

        public float minPinchRelDistance = 0.013f;
        public float maxPinchRelDistance = 0.05f;
        public float minLerpToPinch = 0.95f;
        public float minFlexLerpToDisablePinch = 0.8f;

        public float maxPalmRelDistance = 0.07f;
        public float minPalmRelDistance = 0.03f;

        [HideInInspector]
        public float minLocalRotZLimit = 270.0f;
        [HideInInspector]
        public float maxLocalRotZLimit = 359.0f;

        public float maxLocalRotZ = 350.0f;
        public float minLocalRotZ = 280.0f;
        public float minLerpToClose = 0.5f;

        [Header("Hand theresolds")]
        public float minLerpToFist = 0.5f;
        public float minLerpToGrasp = 0.5f;

        [Header("Error recovery")]
        public float maxErrorAllowed = 0.5f;
    }
}
