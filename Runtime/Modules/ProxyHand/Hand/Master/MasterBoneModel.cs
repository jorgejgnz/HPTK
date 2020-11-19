using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class MasterBoneModel : BoneModel
    {
        [Header("Only wrist")]
        public Transform offset;

        [Header("If generated from armature")]
        public Transform armatureBone;
        public Transform armatureAnchor;
        public Quaternion initialArmatureBoneLocalRot;
        public Quaternion relativeToOriginalArmatureLocal;
        public Quaternion relativeToOriginalArmatureWorld;
    }
}
