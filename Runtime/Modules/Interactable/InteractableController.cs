using HPTK.Components;
using HPTK.Models.Interaction;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Interaction
{
    public class InteractableController : InteractableHandler
    {
        public InteractableModel model;

        private void Awake()
        {
            model.handler = this;
            viewModel = new InteractableViewModel(model);
        }

        private void Start()
        {
            onGrab.AddListener((InteractionModel interaction) => model.totalGrabbing++);
            onUngrab.AddListener((InteractionModel interaction) => model.totalGrabbing--);
            onTouch.AddListener((InteractionModel interaction) => model.totalTouching++);
            onUntouch.AddListener((InteractionModel interaction) => model.totalTouching--);
            onHover.AddListener((InteractionModel interaction) => model.totalHovering++);
            onUnhover.AddListener((InteractionModel interaction) => model.totalHovering--);

            // Get rigidobdyGroup if it exists
            if (model.rigidbodyRef && !model.rigidbodyGroup && model.rigidbodyRef.GetComponent<RigidbodyGroup>())
            {
                model.rigidbodyGroup = model.rigidbodyRef.GetComponent<RigidbodyGroup>();
            }
        }

        private void Update()
        {
            UpdateAbsoluteEvents();
        }

        void UpdateAbsoluteEvents()
        {
            if (model.totalGrabbing > 0 && !model.isGrabbed)
            {
                model.isGrabbed = true;
                onFirstGrab.Invoke();
            }

            if (model.totalGrabbing == 0 && model.isGrabbed)
            {
                model.isGrabbed = false;
                onLastUngrab.Invoke();
            }

            if (model.totalTouching > 0 && !model.isTouched)
            {
                model.isTouched = true;
                onFirstTouch.Invoke();
            }

            if (model.totalTouching == 0 && model.isTouched)
            {
                model.isTouched = false;
                onLastUntouch.Invoke();
            }

            if (model.totalHovering > 0 && !model.isHovered)
            {
                model.isHovered = true;
                onFirstHover.Invoke();
            }

            if (model.totalHovering == 0 && model.isHovered)
            {
                model.isHovered = false;
                onLastUnhover.Invoke();
            }
        }
    }
}
