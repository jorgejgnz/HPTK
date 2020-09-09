using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RelativeRotationDebugger : MonoBehaviour
{
    public Transform parent;
    public Transform child;

    public Quaternion relativeRotation;
    public Vector3 eulerRelativeRotation;

    void Update()
    {
        if (!parent || !child)
            return;

        relativeRotation = Quaternion.Inverse(parent.rotation) * child.rotation;
        eulerRelativeRotation = relativeRotation.eulerAngles;

        if (eulerRelativeRotation.x > 180.0f)
            eulerRelativeRotation.x -= 360.0f;
        if (eulerRelativeRotation.y > 180.0f)
            eulerRelativeRotation.y -= 360.0f;
        if (eulerRelativeRotation.z > 180.0f)
            eulerRelativeRotation.z -= 360.0f;
    }
}
