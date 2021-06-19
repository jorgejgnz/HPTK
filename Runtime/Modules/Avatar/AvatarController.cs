using HandPhysicsToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(AvatarModel))]
    public class AvatarController : HPTKController
    {
        [ReadOnly]
        public AvatarModel model;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<AvatarModel>();
            SetGeneric(model.view, model);
        }

        private void Start()
        {
            if (!HPTK.core)
            {
                Debug.LogError("Please add the HPTK singleton to the scene");
                return;
            }

            HPTK.core.AddAvatar(this);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();

            model.bodies.ForEach(r => StartBody(r));

            // Start proxy-body related controllers
            model.registry.ForEach((c) => c.ControllerStart());

            model.view.onStarted.Invoke();

            model.ready = true;
        }

        void StartBody(BodyModel body)
        {
            body.parts.Clear();

            StartPart(body.root);

            body.registry.ForEach((c) => { if (c.gameObject.activeSelf) c.ControllerStart(); });
        }

        void StartPart(PartModel part)
        {
            if (part.body.parts.Contains(part))
                return;

            part.body.parts.Add(part);

            if (part == part.body.root && part.root.parent)
            {
                // Main root bone should have no parents
                part.root.parent.children.Remove(part.root);
                part.root.parent = null;

                Debug.LogWarning("Root bone of root part should have no parent. Parent removed for bone " + part.root.name);
            }
            else if (part.parent != null)
            {
                // Move to the upper element in hierarchy until you reach a bone or body.root
                BoneModel upperBone = null;
                PartModel upperPart = null;
                Transform tsf = part.transform;
                int siblingIndex;

                for (int i = 0; i < 50; i++)
                {
                    if (upperBone != null || upperPart == part.body.root)
                        break;

                    siblingIndex = tsf.GetSiblingIndex();

                    if (siblingIndex > 0) tsf = tsf.parent.GetChild(siblingIndex - 1);
                    else tsf = tsf.parent;

                    upperBone = tsf.GetComponent<BoneModel>();
                    upperPart = tsf.GetComponent<PartModel>();
                }

                // Connect part.root.parent with some part.parent.bones
                if (upperBone)
                {
                    part.root.parent = upperBone;
                    if (!upperBone.children.Contains(part.root)) upperBone.children.Add(part.root);
                }
                else
                {
                    Debug.LogWarning("Part " + part.name + " has a parent but any upper bone was found to which connect its root bone " + part.root.name);
                }
            }
            else if (part != part.body.root)
            {
                Debug.LogWarning(part.name + " has no parent but it's not the root part of " + part.body.name + ". The root part of that body is " + part.body.root.name);
            }

            // Recursive
            part.parts.ForEach(p =>
            {
                if (p.gameObject.activeSelf)
                {
                    p.body = part.body;
                    StartPart(p);
                }
            });

            if (part is HandModel) HandStart(part as HandModel);
            if (part is FingerModel) FingerStart(part as FingerModel);

            part.registry.ForEach((c) => { if (c.gameObject.activeSelf) c.ControllerStart(); });

            // Scaling
            UpdateScale(part);
        }

        public override void ControllerUpdate()
        {
            // Update body related controllers
            model.bodies.ForEach(r => UpdateBody(r));

            // Update registered controllers

            if (HPTK.core.controlsUpdateCalls) model.registry.ForEach((c) => { if (c.gameObject.activeSelf) c.ControllerUpdate(); });
        }

        void UpdateBody(BodyModel body)
        {
            if (body.followCamera && !body.moveThisAsHead && body.head) body.moveThisAsHead = body.head.transformRef;

            if (body.replicatedTsf && !body.referenceTsf) body.referenceTsf = HPTK.core.trackingSpace;

            if (body.followCamera && body.moveThisAsHead && HPTK.core.trackedCamera)
            {
                if (body.referenceTsf && body.replicatedTsf)
                {
                    Vector3 relPos = body.referenceTsf.InverseTransformPoint(HPTK.core.trackedCamera.position);
                    Quaternion relRot = Quaternion.Inverse(body.referenceTsf.rotation) * HPTK.core.trackedCamera.rotation;

                    body.moveThisAsHead.position = body.replicatedTsf.TransformPoint(relPos);
                    body.moveThisAsHead.rotation = body.replicatedTsf.rotation * relRot;
                }
                else
                {
                    body.moveThisAsHead.position = HPTK.core.trackedCamera.position;
                    body.moveThisAsHead.rotation = HPTK.core.trackedCamera.rotation;
                }
            }

            body.parts.ForEach(p => { if (p.gameObject.activeSelf) UpdatePart(p); });

            // Update registered controllers

            if (HPTK.core.controlsUpdateCalls) body.registry.ForEach((c) => { if (c.gameObject.activeSelf) c.ControllerUpdate(); });
        }

        void UpdatePart(PartModel part)
        {
            if (part is HandModel) UpdateHand(part as HandModel);
            if (part is FingerModel) UpdateFinger(part as FingerModel);

            part.bones.ForEach(b => { if (b.gameObject.activeSelf) UpdateBone(b); });

            // Update registered controllers

            if (HPTK.core.controlsUpdateCalls) part.registry.ForEach((c) => { if (c.gameObject.activeSelf) c.ControllerUpdate(); });
        }

        void UpdateHand(HandModel h)
        {
            if (h.ray)
            {
                BoneModel shoulderBone;
                if (h.side == Side.Left) shoulderBone = h.body.leftArm.shoulder;
                else shoulderBone = h.body.rightArm.shoulder;
                PointModel shoulder = null;
                if (shoulderBone) shoulder = shoulderBone.point;
                Vector3 rayDir;
                foreach (KeyValuePair<string, ReprModel> rayRepr in h.ray.reprs)
                {
                    rayDir = Vector3.zero;

                    // Find ray forward
                    if (shoulder && shoulder.reprs.ContainsKey(rayRepr.Key))
                    {
                        rayDir = (rayRepr.Value.transformRef.position - shoulder.reprs[rayRepr.Key].transformRef.position).normalized;
                    }
                    else if (h.palmNormal.reprs.ContainsKey(rayRepr.Key) && h.body.torso.head.reprs.ContainsKey(rayRepr.Key))
                    {
                        rayDir = Quaternion.Lerp(h.body.torso.head.reprs[rayRepr.Key].transformRef.rotation, h.palmNormal.reprs[rayRepr.Key].transformRef.rotation, 0.5f) * Vector3.forward;
                    }

                    // Apply rotation
                    if (rayDir != Vector3.zero) rayRepr.Value.transformRef.rotation = Quaternion.LookRotation(rayDir, h.palmCenter.reprs[rayRepr.Key].transformRef.up);

                    // Hide / Unhide
                    if (h.palmNormal.reprs.ContainsKey(rayRepr.Key) && h.palmNormal.reprs[rayRepr.Key].transformRef)
                    {
                        rayRepr.Value.transformRef.gameObject.SetActive(Vector3.Dot(h.palmNormal.reprs[rayRepr.Key].transformRef.forward, rayRepr.Value.transformRef.forward) > 0.0f);
                    }
                }
            }
        }

        void UpdateFinger(FingerModel f) { }

        void UpdateBone(BoneModel b) { }

        public float GetFingerLength(FingerModel finger)
        {
            if (!finger.knuckle || !finger.tip || !finger.tip.bone.reprs.ContainsKey(AvatarModel.key))
            {
                Debug.LogWarning("It was not possible to calculate length for finger " + finger.name);
                return 0.0f;
            }

            float length = 0.0f;

            BoneModel bone = finger.tip.bone;
            length += Vector3.Distance(finger.tip.transformRef.position, bone.transformRef.position);

            for (int i = 0; i < 50; i++)
            {
                if (!bone.parent || bone == finger.knuckle.bone)
                    break;

                length += Vector3.Distance(bone.transformRef.position, bone.parent.transformRef.position);

                bone = bone.parent;
            }

            return length;
        }

        public void UpdateScale(PartModel part)
        {
            if (part.scaleRepresentations)
            {
                if (part.root.point)
                {
                    foreach (KeyValuePair<string, ReprModel> entry in part.root.point.reprs)
                    {
                        Transform tsf = entry.Value.transformRef;
                        if (tsf) tsf.localScale = Vector3.one * part.totalScale;
                    }
                }
                else
                {
                    Debug.LogError("Part " + part.name + " cannot be scaled as its root, " + part.root.name + ", does not have any point");
                }
            }
            else
            {
                if (part.root.transformRef) part.root.transformRef.localScale = Vector3.one * part.totalScale;
            }

            if (part is HandModel) (part as HandModel).fingers.ForEach(f => f.length = GetFingerLength(f));
        }

        public void SetPartVisuals(PartModel part, string key, bool enabled)
        {
            if (!part.root.reprs.ContainsKey(key))
            {
                Debug.LogWarning("Part " + part.name + " root does not have a " + key + " representation");
                return;
            }

            SkinnedMeshRenderer smr = part.root.reprs[key].skinnedMeshRenderer;

            if (smr) smr.enabled = enabled;
            else Debug.LogWarning("Part " + part.name + " root does not have a SkinnedMeshRenderer referenced on its " + key + " representation");
        }

        public void SetHandPhysics(HandModel hand, bool enabled)
        {
            PuppetView view = BasicHelpers.FindFirst<HPTKView, PuppetView>(hand.view.registry);
            if (view) view.SetPhysics(enabled);
        }

        public void HandStart(HandModel hand)
        {
            hand.part = HumanBodyPart.Hand;

            if (hand == hand.body.leftHand)
                hand.side = Side.Left;
            else if (hand == hand.body.rightHand)
                hand.side = Side.Right;
            else
                hand.side = Side.None;

            if (hand.wrist)
            {
                if (hand.side == Side.Left)
                    hand.wrist.humanBodyBone = HumanBodyBones.LeftHand;
                else if (hand.side == Side.Right)
                    hand.wrist.humanBodyBone = HumanBodyBones.RightHand;
                else
                    hand.wrist.humanBodyBone = HumanBodyBones.LastBone;
            }
        }

        public void FingerStart(FingerModel finger)
        {
            finger.part = HumanBodyPart.Finger;

            // Find side
            if (finger.hand == finger.hand.body.leftHand)
                finger.side = Side.Left;
            else if (finger.hand == finger.hand.body.rightHand)
                finger.side = Side.Right;
            else
                finger.side = Side.None;

            // Find finger
            if (finger == finger.hand.thumb)
                finger.finger = HumanFinger.Thumb;
            else if (finger == finger.hand.index)
                finger.finger = HumanFinger.Index;
            else if (finger == finger.hand.middle)
                finger.finger = HumanFinger.Middle;
            else if (finger == finger.hand.ring)
                finger.finger = HumanFinger.Ring;
            else if (finger == finger.hand.pinky)
                finger.finger = HumanFinger.Pinky;
            else
                finger.finger = HumanFinger.None;

            // Clean null bones
            finger.bones.RemoveAll(b => b == null);

            // Find bones
            for (int b = 0; b < finger.bones.Count; b++)
            {
                finger.bones[b].humanBodyBone = AvatarHelpers.GetHumanFingerBone(finger.side, finger.finger, finger.bones.Count - 1 - b);
            }

            // Finger points
            if (!finger.knuckle) finger.knuckle = finger.root.point;

            if (!finger.tip)
            {
                List<BoneModel> distalCandidates = finger.bones.FindAll(b => b.children.Count == 0);
                if (distalCandidates.Count == 1 && distalCandidates[0].points.Count > 1) finger.tip = distalCandidates[0].points[1];
                else Debug.LogWarning("Tip not found for finger " + finger.name);
            }

            // Ordered bones
            AvatarHelpers.GetBonesFromRootToTip(finger, finger.bonesFromRootToTip);
        }

        public Quaternion GetLocalRotation(ReprModel repr)
        {
            if (!repr.parent || !repr.relativeToParentBone) return repr.transformRef.localRotation;
            return Quaternion.Inverse(repr.parent.transformRef.rotation) * repr.transformRef.rotation;
        }

        public Quaternion GetWorldFromLocalRotation(Quaternion newLocalRot, ReprModel repr)
        {
            if (!repr.parent || !repr.relativeToParentBone) return repr.transformRef.parent.rotation * newLocalRot;
            return repr.parent.transformRef.rotation * newLocalRot;
        }

        public Vector3 GetLocalPosition(ReprModel repr)
        {
            if (!repr.parent || !repr.relativeToParentBone) return repr.transformRef.localPosition;
            return repr.parent.transformRef.InverseTransformPoint(repr.transformRef.position);
        }

        public Vector3 GetWorldFromLocalPoition(Vector3 newLocalPos, ReprModel repr)
        {
            if (!repr.parent || !repr.relativeToParentBone) return repr.transformRef.parent.TransformPoint(newLocalPos);
            return repr.parent.transformRef.TransformPoint(newLocalPos);
        }

        public float GetProcessedAngleZ(Quaternion rotation)
        {
            float angle = rotation.eulerAngles.z;

            if (angle >= 180.0f)
                angle -= 180.0f;
            else
                angle += 180.0f;

            return angle;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AvatarController)), CanEditMultipleObjects]
