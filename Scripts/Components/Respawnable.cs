using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class Respawnable : MonoBehaviour
    {
        [HideInInspector]
        public Rigidbody rb;

        [HideInInspector]
        public Vector3 initialPosition;

        [HideInInspector]
        public Quaternion initialRotation;

        public Rigidbody[] attachedRbs;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();

            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
    }
}
