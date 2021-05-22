using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    public class PuppetModel : HPTKModel
    {
        public static readonly string key = "slave";
        public static readonly float minLocalRotZ = -1.0f;
        public static readonly float maxLocalRotZ = 361.0f;

        public PartModel part;

        [Header("Control")]
        public PuppetConfiguration configuration;

        public GameObject axisPrefab;

        public string mimicRepr = AvatarModel.key;

        public bool forceUnconnected = false;

        [Header("Read Only")]
        [ReadOnly]
        public List<PartModel> parts = new List<PartModel>();
        [ReadOnly]
        public List<BoneModel> bones = new List<BoneModel>();
        [ReadOnly]
        public bool ready = false;
        [ReadOnly]
        public bool kinematic = false;

        PuppetController _controller;
        public PuppetController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<PuppetController>();
                    if (!_controller) _controller = gameObject.AddComponent<PuppetController>();
                }

                return _controller;
            }
        }

        PuppetView _view;
        public PuppetView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<PuppetView>();
                    if (!_view) _view = gameObject.AddComponent<PuppetView>();
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
