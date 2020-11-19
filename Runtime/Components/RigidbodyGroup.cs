using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyGroup : MonoBehaviour
    {
        Rigidbody main;

        [HideInInspector]
        public Vector3 initialPosition;

        [HideInInspector]
        public Quaternion initialRotation;

        public Rigidbody[] rigidbodies;

        [Header("Control")]
        public bool ignoreCollisionsOnStart = true;

        [Header("Physics")]
        public bool applyValues = false;
        public float maxLinearVelocity = 100.0f;
        public float maxAngularVelocity = 100.0f;
        public float maxDepenetrationVelocity = 10.0f;

        private void Start()
        {       
            main = GetComponent<Rigidbody>();

            //if (includeThisRb)
            //{
                List<Rigidbody> totalRbs = new List<Rigidbody>(rigidbodies);
                totalRbs.Add(main);
                rigidbodies = totalRbs.ToArray();
            //}

            // Store initial conditions
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            // Ignore collisions between them if needed
            if (ignoreCollisionsOnStart)
                IgnoreCollisions(true);
        }

        private void FixedUpdate()
        {
            if (applyValues)
            {
                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    rigidbodies[i].velocity = Vector3.ClampMagnitude(rigidbodies[i].velocity, maxLinearVelocity);
                    rigidbodies[i].maxAngularVelocity = maxAngularVelocity;
                    rigidbodies[i].maxDepenetrationVelocity = maxDepenetrationVelocity;
                }
            }
        }

        void IgnoreCollisions(bool ignore)
        {
            Collider[] rbColliders;
            List<Collider> totalColliders = new List<Collider>();

            for (int rb = 0; rb < rigidbodies.Length; rb++)
            {
                rbColliders = rigidbodies[rb].GetComponents<Collider>();
                totalColliders.AddRange(rbColliders);

                rbColliders = rigidbodies[rb].GetComponentsInChildren<Collider>();
                totalColliders.AddRange(rbColliders);
            }

            for (int i = 0; i < totalColliders.Count; i++)
            {
                for (int j = 0; j < totalColliders.Count; j++)
                {
                    Physics.IgnoreCollision(totalColliders[i], totalColliders[j], ignore);
                }
            }
        }
    }
}
