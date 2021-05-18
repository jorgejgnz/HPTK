using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Physics
{
    [Serializable]
    public class CustomJointDrive
    {
        public static CustomJointDrive zero = new CustomJointDrive();
        public static CustomJointDrive max = new CustomJointDrive(1000000.0f, 1.0f, 1000.0f);

        public float spring;
        public float damper;
        public float maxForce;

        public static CustomJointDrive ScaledCopy(CustomJointDrive normalizedDrive, float scale)
        {
            CustomJointDrive drive = new CustomJointDrive();
            drive.spring = normalizedDrive.spring; // * scale;
            drive.damper = normalizedDrive.damper; // * scale;
            drive.maxForce = normalizedDrive.maxForce * scale;

            return drive;
        }

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

        public CustomJointDrive(CustomJointDrive drive)
        {
            this.spring = drive.spring;
            this.damper = drive.damper;
            this.maxForce = drive.maxForce;
        }

    }
}
