using HandPhysicsToolkit.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace HandPhysicsToolkit.Input
{
    [Serializable]
    public class DeviceConfiguration
    {
        public string deviceName;
        public string deviceManufacturer;

        public Transform wristOffset; // Its local position and local rotation will be added to the wrist position and rotation (bones[0])

        public UnityEvent onDeviceUsed = new UnityEvent();
    }

    public class UnityXRControllerTracker : InputDataProvider
    {
        [Header("Gesture poses")]
        public HandPoseAsset openHand;
        public HandPoseAsset pinch;
        public HandPoseAsset fist;

        [Header("Control")]
        public bool useButtonTouch = true;
        [Range(0.1f, 0.0001f)]
        public float minLerpToTouch = 0.05f;
        [Range(0.5f, 0.9999f)]
        public float minLerpToPress = 0.75f;
        
        [Header("Device-specific")]
        public List<DeviceConfiguration> compatibleDevices = new List<DeviceConfiguration>();

        Matrix4x4 tsfMatrix;
        Vector3 position;
        Quaternion rotation;

        float gripLerp;
        float triggerLerp;
        Vector2 joystick;

        bool touchingGrip, touchingTrigger, touchingJoystick;
        bool pressingGrip, pressingTrigger;
        bool touchingPrimary, touchingSecondary;
        bool touchingThumb;

        bool updatedFeatureValues = false;

        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevice device;

        string currentDeviceName, currentDeviceManufacturer;
        DeviceConfiguration deviceOffset;

        List<InputFeatureUsage> usages = new List<InputFeatureUsage>();

        public override void UpdateData()
        {
            base.UpdateData();

            switch (side)
            {
                case Side.Left:
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.HeldInHand, inputDevices);
                    break;
                case Side.Right:
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.HeldInHand, inputDevices);
                    break;
            }

            if (inputDevices.Count < 1)
            {
                log = Time.time.ToString("F2") + " Controller for " + side.ToString() + " side was not found! Interrupting update";
                Debug.LogError(log);

                confidence = 0.0f;

                return;
            }

            if (inputDevices.Count > 1)
            {
                log = Time.time.ToString("F2") + " More than one controller was found for that side! Interrupting update";
                Debug.LogError(log);

                confidence = 0.0f;

                return;
            }

            if (openHand.fingers.Count != 5 || pinch.fingers.Count != 5 || fist.fingers.Count != 5)
            {
                log = Time.time.ToString("F2") + " Some poses don't have 5 fingers! Interrupting update";
                Debug.LogError(log);

                confidence = 0.0f;

                return;
            }

            device = inputDevices[0];

            ResetValues();

            log = "Manuf:[" + device.manufacturer + "] Name:[" + device.name + "]\n";

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out position)
                && device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation)
                && device.TryGetFeatureValue(CommonUsages.grip, out gripLerp)
                && device.TryGetFeatureValue(CommonUsages.trigger, out triggerLerp)
                && device.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick)
                && (useButtonTouch && device.TryGetFeatureValue(CommonUsages.primaryTouch, out touchingPrimary))
                && (useButtonTouch && device.TryGetFeatureValue(CommonUsages.secondaryTouch, out touchingSecondary)))
            {
                if (currentDeviceName != device.name || currentDeviceManufacturer != device.manufacturer)
                {
                    // On device change
                    currentDeviceName = device.name;
                    currentDeviceManufacturer = device.manufacturer;

                    deviceOffset = compatibleDevices.Find(x => currentDeviceName.Contains(x.deviceName) && currentDeviceManufacturer.Contains(x.deviceManufacturer));
                }

                if (deviceOffset != null && deviceOffset.wristOffset != null)
                {
                    tsfMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                    position = tsfMatrix.MultiplyPoint3x4(deviceOffset.wristOffset.localPosition);
                    rotation *= deviceOffset.wristOffset.localRotation;
                }

                touchingGrip = gripLerp > minLerpToTouch;
                touchingTrigger = triggerLerp > minLerpToTouch;
                touchingJoystick = joystick.magnitude > minLerpToTouch;

                pressingGrip = gripLerp > minLerpToPress;
                pressingTrigger = triggerLerp > minLerpToPress;

                if (useButtonTouch && (touchingPrimary || touchingSecondary))
                    touchingThumb = true;
                else if (touchingJoystick)
                    touchingThumb = true;
                else
                    touchingThumb = false;

                // Debugging
                device.TryGetFeatureUsages(usages);

                log += "trigger: " + triggerLerp.ToString("F2") + "\n";
                log += "grip: " + gripLerp.ToString("F2") + "\n";
                log += "isTouching: " + touchingThumb + "\n";

                updatedFeatureValues = true;
            }
            else
            {
                log += "Unavailable input data\n";

                updatedFeatureValues = false;
            }

            if (updatedFeatureValues)
            {
                Transform trackingSpace = HPTK.core.trackingSpace;

                //Wrist
                if (trackingSpace)
                {
                    bones[0].space = Space.World;
                    bones[0].position = trackingSpace.TransformPoint(position);
                    bones[0].rotation = trackingSpace.rotation * rotation;
                }
                else
                {
                    bones[0].space = Space.World;
                    bones[0].position = position;
                    bones[0].rotation = rotation;
                }

                // Forearm
                // ...

                // Assuming that HandPose.fingers follows the order thumb, index, middle, ring, pinky

                if (!touchingGrip && pressingTrigger)
                {
                    thumb = PoseHelpers.LerpFingerPose(openHand.thumb, pinch.thumb, 1.0f);

                    index = PoseHelpers.LerpFingerPose(openHand.index, pinch.index, triggerLerp);

                    middle = PoseHelpers.LerpFingerPose(openHand.middle, pinch.middle, 1.0f);
                    ring = PoseHelpers.LerpFingerPose(openHand.ring, pinch.ring, 1.0f);
                    pinky = PoseHelpers.LerpFingerPose(openHand.pinky, pinch.pinky, 1.0f);
                }
                else
                {
                    if (touchingThumb)
                    {
                        thumb = PoseHelpers.LerpFingerPose(openHand.thumb, fist.thumb, gripLerp);
                    }
                    else
                    {
                        thumb = PoseHelpers.LerpFingerPose(openHand.thumb, fist.thumb, 0.0f);
                    }

                    index = PoseHelpers.LerpFingerPose(openHand.index, fist.index, triggerLerp);

                    middle = PoseHelpers.LerpFingerPose(openHand.middle, fist.middle, gripLerp);
                    ring = PoseHelpers.LerpFingerPose(openHand.ring, fist.ring, gripLerp);
                    pinky = PoseHelpers.LerpFingerPose(openHand.pinky, fist.pinky, gripLerp);
                }
            }

            confidence = 1.0f;

            // .scale won't change

            UpdateBonesFromFingerPoses();
        }

        void ResetValues()
        {
            gripLerp = triggerLerp = 0.0f;
            joystick = Vector2.zero;
            touchingGrip = touchingTrigger = touchingJoystick = false;
            touchingPrimary = touchingSecondary = touchingThumb = false;
            pressingGrip = pressingTrigger = false;
        }
    }
}
