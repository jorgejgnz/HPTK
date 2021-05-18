using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using HandPhysicsToolkit.Physics;

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    [Serializable]
    public class PuppetBoneConfiguration
    {
        [Header("Drives")]
        public CustomJointDrive rotationDrive;
        public CustomJointDrive positionDrive;

        [Header("Rigidbody")]
        public bool useGravity = false;
        public float rigidbodyMass = 0.1f;
        public float rigidbodyDrag = 0.0f;
        public float rigidbodyAngularDrag = 0.0f;

        [Header("Stability")]
        public bool safeMode = true;
        public float maxLinearVelocity = 20.0f;
        public float maxAngularVelocity = 12.5f;
        public float maxDepenetrationVelocity = 50.0f;
        public float maxErrorAllowed = 0.5f;

        [Header("Joint")]
        public float jointMassScale = 1.0f;
        public float jointConnectedMassScale = 1.0f;

        [Header("Debug")]
        public bool force = false;
    }

    [CreateAssetMenu(menuName = "HPTK/PuppetConfiguration", order = 2)]
    public class PuppetConfiguration : ScriptableObject
    {
        public PuppetBoneConfiguration root;
        public PuppetBoneConfiguration special;
        public PuppetBoneConfiguration standard;
    }

}
