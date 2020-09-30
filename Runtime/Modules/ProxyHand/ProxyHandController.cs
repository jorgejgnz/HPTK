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
            if (model.updateHandValues && model.configuration == null)
            {
                if (!core || !core.model.configuration)
                {
                    Debug.LogError("Lerp values can't be updated as any CoreConfiguration file is available");
                    model.updateHandValues = false;
                }
                else
                {
                    model.configuration = core.model.configuration;
                }
            }

            InitHand(model.master);
            if (model.slave) InitHand(model.slave);
            if (model.ghost) InitHand(model.ghost);

            onInitialized.Invoke();
        }

        private void Update()
        {
            if (model.updateHandValues)
            {
                UpdateHand(model.master, model.configuration);

                if (model.slave)
                {
                    UpdateHand(model.slave, model.configuration);

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

        void UpdateHand(HandModel hand, CoreConfiguration conf)
        {
            bool wasPinching = false;
            bool wasGrasping = false;

            float tempValue;

            int tempIndex;
            for (int f = 0; f < hand.fingers.Length; f++)
            {
                if (hand.fingers[f].bones.Length == 0)
                    continue;

                // Strength
                if (hand.fingers[f].bones.Length >= 3)
                    tempIndex = hand.fingers[f].bones.Length - 2;
                else
                    tempIndex = hand.fingers[f].bones.Length - 1;

                hand.fingers[f].strengthLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[f].bones[tempIndex].transformRef, conf.maxLocalRotZ, conf.minLocalRotZ);

                // Flex
                hand.fingers[f].flexLerp = AvatarHelpers.GetFingerFlexion(hand.fingers[f], conf.minFlexRelDistance, hand.proxyHand.scale);

                // Base rotation (Closed/Opened)
                hand.fingers[f].baseRotationLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[f].fingerBase, conf.maxLocalRotZ, conf.minLocalRotZ);
                hand.fingers[f].isClosed = hand.fingers[f].baseRotationLerp > conf.minLerpToClose;

                // Palm line
                hand.fingers[f].palmLineLerp = AvatarHelpers.GetPalmLineLerp(hand.fingers[f], conf.maxPalmRelDistance, conf.minPalmRelDistance, hand.proxyHand.scale);

                // Pinch
                tempValue = hand.fingers[f].pinchLerp;
                if (hand.fingers[f].flexLerp < conf.minFlexLerpToDisablePinch)
                    hand.fingers[f].pinchLerp = AvatarHelpers.GetFingerPinch(hand.fingers[f], conf.maxPinchRelDistance, conf.minPinchRelDistance, hand.proxyHand.scale);
                else
                    hand.fingers[f].pinchLerp = 0.0f;
                hand.fingers[f].pinchSpeed = (hand.fingers[f].pinchLerp - tempValue) / Time.deltaTime;

                if (hand.fingers[f] == hand.index) wasPinching = hand.fingers[f].isPinching;
                hand.fingers[f].isPinching = hand.fingers[f].pinchLerp > conf.minLerpToPinch;
            }

            // Fist
            hand.fistLerp = AvatarHelpers.GetHandFist(hand);
            hand.isFist = hand.fistLerp > conf.minLerpToFist;

            // Grasp
            tempValue = hand.graspLerp;
            hand.graspLerp = AvatarHelpers.GetHandGrasp(hand);
            hand.graspSpeed = (hand.graspLerp - tempValue) / Time.deltaTime;

            wasGrasping = hand.isGrasping;
            hand.isGrasping = hand.graspLerp > conf.minLerpToGrasp;

            // Ray
            if (hand.ray && hand.proxyHand && hand.proxyHand.shoulderTip)
            {
                hand.ray.forward = AvatarHelpers.GetHandRayDirection(hand, hand.proxyHand.shoulderTip);
                hand.ray.gameObject.SetActive(Vector3.Dot(hand.palmNormal.forward, hand.ray.forward) > 0.0f);
            }

            if (hand is MasterHandModel)
                EmitEvents(hand as MasterHandModel, hand.proxyHand.handler, wasPinching, wasGrasping);

        }

        void EmitEvents(MasterHandModel master, ProxyHandHandler handler, bool wasPinching, bool wasGrasping)
        {
            if (master.index.isPinching != wasPinching)
            {
                if (master.index.isPinching)
                    handler.onIndexPinch.Invoke();
                else
                    handler.onIndexUnpinch.Invoke();
            }

            if (master.isGrasping != wasGrasping)
            {
                if (master.isGrasping)
                    handler.onGrasp.Invoke();
                else
                    handler.onUngrasp.Invoke();
            }
        }
    }
}
