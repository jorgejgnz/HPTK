using HPTK.Helpers;
using HPTK.Input;
using HPTK.Models.Avatar;
using HPTK.Settings;
using HPTK.Views.Handlers.Input;
using System;
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

            // Initialize recording arrays
            model.boneRecords = new AbstractTsf[model.bonesToUpdate.Length][];
            for (int i = 0; i < model.boneRecords.Length; i++)
            {
                model.boneRecords[i] = new AbstractTsf[model.configuration.windowSize];

                for (int j = 0; j < model.boneRecords[i].Length; j++)
                {
                    // Initial state of records is the same as in IDP
                    model.boneRecords[i][j] = new AbstractTsf(model.inputDataProvider.bones[i]);
                }
            }

            // Get weights for noise reduction (WMA)
            model.wmaWeights = GetLinearWeights(model.configuration.windowSize);
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

                // Update record
                if (model.configuration.recordTracking)
                {
                    RecordBone(model.inputDataProvider.bones[0], 0);
                    // RecordBone(model.inputDataProvider.bones[1], 1);
                }

                // Noise reduction
                if (model.configuration.movingAverage != MovingAverage.None)
                {
                    model.inputDataProvider.bones[0] = ReduceNoise(model.boneRecords[0], 0);
                    // model.inputDataProvider.bones[1] = ReduceNoise(model.boneRecords[1], 1);
                }

                // Update pos and rot for wrist and forearm
                if (conf.updateWrist && model.bonesToUpdate[0] != null)
                {
                    // Update wrist position and rotation
                    UpdateMasterBonePos(model.wrist, model.inputDataProvider.bones[0], maxHandLinearSpeed);
                    UpdateMasterBoneRot(model.wrist, model.inputDataProvider.bones[0], maxHandAngularSpeed);
                }

                if (conf.updateForearm && model.bonesToUpdate[1] != null)
                {
                    // Optional
                    if (model.configuration.recordTracking)
                        RecordBone(model.inputDataProvider.bones[1], 1);

                    // Update wrist position and rotation
                    UpdateMasterBonePos(model.forearm, model.inputDataProvider.bones[1], maxHandLinearSpeed);
                    UpdateMasterBoneRot(model.forearm, model.inputDataProvider.bones[1], maxHandAngularSpeed); // Optional
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

                // Finger bones start at i=2 (i=0 -> wrist, i=1 -> forearm)
                for (int i = 2; i < model.bonesToUpdate.Length; i++)
                {
                    // Update record
                    if (model.configuration.recordTracking)
                    {
                        RecordBone(model.inputDataProvider.bones[i], i);
                    }

                    // Noise reduction
                    if (model.configuration.movingAverage != MovingAverage.None)
                    {
                        model.inputDataProvider.bones[i] = ReduceNoise(model.boneRecords[i], i);
                    }

                    // Update only fingers rotation (assuming hierachy)
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

        // "Helperizable" functions

        void RecordBone(AbstractTsf raw, int bone)
        {
            AbstractTsf[] updatedRecords = new AbstractTsf[model.boneRecords[bone].Length];
            Array.Copy(model.boneRecords[bone], 1, updatedRecords, 0, updatedRecords.Length - 1);
            updatedRecords[updatedRecords.Length - 1] = new AbstractTsf(raw);

            model.boneRecords[bone] = updatedRecords;
        }

        AbstractTsf ReduceNoise(AbstractTsf[] updatedTrackingRecord, int boneIndex)
        {
            bool applyPosition, applyRotation;

            if (boneIndex == 0 || boneIndex == 1)
            {
                applyPosition = model.configuration.applyToWristPosition;
                applyRotation = model.configuration.applyToWristRotation;
            }
            else
            {
                applyPosition = model.configuration.applyToFingersPosition;
                applyRotation = model.configuration.applyToFingersRotation;
            }

            switch (model.configuration.movingAverage)
            {
                case MovingAverage.Simple:
                    return SimpleMovingAverage(updatedTrackingRecord, applyPosition, applyRotation);

                case MovingAverage.Weighted:
                    return WeightedMovingAverage(updatedTrackingRecord, model.wmaWeights, applyPosition, applyRotation);

                case MovingAverage.Exponential:
                    Debug.LogError("Exponential Moving Average is not supperted for input noise reduction");
                    return updatedTrackingRecord[updatedTrackingRecord.Length - 1];

                default:
                    return updatedTrackingRecord[updatedTrackingRecord.Length - 1];
            }
        }

        AbstractTsf SimpleMovingAverage(AbstractTsf[] window, bool averagePosition, bool averageRotation)
        {
            // Result is a copy of the last element to preserve its name and space
            AbstractTsf result = new AbstractTsf(window[window.Length - 1]);

            float weight = 1.0f / window.Length;

            if (averagePosition)
            {
                result.position = Vector3.zero;
                for (int i = 0; i < window.Length; i++)
                {
                    result.position += window[i].position * weight;
                }
            }

            if (averageRotation)
            {
                Vector3 averageForward = Vector3.zero;
                Vector3 averageUpwards = Vector3.zero;

                for (int i = 0; i < window.Length; i++)
                {
                    averageForward += (window[i].rotation * Vector3.forward) * weight;
                    averageUpwards += (window[i].rotation * Vector3.up) * weight;
                }

                result.rotation = Quaternion.LookRotation(averageForward, averageUpwards);
            }

            return result;
        }

        AbstractTsf WeightedMovingAverage(AbstractTsf[] window, float[] weights, bool averagePosition, bool averageRotation)
        {
            if (window.Length != weights.Length)
            {
                Debug.LogError("Window and weight arrays are required to have the same length!");
                return window[window.Length - 1];
            }

            // Result is a copy of the last element to preserve its name and space
            AbstractTsf result = new AbstractTsf(window[window.Length - 1]);

            if (averagePosition)
            {
                result.position = Vector3.zero;
                for (int i = 0; i < window.Length; i++)
                {
                    result.position += window[i].position * weights[i];
                }
            }

            if (averageRotation)
            {
                Vector3 averageForward = Vector3.zero;
                Vector3 averageUpwards = Vector3.zero;

                for (int i = 0; i < window.Length; i++)
                {
                    averageForward += (window[i].rotation * Vector3.forward) * weights[i];
                    averageUpwards += (window[i].rotation * Vector3.up) * weights[i];
                }

                result.rotation = Quaternion.LookRotation(averageForward, averageUpwards);
            }

            return result;
        }

        float[] GetLinearWeights(int windowLength)
        {
            float[] result = new float[windowLength];

            result[0] = 1.0f;
            float sum = 1.0f;
            for (int i = 1; i < windowLength; i++)
            {
                result[i] = 1 + result[i - 1];  // ... -> result = {1, 2, 3, 4, 5}
                sum += result[i];               // 1 -> 1+2=3 -> 3+3=6 -> 6+4=10 -> 10+5=15
            }

            for (int i = 0; i < windowLength; i++)
            {
                result[i] = result[i] / sum;
            }

            return result;
        }
    }
}