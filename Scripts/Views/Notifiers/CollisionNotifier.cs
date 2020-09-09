using HPTK.Views.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Notifiers
{
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionNotifier : HPTKElement
    {
        public RigidbodyEvent onRbEnter = new RigidbodyEvent();
        // public RigidbodyEvent onRbStay = new RigidbodyEvent();
        public RigidbodyEvent onRbExit = new RigidbodyEvent();

        private void Start()
        {
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody)
                onRbEnter.Invoke(collision.rigidbody);
        }

        // Disabled to improve performance. It is not being used and it's causing event flood
        /*
        private void OnCollisionStay(Collision collision)
        {
            if (collision.rigidbody)
                onRbStay.Invoke(collision.rigidbody);
        }
        */

        private void OnCollisionExit(Collision collision)
        {
            if (collision.rigidbody)
                onRbExit.Invoke(collision.rigidbody);
        }
    }
}
