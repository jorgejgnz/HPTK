using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Physics.Notifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pheasy : MonoBehaviour
    {
        public static List<Pheasy> registry = new List<Pheasy>();

        public Rigidbody rb;

        [Header("Center of Mass")]
        public Transform centerOfMass;

        [Header("Targets")]
        public List<TargetConstraint> targets = new List<TargetConstraint>();

        [Header("Colliders")]
        public List<Collider> colliders = new List<Collider>();

        [Header("Notifiers")]
        public CollisionNotifier collisionNotifier;
        public TriggerNotifier[] triggerNotifiers;

        [Header("Stability")]
        public bool safeMode = true;
        public bool gradualMode = true;
        public float maxVelocity = 20.0f;
        public float maxAngularVelocity = 12.5f;
        public float maxDepenetrationVelocity = 1.0f;
        public float maxErrorAllowed = 0.5f;

        [Header("Connections")]
        public bool ignoreCollisionsOnStart = false;
        public List<Pheasy> relations = new List<Pheasy>();

        [Header("Events")]
        public UnityEvent onInit = new UnityEvent();

        [Header("Debug")]
        public GameObject axis;
        public float axisScale = 0.02f;

        [Tooltip("Set false for better performance")]
        public bool editMode = true;

        public List<Collider> ignoring = new List<Collider>();

        Vector3 comPosition;

        TriggerNotifier triggerNotifier;
        List<Collider> myColliders = new List<Collider>();
        List<TriggerNotifier> myTriggerNotifiers = new List<TriggerNotifier>();
        Collider[] totalColliders;

        List<Pheasy> meAndMyRels = new List<Pheasy>();
        List<Pheasy> otherAndItsRels = new List<Pheasy>();

        private void Awake()
        {
            registry.Add(this);

            rb = GetComponent<Rigidbody>();

            if (!centerOfMass)
            {
                centerOfMass = new GameObject("CenterOfMass").transform;
                centerOfMass.transform.parent = rb.transform;
                centerOfMass.transform.localPosition = rb.centerOfMass;
            }

            for (int t = 0; t < targets.Count; t++)
            {
                if (targets[t].enabled) EnableTarget(targets[t]);
            }

            if (!collisionNotifier) collisionNotifier = rb.GetComponent<CollisionNotifier>();
            if (!collisionNotifier) collisionNotifier = rb.gameObject.AddComponent<CollisionNotifier>();

            UpdateCollidersAndTriggerNotifiers();

            ignoring.Clear();

            onInit.Invoke();
        }

        private void Start()
        {
            // After related pheasies finished gathering their colliders
            if (ignoreCollisionsOnStart)
            {
                List<Pheasy> processed = new List<Pheasy>();
                relations.ForEach(r => IgnoreCollisions(r, true, true));
            } 
        }

        private void Update()
        {
            if (centerOfMass)
            {
                comPosition = rb.transform.InverseTransformPoint(centerOfMass.position);
                if (rb.centerOfMass != comPosition) rb.centerOfMass = comPosition;
            }

            for (int t = 0; t < targets.Count; t++)
            {
                if (targets[t].enabled && !TargetIsValid(targets[t]))
                    targets[t].enabled = false;

                UpdateTarget(targets[t]);
            }
        }

        private void FixedUpdate()
        {
            for (int t = 0; t < targets.Count; t++)
            {
                FixedUpdateJoint(targets[t]);
            }

            if (safeMode)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularVelocity);
                rb.maxDepenetrationVelocity = maxDepenetrationVelocity;
            }
        }

        void FixedUpdateJoint(TargetConstraint t)
        {
            if (editMode && t.joint)
            {
                // Anchor
                t.joint.anchor = t.joint.transform.InverseTransformPoint(t.anchor.position);

                // Axis
                t.axis.position = t.connectedAnchor.position;
                t.axis.rotation = t.GetJointAxisWorldRotation();
            }
        }

        void UpdateJoint(TargetConstraint t, bool editMode)
        {
            if (editMode)
            {
                // Connected body/anchor
                if (t.connectedBody) t.joint.connectedBody = t.connectedBody;
                else t.joint.connectedBody = null;

                // Freedom
                SetPositionLock(t.joint, t.settings.linearMotion);
                SetRotationLock(t.joint, t.settings.angularMotion);

                // Drives
                if (gradualMode)
                {
                    PhysHelpers.UpdateJointMotionDrive(t.joint, PhysHelpers.JointDriveLerp(new JointDrive(), t.settings.motionDrive.toJointDrive(), t.gradualLerp));
                    PhysHelpers.UpdateJointAngularDrive(t.joint, PhysHelpers.JointDriveLerp(new JointDrive(), t.settings.angularDrive.toJointDrive(), t.gradualLerp));
                }
                else
                {
                    PhysHelpers.UpdateJointMotionDrive(t.joint, t.settings.motionDrive.toJointDrive());
                    PhysHelpers.UpdateJointAngularDrive(t.joint, t.settings.angularDrive.toJointDrive());
                }

                // Control
                t.joint.enableCollision = t.settings.collideWithConnectedRb;

                // Prevent modification
                t.joint.configuredInWorldSpace = !t.keepAxisRelativeToObject;
            }

            if (!t.setAxisWhenEnabled) t.joint.autoConfigureConnectedAnchor = false; // Called every frame

            // Connected body/anchor
            if (t.connectedBody) t.tmpConnAnchor = t.joint.connectedBody.transform.InverseTransformPoint(t.connectedAnchor.position);
            else t.tmpConnAnchor = t.connectedAnchor.position;

            if (t.joint.connectedAnchor != t.tmpConnAnchor) t.joint.connectedAnchor = t.tmpConnAnchor;

            // Target rotation
            t.jointFrameRotation = t.GetJointAxisWorldRotation();

            Quaternion resultRotation = Quaternion.Inverse(t.jointFrameRotation);
            resultRotation *= t.anchor.rotation;
            resultRotation *= Quaternion.Inverse(t.connectedAnchor.rotation);
            resultRotation *= t.jointFrameRotation;

            t.joint.targetRotation = resultRotation;

            // Target position
            if (t.targetPosition)
            {
                t.tmpTargetPos = -1.0f * t.axis.InverseTransformPoint(t.targetPosition.position);
                if (t.connectedBody) t.joint.targetPosition = Vector3.Scale(t.tmpTargetPos, t.connectedBody.transform.localScale);
                else t.joint.targetPosition = t.tmpTargetPos;
            }
            else if (t.joint.targetPosition != Vector3.zero)
            {
                t.joint.targetPosition = Vector3.zero;
            }

            // Update flag
            if (!t.updated) t.updated = true;

            // Debug
            if (t.showAxis) Debug.DrawLine(t.axis.position, t.axis.position + t.GetJointAxisWorldRotation() * Vector3.forward * 0.5f, Color.blue);
        }

        void UpdateTarget(TargetConstraint t)
        {
            // Enable / Disable
            if (t.enabled && !t.joint)
                EnableTarget(t);
            else if (!t.enabled && t.joint)
                DisableTarget(t);

            // Update settings
            if (t.enabled)
            {
                // Lerp
                if (t.gradualLerp < 1.0f)
                {
                    t.gradualLerp += Time.deltaTime / t.gradualMaxTime;
                    if (t.gradualLerp >= 1.0f) t.gradualLerp = 1.0f;
                }

                // Joint
                UpdateJoint(t, editMode || (!editMode && !t.updated));

                // Extensibility
                for (int e = 0; e < t.extensions.Count; e++)
                {
                    t.extensions[e].UpdateExtension(t);
                }

                // Force
                if (t.force) TeleportToDestination(t);

                // Error
                t.error = Vector3.Distance(t.anchor.position, t.connectedAnchor.position);

                if (t.error < 0.00001f) t.error = 0.0f;
            }

            // Stability
            if (safeMode && t.error > maxErrorAllowed)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Axis
            if (t.anchorAxis)
            {
                if (t.anchorAxis.activeSelf != t.showAnchor) t.anchorAxis.SetActive(t.showAnchor);
                if (t.showAnchor) UpdateAxis(t.anchorAxis.transform, t.anchor, axisScale);
            }

            if (t.connAnchorAxis)
            {
                if (t.connAnchorAxis.activeSelf != t.showConnAcnhor) t.connAnchorAxis.SetActive(t.showConnAcnhor);
                if (t.showConnAcnhor) UpdateAxis(t.connAnchorAxis.transform, t.connectedAnchor, axisScale);
            }
        }

        void EnableTarget(TargetConstraint t)
        {
            if (!t.pheasy) t.pheasy = this;
            
            if (!t.joint) InstantiateJoint(t);

            if (!t.anchor) InstantiateAnchor(t);

            if (!t.connectedAnchor) InstantiateConnectedAnchor(t);

            if (t.name == null || t.name == "") t.name = t.anchor.name + "->" + t.connectedAnchor.name;

            if (t.keepAxisRelativeToObject)
            {
                t.joint.configuredInWorldSpace = false;
            }
            else
            {
                t.joint.configuredInWorldSpace = true;
            }

            if (!t.axis)
            {
                t.setAxisWhenEnabled = false;

                InstantiateAxis(t);
            }
            else
            {
                t.setAxisWhenEnabled = true;

                t.SetAxis(t.axis.rotation);
            }

            if (t.connectedBody)
            {
                t.joint.connectedBody = t.connectedBody;
                t.initialLocalRotation = t.joint.connectedBody.transform.localRotation;
            }

            t.initialWorldRotation = t.joint.transform.rotation;

            t.jointFrameRotation = t.GetJointAxisWorldRotation();

            if (axis != null)
            {
                if (!t.anchorAxis)
                {
                    t.anchorAxis = Instantiate(axis, rb.transform);
                    UpdateAxis(t.anchorAxis.transform, t.anchor, axisScale);
                }

                if (!t.connAnchorAxis)
                {
                    t.connAnchorAxis = Instantiate(axis, rb.transform);
                    UpdateAxis(t.connAnchorAxis.transform, t.connectedAnchor, axisScale);
                }
            }

            t.gradualLerp = 0.0f;

            t.extensions.ForEach(e => e.InitExtension(t));

            t.enabled = true;
        }

        void DisableTarget(TargetConstraint t)
        {
            Destroy(t.joint);

            t.enabled = false;
        }

        bool TargetIsValid(TargetConstraint t)
        {
            if (!t.connectedAnchor) InstantiateConnectedAnchor(t);

            if (t.connectedBody && !t.connectedAnchor.IsChildOf(t.connectedBody.transform))
            {
                Debug.LogWarning("Target " + t.name + " is not valid! Conn anchor is not child of connBody");

                return false;
            }

            return true;
        }

        void InstantiateJoint(TargetConstraint t)
        {
            if (t.joint)
                return;

            t.joint = gameObject.AddComponent<ConfigurableJoint>();

            t.joint.autoConfigureConnectedAnchor = false;
            t.joint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        void InstantiateAnchor(TargetConstraint t)
        {
            if (t.anchor)
                return;

            Transform anchor = new GameObject(t.pheasy.name + ".Anchor").transform;

            anchor.position = t.pheasy.transform.position;
            anchor.rotation = t.pheasy.transform.rotation;

            anchor.parent = t.pheasy.transform;

            t.anchor = anchor;
        }

        void InstantiateConnectedAnchor(TargetConstraint t)
        {
            if (t.connectedAnchor)
                return;

            Transform connectedAnchor = new GameObject(t.pheasy.name + ".Goal").transform;

            connectedAnchor.position = t.joint.transform.position;
            connectedAnchor.rotation = t.joint.transform.rotation;

            if (t.connectedBody)
                connectedAnchor.parent = t.connectedBody.transform;
            else
                connectedAnchor.parent = t.joint.transform.parent;

            t.connectedAnchor = connectedAnchor;
        }

        void InstantiateAxis(TargetConstraint t)
        {
            if (t.axis)
                return;

            Transform axis = new GameObject(t.name + ".Axis").transform;

            axis.position = t.connectedAnchor.position;
            axis.rotation = t.GetJointAxisWorldRotation();

            axis.parent = t.connectedAnchor.parent;

            t.axis = axis;
        }

        // Public functions

        public void UpdateCollidersAndTriggerNotifiers()
        {
            myColliders.Clear();
            myTriggerNotifiers.Clear();
            totalColliders = rb.GetComponentsInChildren<Collider>();
            
            for (int c = 0; c < totalColliders.Length; c++)
            {
                if (totalColliders[c].attachedRigidbody == rb)
                {
                    myColliders.Add(totalColliders[c]);

                    triggerNotifier = totalColliders[c].transform.GetComponent<TriggerNotifier>();
                    if (triggerNotifier != null) myTriggerNotifiers.Add(triggerNotifier);
                }
            }
            colliders = myColliders;
            triggerNotifiers = myTriggerNotifiers.ToArray();
        }

        public void TeleportToDestination(TargetConstraint t)
        {
            // Rotate transform until anchor and connAnchor have the same world rotation
            Quaternion anchorToDestination = Quaternion.Inverse(t.anchor.rotation) * t.connectedAnchor.rotation;
            rb.transform.rotation *= anchorToDestination;

            // Move transform until anchor and connAnchor have the same world position
            Vector3 displacement = t.connectedAnchor.position - t.anchor.position;
            rb.transform.position += displacement;
        }

        public TargetConstraint AddTargetConstraint(string name, Rigidbody connectedBody, bool keepAxisRelativeWithObject, Transform setAxisOnEnable)
        {
            TargetConstraint newTarget = new TargetConstraint(this, name);

            newTarget.connectedBody = connectedBody;

            newTarget.keepAxisRelativeToObject = keepAxisRelativeWithObject;

            if (setAxisOnEnable)
            {
                newTarget.axis = setAxisOnEnable;
                newTarget.setAxisWhenEnabled = true;
            }
            else
            {
                newTarget.setAxisWhenEnabled = false;
            }

            EnableTarget(newTarget);

            targets.Add(newTarget);

            return newTarget;
        }

        public void RemoveTargetConstraint(TargetConstraint t)
        {
            if (targets.Contains(t))
            {
                DisableTarget(t);
                targets.Remove(t);
            }
        }

        public void IgnoreCollisions(Collider collider, bool ignore)
        {
            if (ignore && !ignoring.Contains(collider)) ignoring.Add(collider);
            else if (!ignore && ignoring.Contains(collider)) ignoring.Remove(collider);

            colliders.ForEach(c => UnityEngine.Physics.IgnoreCollision(c, collider, ignore));
        }

        public void FindMeAndRelated(List<Pheasy> related)
        {
            if (related.Contains(this))
                return;

            related.Add(this);

            relations.ForEach(p => p.FindMeAndRelated(related));
        }

        public void IgnoreCollisions(Pheasy other, bool ignore, bool includeRelated)
        {
            if (includeRelated)
            {
                meAndMyRels.Clear();
                FindMeAndRelated(meAndMyRels);

                otherAndItsRels.Clear();
                other.FindMeAndRelated(otherAndItsRels);

                for (int i = 0; i < meAndMyRels.Count; i++)
                {
                    for (int j = 0; j < otherAndItsRels.Count; j++)
                    {
                        meAndMyRels[i].IgnoreCollisions(otherAndItsRels[j], ignore, false);
                    }
                }
            }
            else other.colliders.ForEach(c => IgnoreCollisions(c, ignore));
        }

        public void StopLinearMovement()
        {
            rb.velocity = Vector3.zero;
        }

        public void StopAngularMovement()
        {
            rb.angularVelocity = Vector3.zero;
        }

        // Helpearizable functions

        static void SetCenterOfMass(Rigidbody rb, Transform com)
        {
            rb.centerOfMass = rb.transform.InverseTransformPoint(com.position);
        }

        static void UpdateAxis(Transform t, Transform parent, float axisScale)
        {
            if (t.parent != parent)  t.parent = parent;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.SetGlobalScale(Vector3.one * axisScale);
        }

        static void SetPositionLock(ConfigurableJoint joint, ConfigurableJointMotion motion)
        {
            joint.xMotion = joint.yMotion = joint.zMotion = motion;
        }

        static void SetRotationLock(ConfigurableJoint joint, ConfigurableJointMotion motion)
        {
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = motion;
        }
    }
}