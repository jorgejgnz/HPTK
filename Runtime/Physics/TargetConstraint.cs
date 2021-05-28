using HandPhysicsToolkit.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Physics
{
    [Serializable]
    public class TargetConstraint
    {
        [HideInInspector]
        public Pheasy pheasy;

        public bool enabled = true;

        public string name;

        public ConfigurableJoint joint;

        [Header("Connections")]
        [Tooltip("If null, use center of mass")]
        public Transform anchor;
        public Transform connectedAnchor;
        [Tooltip("Always in world space")]
        public Rigidbody connectedBody;

        [Header("Control")]
        public bool keepAxisRelativeToObject = true;
        public Transform axis;
        public Transform targetPosition;
        public JointSettings settings = new JointSettings();

        [Header("Stability")]
        public float gradualMaxTime = 1.0f;
        public float error = 0.0f;

        [Header("Extensibility")]
        public List<TargetExtension> extensions = new List<TargetExtension>();

        [Header("Debug")]
        public bool force = false;
        public bool showAxis = false;
        public bool showAnchor = false;
        public bool showConnAcnhor = false;

        [HideInInspector]
        public float gradualLerp = 0.0f;

        [HideInInspector]
        public Quaternion jointFrameRotation;

        [HideInInspector]
        public Quaternion initialWorldRotation;

        [HideInInspector]
        public Quaternion initialLocalRotation;

        [HideInInspector]
        public CustomJointDrive minAngularDrive = CustomJointDrive.zero;

        [HideInInspector]
        public CustomJointDrive minMotionDrive = CustomJointDrive.zero;

        [HideInInspector]
        public GameObject anchorAxis;

        [HideInInspector]
        public GameObject connAnchorAxis;

        [HideInInspector]
        public bool setAxisWhenEnabled = true;

        [HideInInspector]
        public Vector3 tmpConnAnchor;

        [HideInInspector]
        public Vector3 tmpTargetPos;

        [HideInInspector]
        public bool updated = false;

        Vector3 xAxis, yAxis, zAxis;

        public TargetConstraint(Pheasy pheasy, string name)
        {
            this.pheasy = pheasy;
            this.name = name;
        }

        public void Remove()
        {
            pheasy.RemoveTargetConstraint(this);
        }

        public void SetAxis(Quaternion worldRot)
        {
            if (joint.configuredInWorldSpace)
            {
                joint.axis = worldRot * Vector3.right;
                joint.secondaryAxis = worldRot * Vector3.up;
            }
            else
            {
                Quaternion relativeAxis = Quaternion.Inverse(joint.transform.rotation) * worldRot;
                joint.axis = relativeAxis * Vector3.right;
                joint.secondaryAxis = relativeAxis * Vector3.up;
            }
        }
        public Quaternion GetJointAxisWorldRotation()
        {
            xAxis = joint.axis;
            zAxis = Vector3.Cross(joint.axis, joint.secondaryAxis);
            yAxis = Vector3.Cross(zAxis, xAxis);

            Quaternion axisRot = Quaternion.LookRotation(zAxis, yAxis);

            if (joint.configuredInWorldSpace) return axisRot;
            else return joint.transform.rotation * axisRot;
        }
    }
}