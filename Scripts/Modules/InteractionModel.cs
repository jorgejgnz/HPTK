using HPTK.Views.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Models.Interaction
{
    [Serializable]
    public class InteractionModel
    {
        public InteractorHandler interactor;
        public InteractableHandler interactable;

        public int totalTouchCount = 0;
        public int specialTouchCount = 0;
        public float graspLerp = 0.0f;

        public float time = 0.0f;
        public float hoverTime = 0.0f;
        public float touchTime = 0.0f;
        public float grabTime = 0.0f;

        public bool isHovered = false;
        public bool isTouched = false;
        public bool isGrabbed = false;
    }
}
