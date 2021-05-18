using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Physics
{
    public class LinealConstraint : TargetExtension
    {
        [Header("Linear limit")]
        public Transform maxDistance;

        public float spring = 100.0f;
        public float damper = 1.0f;

        public bool showLimitedAxis = true;

        [Header("Threshold")]
        public bool detectThreshold = false;
        public bool inverted = false;
        [Range(0.0f, 1.0f)]
        public float threshold = 0.5f;
        public float minTimeBetweenDetections = 0.5f;
        public UnityEvent onOverThreshold = new UnityEvent();
        public UnityEvent onUnderThreshold = new UnityEvent();

        SoftJointLimit limit = new SoftJointLimit();
        SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();

        float togglingThresholdDetectionIn = 0.0f;

        ThresholdState thresholdState = ThresholdState.None;

        Vector3 worldLimitStart, worldLimitEnd;
        Vector3 closestToLine;
        float positionLerp;

        public sealed override void InitExtension(TargetConstraint t)
        {
            base.InitExtension(t);

            if (!maxDistance)
            {
                Debug.LogError("Missing reference to Max Distance");
                return;
            }

            t.axis.position = t.connectedAnchor.position;
            t.axis.LookAt(maxDistance);

            t.setAxisWhenEnabled = true;
            t.SetAxis(t.axis.rotation);

            limit.limit = Vector3.Distance(t.connectedAnchor.position, maxDistance.position);
            t.joint.linearLimit = limit;
        }

        public override sealed void UpdateExtension(TargetConstraint t)
        {
            // Linear limit
            t.joint.xMotion = ConfigurableJointMotion.Locked;
            t.joint.yMotion = ConfigurableJointMotion.Locked;
            t.joint.zMotion = ConfigurableJointMotion.Limited;

            limitSpring.spring = spring;
            limitSpring.damper = damper;
            t.joint.linearLimitSpring = limitSpring;

            worldLimitStart = t.connectedAnchor.position - t.GetJointAxisWorldRotation() * Vector3.forward * limit.limit;
            worldLimitEnd = t.connectedAnchor.position + t.GetJointAxisWorldRotation() * Vector3.forward * limit.limit;

            if (showLimitedAxis) Debug.DrawLine(worldLimitStart, worldLimitEnd, Color.black);

            // Threshold detection
            if (detectThreshold)
            {
                if (togglingThresholdDetectionIn <= 0.0f)
                {
                    closestToLine = BasicHelpers.NearestPointOnFiniteLine(worldLimitStart, worldLimitEnd, t.anchor.position);
                    
                    if (inverted) positionLerp = Vector3.Distance(worldLimitStart, closestToLine) / (limit.limit * 2.0f);
                    else positionLerp = Vector3.Distance(worldLimitEnd, closestToLine) / (limit.limit * 2.0f);

                    if (positionLerp > threshold && thresholdState != ThresholdState.Over)
                    {
                        togglingThresholdDetectionIn = minTimeBetweenDetections;
                        thresholdState = ThresholdState.Over;
                        onOverThreshold.Invoke();
                    }
                    else if (positionLerp < threshold && thresholdState != ThresholdState.Under)
                    {
                        togglingThresholdDetectionIn = minTimeBetweenDetections;
                        thresholdState = ThresholdState.Under;
                        onUnderThreshold.Invoke();
                    }
                }
                else togglingThresholdDetectionIn -= Time.deltaTime;
            }
        }
    }
}
