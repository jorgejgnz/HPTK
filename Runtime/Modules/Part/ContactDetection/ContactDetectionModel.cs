using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    public enum HoverDetectionSystem
    {
        Triggers,
        OverlapSphereFromRoot
    }

    public class ContactDetectionModel : HPTKModel
    {
        public PartModel part;

        [Header("Control")]
        [Tooltip("This option cannot be changed in runtime")]
        public HoverDetectionSystem hoverDetectionSystem = HoverDetectionSystem.OverlapSphereFromRoot;
        public float sphereCastRadius = 0.5f;
        [Tooltip("Set false to recursively search for valid bones from part")]
        public bool detectOnlyThese = false;
        public List<BoneModel> bonesToDetect = new List<BoneModel>();

        [Header("Debug")]
        public bool drawContacts = false;
        public bool drawCollidedBones = false;

        [Header("Read Only")]
        [ReadOnly]
        public bool isEntered = false;
        [ReadOnly]
        public bool isTouched = false;
        [ReadOnly]
        public bool isGrasped = false;

        [ReadOnly]
        public List<Contact> contacts = new List<Contact>();
        [ReadOnly]
        public PuppetModel puppet;

        ContactDetectionController _controller;
        public ContactDetectionController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<ContactDetectionController>();
                    if (!_controller) _controller = gameObject.AddComponent<ContactDetectionController>();
                }

                return _controller;
            }
        }

        ContactDetectionView _view;
        public ContactDetectionView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<ContactDetectionView>();
                    if (!_view) _view = gameObject.AddComponent<ContactDetectionView>();
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
