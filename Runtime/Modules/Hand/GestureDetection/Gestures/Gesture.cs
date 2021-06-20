using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.GestureDetection
{
    public abstract class Gesture : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float activeIfGreaterThan = 0.8f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        protected float _lerp;
        public float lerp { get { return _lerp; } }

        [SerializeField]
        [ReadOnly]
        protected bool _isActive;
        public bool isActive { get { return _isActive; } }

        [Header("Time")]

        [SerializeField]
        [ReadOnly]
        protected float speed;

        [SerializeField]
        [ReadOnly]
        protected float timeActive;

        [Header("Intention")]

        [SerializeField]
        protected bool _providesIntention = false;
        public bool providesIntention { get { return _providesIntention; } }

        [ReadOnly]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        protected float _intentionLerp;
        public float intentionLerp { get { return _intentionLerp; } }

        [SerializeField]
        [ReadOnly]
        protected float intentionTime;

        [SerializeField]
        [ReadOnly]
        protected bool _isIntentionallyActive;
        public bool isIntentionallyActive { get { return _isIntentionallyActive; } }

        [Header("Activation Events")]
        public UnityEvent onActivation = new UnityEvent();
        public UnityEvent onDeactivation = new UnityEvent();

        [Header("Intention Events")]
        public UnityEvent onIntendedActivation = new UnityEvent();
        public UnityEvent onIntendedDeactivation = new UnityEvent();

        protected float previousLerp;
        protected bool wasActive;
        protected bool wasIntentionallyActive;

        public virtual void InitGesture() { }

        public void UpdateGesture()
        {
            EarlyGestureUpdate();

            if (gameObject.activeSelf) LerpUpdate();

            LateGestureUpdate();
        }

        public virtual void EarlyGestureUpdate()
        {
            // Backup previous values
            wasActive = isActive;

            // Speed
            speed = (lerp - previousLerp) / Time.deltaTime;

            // Activation
            if (lerp > activeIfGreaterThan && !isActive)
            {
                _isActive = true;
            }
            else if (lerp <= activeIfGreaterThan && isActive)
            {
                _isActive = false;
            }

            // Time-related
            if (isActive)
                timeActive += Time.deltaTime;
            else if (timeActive != 0.0f)
                timeActive = 0.0f;

            if (!gameObject.activeSelf)
            {
                _lerp = 0.0f;
                return;
            }
        }

        public virtual void LerpUpdate() { }

        public virtual void LateGestureUpdate()
        {
            // Activation events
            if (isActive && !wasActive)
                onActivation.Invoke();
            else if (!isActive && wasActive)
                onDeactivation.Invoke();
        }
    }
}
