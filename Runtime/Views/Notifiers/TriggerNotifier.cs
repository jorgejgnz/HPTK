using HPTK.Components;
using HPTK.Views.Events;
using HPTK.Views.Notifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace HPTK.Views.Notifiers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerNotifier : MonoBehaviour
    {
        [Header("Colliders")]
        [SerializeField]
        public ColliderEvent onEnterCollider;
        [SerializeField]
        public ColliderEvent onStayCollider;
        [SerializeField]
        public ColliderEvent onExitCollider;

        [Header("Rigidbodies")]
        [SerializeField]
        public RigidbodyEvent onEnterRigidbody;
        [SerializeField]
        public RigidbodyEvent onExitRigidbody;

        [Header("Control")]
        public Transform ignoreChildren;

        [Header("Debugging")]
        [SerializeField]
        protected List<Collider> currentColliders = new List<Collider>();
        [SerializeField]
        protected List<Rigidbody> currentRigidbodies = new List<Rigidbody>();

        Collider[] tempColliders;
        Rigidbody[] tempRbs;

        protected bool ready = false;

        private void Start()
        {
            ready = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!ready || (ignoreChildren && other.transform.IsChildOf(ignoreChildren)))
                return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb && !currentRigidbodies.Contains(rb))
            {
                currentRigidbodies.Add(rb);
                onEnterRigidbody.Invoke(rb);
            }

            if (!currentColliders.Contains(other))
            {
                currentColliders.Add(other);
                onEnterCollider.Invoke(other);
            }
        }

        /*
        protected virtual void OnTriggerStay(Collider other)
        {
            if (!ready || (ignoreChildren && other.transform.IsChildOf(ignoreChildren)))
                return;

            onStayCollider.Invoke(other);
        }
        */

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!ready || (ignoreChildren && other.transform.IsChildOf(ignoreChildren)))
                return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb && currentRigidbodies.Contains(rb))
            {
                currentRigidbodies.Remove(rb);
                onExitRigidbody.Invoke(rb);
            }

            if (currentColliders.Contains(other))
            {
                currentColliders.Remove(other);
                onExitCollider.Invoke(other);
            }
        }

        private void Update()
        {
            tempRbs = currentRigidbodies.ToArray();
            for (int i = 0; i < tempRbs.Length; i++)
            {
                if (tempRbs[i] == null || !tempRbs[i].gameObject.activeInHierarchy)
                {
                    onExitRigidbody.Invoke(tempRbs[i]);
                    currentRigidbodies.Remove(tempRbs[i]);
                }
            }

            tempColliders = currentColliders.ToArray();
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
