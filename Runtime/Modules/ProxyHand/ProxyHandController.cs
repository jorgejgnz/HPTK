using HPTK.Helpers;
using HPTK.Models.Avatar;
using HPTK.Settings;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Avatar
{
    public class ProxyHandController : ProxyHandHandler
    {
        public ProxyHandModel model;

        private void Awake()
        {
            viewModel = new ProxyHandViewModel(model);
            model.handler = this;
        }

        private void Start()
        {
            // Set default configuration if needed
            if (model.configuration == null)
            {
                if (!core || !core.model.configuration)
                    Debug.LogError("Any CoreConfiguration asset is available!");
                else
                    model.configuration = core.model.configuration;
            }

            // Get hand side
            if (model.avatar)
            {
                if (model == model.avatar.leftHand)
                    model.side = Side.Left;
                else if (model == model.avatar.rightHand)
                    model.side = Side.Right;
            }   

            InitHand(model.master);
            if (model.slave) InitHand(model.slave);
            if (model.ghost) InitHand(model.ghost);

            onInitialized.Invoke();
        }

        private void Update()
        {
            if (model.configuration != null)
            {
                /*
                for (int h = 0; h < model.hands.Length; h++)
                {
                    if (model.hands[h] == model.master && !model.updateValuesForMaster)
                        continue;

                    if (model.hands[h] == model.slave && !model.updateValuesForSlave)
                        continue;

                    if (model.hands[h] != model.master && model.hands[h] != model.slave && !model.updateValuesForOther)
                        continue;

                    UpdateHand(model.hands[h], viewModel.hands[h], model.configuration);
                }
                */

                UpdateHand(model.master, viewModel.master, model.configuration);
                UpdateHand(model.slave, viewModel.slave, model.configuration);

                if (model.slave)
                {
                    model.error = Vector3.Distance(
                        model.master.wrist.transformRef.position,
                        model.slave.wrist.transformRef.position
                    );

                    model.errorLerp = Mathf.InverseLerp(0.0f, core.model.configuration.maxErrorAllowed, model.error);
                }
            }
        }

        void InitHand(HandModel hand)
        {
            AvatarHelpers.UpdateFingerLengths(hand, hand.proxyHand.scale);
        }

        void UpdateHand(HandModel hand, HandViewModel viewModel, CoreConfiguration conf)
        {
            bool wasGrasping = hand.isGrasping;

            float tempValue;

            int tempIndex;
            for (int f = 0; f < hand.fingers.Length; f++)
            {
                if (hand.fingers[f].bones.Length == 0)
                    continue;

                bool wasPinching = hand.fingers[f].isPinching;

                if (model.strength)
                {
                    // Strength
                    if (hand.fingers[f].bones.Length >= 3)
                        tempIndex = hand.fingers[f].bones.Length - 2;
                    else
                        tempIndex = hand.fingers[f].bones.Length - 1;

                    hand.fingers[f].strengthLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[f].bones[tempIndex].transformRef, conf.maxLocalRotZ, conf.minLocalRotZ);
                }

                if (model.flex)
                {
                    // Flex
                    hand.fingers[f].flexLerp = AvatarHelpers.GetFingerFlexion(hand.fingers[f], conf.minFlexRelDistance, hand.proxyHand.scale);
                }

                if (model.grasp)
                {
                    // Base rotation (Closed/Opened)
                    hand.fingers[f].baseRotationLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[f].fingerBase, conf.maxLocalRotZ, conf.minLocalRotZ);
                    hand.fingers[f].isClosed = hand.fingers[f].baseRotationLerp > conf.minLerpToClose;
                }

                if (model.fist)
                {
                    // Palm line
                    hand.fingers[f].palmLineLerp = AvatarHelpers.GetPalmLineLerp(hand.fingers[f], conf.maxPalmRelDistance, conf.minPalmRelDistance, hand.proxyHand.scale);
                }

                if (model.pinch)
                {
                    // Pinch
                    tempValue = hand.fingers[f].pinchLerp;

                    if (hand.fingers[f].flexLerp < conf.minFlexLerpToDisablePinch)
                        hand.fingers[f].pinchLerp = AvatarHelpers.GetFingerPinch(hand.fingers[f], conf.maxPinchRelDistance, conf.minPinchRelDistance, hand.proxyHand.scale);
                    else
                        hand.fingers[f].pinchLerp = 0.0f;

                    // Pinch speed
                    hand.fingers[f].pinchSpeed = (hand.fingers[f].pinchLerp - tempValue) / Time.deltaTime;

                    // Is Pinching
                    hand.fingers[f].isPinching = hand.fingers[f].pinchLerp > conf.minLerpToPinch;

                    // Time counter
                    if (hand.fingers[f].isPinching)
                        hand.fingers[f].timePinching += Time.deltaTime;
                    else
                        hand.fingers[f].timePinching = 0.0f;

                    // Gesture intention     
                    if (hand.fingers[f].isPinching)
                    {
                        hand.fingers[f].pinchIntentionTime = Mathf.Clamp(hand.fingers[f].timePinching, 0.0f, model.configuration.minTimeToIntention);
                    }
                    else
                    {
                        hand.fingers[f].pinchIntentionTime = Mathf.Clamp(hand.fingers[f].pinchIntentionTime - Time.deltaTime, 0.0f, model.configuration.minTimeToIntention);
                    }

                    hand.fingers[f].pinchIntentionLerp = Mathf.InverseLerp(0.0f, model.configuration.minTimeToIntention, hand.fingers[f].pinchIntentionTime);
                }

                // Events
                EmitFingerEvents(hand.fingers[f], viewModel.fingers[f], wasPinching);               
            }

            if (model.fist)
            {
                // Fist
                hand.fistLerp = AvatarHelpers.GetHandFist(hand);
                hand.isFist = hand.fistLerp > model.minLerpToFist;
            }

            if (model.grasp)
            {
                // Grasp
                tempValue = hand.graspLerp;
                hand.graspLerp = AvatarHelpers.GetHandGrasp(hand);
                hand.graspSpeed = (hand.graspLerp - tempValue) / Time.deltaTime;

                hand.isGrasping = hand.graspLerp > model.minLerpToGrasp;

                // Time grasping
                if (hand.isGrasping)
                    hand.timeGrasping += Time.deltaTime;
                else
                    hand.timeGrasping = 0.0f;

                // Gesture intention     
                if (hand.isGrasping)
                {
                    hand.graspIntentionTime = Mathf.Clamp(hand.timeGrasping, 0.0f, model.configuration.minTimeToIntention);
                }
                else
                {
                    hand.graspIntentionTime = Mathf.Clamp(hand.graspIntentionTime - Time.deltaTime, 0.0f, model.configuration.minTimeToIntention);
                }

                hand.graspIntentionLerp = Mathf.InverseLerp(0.0f, model.configuration.minTimeToIntention, hand.graspIntentionTime);
            }

            if (model.rays)
            {
                // Ray
                if (hand.ray && hand.proxyHand && hand.proxyHand.shoulderTip)
                {
                    hand.ray.forward = AvatarHelpers.GetHandRayDirection(hand, hand.proxyHand.shoulderTip);
                    hand.ray.gameObject.SetActive(Vector3.Dot(hand.palmNormal.forward, hand.ray.forward) > 0.0f);
                }
            }

            // Events
            EmitHandEvents(hand, viewModel, wasGrasping);
        }

        void EmitHandEvents(HandModel hand, HandViewModel viewModel, bool wasGrasping)
        {
            // Grasp events
            if (hand.isGrasping != wasGrasping)
            {
                if (hand.isGrasping)
                    viewModel.onGrasp.Invoke();
                else
                    viewModel.onUngrasp.Invoke();
            }

            // Grasp intention events
            if (hand.graspIntentionLerp == 1.0f && !hand.isIntentionallyGrasping)
            {
                hand.isIntentionallyGrasping = true;
                viewModel.onLongGrasp.Invoke();
            }
            else if (hand.graspIntentionLerp == 0.0f && hand.isIntentionallyGrasping)
            {
                hand.isIntentionallyGrasping = false;
                viewModel.onLongUngrasp.Invoke();
            }        
        }

        void EmitFingerEvents(FingerModel finger, FingerViewModel viewModel, bool wasPinching)
        {
            // Pinch events
            if (finger.isPinching != wasPinching)
            {
                if (finger.isPinching)
                    viewModel.onPinch.Invoke();
                else
                    viewModel.onUnpinch.Invoke();
            }

            // Pinch intention events
            if (finger.pinchIntentionLerp == 1.0f && !finger.isIntentionallyPinching)
            {
                finger.isIntentionallyPinching = true;
                viewModel.onLongPinch.Invoke();
            }
            else if (finger.pinchIntentionLerp == 0.0f && finger.isIntentionallyPinching)
            {
                finger.isIntentionallyPinching = false;
                viewModel.onLongUnpinch.Invoke();
            }
        }
    }
}
