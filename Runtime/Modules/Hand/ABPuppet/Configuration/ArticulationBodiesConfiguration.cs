
namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    using UnityEngine;
    using System;
    using UnityEngine.Events;

    [CreateAssetMenu(menuName = "HPTK/ArticulationBodiesConfiguration", order = 2)]
    public class ArticulationBodiesConfiguration : ScriptableObject
    {
        [Header("Articulation Bodies")]
        public ArticulationBodyBaseConfiguration Root;
        public bool UseWristAnchorRotation;
        public bool DisableFingerToPalmCollision;
        public bool DisableFingerToFingerCollision;
        public bool AddFakeColliders;
        public LayerMask NoCollisionLayer;
        public ArticulationBodyConfiguration Palm;
        public ArticulationBodyFingerConfiguration FingerBone;


        [Header("Stability")]
        public float MaxLinearVelocity = 2f;
        public float MaxAngularVelocity = 1f;
        public float MaxDepenetrationVelocity = 1f;
        public float MaxJointVelocity = 1f;
        public PhysicMaterial HandPhysicMaterial;

        public UnityEvent OnConfigurationChange;

        void OnValidate()
        {
            OnConfigurationChange.Invoke();
        }
    }

    [Serializable]
    public class ArticulationBodyBaseConfiguration
    {
        [Header("Physics Properties")]
        public bool UseGravity = false;
        public float Mass = 0.1f;
        public CollisionDetectionMode CollisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    [Serializable]
    public class ArticulationBodyConfiguration : ArticulationBodyBaseConfiguration
    {
        public float LinearDamping = 0.05f;
        public float AngularDamping = 0.05f;
        public float JointFriction = 0.05f;

        [Header("Articulation Drives")]
        public ArticulationDriveConfiguration PositionDrives;
        public ArticulationDriveConfiguration RotationDrives;
    }

    [Serializable]
    public class ArticulationBodyFingerConfiguration : ArticulationBodyConfiguration
    {
        public ArticulationDriveConfiguration StrongRotationDrives;
    }

    [Serializable]
    public class ArticulationDriveConfiguration
    {
        public float Stiffness = 10000f;
        public float Damping = 1f;
        public float ForceLimit = 1000f;
    }
}

