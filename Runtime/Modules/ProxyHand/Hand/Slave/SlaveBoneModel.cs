using HPTK.Views.Notifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class SlaveBoneModel : BoneModel
    {
        public Rigidbody rigidbodyRef;
        public ConfigurableJoint jointRef;
        public Collider colliderRef;
        public bool isSpecial;
        public Vector3 targetEulerOffsetRot;

        public MasterBoneModel masterBone;

        [HideInInspector]
        public Quaternion initialConnectedBodyLocalRotation;

        [HideInInspector]
        public Quaternion minLocalRot = Quaternion.identity;
    }
}