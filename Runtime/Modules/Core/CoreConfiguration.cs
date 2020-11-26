using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Settings
{
    [CreateAssetMenu(menuName = "HPTK/CoreConfiguration Asset", order = 2)]
    public class CoreConfiguration : ScriptableObject
    {
        public string alias;

        [HideInInspector]
        public float minLocalRotZLimit = 270.0f;
        [HideInInspector]
        public float maxLocalRotZLimit = 359.0f;

        public float maxLocalRotZ = 350.0f;
        public float minLocalRotZ = 280.0f;

        [Header("Grasp")]
        [Range(0.0f, 1.0f)]
        public float minLerpToClose = 0.5f;
        [Range(0.0f, 1.0f)]
        public float defaultMinLerpToGrasp = 0.5f;

        [Header("Fist")]
        public float maxPalmRelDistance = 0.07f;
        public float minPalmRelDistance = 0.03f;
        [Range(0.0f, 1.0f)]
        public float defaultMinLerpToFist = 0.5f;

        [Header("Pinch")]
        public float minPinchRelDistance = 0.013f;
        public float maxPinchRelDistance = 0.05f;
        [Range(0.0f,1.0f)]
        public float minLerpToPinch = 0.95f;
        [Range(0.0f, 1.0f)]
        public float minFlexLerpToDisablePinch = 0.8f;

        [Header("Flex")]
        public float minFlexRelDistance = 0.03f;

        [Header("Gesture intention")]
        public float minTimeToIntention = 0.5f;

        [Header("Error recovery")]
        public float maxErrorAllowed = 0.5f;
    }
}
