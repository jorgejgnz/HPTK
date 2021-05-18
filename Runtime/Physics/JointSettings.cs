using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Physics
{
    [Serializable]
    public class JointSettings
    {
        [Header("Freedom")]
        public ConfigurableJointMotion linearMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion angularMotion = ConfigurableJointMotion.Free;

        [Header("Drives")]
        public CustomJointDrive angularDrive;
        public CustomJointDrive motionDrive;

        [Header("Control")]
        public bool collideWithConnectedRb = false;

        public JointSettings(JointSettings values)
        {
            angularDrive = new CustomJointDrive(values.angularDrive);
            motionDrive = new CustomJointDrive(values.motionDrive);
        }

        public JointSettings()
        {
            angularDrive = new CustomJointDrive(CustomJointDrive.zero);
            motionDrive = new CustomJointDrive(CustomJointDrive.zero);
        }
    }
}
