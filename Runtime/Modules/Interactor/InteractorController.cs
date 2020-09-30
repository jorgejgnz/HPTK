using HPTK.Helpers;
using HPTK.Models.Interaction;
using HPTK.Models.Avatar;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Interaction
{
    public class InteractorController : InteractorHandler
    {
        public InteractorModel model;

        bool validStart = true;

        private void Awake()
        {
            model.handler = this;
            viewModel = new InteractorViewModel(model);
        }

        private void Start()
        {
            SlaveHandModel hand = model.proxyHand.slave;

            if (hand.palmCollisionNotifier)
            {
                hand.palmCollisionNotifier.onRbEnter.AddListener(OnEnter_Special);
                hand.palmCollisionNotifier.onRbExit.AddListener(OnExit_Special);
            }
            else
            {
                Debug.LogError("InteractorModule requires a slave hand with palmCollisionNotifier");
                validStart = false;
            }

            if (hand.thumb)
            {
                hand.thumb.fingerTipCollisionNotifier.onRbEnter.AddListener(OnEnter_Special);
                hand.thumb.fingerTipCollisionNotifier.onRbExit.AddListener(OnExit_Special);
            }
            else
            {
                Debug.LogError("InteractorModule requires a slave hand with CollisionNotifier attached to a thumb finger tip (slave hand)");
                validStart = false;
            }

            if (hand.index)
            {
                hand.index.fingerTipCollisionNotifier.onRbEnter.AddListener(OnEnter_Default);
                hand.index.fingerTipCollisionNotifier.onRbExit.AddListener(OnExit_Default);
            }
            else
            {
                Debug.LogError("InteractorModule requires a slave hand with CollisionNotifier attached to a index finger tip (slave hand)");
                validStart = false;
            }

            if (hand.middle)
            {
                hand.middle.fingerTipCollisionNotifier.onRbEnter.AddListener(OnEnter_Default);
                hand.middle.fingerTipCollisionNotifier.onRbExit.AddListener(OnExit_Default);
            }

            if (hand.ring)
            {
                hand.ring.fingerTipCollisionNotifier.onRbEnter.AddListener(OnEnter_Default);
                hand.ring.fingerTipCollisionNotifier.onRbExit.AddListener(OnExit_Default);
            }

            if (hand.pinky)
            {
                hand.pinky.fingerTipCollisionNotifier.onRbEnter.AddListener(OnEnter_Default);
                hand.pinky.fingerTipCollisionNotifier.onRbExit.AddListener(OnExit_Default);
            }

            hand.handTrigger.onEnterRigidbody.AddListener(OnEnter_Hover);
            hand.handTrigger.onExitRigidbody.AddListener(OnExit_Hover);
        }

        private void Update()
        {
            for (int i = 0; i < model.interactions.Count; i++)
            {
                EventWaterfall(model.interactions[i],
                    model.interactions[i].isHovered,
                    model.interactions[i].totalTouchCount > 0,
                    model.interactions[i].totalTouchCount >= 2 &&
                        model.interactions[i].specialTouchCount > 0 &&
                        model.interactions[i].specialTouchCount != model.interactions[i].totalTouchCount /* &&
                        IsWrapped(model.hand.palmCenter,model.hand.pinchCenter.position,model.interactions[i].interactable.viewModel.rigidbodyRef,0.2f) */
                        );

                if (!model.interactions[i].isHovered)
                    model.interactions.RemoveAt(i);
            }

            UpdateAbsoluteEvents();
        }

        void OnEnter_Hover(Rigidbody rb)
        {
            InteractableRef interactableRef = rb.GetComponent<InteractableRef>();

            if (!interactableRef)
                return;

            InteractionModel interaction = model.interactions.Find(x => x.interactable == interactableRef.interactable);

            if (interaction == null)
            {
                interaction = new InteractionModel();
                interaction.interactable = interactableRef.interactable;
                interaction.interactor = this;

                model.interactions.Add(interaction);

                Hover(interaction, true);
            }
        }


        void OnExit_Hover(Rigidbody rb)
        {
            InteractableRef interactableRef = rb.GetComponent<InteractableRef>();

            if (!interactableRef)
                return;

            InteractionModel interaction = model.interactions.Find(x => x.interactable == interactableRef.interactable);

            if (interaction != null)
            {
                Hover(interaction,false);

                // Update() will .Remove unhovered interactions after calling EventWaterfall()
            }
        }


        void OnEnter_Default(Rigidbody rb)
        {
            OnEnter(rb, false);
        }

        void OnEnter_Special(Rigidbody rb)
        {
            OnEnter(rb, true);
        }

        void OnExit_Default(Rigidbody rb)
        {
            OnExit(rb, false);
        }

        void OnExit_Special(Rigidbody rb)
        {
            OnExit(rb, true);
        }

        void OnEnter(Rigidbody rb, bool isSpecial)
        {
            InteractableRef interactableRef = rb.GetComponent<InteractableRef>();

            if (!interactableRef)
                return;

            InteractionModel interaction = model.interactions.Find(x => x.interactable == interactableRef.interactable);

            if (interaction != null)
            {
                if (interaction.totalTouchCount == 0)
                {
                    Touch(interaction, true);
                }

                interaction.totalTouchCount++;
                if (isSpecial) interaction.specialTouchCount++;
            }
        }

        void OnExit(Rigidbody rb, bool isSpecial)
        {
            InteractableRef interactableRef = rb.GetComponent<InteractableRef>();

            if (!interactableRef)
                return;

            InteractionModel interaction = model.interactions.Find(x => x.interactable == interactableRef.interactable);

            if (interaction != null)
            {
                if (interaction.totalTouchCount == 1)
                {
                    Touch(interaction,false);
                }

                if (interaction.totalTouchCount > 0)
                    interaction.totalTouchCount--;
                if (interaction.specialTouchCount > 0 && isSpecial)
                    interaction.specialTouchCount--;
            }

        }

        void EventWaterfall(InteractionModel interaction, bool willHover, bool willTouch, bool willGrab)
        {
            if ((!willHover || !willTouch || !willGrab) && interaction.isGrabbed)
            {
                Grab(interaction, false);
            }

            if ((!willHover || !willTouch) && interaction.isTouched)
            {
                Touch(interaction, false);
            }

            if (!willHover && interaction.isHovered)
            {
                Hover(interaction, false);
            }

            if (willHover && !interaction.isHovered)
            {
                Hover(interaction, true);
            }

            if (willTouch && interaction.isHovered && !interaction.isTouched)
            {
                Touch(interaction, true);
            }

            if (willGrab && interaction.isHovered && interaction.isTouched && !interaction.isGrabbed)
            {
                Grab(interaction, true);
            }

        }

        void Hover(InteractionModel interaction, bool willHover)
        {
            if (interaction.isHovered == willHover)
                return;

            interaction.isHovered = willHover;

            if (willHover)
            {
                interaction.interactable.onHover.Invoke(interaction);
                interaction.interactor.onHover.Invoke(interaction);

                model.totalHovering++;
            }
            else
            {
                interaction.interactable.onUnhover.Invoke(interaction);
                interaction.interactor.onUnhover.Invoke(interaction);

                model.totalHovering--;
            } 
        }

        void Touch(InteractionModel interaction, bool willTouch)
        {
            if (interaction.isTouched == willTouch)
                return;

            interaction.isTouched = willTouch;

            if (willTouch)
            {
                interaction.interactable.onTouch.Invoke(interaction);
                interaction.interactor.onTouch.Invoke(interaction);

                model.totalTouching++;
            }
            else
            {
                interaction.interactable.onUntouch.Invoke(interaction);
                interaction.interactor.onUntouch.Invoke(interaction);

                model.totalTouching--;
            }
        }

        void Grab(InteractionModel interaction, bool willGrab)
        {
            if (interaction.isGrabbed == willGrab)
                return;

            interaction.isGrabbed = willGrab;

            if (willGrab)
            {
                interaction.interactable.onGrab.Invoke(interaction);
                interaction.interactor.onGrab.Invoke(interaction);

                model.totalGrabbing++;
            }
            else
            {
                interaction.interactable.onUngrab.Invoke(interaction);
                interaction.interactor.onUngrab.Invoke(interaction);

                model.totalGrabbing--;
            }
        }

        void UpdateAbsoluteEvents()
        {
            if (model.totalGrabbing > 0 && !model.isGrabbing)
            {
                model.isGrabbing = true;
                onFirstGrab.Invoke();
            }

            if (model.totalGrabbing == 0 && model.isGrabbing)
            {
                model.isGrabbing = false;
                onLastUngrab.Invoke();
            }

            if (model.totalTouching > 0 && !model.isTouching)
            {
                model.isTouching = true;
                onFirstTouch.Invoke();
            }

            if (model.totalTouching == 0 && model.isTouching)
            {
                model.isTouching = false;
                onLastUntouch.Invoke();
            }

            if (model.totalHovering > 0 && !model.isHovering)
            {
                model.isHovering = true;
                onFirstHover.Invoke();
            }

            if (model.totalHovering == 0 && model.isHovering)
            {
                model.isHovering = false;
                onLastUnhover.Invoke();
            }
        }

        public bool IsWrapped(Transform palmCenter, Vector3 pinchPoint, Rigidbody rb, float rayDistance)
        {
            RaycastHit[] hits;
            Ray ray;

            ray = new Ray(palmCenter.position, palmCenter.forward);
            hits = Physics.RaycastAll(ray, rayDistance);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].rigidbody == rb)
                    return true;
            }

            ray = new Ray(palmCenter.position, (pinchPoint - palmCenter.position));
            hits = Physics.RaycastAll(ray, rayDistance);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].rigidbody == rb)
                    return true;
            }

            return false;
        }
    }
}
