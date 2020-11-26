using HPTK.Helpers;
using HPTK.Input;
using HPTK.Models.Avatar;
using HPTK.Settings;
using HPTK.Views.Handlers.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Input
{
    public class InputController : InputHandler
    {
        public InputModel model;

        bool initialized = false;

        Vector3 wristPosition;
        Vector3 wristVelocityDirection;
        Quaternion wristDirectionChange;
        float displacement;
        float wristSpeed;

        float timeOfLastRecord = 0.0f;
        float deltaTime = 0.0f;
        Vector3 lastWristPosition;

        float timeSinceLastValidRecord;
        float acceleration;
        float newDisplacement;
        Vector3 predictedDirection;

        // Clamping
        float handClampLerp;
        float maxHandLinearSpeed;
        float maxHandAngularSpeed;
        float fingerClampLerp;
        float maxFingerLinearSpeed;
        float maxFingerAngularSpeed;

        // Shortcut
        InputConfiguration conf;

        private void Awake()
        {
            viewModel = new InputViewModel(model);
        }

        private void Start()
        {
            // Module registry
            model.proxyHand.relatedHandlers.Add(this);

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = BasicHelpers.FindScriptableObject<InputConfiguration>(core.model.defaultConfAssets);
            conf = model.configuration;

            // Input data for finger tips can be ignored as it will depend on the master hand model
            // This array MUST match model.inputDataProvider.bones[]
            model.bonesToUpdate = new MasterBoneModel[19]
            {
                model.wrist,
                model.forearm,
                model.thumb0,
                model.thumb1,
                model.thumb2,
                model.thumb3,
                model.index1,
                model.index2,
                model.index3,
                model.middle1,
                model.middle2,
                model.middle3,
                model.ring1,
                model.ring2,
                model.ring3,
                model.pinky0,
                model.pinky1,
                model.pinky2,
                model.pinky3
            };

            if (model.inputDataProvider)
            {
                model.inputDataProvider.InitData();
                initialized = true;
            }
            else
            {
                Debug.LogError("InputModel.InputDataProvider is required!");
            }
        }

        void Update()
        {
            if (!initialized)
                return;

            if (!model.isActive)
                return;

            model.inputDataProvider.UpdateData();

            // On hand tracked
            if (conf.ignoreTrackingLoss || model.inputDataProvider.confidence > conf.handTrackingLostUnderConfidence)
            {
                if (!model.handIsTracked)
                {
                    model.handIsTracked = true;
                    onHandTrackingRecovered.Invoke();
                }

                if (model.isPredicting)
                {
                    model.isPredicting = false;
                    onPredictionInterrupted.Invoke();
                }

                // If recovered from tracking loss
                if (timeSinceLastValidRecord != 0.0f)
                {
                    timeSinceLastValidRecord = 0.0f;

                    if (conf.hideMasterWhenLost)
                        model.proxyHand.handler.viewModel.SetMasterActive(true);

                    if (conf.hideSlaveWhenLost)
                        model.proxyHand.handler.viewModel.SetSlaveActive(true);
                }

                // Calculate clamping
                if (conf.useHandClamping)
                {
                    if (conf.gradualHandClamping)
                        handClampLerp = Mathf.Lerp(0.0f, conf.startDecreasingHandClampUnderConfidence, model.inputDataProvider.confidence);
                    else if (model.inputDataProvider.confidence > conf.startDecreasingHandClampUnderConfidence)
                        handClampLerp = 1.0f; // High confidece. Clamp set to highest
                    else
                        handClampLerp = 0.0f; // Low confidence. Clamp set to lowest

                    maxHandLinearSpeed = Mathf.Lerp(conf.lowestHandLinearSpeed, model.highestLinearSpeed, handClampLerp);
                    maxHandAngularSpeed = Mathf.Lerp(conf.lowestHandAngularSpeed, model.highestAngularSpeed, handClampLerp);
                }
                else
                {
                    maxHandLinearSpeed = -1.0f;
                    maxHandAngularSpeed = -1.0f;
                }

                // Update pos and rot for wrist and forearm
                if (conf.updateWrist && model.bonesToUpdate[0] != null)
                {
                    UpdateMasterBonePos(model.wrist, model.inputDataProvider.bones[0], maxHandLinearSpeed);
                    UpdateMasterBoneRot(model.wrist, model.inputDataProvider.bones[0], maxHandAngularSpeed);
                }

                if (conf.updateForearm && model.bonesToUpdate[1] != null)
                {
                    UpdateMasterBonePos(model.forearm, model.inputDataProvider.bones[1], maxHandLinearSpeed);
                    UpdateMasterBoneRot(model.forearm, model.inputDataProvider.bones[1], maxHandAngularSpeed);
                }
            }
            // On hand tracking loss
            else
            {
                if (model.handIsTracked)
                {
                    model.handIsTracked = false;
                    onHandTrackingLost.Invoke();
                }

                if (timeSinceLastValidRecord == 0.0f)
                {
                    if (conf.usePredictiveTrackingWhenLost)
                    {
                        acceleration = (0.0f - wristSpeed) / conf.maxPredictionTime;
                        predictedDirection = wristVelocityDirection;
                        lastWristPosition = model.wrist.transformRef.position;

                        if (!model.isPredicting)
                        {
                            model.isPredicting = true;
                            onPredictionStart.Invoke();
                        }
                    }

                    if (conf.hideMasterWhenLost)
                        model.proxyHand.handler.viewModel.SetMasterActive(false);

                    if (conf.hideSlaveWhenLost)
                        model.proxyHand.handler.viewModel.SetSlaveActive(false);
                }

                timeSinceLastValidRecord = Time.timeSinceLevelLoad - timeOfLastRecord;

                if (conf.usePredictiveTrackingWhenLost)
                {
                    if (timeSinceLastValidRecord < conf.maxPredictionTime)
                    {
                        // Predict new position

                        // currentTime - lastRecordTime = 0 -> velocity = initial
                        // currentTime - lastRecordTime = maxPeedictionTime -> velocity = 0

                        newDisplacement = wristSpeed * timeSinceLastValidRecord + 0.5f * acceleration * timeSinceLastValidRecord * timeSinceLastValidRecord;
                        predictedDirection = Quaternion.Slerp(wristDirectionChange, Quaternion.identity, timeSinceLastValidRecord / conf.maxPredictionTime) * wristVelocityDirection;

                        model.wrist.transformRef.position = lastWristPosition + predictedDirection * newDisplacement;
                    }
                    else
                    {
                        if (model.isPredicting)
                        {
                            model.isPredicting = false;
                            onPredictionTimeLimitReached.Invoke();
                        }
                    }
                }
            }

            // On fingers tracked
            if (conf.ignoreTrackingLoss || model.inputDataProvider.confidence > conf.fingersTrackingLostUnderConfidence)
            {
                if (!model.fingersAreTracked)
                {
                    model.fingersAreTracked = true;
                    onFingersTrackingRecovered.Invoke();
                }

                // Calculate clamping
                if (conf.useFingerClamping)
                {
                    if (conf.gradualFingerClamping)
                        fingerClampLerp = Mathf.Lerp(0.0f, conf.startDecreasingFingerClampUnderConfidence, model.inputDataProvider.confidence);
                    else if (model.inputDataProvider.confidence > conf.startDecreasingFingerClampUnderConfidence)
                        fingerClampLerp = 1.0f; // High confidece. Clamp set to highest
                    else
                        fingerClampLerp = 0.0f; // Low confidence. Clamp set to lowest

                    // maxFingerLinearSpeed = Mathf.Lerp(model.lowestMasterBoneLinearSpeed, model.highestLinearSpeed, fingerClampLerp);
                    maxFingerAngularSpeed = Mathf.Lerp(conf.lowestFingerAngularSpeed, model.highestAngularSpeed, fingerClampLerp);
                }
                else
                {
                    // maxFingerLinearSpeed = -1.0f;
                    maxFingerAngularSpeed = -1.0f;
                }

                // Update only rot for bones as input data may assume master bone lengths that don't match master on custom rigged hands
                for (int i = 2; i < model.bonesToUpdate.Length; i++)
                {
                    if (model.bonesToUpdate[i] != null)
                        UpdateMasterBoneRot(model.bonesToUpdate[i], model.inputDataProvider.bones[i], maxFingerAngularSpeed);
                }
            }
            // On fingers tracking loss
            else
            {
                if (model.fingersAreTracked)
                {
                    model.fingersAreTracked = false;
                    onFingersTrackingLost.Invoke();
                }
            }

            if (conf.usePredictiveTrackingWhenLost && model.inputDataProvider.confidence > conf.saveHandHistoricOverConfidence)
            {
                // Before updating wristVelocityDirection and wristPosition
                wristDirectionChange = Quaternion.FromToRotation(wristVelocityDirection, (model.wrist.transformRef.position - wristPosition).normalized);

                // Before updating wristPosition and timeOfLastRecord
                deltaTime = Time.timeSinceLevelLoad - timeOfLastRecord;
                displacement = Vector3.Distance(wristPosition, model.wrist.transformRef.position);
                displacement = Mathf.Clamp(displacement, 0.0f, conf.maxPredictionDisplacement * deltaTime);
                wristSpeed = displacement / deltaTime;
                wristVelocityDirection = (model.wrist.transformRef.position - wristPosition).normalized;

                // Update wristPosition
                wristPosition = model.wrist.transformRef.position;

                // Update timeOfLastRecord
                timeOfLastRecord = Time.timeSinceLevelLoad;
            }
            
        }

        void UpdateMasterBonePos(MasterBoneModel masterBone, AbstractTsf inputData, float maxSpeed)
        {
            if (inputData.space == Space.World)
            {
                if (maxSpeed > 0.0f)
                    masterBone.transformRef.position += Vector3.ClampMagnitude(inputData.position - masterBone.transformRef.position, maxSpeed * Time.deltaTime);
                else
                    masterBone.transformRef.position = inputData.position;
            }
            else
            {
                if (maxSpeed > 0.0f)
                    // Clamp is relative to scale (!)
                    masterBone.transformRef.localPosition += Vector3.ClampMagnitude(inputData.position - masterBone.transformRef.localPosition, maxSpeed * Time.deltaTime);
                else
                    masterBone.transformRef.localPosition = inputData.position;
            }  
        }

        void UpdateMasterBoneRot(MasterBoneModel masterBone, AbstractTsf inputData, float maxSpeed)
        {
            if (inputData.space == Space.World)
            {
                if (maxSpeed > 0.0f)
                    masterBone.transformRef.localRotation = BasicHelpers.ClampQuaternion(masterBone.transformRef.localRotation, Quaternion.Inverse(masterBone.transformRef.parent.rotation) * inputData.rotation, maxSpeed * Time.deltaTime);
                else
                    masterBone.transformRef.rotation = inputData.rotation;
            }
            else
            {
                if (maxSpeed > 0.0f)
                    masterBone.transformRef.localRotation = BasicHelpers.ClampQuaternion(masterBone.transformRef.localRotation, inputData.rotation, maxSpeed * Time.deltaTime);
                else
                    masterBone.transformRef.localRotation = inputData.rotation;
            }
        }

    }
}