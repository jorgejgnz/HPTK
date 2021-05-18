using HandPhysicsToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Physics.Notifiers
{
    [Serializable]
    public class CollisionEvent : UnityEvent<Collision> { }

    [RequireComponent(typeof(Rigidbody))]
    public class CollisionNotifier : HPTKElement
    {
        public bool invokeStayEvents = false;

        public CollisionEvent onCollisionEnter = new CollisionEvent();
        public CollisionEvent onCollisionStay = new CollisionEvent();
        public CollisionEvent onCollisionExit = new CollisionEvent();

        public CollisionEvent onRbEnter = new CollisionEvent();
        public CollisionEvent onRbStay = new CollisionEvent();
        public CollisionEvent onRbExit = new CollisionEvent();

        private void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter.Invoke(collision);
            if (collision.rigidbody) onRbEnter.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!invokeStayEvents)
                return;

            onCollisionStay.Invoke(collision);
            if (collision.rigidbody) onRbStay.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            onCollisionExit.Invoke(collision);
            if (collision.rigidbody) onRbExit.Invoke(collision);
        }
    }
}
