using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit;
using UnityEngine.Events;
using System.Collections.ObjectModel;
using HandPhysicsToolkit.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class AvatarModel : HPTKModel
    {
        public static readonly string key = "master";

        [Header("Read Only")]
        [SerializeField]
        [ReadOnly]
        List<HPTKController> _registry;
        public HPTKRegistry registry = new HPTKRegistry();
        [ReadOnly]
        public List<BodyModel> bodies = new List<BodyModel>();
        [ReadOnly]
        public BodyModel body;
        [ReadOnly]
        public bool ready = false;

        AvatarController _controller;
        public AvatarController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<AvatarController>();
                    if (!_controller) _controller = gameObject.AddComponent<AvatarController>();
                }

                return _controller;
            }
        }

        AvatarView _view;
        public AvatarView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<AvatarView>();
                    if (!_view) _view = gameObject.AddComponent<AvatarView>();
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