using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Physics
{
    public enum ThresholdState
    {
        None,
        Under,
        Over
    }

    public class AngularConstraint : TargetExtension
    {
        public float localRotZ;
        public ConfigurableJointMotion angularZMotion = ConfigurableJointMotion.Free;

        [Header("Threshold")]
        public bool detectThreshold = false;
        [Range(0.01f, 359.99f)]
        public float threshold = 0.0f;
        public float minTimeBetweenDetections = 0.2f;
        public ThresholdState thresholdState = ThresholdState.None;
        public UnityEvent onOverThreshold = new UnityEvent();
        public UnityEvent onUnderThreshold = new UnityEvent();

        [SerializeField]
        float togglingThresholdDetectionIn = 0.0f;

        [Header("Stucking")]
        public bool canStuck = false;
        [Range(1.0f, 5.0f)]
        public float minRandomDuration = 0.5f;
        [Range(1.0f, 5.0f)]
        public float maxRandomDuration = 1.0f;
        public float unstuckingTorque = 50.0f;
        public UnityEvent onStucked = new UnityEvent();
        public UnityEvent onUnstucked = new UnityEvent();

        [SerializeField]
        bool stucked = false;
        [SerializeField]
        float stuckingIn = 0.0f;
        [SerializeField]
        ConfigurableJoint breakableJoint = null;

        public override void InitExtension(TargetConstraint t)
        {
            base.InitExtension(t);
        }

        public override sealed void UpdateExtension(TargetConstraint t)
        {
            if (!t.joint)
            {
                Debug.LogWarning("Angular limit cannot be applied. Missing joint");
                return;
            }

            if (!t.axis)
            {
                Debug.LogWarning("Angular limit cannot be applied. Missing axis");
                return;
            }

            // Prevent modification
            t.joint.angularXMotion = ConfigurableJointMotion.Locked;
            t.joint.angularYMotion = ConfigurableJointMotion.Locked;
            t.joint.angularZMotion = angularZMotion;

            // Rotation
            localRotZ = GetLocalRotZ(t.joint.transform);

            // Threshold detection
            if (detectThreshold)
            {
                if (togglingThresholdDetectionIn <= 0.0f)
                {
                    if (localRotZ > threshold && thresholdState != ThresholdState.Over)
                    {
                        togglingThresholdDetectionIn = minTimeBetweenDetections;
                        thresholdState = ThresholdState.Over;
                        onOverThreshold.Invoke();
                    }
                    else if (localRotZ < threshold && thresholdState != ThresholdState.Under)
                    {
                        togglingThresholdDetectionIn = minTimeBetweenDetections;
                        thresholdState = ThresholdState.Under;
                        onUnderThreshold.Invoke();
                    }
                }
                else togglingThresholdDetectionIn -= Time.deltaTime;
            }

            // Stucking
            if (canStuck)
            {
                if (breakableJoint == null && stucked)
                {
                    SetStuck(false, t);
                    stuckingIn = Random.Range(minRandomDuration, maxRandomDuration);
                }

                if (!stucked && stuckingIn <= 0.0f) SetStuck(true, t);
                else stuckingIn -= Time.deltaTime;
            }
            else if (breakableJoint != null) SetStuck(false, t);
        }

        void SetStuck(bool stucked, TargetConstraint t)
        {
            this.stucked = stucked;

            if (!stucked && breakableJoint != null)
            {
                Destroy(breakableJoint);
            }
            else if (stucked && breakableJoint == null)
            {
                breakableJoint = t.pheasy.rb.gameObject.AddComponent<ConfigurableJoint>();
                breakableJoint.angularZMotion = ConfigurableJointMotion.Locked;
                breakableJoint.breakTorque = unstuckingTorque;
            }

            if (stucked) onStucked.Invoke();
            else onUnstucked.Invoke();
        }

        float GetLocalRotZ(Transform t)
        {
            float angle = t.localRotation.eulerAngles.z;

            if (angle >= 180.0f)
                angle -= 180.0f;
            else
                angle += 180.0f;

            return angle;
        }
    }
}
