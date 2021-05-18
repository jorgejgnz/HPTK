using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class ScaleLerper : MonoBehaviour
    {
        public Transform pivot;

        public SnapAxis axis;

        Vector3 newLocalScale;

        public void UpdateSize(float lerp)
        {
            newLocalScale = pivot.localScale;

            switch (axis)
            {
                case SnapAxis.X:
                    newLocalScale.x = lerp;
                    break;
                case SnapAxis.Y:
                    newLocalScale.y = lerp;
                    break;
                case SnapAxis.Z:
                    newLocalScale.z = lerp;
                    break;
                case SnapAxis.All:
                    newLocalScale = new Vector3(lerp, lerp, lerp);
                    break;
            }

            pivot.localScale = newLocalScale;
        }
    }
}
