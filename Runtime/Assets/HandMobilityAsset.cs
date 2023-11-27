using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Assets
{
    [CreateAssetMenu(menuName = "HPTK/HandMobility", order = 2)]
    public class HandMobilityAsset : ScriptableObject
    {
        public enum MobilityLimit { Free, CanBeLower, CanBeHigher, Locked }

        [Serializable]
        public class FingerMobility
        {
            public MobilityLimit others = MobilityLimit.Locked;
            public MobilityLimit proximal = MobilityLimit.Locked;
            public MobilityLimit middle = MobilityLimit.Locked;
            public MobilityLimit distal = MobilityLimit.Locked;
            [Space]
            public bool collidesWithProximal = true;
            public bool collidesWithMiddle = true;
            public bool collidesWithDistal = true;
        }

        public FingerMobility thumb, index, middle, ring, pinky;
    }
}
