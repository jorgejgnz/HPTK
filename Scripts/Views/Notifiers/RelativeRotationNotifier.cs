using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RelativeRotationNotifier : MonoBehaviour
{
    public Transform parent;
    public Transform child;

    public Quaternion relativeRotation;
    public Vector3 eulerRelativeRotation;

    public float LimitX = -1;
    public float LimitY = -1;
    public float LimitZ = -1;

    public UnityEvent onOverpass;
    public UnityEvent onUnderpass;

    bool overpassed = false;

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

        if (LimitX >= 0.0f)
        {
            if (eulerRelativeRotation.x > LimitX)
                SetOverpassed(true);
            else
                SetOverpassed(false);
        }

        if (LimitY >= 0.0f)
        {
            if (eulerRelativeRotation.y > LimitY)
                SetOverpassed(true);
            else
                SetOverpassed(false);
        }

        if (LimitZ >= 0.0f)
        {
            if (eulerRelativeRotation.z > LimitZ)
                SetOverpassed(true);
            else
                SetOverpassed(false);
        }
    }
    
    void SetOverpassed(bool newOverpassed)
    {
        if (overpassed == newOverpassed)
            return;

        overpassed = newOverpassed;

        if (newOverpassed)
            onOverpass.Invoke();
        else
            onUnderpass.Invoke();
    }
}
