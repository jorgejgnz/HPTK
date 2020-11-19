using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HPTK.Settings
{
    [Serializable]
    public class CustomJointDrive
    {
        public static CustomJointDrive zero = new CustomJointDrive();

        public float spring;
        public float damper;
        public float maxForce;

        public JointDrive toJointDrive()
        {
            JointDrive jDrive = new JointDrive();
            jDrive.positionSpring = spring;
            jDrive.positionDamper = damper;
            jDrive.maximumForce = maxForce;

            return jDrive;
        }

        public SoftJointLimitSpring toSoftJointLimitSpring()
        {
            SoftJointLimitSpring lSpring = new SoftJointLimitSpring();
            lSpring.spring = spring;
            lSpring.damper = damper;

            return lSpring;
        }

        public CustomJointDrive()
        {
            spring = 0.0f;
            damper = 0.0f;
            maxForce = 0.0f;
        }

        public CustomJointDrive(float spring, float damper, float maxForce)
        {
            this.spring = spring;
            this.damper = damper;
            this.maxForce = maxForce;
        }

    }

    [Serializable]
    public class SlaveBoneConfiguration
    {
        [Header("Rotation")]
        public bool followsRotation = true;
        public float maxAngularVelocity = 2.0f;
        public bool clampAngularVelocity = true;

        [Header("Target rotation")]
        public CustomJointDrive rotationDrive;
        public bool useDynamicStrength = false;
        public CustomJointDrive minDynamicRotDrive;
        public CustomJointDrive maxDynamicRotDrive;

        [Header("Position")]
        public bool followsPosition = false;
        public bool useTargetPos = false;
        public float maxLinearVelocity = 5.0f;
        public bool clampLinearVelocity = true;

        [Header("Target position")]
        public CustomJointDrive positionDrive;

        [Header("Rigidbody")]
        public float rigidbodyMass = 0.1f;
        public float rigidbodyDrag = 0.0f;
        public float rigidbodyAngularDrag = 0.0f;
        public bool useGravity = false;

        [Header("Joint")]
        public float jointMassScale = 1.6f;
        public float jointConnectedMassScale = 1.0f;

        public SlaveBoneConfiguration(SlaveBoneConfiguration conf)
        {
            followsRotation = conf.followsRotation;
            maxAngularVelocity = conf.maxAngularVelocity;

            rotationDrive = conf.rotationDrive;
            useDynamicStrength = conf.useDynamicStrength;
            minDynamicRotDrive = conf.minDynamicRotDrive;
            maxDynamicRotDrive = conf.maxDynamicRotDrive;

            followsPosition = conf.followsPosition;
            useTargetPos = conf.useTargetPos;
            maxLinearVelocity = conf.maxLinearVelocity;

            positionDrive = conf.positionDrive;

            rigidbodyMass = conf.rigidbodyMass;
            rigidbodyDrag = conf.rigidbodyDrag;
            useGravity = conf.useGravity;

            jointMassScale = conf.jointMassScale;
            jointConnectedMassScale = conf.jointConnectedMassScale;
        }
    }

    [CreateAssetMenu(menuName = "HPTK/HandPhysicsConfiguration Asset", order = 2)]
    public class HandPhysicsConfiguration : ScriptableObject
    {
        public string alias;

        public SlaveBoneConfiguration fingers;
        public SlaveBoneConfiguration wrist;
        public SlaveBoneConfiguration specials;
    }

}
