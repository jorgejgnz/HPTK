using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Helpers;

namespace HandPhysicsToolkit.Utils
{
    [ExecuteInEditMode]
    public class IndependentRotator : MonoBehaviour
    {
        public Quaternion rotation;

        public bool modifyRotation = false;

        List<Transform> overwritableChildren = new List<Transform>();

        private void Update()
        {
            if (modifyRotation) transform.RotateIndependently(rotation, overwritableChildren);
            else rotation = transform.rotation;
        }
    }
}