public class AvatarControllerEditor : HPTKControllerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AvatarController myScript = (AvatarController)target;

        if (GUILayout.Button("AWAKE CHILDREN MODELS"))
        {
            Debug.Log("Awaking children models...");

            HPTKModel[] childrenModels = myScript.transform.GetComponentsInChildren<HPTKModel>();

            for (int c = 0; c < childrenModels.Length; c++)
            {
                childrenModels[c].Awake();
            }

            Debug.Log("Done");
        }

        if (GUILayout.Button("AWAKE CHILDREN VIEWS"))
        {
            Debug.Log("Awaking children views...");

            HPTKView[] childrenViews = myScript.transform.GetComponentsInChildren<HPTKView>();

            for (int c = 0; c < childrenViews.Length; c++)
            {
                childrenViews[c].Awake();
            }

            Debug.Log("Done");
        }

        if (GUILayout.Button("AWAKE CHILDREN CONTROLLERS"))
        {
            Debug.Log("Awaking children controllers...");

            HPTKController[] childrenControllers = myScript.transform.GetComponentsInChildren<HPTKController>();

            for (int c = 0; c < childrenControllers.Length; c++)
            {
                childrenControllers[c].Awake();
            }

            Debug.Log("Done");
        }

        if (GUILayout.Button("START THIS AND REGISTERED"))
        {
            Debug.Log("Starting all...");

            if (myScript.genericModel.awaked) myScript.ControllerStart();
            else Debug.LogWarning("The model related with this controller hasn't been initialized");

            Debug.Log("Done");
        }
    }
}
#endif