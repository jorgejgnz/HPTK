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

        CoreConfiguration conf;

        private void Awake()
        {
            viewModel = new ProxyHandViewModel(model);
            model.handler = this;
        }

        private void Start()
        {
            if (model.updateHandValues)
                conf = core.model.configuration;

            InitHand(model.master);
            if (model.slave) InitHand(model.slave);
            if (model.ghost) InitHand(model.ghost);

            onInitialized.Invoke();
        }

        private void Update()
        {
            if (model.updateHandValues)
            {
                UpdateHand(model.master);

                if (model.slave)
                {
                    UpdateHand(model.slave);

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

        void UpdateHand(HandModel hand)
        {
            bool wasPinching = false;
            bool wasGrasping = false;

            float tempValue;

            int tempIndex;
            for (int i = 0; i < hand.fingers.Length; i++)
            {
                // Strength
                tempIndex = hand.fingers[i].bones.Length - 2;
                hand.fingers[i].strengthLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[i].bones[tempIndex], conf.maxLocalRotZ, conf.minLocalRotZ);

                // Flex
                hand.fingers[i].flexLerp = AvatarHelpers.GetFingerFlexion(hand.fingers[i], conf.minFlexRelDistance, hand.proxyHand.scale);

                // Base rotation (Closed/Opened)
                tempIndex = hand.fingers[i].bones.Length - 3;
                hand.fingers[i].baseRotationLerp = AvatarHelpers.GetBoneRotLerp(hand.fingers[i].bones[tempIndex], conf.maxLocalRotZ, conf.minLocalRotZ);
                hand.fingers[i].isClosed = hand.fingers[i].baseRotationLerp > conf.minLerpToClose;

                // Palm line
                hand.fingers[i].palmLineLerp = AvatarHelpers.GetPalmLineLerp(hand.fingers[i], conf.maxPalmRelDistance, conf.minPalmRelDistance, hand.proxyHand.scale);

                // Pinch
                tempValue = hand.fingers[i].pinchLerp;
                if (hand.fingers[i].flexLerp < conf.minFlexLerpToDisablePinch)
                    hand.fingers[i].pinchLerp = AvatarHelpers.GetFingerPinch(hand.fingers[i], conf.maxPinchRelDistance, conf.minPinchRelDistance, hand.proxyHand.scale);
                else
                    hand.fingers[i].pinchLerp = 0.0f;
                hand.fingers[i].pinchSpeed = (hand.fingers[i].pinchLerp - tempValue) / Time.deltaTime;

                if (hand.fingers[i] == hand.index) wasPinching = hand.fingers[i].isPinching;
                hand.fingers[i].isPinching = hand.fingers[i].pinchLerp > conf.minLerpToPinch;
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
            if (hand.ray)
            {
                hand.ray.forward = AvatarHelpers.GetHandRayDirection(hand);
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
