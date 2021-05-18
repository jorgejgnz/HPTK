using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace HandPhysicsToolkit.Physics.Notifiers
{
    [Serializable]
    public class ColliderEvent : UnityEvent<Collider> { }

    [Serializable]
    public class RigidbodyEvent : UnityEvent<Rigidbody> { }

    [RequireComponent(typeof(Collider))]
    public class TriggerNotifier : MonoBehaviour
    {
        [Header("Colliders")]
        [SerializeField]
        public ColliderEvent onColliderEnter;
        [SerializeField]
        public ColliderEvent onColliderStay;
        [SerializeField]
        public ColliderEvent onColliderExit;

        [Header("Rigidbodies")]
        [SerializeField]
        public RigidbodyEvent onRbEnter;
        [SerializeField]
        public RigidbodyEvent onRbExit;

        [Header("Control")]
        public Transform ignoreChildren;

        [Header("Debugging")]
        public List<Collider> enteredColliders = new List<Collider>();
        public List<Rigidbody> enteredRbs = new List<Rigidbody>();

        Collider[] tempColliders;
        Rigidbody[] tempRbs;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (ignoreChildren && other.transform.IsChildOf(ignoreChildren))
                return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb && !enteredRbs.Contains(rb))
            {
                enteredRbs.Add(rb);
                onRbEnter.Invoke(rb);
            }

            if (!enteredColliders.Contains(other))
            {
                enteredColliders.Add(other);
                onColliderEnter.Invoke(other);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (ignoreChildren && other.transform.IsChildOf(ignoreChildren))
                return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb && enteredRbs.Contains(rb))
            {
                enteredRbs.Remove(rb);
                onRbExit.Invoke(rb);
            }

            if (enteredColliders.Contains(other))
            {
                enteredColliders.Remove(other);
                onColliderExit.Invoke(other);
            }
        }

        private void Update()
        {
            tempRbs = enteredRbs.ToArray();
            for (int i = 0; i < tempRbs.Length; i++)
            {
                if (tempRbs[i] == null || !tempRbs[i].gameObject.activeInHierarchy)
                {
                    onRbExit.Invoke(tempRbs[i]);
                    enteredRbs.Remove(tempRbs[i]);
                }
            }

            tempColliders = enteredColliders.ToArray();
            for (int i = 0; i < tempColliders.Length; i++)
            {
                if (tempColliders[i] == null || !tempColliders[i].gameObject.activeInHierarchy)
                {
                    OnTriggerExit(tempColliders[i]);
                }
            }
        }

    }
}
