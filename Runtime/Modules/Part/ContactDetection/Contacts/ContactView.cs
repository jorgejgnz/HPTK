using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    [Serializable]
    public class ContactEvent : UnityEvent<ContactView> { }

    public class BoneCollisionView
    {
        public BoneView bone;
        public List<ContactPoint> points;

        public BoneCollisionView(BoneCollisionModel boneCollision)
        {
            this.bone = boneCollision.bone.view;
            this.points = boneCollision.points;
        }
    }

    public sealed class ContactView
    {
        Contact model;

        public ContactDetectionView detector { get { return model.detector.view; } }
        public ContactableView contactable { get { return model.contactable; } }

        List<BoneView> _bonesEntered = new List<BoneView>();
        public List<BoneView> bonesEntered { get { model.bonesEntered.ConvertAll(b => b.view, _bonesEntered); return _bonesEntered; } }
        
        List<BoneCollisionView> _bonesTouching = new List<BoneCollisionView>();
        public List<BoneCollisionView> bonesTouching { get { model.bonesTouching.ConvertAll(bc => bc.view, _bonesTouching); return _bonesTouching; } }
        
        public ContactType type { get { return model.type; } }

        public ContactView(Contact model)
        {
            this.model = model;
        }
    }
}