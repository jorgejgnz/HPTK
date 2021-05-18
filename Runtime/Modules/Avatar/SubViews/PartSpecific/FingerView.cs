using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public sealed class FingerView : PartView
    {
        FingerModel finger { get { return model as FingerModel; } }

        // Hand
        public HandView hand { get { return finger.hand.specificView; } }

        // Bones
        public BoneView threeUnderLast { get { if (finger.threeUnderLast) return finger.threeUnderLast.view; else return null; } }
        public BoneView twoUnderLast { get { if (finger.twoUnderLast) return finger.twoUnderLast.view; else return null; } }
        public BoneView oneUnderLast { get { if (finger.oneUnderLast) return finger.oneUnderLast.view; else return null; } }
        public BoneView last { get { if (finger.last) return finger.last.view; else return null; } }

        List<BoneView> _bonesFromRootToTip = new List<BoneView>();
        public List<BoneView> bonesFromRootToTip { get { model.bones.ConvertAll(b => b.view, _bonesFromRootToTip); return _bonesFromRootToTip; } }

        // Transforms
        public PointView knuckle { get { return finger.knuckle.view; } }
        public PointView tip { get { return finger.tip.view; } }

        // Values
        public float length { get { return finger.length; } }
    }
}
