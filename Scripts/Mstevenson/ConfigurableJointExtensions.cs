using UnityEngine;

/*
 * 
 * Modified script from https://github.com/mstevenson: https://gist.github.com/mstevenson/7b85893e8caf5ca034e6
 * 
 */

public static class ConfigurableJointExtensions
{
    /// <summary>
    /// Sets a joint's targetRotation to match a given local rotation.
    /// The joint transform's local rotation must be cached on Start and passed into this method.
    /// </summary>
    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion initialLocalRotation)
    {
        if (joint.configuredInWorldSpace)
        {
            Debug.LogError(joint.name + " SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
        }
        SetTargetRotationInternal(joint, targetLocalRotation, initialLocalRotation, Space.Self);
    }

    /// <summary>
    /// Sets a joint's targetRotation to match a given world rotation.
    /// The joint transform's world rotation must be cached on Start and passed into this method.
    /// </summary>
    public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion initialWorldRotation)
    {
        if (!joint.configuredInWorldSpace)
        {
            Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
        }
        SetTargetRotationInternal(joint, targetWorldRotation, initialWorldRotation, Space.World);
    }

    static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion initialRotation, Space space)
    {
        Quaternion worldToJointSpace = GetWorldToJointSpace(joint);

        // Transform into world space
        Quaternion jointSpaceToWorld = Quaternion.Inverse(worldToJointSpace);

        Quaternion resultRotation = GetWorldResultRotation(joint, targetRotation,initialRotation, space, jointSpaceToWorld);

        // Transform back into joint space
        resultRotation *= worldToJointSpace;

        // Set target rotation to our newly calculated rotation
        joint.targetRotation = resultRotation;
    }

    public static Quaternion GetWorldResultRotation(ConfigurableJoint joint, Quaternion targetRotation, Quaternion initialRotation, Space space, Quaternion jointSpaceToWorld)
    {
        Quaternion resultRotation;

        // Counter-rotate and apply the new local rotation.
        // Joint space is the inverse of world space, so we need to invert our value
        if (space == Space.World)
        {
            resultRotation = jointSpaceToWorld * initialRotation * Quaternion.Inverse(targetRotation);
        }
        else
        {
            resultRotation = jointSpaceToWorld * Quaternion.Inverse(targetRotation) * initialRotation;
        }

        // In world space
        return resultRotation;
    }

    public static Quaternion GetWorldToJointSpace(ConfigurableJoint joint)
    {
        // Calculate the rotation expressed by the joint's axis and secondary axis
        var right = joint.axis;
        var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        var up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        return worldToJointSpace;
    }

    /// <summary>
    /// Adjust ConfigurableJoint settings to closely match CharacterJoint behaviour
    /// </summary>
    public static void SetupAsCharacterJoint(this ConfigurableJoint joint)
    {
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        joint.rotationDriveMode = RotationDriveMode.Slerp;
        var slerpDrive = joint.slerpDrive;
        slerpDrive.mode = JointDriveMode.Position;
        slerpDrive.maximumForce = Mathf.Infinity;
        joint.slerpDrive = slerpDrive;
    }
}