using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [Serializable]
    public class HumanTorsoModel
    {
        public BoneModel hips;
        public BoneModel spine;
        public BoneModel chest;
        public BoneModel neck;
        public BoneModel head;
        public PointModel eyes;
        public PointModel headTop;
    }

    [Serializable]
    public class HumanArmModel
    {
        public BoneModel shoulder;
        public BoneModel upper;
        public BoneModel forearm;
        public HandModel hand;
    }

    [Serializable]
    public class HumanLegModel
    {
        public BoneModel thigh;
        public BoneModel calf;
        public BoneModel foot;
        public PointModel toes;
    }
}
