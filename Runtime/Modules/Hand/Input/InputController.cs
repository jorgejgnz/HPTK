using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.Input;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Hand.Input
{
    [RequireComponent(typeof(InputModel))]
    public class InputController : HPTKController
    {
        [ReadOnly]
        public InputModel model;

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

        string masterReprKey;

        // Clamping
        float handClampLerp;
        float maxHandLinearSpeed;
        float maxHandAngularSpeed;
        float fingerClampLerp;
        float maxFingerLinearSpeed;
        float maxFingerAngularSpeed;

        // Shortcut
        InputConfiguration conf;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<InputModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.hand.registry.Add(this);
        }

        private void OnDisable()
        {
            model.hand.registry.Remove(this);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();

            if (model.autoRigMappingOnStart) AutomaticRigMapping();

            // Set default configuration if needed
            if (model.configuration == null)
                model.configuration = BasicHelpers.FindScriptableObject<InputConfiguration>(HPTK.core.defaultConfAssets);

            if (model.configuration == null)
            {
                Debug.LogError("Any InputConfiguration found in InputModel or HPTK.core.defaultConfAssets. The module cannot continue");
                gameObject.SetActive(false);
                return;
            }

            // Input data for finger tips can be ignored as it will depend on the master hand model
            // This array MUST match model.inputDataProvider.bones[]
            model.bonesToUpdate = new BoneModel[19]
            {
                model.rigMap.wrist,
                model.rigMap.forearm,
                model.rigMap.thumb0,
                model.rigMap.thumb1,
                model.rigMap.thumb2,
                model.rigMap.thumb3,
                model.rigMap.index1,
                model.rigMap.index2,
                model.rigMap.index3,
                model.rigMap.middle1,
                model.rigMap.middle2,
                model.rigMap.middle3,
                model.rigMap.ring1,
                model.rigMap.ring2,
                model.rigMap.ring3,
                model.rigMap.pinky0,
                model.rigMap.pinky1,
                model.rigMap.pinky2,
                model.rigMap.pinky3
            };

            if (!model.inputDataProvider) model.inputDataProvider = HPTK.core.GetDefaultIdp(model.hand.side);

            if (model.inputDataProvider) // Force/Trigger set function from property
                model.view.inputDataProvider = model.inputDataProvider;
            else
                Debug.LogWarning("InputModel.InputDataProvider is not assigned on Start");

            // Get weights for noise reduction (WMA)
            model.wmaWeights = InputHelpers.GetLinearWeights(model.configuration.windowSize);

            // Conf shortcut
            conf = model.configuration;
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!model.inputDataProvider)
                return;

            if (!gameObject.activeSelf)
                return;

            masterReprKey = AvatarModel.key;

            model.inputDataProvider.UpdateData();

            // On hand tracked
            if (conf.ignoreTrackingLoss || model.inputDataProvider.confidence > conf.handTrackingLostUnderConfidence)
            {
                if (!model.handIsTracked)
                {
                    model.handIsTracked = true;
                    model.view.onHandTrackingRecovered.Invoke();
                }

                if (model.isPredicting)
                {
                    model.isPredicting = false;
                    model.view.onPredictionInterrupted.Invoke();
                }

                // If recovered from tracking loss
                if (timeSinceLastValidRecord != 0.0f)
                {
                    timeSinceLastValidRecord = 0.0f;

                    if (conf.hideMasterWhenLost)
                    {
                        model.hand.specificView.SetHandVisuals(true, masterReprKey);
                    }

                    if (conf.hideSlaveWhenLost)
                    {
                        model.hand.specificView.SetHandVisuals(true, PuppetModel.key);
                        model.hand.specificView.SetHandPhysics(true);
                    }
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
                    InputHelpers.RecordBone(model.boneRecords, model.inputDataProvider.bones[0], 0);
                    // RecordBone(model.inputDataProvider.bones[1], 1);
                }

                // Noise reduction
                if (model.configuration.movingAverage != MovingAverage.None)
                {
                    model.inputDataProvider.bones[0] = ReduceNoise(model.boneRecords[0], 0);
                    // model.inputDataProvider.bones[1] = ReduceNoise(model.boneRecords[1], 1);
                }

                // Use or get what to move as wrist
                if (!model.moveThisAsWrist) model.moveThisAsWrist = model.rigMap.wrist.master.transformRef;

                // Find a point of reference (if neede)
                if (model.replicatedTsf && !model.referenceTsf) model.referenceTsf = HPTK.core.trackingSpace;

                // Update pos and rot for wrist and forearm
                if (conf.updateWrist && model.bonesToUpdate[0] != null)
                {
                    // Update wrist position and rotation
                    UpdateBoneTsfPos(model.rigMap.wrist.master, model.moveThisAsWrist, model.inputDataProvider.bones[0], maxHandLinearSpeed, model.referenceTsf, model.replicatedTsf);
                    UpdateBoneTsfRot(model.rigMap.wrist.master, model.moveThisAsWrist, model.inputDataProvider.bones[0], maxHandAngularSpeed, model.referenceTsf, model.replicatedTsf);
                }

                if (conf.updateForearm && model.bonesToUpdate[1] != null)
                {
                    // Optional
                    if (model.configuration.recordTracking)
                        InputHelpers.RecordBone(model.boneRecords, model.inputDataProvider.bones[1], 1);

                    // Update wrist position and rotation
                    UpdateBoneTsfPos(model.rigMap.forearm.master, model.rigMap.forearm.master.transformRef, model.inputDataProvider.bones[1], maxHandLinearSpeed, model.referenceTsf, model.replicatedTsf);
                    UpdateBoneTsfRot(model.rigMap.forearm.master, model.rigMap.forearm.master.transformRef, model.inputDataProvider.bones[1], maxHandAngularSpeed, model.referenceTsf, model.replicatedTsf); // Optional
                }
            }
            // On hand tracking loss
            else
            {
                if (model.handIsTracked)
                {
                    model.handIsTracked = false;
                    model.view.onHandTrackingLost.Invoke();
                }

                if (timeSinceLastValidRecord == 0.0f)
                {
                    if (conf.usePredictiveTrackingWhenLost)
                    {
                        acceleration = (0.0f - wristSpeed) / conf.maxPredictionTime;
                        predictedDirection = wristVelocityDirection;
                        lastWristPosition = model.rigMap.wrist.transformRef.position;

                        if (!model.isPredicting)
                        {
                            model.isPredicting = true;
                            model.view.onPredictionStart.Invoke();
                        }
                    }

                    if (conf.hideMasterWhenLost)
                    {
                        model.hand.specificView.SetHandVisuals(false, masterReprKey);
                    }

                    if (conf.hideSlaveWhenLost)
                    {
                        model.hand.specificView.SetHandVisuals(false, PuppetModel.key);
                        model.hand.specificView.SetHandPhysics(false);
                    }
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

                        model.rigMap.wrist.transformRef.position = lastWristPosition + predictedDirection * newDisplacement;
                    }
                    else
                    {
                        if (model.isPredicting)
                        {
                            model.isPredicting = false;
                            model.view.onPredictionTimeLimitReached.Invoke();
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
                    model.view.onFingersTrackingRecovered.Invoke();
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
                        InputHelpers.RecordBone(model.boneRecords, model.inputDataProvider.bones[i], i);
                    }

                    // Noise reduction
                    if (model.configuration.movingAverage != MovingAverage.None)
                    {
                        model.inputDataProvider.bones[i] = ReduceNoise(model.boneRecords[i], i);
                    }

                    // Update only fingers rotation (assuming hierachy)
                    if (model.bonesToUpdate[i] != null)
                    {
                        BoneModel bone = model.bonesToUpdate[i];

                        AbstractTsf inputData = model.inputDataProvider.bones[i];

                        // Estimate rotation. Assuming sorted array of bones (i-1 => thumb0, i => thumb1, i+1 => thumb2)
                        if (!model.rigMap.thumb0 && bone == model.rigMap.thumb1)
                        {
                            inputData.rotation = EstimateLocalRotation(model.inputDataProvider.bones[i - 1], model.inputDataProvider.bones[i]);
                        }
                        else if (!model.rigMap.pinky0 && bone == model.rigMap.pinky1)
                        {
                            inputData.rotation = EstimateLocalRotation(model.inputDataProvider.bones[i - 1], model.inputDataProvider.bones[i]);
                        }

                        UpdateBoneTsfRot(bone.master, bone.master.transformRef, inputData, maxFingerAngularSpeed, null, null);
                    }
                }
            }
            // On fingers tracking loss
            else
            {
                if (model.fingersAreTracked)
                {
                    model.fingersAreTracked = false;
                    model.view.onFingersTrackingLost.Invoke();
                }
            }

            // If we need to record and confidence is good enough to record
            if (conf.usePredictiveTrackingWhenLost && model.inputDataProvider.confidence > conf.saveHandHistoricOverConfidence)
            {
                // Before updating wristVelocityDirection and wristPosition
                wristDirectionChange = Quaternion.FromToRotation(wristVelocityDirection, (model.rigMap.wrist.transformRef.position - wristPosition).normalized);

                // Before updating wristPosition and timeOfLastRecord
                deltaTime = Time.timeSinceLevelLoad - timeOfLastRecord;
                displacement = Vector3.Distance(wristPosition, model.rigMap.wrist.transformRef.position);
                displacement = Mathf.Clamp(displacement, 0.0f, conf.maxPredictionDisplacement * deltaTime);
                wristSpeed = displacement / deltaTime;
                wristVelocityDirection = (model.rigMap.wrist.transformRef.position - wristPosition).normalized;

                // Update wristPosition
                wristPosition = model.rigMap.wrist.transformRef.position;

                // Update timeOfLastRecord
                timeOfLastRecord = Time.timeSinceLevelLoad;
            }

            // Hand scaling
            if (model.updateRealScale && model.inputDataProvider.scale != model.hand.realScale)
            {
                model.hand.realScale = model.inputDataProvider.scale;
            }
        }

        public void AutomaticRigMapping()
        {
            HandModel hand = model.hand;

            if (hand != null && hand.wrist && hand.body)
            {
                model.rigMap.wrist = hand.wrist;

                BoneModel b;

                if (hand.thumb && hand.thumb.tip && hand.wrist)
                {
                    b = hand.thumb.tip.bone;
                    model.rigMap.thumb3 = b;

                    if (b && b.parent && b.parent != hand.wrist)
                    {
                        b = b.parent;
                        model.rigMap.thumb2 = b;

                        if (b.parent && b.parent != hand.wrist)
                        {
                            b = b.parent;
                            model.rigMap.thumb1 = b;

                            if (b.parent && b.parent != hand.wrist)
                            {
                                b = b.parent;
                                model.rigMap.thumb0 = b;
                            }
                        }
                    }
                }
                else Debug.LogWarning("Automatic rig mapping for thumb was not possible");

                if (hand.index)
                {
                    b = hand.index.tip.bone;
                    model.rigMap.index3 = b;

                    if (b && b.parent && b.parent != hand.wrist)
                    {
                        b = b.parent;
                        model.rigMap.index2 = b;

                        if (b.parent && b.parent != hand.wrist)
                        {
                            b = b.parent;
                            model.rigMap.index1 = b;
                        }
                    }
                }
                else Debug.LogWarning("Automatic rig mapping for index was not possible");

                if (hand.middle)
                {
                    b = hand.middle.tip.bone;
                    model.rigMap.middle3 = b;

                    if (b && b.parent && b.parent != hand.wrist)
                    {
                        b = b.parent;
                        model.rigMap.middle2 = b;

                        if (b.parent && b.parent != hand.wrist)
                        {
                            b = b.parent;
                            model.rigMap.middle1 = b;
                        }
                    }
                }
                else Debug.LogWarning("Automatic rig mapping for middle was not possible");

                if (hand.ring)
                {
                    b = hand.ring.tip.bone;
                    model.rigMap.ring3 = b;

                    if (b && b.parent && b.parent != hand.wrist)
                    {
                        b = b.parent;
                        model.rigMap.ring2 = b;

                        if (b.parent && b.parent != hand.wrist)
                        {
                            b = b.parent;
                            model.rigMap.ring1 = b;
                        }
                    }
                }
                else Debug.LogWarning("Automatic rig mapping for ring was not possible");

                if (hand.pinky)
                {
                    b = hand.pinky.tip.bone;
                    model.rigMap.pinky3 = b;

                    if (b && b.parent && b.parent != hand.wrist)
                    {
                        b = b.parent;
                        model.rigMap.pinky2 = b;

                        if (b.parent && b.parent != hand.wrist)
                        {
                            b = b.parent;
                            model.rigMap.pinky1 = b;

                            if (b.parent && b.parent != hand.wrist)
                            {
                                b = b.parent;
                                model.rigMap.pinky0 = b;
                            }
                        }
                    }
                }
                else Debug.LogWarning("Automatic rig mapping for pinky was not possible");
            }
            else Debug.LogWarning("Automatic rig mapping was not possible. AvatarController may has not been started");
        }

        public void InitRecordingArrays()
        {
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
        }

        void UpdateBoneTsfPos(ReprModel repr, Transform applyToThis, AbstractTsf inputData, float maxSpeed, Transform referenceTsf, Transform replicatedTsf)
        {
            AvatarController avatarCtrl = repr.point.bone.part.body.avatar.controller;

            Vector3 desiredWorldPos;

            if (inputData.space == Space.World) desiredWorldPos = inputData.position;
            else desiredWorldPos = avatarCtrl.GetWorldFromLocalPoition(inputData.position, repr);

            if (referenceTsf && replicatedTsf)
            {
                Vector3 relPos = referenceTsf.InverseTransformPoint(desiredWorldPos);
                desiredWorldPos = replicatedTsf.TransformPoint(relPos);
            }

            if (maxSpeed > 0.0f)
            {
                applyToThis.position += Vector3.ClampMagnitude(desiredWorldPos - applyToThis.position, maxSpeed * Time.deltaTime);
            }
            else
            {
                applyToThis.position = desiredWorldPos;
            }
        }

        void UpdateBoneTsfRot(ReprModel repr, Transform applyToThis, AbstractTsf inputData, float maxSpeed, Transform referenceTsf, Transform replicatedTsf)
        {
            AvatarController avatarCtrl = repr.point.bone.part.body.avatar.controller;

            Quaternion desiredWorldRot;

            if (inputData.space == Space.World) desiredWorldRot = inputData.rotation;
            else desiredWorldRot = avatarCtrl.GetWorldFromLocalRotation(inputData.rotation, repr);

            if (referenceTsf && replicatedTsf)
            {
                Quaternion relRot = Quaternion.Inverse(referenceTsf.rotation) * desiredWorldRot;
                desiredWorldRot = replicatedTsf.rotation * relRot;
            }

            if (maxSpeed > 0.0f)
            {
                applyToThis.rotation = BasicHelpers.ClampQuaternion(applyToThis.rotation, desiredWorldRot, maxSpeed * Time.deltaTime);
            }
            else
            {
                applyToThis.rotation = desiredWorldRot;
            }
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
                    return InputHelpers.SimpleMovingAverage(updatedTrackingRecord, applyPosition, applyRotation);

                case MovingAverage.Weighted:
                    return InputHelpers.WeightedMovingAverage(updatedTrackingRecord, model.wmaWeights, applyPosition, applyRotation);

                case MovingAverage.Exponential:
                    Debug.LogError("Exponential Moving Average is not supperted for input noise reduction");
                    return updatedTrackingRecord[updatedTrackingRecord.Length - 1];

                default:
                    return updatedTrackingRecord[updatedTrackingRecord.Length - 1];
            }
        }

        Quaternion EstimateLocalRotation(AbstractTsf parentBone, AbstractTsf currentBone)
        {
            return parentBone.rotation * currentBone.rotation;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InputController)), CanEditMultipleObjects]
public class InputControllerEditor : HPTKControllerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InputController myScript = (InputController)target;
        if (GUILayout.Button("AUTO RIG MAPPING"))
        {
            myScript.AutomaticRigMapping();
        }
    }
}
#endif