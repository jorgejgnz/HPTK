using HPTK.Components;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Interaction
{
    public class InteractableModel : HPTKModel
    {
        [HideInInspector]
        public InteractableHandler handler;

        public Rigidbody rigidbodyRef;
        public RigidbodyGroup rigidbodyGroup;

        [Header("Module registry")]
        public List<HPTKHandler> relatedHandlers = new List<HPTKHandler>();

        [Header("Updated by Controller")]

        public int totalHovering = 0;
        public int totalTouching = 0;
        public int totalGrabbing = 0;

        public bool isHovered = false;
        public bool isTouched = false;
        public bool isGrabbed = false;
    }
}
