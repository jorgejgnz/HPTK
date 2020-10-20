using HPTK.Models.Avatar;
using HPTK.Views.Notifiers;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class ProxyHandHandler : HPTKHandler
    {
        public sealed class ProxyHandViewModel
        {
            ProxyHandModel model;

            // Most used properties are easily accessible
            public float indexPinchLerp { get { return model.master.index.pinchLerp; } }
            public float graspLerp { get { return model.master.graspLerp; } }
            public float fistLerp { get { return model.master.fistLerp; } }
            public float indexBaseRotationLerp { get { return model.master.index.baseRotationLerp; } }
            public float indexFlexLerp { get { return model.master.index.flexLerp; } }
            public float indexStrengthLerp { get { return model.master.index.strengthLerp; } }
            public bool isIndexPinching { get { return model.master.index.isPinching; } }
            public bool isGrasping { get { return model.master.isGrasping; } }

            // All properties are accessible through HandViewModel and FingerviewModel
            public HandViewModel master { get { return model.master ? new HandViewModel(model.master) : null; } }
            public HandViewModel slave { get { return model.slave ? new HandViewModel(model.slave) : null; } }
            public HandViewModel ghost { get { return model.ghost ? new HandViewModel(model.ghost) : null; } }
            public HandViewModel[] hands { get { return GetHandViewModelsArray(); } }

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
                        handViewModelsList.Add(new HandViewModel(model.hands[h]));
                }

                return handViewModelsList.ToArray();
            }
        }

        public sealed class HandViewModel
        {
            HandModel model;

            // Gestures
            public float graspLerp { get { return model.graspLerp; } }
            public float graspSpeed { get { return model.graspSpeed; } }
            public bool isGrasping { get { return model.isGrasping; } }
            public float fistLerp { get { return model.fistLerp; } }
            public bool isFist { get { return model.isFist; } }

            // Fingers
            public FingerViewModel thumb { get { return model.thumb ? new FingerViewModel(model.thumb) : null; } }
            public FingerViewModel index { get { return model.index ? new FingerViewModel(model.index) : null; } }
            public FingerViewModel middle { get { return model.middle ? new FingerViewModel(model.middle) : null; } }
            public FingerViewModel ring { get { return model.ring ? new FingerViewModel(model.ring) : null; } }
            public FingerViewModel pinky { get { return model.pinky ? new FingerViewModel(model.pinky) : null; } }
            public FingerViewModel[] fingers { get { return GetFingerViewModelsArray(); } }

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
            public SkinnedMeshRenderer skinnedMR;

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
                        fingerViewModelsList.Add(new FingerViewModel(model.fingers[f]));
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
            public BoneViewModel[] bones { get { return GetBoneViewModelsArray(); } }

            // Transforms
            public Transform knuckle { get { return model.fingerBase; } }
            public Transform tip { get { return model.fingerTip; } }

            // Extra
            public BoneViewModel distal { get { return model.distal ? new BoneViewModel(model.distal) : null; } }
            public CollisionNotifier fingerTipCollisionNotifier { get { return model.fingerTipCollisionNotifier; } }

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

        // Gesture events
        public UnityEvent onIndexPinch;
        public UnityEvent onIndexUnpinch;
        public UnityEvent onGrasp;
        public UnityEvent onUngrasp;
    }
}
