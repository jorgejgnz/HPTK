using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Models.Avatar
{
    public class BoneModel : HPTKModel
    {
        [HideInInspector]
        public FingerModel finger;

        public Transform transformRef;
        public HumanBodyBones humanBodyBone;
        public MeshRenderer meshRef;
    }
}