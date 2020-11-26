using HPTK.Helpers;
using HPTK.Models.Avatar;
using HPTK.Views.Handlers.Input;
using HPTK.Views.Notifiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class ProxyHandHandler : HPTKHandler
    {
        [Serializable]
        public sealed class ProxyHandViewModel
        {
            ProxyHandModel model;

            // All properties are accessible through HandViewModel and FingerviewModel
            HandViewModel _master;
            public HandViewModel master
            {
                get
                {
                    if (_master == null && model.master != null)  _master = new HandViewModel(model.master);
                    return _master;
                }
            }
            HandViewModel _slave;
            public HandViewModel slave
            {
                get
                {
                    if (_slave == null && model.slave != null) _slave = new HandViewModel(model.slave);
                    return _slave;
                }
            }
            HandViewModel _ghost;
            public HandViewModel ghost
            {
                get
                {
                    if (_ghost == null && model.ghost != null) _ghost = new HandViewModel(model.ghost);
                    return _ghost;
                }
            }
            HandViewModel[] _hands;
            public HandViewModel[] hands
            {
                get
                {
                    if (_hands == null && model.hands != null) _hands = GetHandViewModelsArray();
                    return _hands;
                }
            }

            // Related modules
            public HPTKHandler[] relatedHandlers { get { return model.relatedHandlers.ToArray(); } }

            // Transforms
            public Transform shoulderTip { get { return model.shoulderTip; } }

            // Extra
            public float errorLerp { get { return model.errorLerp; } }
            public float scale { get { return model.scale; } }

            public ProxyHandViewModel(ProxyHandModel model)
            {
                this.model = model;
            }

            HandViewModel[] GetHandViewModelsArray()
            {
                List<HandViewModel> handViewModelsList = new List<HandViewModel>();

                for (int h = 0; h < model.hands.Length; h++)
                {
                    if (model.hands[h] != null)
                    {
                        if (model.hands[h] == model.master)
                            handViewModelsList.Add(master);

                        else if (model.hands[h] == model.slave)
                            handViewModelsList.Add(slave);

                        else if (model.hands[h] == model.ghost)
                            handViewModelsList.Add(ghost);

                        else
                            handViewModelsList.Add(new HandViewModel(model.hands[h]));
                    }
                }

                return handViewModelsList.ToArray();
            }

            public void SetMasterActive(bool active)
            {
                // Visuals
                model.master.skinnedMR.enabled = active;
            }

            public void SetSlaveActive(bool active)
            {
                if (active)
                {
                    // Teleport
                    model.slave.wrist.transformRef.position = model.master.wrist.transformRef.position;
                    model.slave.wrist.transformRef.rotation = model.master.wrist.transformRef.rotation;

                    // Physics
                    PhysHelpers.SetHandPhysics(model, active);

                    // Module
                    HandPhysicsHandler handPhysics = BasicHelpers.FindHandler<HandPhysicsHandler>(model.relatedHandlers.ToArray());
                    handPhysics.viewModel.isActive = active;
                }                    
                else
                {
                    // Module
                    HandPhysicsHandler handPhysics = BasicHelpers.FindHandler<HandPhysicsHandler>(model.relatedHandlers.ToArray());
                    handPhysics.viewModel.isActive = active;

                    // Physics
                    PhysHelpers.SetHandPhysics(model, active);
                }

                // Visuals
                model.slave.skinnedMR.enabled = active;
            }
        }

        [Serializable]
        public sealed class HandViewModel
        {
            HandModel model;

            // Gestures
            public float graspLerp { get { return model.graspLerp; } }
            public float graspSpeed { get { return model.graspSpeed; } }
            public bool isGrasping { get { return model.isGrasping; } }
            public bool isIntentionallyGrasping { get { return model.isIntentionallyGrasping; } }
            public float fistLerp { get { return model.fistLerp; } }
            public bool isFist { get { return model.isFist; } }

            // Fingers
            FingerViewModel _thumb;
            public FingerViewModel thumb
            {
                get
                {
                    if (_thumb == null && model.thumb != null) _thumb = new FingerViewModel(model.thumb);
                    return _thumb;
                }
            }
            FingerViewModel _index;
            public FingerViewModel index
            {
                get
                {
                    if (_index == null && model.index != null) _index = new FingerViewModel(model.index);
                    return _index;
                }
            }
            FingerViewModel _middle;
            public FingerViewModel middle
            {
                get
                {
                    if (_middle == null && model.middle != null) _middle = new FingerViewModel(model.middle);
                    return _middle;
                }
            }
            FingerViewModel _ring;
            public FingerViewModel ring
            {
                get
                {
                    if (_ring == null && model.ring != null) _ring = new FingerViewModel(model.ring);
                    return _ring;
                }
            }
            FingerViewModel _pinky;
            public FingerViewModel pinky
            {
                get
                {
                    if (_pinky == null && model.pinky != null) _pinky = new FingerViewModel(model.pinky);
                    return _pinky;
                }
            }
            FingerViewModel[] _fingers;
            public FingerViewModel[] fingers
            {
                get
                {
                    if (_fingers == null && model.fingers != null) _fingers = GetFingerViewModelsArray();
                    return _fingers;
                }
            }

            // Transforms
            public Transform pinchCenter { get { return model.pinchCenter; } }
            public Transform throatCenter { get { return model.throatCenter; } }
            public Transform palmCenter { get { return model.palmCenter; } }
            public Transform palmNormal { get { return model.palmNormal; } }
            public Transform palmExterior { get { return model.palmExterior; } }
            public Transform palmInterior { get { return model.palmInterior; } }
            public Transform ray { get { return model.ray; } }

            // Bones
            public BoneViewModel wrist { get { return model.wrist ? new BoneViewModel(model.wrist) : null; } }
            public BoneViewModel forearm { get { return model.forearm ? new BoneViewModel(model.forearm) : null; } }
            public BoneViewModel[] bones { get { return GetBoneViewModelsArray(); } }

            // Extra
            public SkinnedMeshRenderer skinnedMR { get { return model.skinnedMR; } }

            // Events
            public UnityEvent onGrasp = new UnityEvent();
            public UnityEvent onLongGrasp = new UnityEvent();
            public UnityEvent onUngrasp = new UnityEvent();
            public UnityEvent onLongUngrasp = new UnityEvent();

            public HandViewModel(HandModel model)
            {
                this.model = model;
            }

            FingerViewModel[] GetFingerViewModelsArray()
            {
                List<FingerViewModel> fingerViewModelsList = new List<FingerViewModel>();

                for (int f = 0; f < model.fingers.Length; f++)
                {
                    if (model.fingers[f] != null)
                    {
                        if (model.fingers[f] == model.thumb)
                            fingerViewModelsList.Add(thumb);

                        else if (model.fingers[f] == model.index)
                            fingerViewModelsList.Add(index);

                        else if (model.fingers[f] == model.middle)
                            fingerViewModelsList.Add(middle);

                        else if (model.fingers[f] == model.ring)
                            fingerViewModelsList.Add(ring);

                        else if (model.fingers[f] == model.pinky)
                            fingerViewModelsList.Add(pinky);

                        else
                            fingerViewModelsList.Add(new FingerViewModel(model.fingers[f]));
                    }
                }

                return fingerViewModelsList.ToArray();
            }

            BoneViewModel[] GetBoneViewModelsArray()
            {
                List<BoneViewModel> boneViewModelsList = new List<BoneViewModel>();

                for (int b = 0; b < model.bones.Length; b++)
                {
                    boneViewModelsList.Add(new BoneViewModel(model.bones[b]));
                }

                return boneViewModelsList.ToArray();
            }
        }

        [Serializable]
        public sealed class FingerViewModel
        {
            FingerModel model;

            // Gestures
            public float pinchSpeed { get { return model.pinchSpeed; } }
            public float pinchLerp { get { return model.pinchLerp; } }
            public float baseRotationLerp { get { return model.baseRotationLerp; } }
            public float flexLerp { get { return model.flexLerp; } }
            public float strengthLerp { get { return model.strengthLerp; } }
            public float palmLineLerp { get { return model.palmLineLerp; } }
            public bool isPinching { get { return model.isPinching; } }
            public bool isClosed { get { return model.isClosed; } }

            // Bones
            BoneViewModel[] _bones;
            public BoneViewModel[] bones
            {
                get
                {
                    if (_bones == null && model.bones != null) _bones = GetBoneViewModelsArray();
                    return _bones;
                }
            }

            // Transforms
            public Transform knuckle { get { return model.fingerBase; } }
            public Transform tip { get { return model.fingerTip; } }

            // Extra
            public BoneViewModel distal { get { return model.distal ? new BoneViewModel(model.distal) : null; } }
            public CollisionNotifier fingerTipCollisionNotifier { get { return model.fingerTipCollisionNotifier; } }

            // Events
            public UnityEvent onPinch = new UnityEvent();
            public UnityEvent onLongPinch = new UnityEvent();
            public UnityEvent onUnpinch = new UnityEvent();
            public UnityEvent onLongUnpinch = new UnityEvent();

            public FingerViewModel(FingerModel model)
            {
                this.model = model;
            }

            BoneViewModel[] GetBoneViewModelsArray()
            {
                List<BoneViewModel> boneViewModelsList = new List<BoneViewModel>();

                for (int b = 0; b < model.bones.Length; b++)
                {
                    boneViewModelsList.Add(new BoneViewModel(model.bones[b]));
                }

                return boneViewModelsList.ToArray();
            }
        }

        [Serializable]
        public sealed class BoneViewModel
        {
            BoneModel model;
            
            // Generic bone
            public Transform transformRef { get { return model.transformRef; } }
            public MeshRenderer meshRendererRef { get { return model.meshRef; } }

            // Slave bone
            public Rigidbody rigidbodyRef { get { return model is SlaveBoneModel ? (model as SlaveBoneModel).rigidbodyRef : null; } }
            public Collider colliderRef { get { return model is SlaveBoneModel ? (model as SlaveBoneModel).colliderRef : null; } }
            public ConfigurableJoint jointref { get { return model is SlaveBoneModel ? (model as SlaveBoneModel).jointRef : null; } }

            public BoneViewModel(BoneModel model)
            {
                this.model = model;
            }
        }

        public ProxyHandViewModel viewModel;

        // Generic events
        public UnityEvent onInitialized;
    }
}
