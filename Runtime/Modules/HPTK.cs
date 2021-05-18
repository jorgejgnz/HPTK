using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit
{
    [Serializable]
    public class AvatarEvent : UnityEvent<AvatarView> { }

    public class HPTK : MonoBehaviour
    {
        // Singleton
        static HPTK _core;
        public static HPTK core
        {
            get
            {
                if (_core == null)
                    _core = GameObject.FindObjectOfType<HPTK>();

                return _core;
            }
        }

        [Header("Refs")]
        public Transform trackedCamera;
        public Transform trackingSpace;

        [Header("Configuration Assets")]
        public ScriptableObject[] defaultConfAssets;

        [Header("Input Data Providers")]
        public InputDataProvider leftEditorIdp;
        public InputDataProvider leftBuildIdp;
        public InputDataProvider rightEditorIdp;
        public InputDataProvider rightBuildIdp;

        [Header("Control")]
        public bool controlsUpdateCalls = true;
        public string applyThisLayerToAvatars = "HPTK";

        List<AvatarController> _avatars = new List<AvatarController>();
        List<AvatarView> avatars_views = new List<AvatarView>();
        public List<AvatarView> avatars {
            get
            {
                _avatars.ConvertAll(a => a.model.view, avatars_views);
                return avatars_views;
            }
        }

        [Header("Avatar registry")]

        public AvatarEvent onAvatarEntered = new AvatarEvent();

        public AvatarEvent onAvatarLeft = new AvatarEvent();

        public void AddAvatar(AvatarController avatar)
        {
            this._avatars.Add(avatar);

            avatar.ControllerStart();

            if (applyThisLayerToAvatars != "")
            {
                int layer = LayerMask.NameToLayer(applyThisLayerToAvatars);
                if (layer >= 0)
                {
                    foreach(BodyModel body in avatar.model.bodies)
                    {
                        BoneModel rootBone = body.root.root;

                        foreach(KeyValuePair<string, ReprModel> entry in rootBone.point.reprs)
                        {
                            BasicHelpers.ApplyLayerRecursively(entry.Value.transformRef, layer);
                        }
                    }
                }
            }

            onAvatarEntered.Invoke(avatar.model.view);
        }

        public void RemoveAvatar(AvatarController avatar)
        {
            onAvatarLeft.Invoke(avatar.model.view);

            this._avatars.Remove(avatar);
        }

        public InputDataProvider GetDefaultIdp(Side s)
        {
            switch(s)
            {
                case Side.Left:
#if UNITY_EDITOR
                    return leftEditorIdp;
#else
                    return leftBuildIdp;
#endif
                case Side.Right:
                default:
#if UNITY_EDITOR
                    return rightEditorIdp;
#else
                    return rightBuildIdp;
#endif
            }

        }

        private void Update()
        {
            if (controlsUpdateCalls) _avatars.ForEach((a) => a.ControllerUpdate());
        }
    }
}
