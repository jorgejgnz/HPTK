using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    public enum ContactType
    {
        None = 0,
        Entered = 1,
        Touched = 2,
        Grasped = 3
    }

    public class BoneCollisionModel
    {
        public BoneModel bone;
        public List<ContactPoint> points;

        BoneCollisionView _view;
        public BoneCollisionView view
        {
            get
            {
                if (_view == null) _view = new BoneCollisionView(this);
                return _view;
            }
        }

        public BoneCollisionModel(BoneModel bone)
        {
            this.bone = bone;
            this.points = new List<ContactPoint>();
        }
    }

    [Serializable]
    public class Contact
    {
        public string name;

        public ContactDetectionModel detector;
        public ContactableView contactable;

        public List<BoneModel> bonesEntered = new List<BoneModel>();

        public List<BoneCollisionModel> bonesTouching = new List<BoneCollisionModel>();

        public int specialPartsTouchingCount = 0;

        public ContactType type = ContactType.None;

        public float time = 0.0f;
        public float enteredTime = 0.0f;
        public float touchedTime = 0.0f;
        public float graspedTime = 0.0f;

        ContactView _view;
        public ContactView view
        {
            get
            {
                if (_view == null) _view = new ContactView(this);
                return _view;
            }
        }
    }
}
