using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Physics
{
    public class TargetExtension : MonoBehaviour
    {
        public virtual void InitExtension(TargetConstraint t)
        {
            if (!Valid(t)) return;
        }
        public virtual void UpdateExtension(TargetConstraint t)
        {
            if (!Valid(t)) return;
        }

        bool Valid(TargetConstraint t)
        {
            if (!t.joint)
            {
                Debug.LogWarning("Extension cannot be applied. Missing joint");
                return false;
            }

            if (!t.axis)
            {
                Debug.LogWarning("Extension cannot be applied. Missing axis");
                return false;
            }

            return true;
        }
    }
}
