using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.Input;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.Input
{
    public enum MovingAverage
    {
        None,
        Simple,
        Weighted,
        Exponential
    }

    [Serializable]
    public class RigMap
    {
        public BoneModel wrist;     // 0

        public BoneModel forearm;   // 1

        public BoneModel thumb0;    // 2
        public BoneModel thumb1;    // 3
        public BoneModel thumb2;    // 4
        public BoneModel thumb3;    // 5

        public BoneModel index1;    // 6
        public BoneModel index2;    // 7
        public BoneModel index3;    // 8

        public BoneModel middle1;   // 9
        public BoneModel middle2;   // 10
        public BoneModel middle3;   // 11

        public BoneModel ring1;     // 12
        public BoneModel ring2;     // 13
        public BoneModel ring3;     // 14

        public BoneModel pinky0;    // 15
        public BoneModel pinky1;    // 16
        public BoneModel pinky2;    // 17
        public BoneModel pinky3;    // 18
    }

    public class InputModel : HPTKModel
    {
        public HandModel hand;

        public InputConfiguration configuration;

        public InputDataProvider inputDataProvider;

        [Header("Wrist")]
        public Transform moveThisAsWrist;
        public Transform referenceTsf;
        public Transform replicatedTsf;      

        [Header("Master rig mapping")]
        public bool autoRigMappingOnStart = true;
        public RigMap rigMap;

        [Header("Scaling")]
        public bool updateRealScale = true;

        [Header("Updated by Controller")]
        [ReadOnly]
        public bool handIsTracked = false;
        [ReadOnly]
        public bool fingersAreTracked = false;
        [ReadOnly]
        public bool isPredicting = false;

        // Noise reduction
        [ReadOnly]
        public float[] wmaWeights; // Assuming that window size won't change
        [ReadOnly]
        public AbstractTsf[][] boneRecords; // Assuming that bonesToUpdate and window size won't change. [boneToUpdate][record]
        [ReadOnly]
        public BoneModel[] bonesToUpdate;

        [Header("Read Only")]
        [ReadOnly]
        public float highestLinearSpeed = 1000.0f;
        [ReadOnly]
        public float highestAngularSpeed = 1000.0f;

        InputController _controller;
        public InputController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<InputController>();
                    if (!_controller) _controller = gameObject.AddComponent<InputController>();
                }

                return _controller;
            }
        }

        InputView _view;
        public InputView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<InputView>();
                    if (!_view) _view = gameObject.AddComponent<InputView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();
        }
    }
}
