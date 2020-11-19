using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Models.Avatar;
using UnityEngine.Animations;
using HPTK.Views.Notifiers;
using HPTK.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Anchors
{
    None = 0,
    AsChildren = 1,
    AsParents = 2
}

public class AutomaticHandSetup : MonoBehaviour
{
    public Side handType;
    public float fingerRadius = 0.01f;
    
    public Transform wrist;
    public Transform thumbRootBone;
    public Transform indexRootBone;
    public Transform middleRootBone;
    public Transform ringRootBone;
    public Transform pinkyRootBone;
    public GameObject proxyHandModulePrefab;

    GameObject mainRoot;
    GameObject objects;
    GameObject modules;

    int thumbSiblingIndex = -1;
    int indexSiblingIndex = -1;
    int middleSiblingIndex = -1;
    int ringSiblingIndex = -1;
    int pinkySiblingIndex = -1;

    GameObject masterRootObject;
    GameObject masterWrist;
    GameObject masterWristOffset;
    Transform wristAnchor;

    GameObject slaveRootObject;
    GameObject slaveWrist;

    GameObject ghostRootObject;
    GameObject ghostWrist;

    ProxyHandModel phModel;
    MasterHandModel masterHandModel;
    SlaveHandModel slaveHandModel;
    HandModel ghostHandModel;

    List<Transform[]> _fingers;
    List<Transform[]> _anchors;
    List<Quaternion[]> _localRots;
    List<Quaternion[]> _worldRots;

    [Header("Special points, triggers and ray")]
    public float rayWidth = 0.005f;
    public Material rayMat;

    Vector3 meanPoint;
    float palmRadius;

    [Header("Collider Generation")]
    public Mesh defaultPalmMesh;
    public PhysicMaterial skinPhysMat;

    [Header("Control")]
    public Anchors generateArmatureAnchors = Anchors.None;
    public bool generateMasterOffset = true;
    public bool generateRays = true;
    public bool generateSlave = true;
    public bool generateGhost = true;

    [Header("Debug")]
    public bool showGizmos = true;
    float gizmoDirSize = 0.2f;
    float gizmoSphereSize = 0.005f;

    public void Setup()
    {
        // Check errors
        if (!AllSystemsNominal())
            return;

        // Initialize / Clean cache
        _fingers = new List<Transform[]>();
        _anchors = new List<Transform[]>();
        _localRots = new List<Quaternion[]>();
        _worldRots = new List<Quaternion[]>();

        // CustomHand
        mainRoot = new GameObject();
        mainRoot.name = "CustomHand." + handType.ToString();
        mainRoot.transform.position = wrist.position + new Vector3(0.0f, 0.0f, 0.1f);

        // CustomHand > Objects
        objects = BasicHelpers.InstantiateEmptyChild(mainRoot);
        objects.name = "Objects." + handType.ToString();

        // CustomHand > [Modules]
        modules = BasicHelpers.InstantiateEmptyChild(mainRoot);
        modules.name = "[Modules]";

        // Get sibilng index for index and thumb
        thumbSiblingIndex = thumbRootBone.GetSiblingIndex();
        indexSiblingIndex = indexRootBone.GetSiblingIndex();
        if (middleRootBone) middleSiblingIndex = middleRootBone.GetSiblingIndex();
        if (ringRootBone) ringSiblingIndex = ringRootBone.GetSiblingIndex();
        if (pinkyRootBone) pinkySiblingIndex = pinkyRootBone.GetSiblingIndex();

        // Initialize phModel, masterhandModel and slaveHandModel
        SetupProxyHandModule();

        // Initialize masterWrist and slaveWrist
        SetupMasterObjects();
        if (generateSlave) SetupSlaveObjects();
        if (generateGhost) SetupGhostObjects();

        // Setup HPTK models
        SetupMasterHandModel(masterHandModel, masterWrist.transform, masterWristOffset);
        if (phModel.slave) SetupSlaveHandModel(slaveHandModel, slaveWrist.transform); // Depends on phModel.master for automatic rig mapping
        if (phModel.ghost) SetupGhostHandModel(ghostHandModel, ghostWrist.transform);

        // Setup special points, triggers, ray (and colliders, rbs and joints if needed)
        FixHand(phModel.master, handType, masterWristOffset);
        if (phModel.slave) FixHand(phModel.slave, handType, null);
        if (phModel.ghost) FixHand(phModel.ghost, handType, null);

        // Fix BoneModel.humanBodyBone
        FixHumanBodyBones(phModel.master, handType);
        if (phModel.slave) FixHumanBodyBones(phModel.slave, handType);
        if (phModel.ghost) FixHumanBodyBones(phModel.ghost, handType);

        // Add lines
        AddLines(phModel.master, Color.blue);
        if (phModel.slave) AddLines(phModel.slave, Color.black);
        if (phModel.ghost) AddLines(phModel.ghost, Color.white);
    }

    void SetupMasterObjects()
    {
        // CustomHand > Objects > Master
        masterRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        masterRootObject.name = "Master." + handType.ToString();

        // CustomHand > Objects > Master > (content)

        // Created 
        GameObject masterFingersRoot;

        List<Transform[]> masterFingers = new List<Transform[]>();

        // Get original bones
        for (int f = 0; f < wrist.childCount; f++)
        {
            Transform[] _fingerTransforms = GetFingerTransforms(wrist.GetChild(f));
            Quaternion[] _fingerLocalRotations = BasicHelpers.GetRotations(_fingerTransforms,Space.Self);
            Quaternion[] _fingerWorldRotations = BasicHelpers.GetRotations(_fingerTransforms, Space.World);

            _fingers.Add(_fingerTransforms);
            _localRots.Add(_fingerLocalRotations);
            _worldRots.Add(_fingerWorldRotations);
        }

        // Spawn wrist
        masterWrist = BasicHelpers.InstantiateEmptyChild(masterRootObject, "Wrist");
        masterWrist.transform.position = wrist.position;
        masterFingersRoot = masterWrist.gameObject;

        // Create offset (if needed)
        if (generateMasterOffset)
        {
            masterWristOffset = BasicHelpers.InstantiateEmptyChild(masterWrist, "Offset");
            masterWristOffset.transform.position = masterWrist.transform.position;
            masterFingersRoot = masterWristOffset;
        }

        // Armature wrist anchor
        if (generateArmatureAnchors > Anchors.None)
        {
            wristAnchor = new GameObject().transform;
            wristAnchor.name = wrist.name + ".Anchor";
            wristAnchor.position = wrist.position;

            if (generateArmatureAnchors == Anchors.AsParents)
            {
                wristAnchor.parent = wrist.parent;
                wrist.parent = wristAnchor;
            }
            else if (generateArmatureAnchors == Anchors.AsChildren)
            {
                wristAnchor.parent = wrist;
            }  
        }

        // Create bones
        for (int f = 0; f < _fingers.Count; f++)
        {
            List<Transform> finger = new List<Transform>();
            List<Transform> _fingerAnchors = new List<Transform>();

            for (int b = 0; b < _fingers[f].Length; b++)
            {
                Transform bone = BasicHelpers.InstantiateEmptyChild(masterFingersRoot).transform;

                if (b==0 && _fingers[f][b].childCount == 0) 
                    bone.name = "Extra.Bone";
                else
                    bone.name = "Finger" + f + ".Bone" + b;

                bone.position = _fingers[f][b].position;

                // Parenting
                if (b == 0)
                    bone.parent = masterFingersRoot.transform;
                else
                    bone.parent = finger[b - 1];

                // Not rotation needed

                //Armature anchor
                if (generateArmatureAnchors > Anchors.None)
                {
                    Transform anchor = new GameObject().transform;
                    anchor.name = _fingers[f][b].name + ".Anchor";
                    anchor.position = _fingers[f][b].position;

                    if (_fingers[f][b].childCount > 0)
                        _fingers[f][b].GetChild(0).parent = anchor;

                    if (generateArmatureAnchors == Anchors.AsParents)
                    {
                        anchor.parent = _fingers[f][b].parent;
                        _fingers[f][b].parent = anchor;
                    }
                    else if (generateArmatureAnchors == Anchors.AsChildren)
                    {
                        anchor.parent = _fingers[f][b];

                        if (b == 0)
                            _fingers[f][b].parent = wristAnchor;
                    }

                    _fingerAnchors.Add(anchor);
                }

                finger.Add(bone);
            }

            masterFingers.Add(finger.ToArray());
            _anchors.Add(_fingerAnchors.ToArray());
        }

        // Move wrist to rootObject and apply offset
        masterWrist.transform.localPosition = Vector3.zero;
    }

    void SetupSlaveObjects()
    {
        // CustomHand > Objects > Slave
        slaveRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        slaveRootObject.name = "Slave." + handType.ToString();

        // Move it to avoid overlay
        slaveRootObject.transform.position += new Vector3(0.0f, 0.2f, 0.0f);

        // CustomHand > Objects > Slave > (content)
        GameObject handRoot;
        if (generateMasterOffset)
            handRoot = masterWristOffset;
        else
            handRoot = masterWrist;

        slaveWrist = Instantiate(handRoot, slaveRootObject.transform.position, Quaternion.identity);
        slaveWrist.transform.parent = slaveRootObject.transform;
        slaveWrist.transform.name = "Wrist";
    }

    void SetupGhostObjects()
    {
        // CustomHand > Objects > Ghost
        ghostRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        ghostRootObject.name = "Ghost." + handType.ToString();

        // Move it to avoid overlay
        ghostRootObject.transform.position += new Vector3(0.0f, 0.4f, 0.0f);

        // CustomHand > Objects > Ghost > (content)
        GameObject handRoot;
        if (generateMasterOffset)
            handRoot = masterWristOffset;
        else
            handRoot = masterWrist;

        ghostWrist = Instantiate(handRoot, ghostRootObject.transform.position, Quaternion.identity);
        ghostWrist.transform.parent = ghostRootObject.transform;
        ghostWrist.transform.name = "Wrist";
    }

    void SetupProxyHandModule()
    {
        // CustomHand > [Modules] > ProxyHandModule
        GameObject proxyHandModule = Instantiate(proxyHandModulePrefab, modules.transform.position, modules.transform.rotation);
        proxyHandModule.transform.parent = modules.transform;
        proxyHandModule.transform.name = "ProxyHandModule." + handType.ToString();

        phModel = proxyHandModule.GetComponentInChildren<ProxyHandModel>();

        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Master
        GameObject masterHandModelObj = BasicHelpers.InstantiateEmptyChild(phModel.gameObject);
        masterHandModelObj.name = "Master." + handType.ToString();
        masterHandModel = masterHandModelObj.AddComponent<MasterHandModel>();

        if (generateSlave)
        {
            // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Slave
            GameObject slaveHandModelObj = BasicHelpers.InstantiateEmptyChild(phModel.gameObject);
            slaveHandModelObj.name = "Slave." + handType.ToString();
            slaveHandModel = slaveHandModelObj.AddComponent<SlaveHandModel>();
        }

        if (generateGhost)
        {
            // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Ghost
            GameObject ghostHandModelObj = BasicHelpers.InstantiateEmptyChild(phModel.gameObject);
            ghostHandModelObj.name = "Ghost." + handType.ToString();
            ghostHandModel = ghostHandModelObj.AddComponent<HandModel>();
        }

        // Make HandModels accessible from ProxyHandModel
        phModel.master = masterHandModel;
        if (generateSlave) phModel.slave = slaveHandModel;
        if (generateGhost) phModel.ghost = ghostHandModel;

        // Make ProxyHandModel accessible from HandModel
        masterHandModel.proxyHand = phModel;
        if (generateSlave) slaveHandModel.proxyHand = phModel;
        if (generateGhost) ghostHandModel.proxyHand = phModel;
    }

    void SetupMasterHandModel(MasterHandModel handModel, Transform masterWrist, GameObject masterWristOffset)
    {
        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Master > Wrist
        GameObject wristBoneModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
        wristBoneModelObj.name = "Wrist";

        MasterBoneModel wristBone = wristBoneModelObj.AddComponent<MasterBoneModel>();
        wristBone.transformRef = masterWrist;
        handModel.wrist = wristBone;

        /* MASTER BONE MODEL SPECIFIC
        */

        if (masterWristOffset != null)
        {
            wristBone.offset = masterWristOffset.transform;
        }

        wristBone.armatureBone = wrist;
        if (wristAnchor) wristBone.armatureAnchor = wristAnchor;
        wristBone.relativeToOriginalArmatureLocal = Quaternion.Inverse(wristBone.transformRef.localRotation) * wrist.localRotation;
        wristBone.relativeToOriginalArmatureWorld = Quaternion.Inverse(wristBone.transformRef.rotation) * wrist.rotation;

        /* 
        */

        // Set SkinnedMR
        handModel.skinnedMR = masterWrist.GetComponent<SkinnedMeshRenderer>();

        List<FingerModel> fingers = new List<FingerModel>();

        Transform handRoot;
        if (masterWristOffset != null)
            handRoot = masterWristOffset.transform;
        else
            handRoot = masterWrist;

        for (int f = 0; f < handRoot.childCount; f++)
        {
            // If childCount is 0, then it's not a finger
            if (handRoot.GetChild(f).childCount == 0)
            {
                continue;
            }

            // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Master > Finger
            GameObject fingerModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
            fingerModelObj.name = "Finger" + f;

            FingerModel fingerModel = fingerModelObj.AddComponent<FingerModel>();

            List<BoneModel> bones = new List<BoneModel>();

            Transform[] fingerTransforms = GetFingerTransforms(handRoot.GetChild(f));
            for (int b = 0; b < fingerTransforms.Length; b++)
            {
                // If childCount is 0, then it's a fingerTip
                if (fingerTransforms[b].childCount == 0)
                {
                    fingerModel.fingerTip = fingerTransforms[b];
                    continue;
                }

                // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Master > Finger > Bone

                GameObject boneModelObj = BasicHelpers.InstantiateEmptyChild(fingerModelObj);
                boneModelObj.transform.name = "Bone" + b;

                MasterBoneModel masterBone = boneModelObj.AddComponent<MasterBoneModel>();
                masterBone.transformRef = fingerTransforms[b];

                /* MASTER BONE MODEL SPECIFIC
                */
                masterBone.armatureBone = _fingers[f][b];

                if (generateArmatureAnchors > Anchors.None && _anchors.Count >= f - 1 && _anchors[f].Length >= b - 1) masterBone.armatureAnchor = _anchors[f][b];

                masterBone.initialArmatureBoneLocalRot = _localRots[f][b];
                masterBone.relativeToOriginalArmatureLocal = Quaternion.Inverse(fingerTransforms[b].localRotation) * _localRots[f][b];
                masterBone.relativeToOriginalArmatureWorld = Quaternion.Inverse(fingerTransforms[b].rotation) * _worldRots[f][b];
                /*
                 */

                bones.Add(masterBone);
            }

            fingerModel.fingerBase = bones[0].transformRef;
            fingerModel.distal = bones[bones.Count - 1];

            fingerModel.bones = bones.ToArray();

            fingerModel.hand = handModel;

            if (f == thumbSiblingIndex)
            {
                fingerModel.name = "Thumb";
                handModel.thumb = fingerModel;
            }
            else if (f == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }
            else if (f == middleSiblingIndex)
            {
                fingerModel.name = "Middle";
                handModel.middle = fingerModel;
            }
            else if (f == ringSiblingIndex)
            {
                fingerModel.name = "Ring";
                handModel.ring = fingerModel;
            }
            else if (f == pinkySiblingIndex)
            {
                fingerModel.name = "Pinky";
                handModel.pinky = fingerModel;
            }
        }

        // Homogeneus order for .fingers
        if (handModel.thumb) fingers.Add(handModel.thumb);
        if (handModel.index) fingers.Add(handModel.index);
        if (handModel.middle) fingers.Add(handModel.middle);
        if (handModel.ring) fingers.Add(handModel.ring);
        if (handModel.pinky) fingers.Add(handModel.pinky);

        handModel.fingers = fingers.ToArray();
    }

    void SetupSlaveHandModel(SlaveHandModel handModel, Transform slaveWrist)
    {
        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Slave > Wrist
        GameObject wristBoneModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
        wristBoneModelObj.name = "Wrist";

        SlaveBoneModel wristBone = wristBoneModelObj.AddComponent<SlaveBoneModel>();
        wristBone.transformRef = slaveWrist;
        handModel.wrist = wristBone;

        /* SLAVE BONE MODEL SPECIFIC
        */

        wristBone.masterBone = handModel.proxyHand.master.wrist as MasterBoneModel;

        /* 
        */

        // Set SkinnedMR
        handModel.skinnedMR = slaveWrist.GetComponent<SkinnedMeshRenderer>();

        List<FingerModel> fingers = new List<FingerModel>();

        int f = 0;
        for (int i = 0; i < slaveWrist.childCount; i++)
        {
            // If childCount is 0, then it's not a finger
            if (slaveWrist.GetChild(i).childCount == 0)
            {
                continue;
            }

            // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Slave > Finger
            GameObject fingerModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
            fingerModelObj.name = "Finger" + f;

            FingerModel fingerModel = fingerModelObj.AddComponent<FingerModel>();

            List<BoneModel> bones = new List<BoneModel>();

            Transform[] fingerTransforms = GetFingerTransforms(slaveWrist.GetChild(i));
            for (int b = 0; b < fingerTransforms.Length; b++)
            {
                if (fingerTransforms[b].childCount == 0)
                {
                    fingerModel.fingerTip = fingerTransforms[b];
                    continue;
                }

                // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Slave > Finger > Bone

                GameObject boneModelObj = BasicHelpers.InstantiateEmptyChild(fingerModelObj);
                boneModelObj.transform.name = "Bone" + b;

                SlaveBoneModel slaveBone = boneModelObj.AddComponent<SlaveBoneModel>();
                slaveBone.transformRef = fingerTransforms[b];

                bones.Add(slaveBone);
            }

            fingerModel.fingerBase = bones[0].transformRef;
            fingerModel.distal = bones[bones.Count - 1];

            fingerModel.bones = bones.ToArray();

            fingerModel.hand = handModel;

            if (i == thumbSiblingIndex)
            {
                fingerModel.name = "Thumb";
                handModel.thumb = fingerModel;

                /* SLAVE BONE MODEL SPECIFIC
                 */

                // Thumb is usually a finger with special participation in hand physics and grabbign detection
                for (int b = 0; b < fingerModel.bones.Length; b++)
                {
                    (fingerModel.bones[b] as SlaveBoneModel).isSpecial = true;
                }

                /* 
                */
            }
            else if (f == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }
            else if (f == middleSiblingIndex)
            {
                fingerModel.name = "Middle";
                handModel.middle = fingerModel;
            }
            else if (f == ringSiblingIndex)
            {
                fingerModel.name = "Ring";
                handModel.ring = fingerModel;
            }
            else if (f == pinkySiblingIndex)
            {
                fingerModel.name = "Pinky";
                handModel.pinky = fingerModel;
            }

            // Increase finger counter
            f++;
        }

        // Homogeneus order for .fingers
        if (handModel.thumb) fingers.Add(handModel.thumb);
        if (handModel.index) fingers.Add(handModel.index);
        if (handModel.middle) fingers.Add(handModel.middle);
        if (handModel.ring) fingers.Add(handModel.ring);
        if (handModel.pinky) fingers.Add(handModel.pinky);

        handModel.fingers = fingers.ToArray();

        // Operations that depends on having master.fingers and slave.fingers in the correct order
        for (f = 0; f < handModel.fingers.Length; f++)
        {
            for (int b = 0; b < handModel.fingers[f].bones.Length; b++)
            {
                /* SLAVE BONE MODEL SPECIFIC
                */

                SlaveBoneModel slaveBone = handModel.fingers[f].bones[b] as SlaveBoneModel;

                // Simple automatic rig mapping
                if (f > handModel.proxyHand.master.fingers.Length - 1)
                    Debug.LogError("Trying to access a non-existing finger!");

                else if (b > handModel.proxyHand.master.fingers[f].bones.Length - 1)
                    Debug.LogError("Trying to access a non-existing bone!");

                else if (handModel.proxyHand.master.fingers[f] != null && handModel.proxyHand.master.fingers[f].bones[b] != null)
                    slaveBone.masterBone = handModel.proxyHand.master.fingers[f].bones[b] as MasterBoneModel;

                /* 
                */
            }
        }
        
    }

    void SetupGhostHandModel(HandModel handModel, Transform ghostWrist)
    {
        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Ghost > Wrist
        GameObject wristBoneModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
        wristBoneModelObj.name = "Wrist";

        BoneModel wristBone = wristBoneModelObj.AddComponent<BoneModel>();
        wristBone.transformRef = ghostWrist;
        handModel.wrist = wristBone;

        // Set SkinnedMR
        handModel.skinnedMR = ghostWrist.GetComponent<SkinnedMeshRenderer>();

        List<FingerModel> fingers = new List<FingerModel>();

        for (int f = 0; f < ghostWrist.childCount; f++)
        {
            // If childCount is 0, then it's not a finger
            if (ghostWrist.GetChild(f).childCount == 0)
            {
                continue;
            }

            // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Ghost > Finger
            GameObject fingerModelObj = BasicHelpers.InstantiateEmptyChild(handModel.gameObject);
            fingerModelObj.name = "Finger" + f;

            FingerModel fingerModel = fingerModelObj.AddComponent<FingerModel>();

            List<BoneModel> bones = new List<BoneModel>();

            Transform[] fingerTransforms = GetFingerTransforms(ghostWrist.GetChild(f));
            for (int b = 0; b < fingerTransforms.Length; b++)
            {
                // If childCount is 0, then it's a fingerTip
                if (fingerTransforms[b].childCount == 0)
                {
                    fingerModel.fingerTip = fingerTransforms[b];
                    continue;
                }

                // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Ghost > Finger > Bone

                GameObject boneModelObj = BasicHelpers.InstantiateEmptyChild(fingerModelObj);
                boneModelObj.transform.name = "Bone" + b;

                BoneModel ghostBone = boneModelObj.AddComponent<BoneModel>();
                ghostBone.transformRef = fingerTransforms[b];

                bones.Add(ghostBone);
            }

            fingerModel.fingerBase = bones[0].transformRef;
            fingerModel.distal = bones[bones.Count - 1];

            fingerModel.bones = bones.ToArray();

            fingerModel.hand = handModel;

            if (f == thumbSiblingIndex)
            {
                fingerModel.name = "Thumb";
                handModel.thumb = fingerModel;
            }
            else if (f == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }
            else if (f == middleSiblingIndex)
            {
                fingerModel.name = "Middle";
                handModel.middle = fingerModel;
            }
            else if (f == ringSiblingIndex)
            {
                fingerModel.name = "Ring";
                handModel.ring = fingerModel;
            }
            else if (f == pinkySiblingIndex)
            {
                fingerModel.name = "Pinky";
                handModel.pinky = fingerModel;
            }
        }

        // Homogeneus order for .fingers
        if (handModel.thumb) fingers.Add(handModel.thumb);
        if (handModel.index) fingers.Add(handModel.index);
        if (handModel.middle) fingers.Add(handModel.middle);
        if (handModel.ring) fingers.Add(handModel.ring);
        if (handModel.pinky) fingers.Add(handModel.pinky);

        handModel.fingers = fingers.ToArray();
    }

    void FixHand(HandModel handModel, Side side, GameObject wristOffset)
    {
        // Check errors
        if (handModel.fingers.Length == 0)
        {
            Debug.LogError("Fingers array is empty!");
            return;
        }

        // Not all hands need an offset
        if (!wristOffset)
            wristOffset = handModel.wrist.transformRef.gameObject;

        // Pinch center
        if (!handModel.pinchCenter)
        {
            GameObject pinchCenter = BasicHelpers.InstantiateEmptyChild(handModel.wrist.transformRef.parent.gameObject);
            pinchCenter.name = "PinchCenter";

            PositionConstraint pinchPosConstraint = pinchCenter.AddComponent<PositionConstraint>();

            ConstraintSource indexTipSource = new ConstraintSource();
            indexTipSource.sourceTransform = handModel.index.fingerTip;
            indexTipSource.weight = 1.0f;

            ConstraintSource thumbTipSource = new ConstraintSource();
            thumbTipSource.sourceTransform = handModel.thumb.fingerTip;
            thumbTipSource.weight = 1.0f;

            pinchPosConstraint.AddSource(indexTipSource);
            pinchPosConstraint.AddSource(thumbTipSource);

            pinchPosConstraint.translationOffset = Vector3.zero;

            pinchPosConstraint.constraintActive = true;

            handModel.pinchCenter = pinchCenter.transform;
        }

        // Throat center
        if (!handModel.throatCenter)
        {
            GameObject throatCenter = BasicHelpers.InstantiateEmptyChild(handModel.wrist.transformRef.parent.gameObject);
            throatCenter.name = "ThroatCenter";

            PositionConstraint throatPosConstraint = throatCenter.AddComponent<PositionConstraint>();

            ConstraintSource indexBaseSource = new ConstraintSource();
            indexBaseSource.sourceTransform = handModel.index.fingerBase;
            indexBaseSource.weight = 1.0f;

            ConstraintSource thumbBaseSource = new ConstraintSource();
            thumbBaseSource.sourceTransform = handModel.thumb.fingerBase;
            thumbBaseSource.weight = 1.0f;

            throatPosConstraint.AddSource(indexBaseSource);
            throatPosConstraint.AddSource(thumbBaseSource);

            throatPosConstraint.translationOffset = Vector3.zero;

            throatPosConstraint.constraintActive = true;

            handModel.throatCenter = throatCenter.transform;
        }

        // meanPoint
        meanPoint = Vector3.zero;
        for (int f = 0; f < handModel.fingers.Length; f++)
        {
            meanPoint += handModel.fingers[f].fingerBase.position;
        }
        meanPoint = meanPoint / handModel.fingers.Length;

        // palmRadius
        float meanDistance = 0.0f;
        for (int f = 0; f < handModel.fingers.Length; f++)
        {
            meanDistance += Vector3.Distance(meanPoint, handModel.fingers[f].fingerBase.position);
        }
        meanDistance = meanDistance / handModel.fingers.Length;
        palmRadius = meanDistance;

        // Palm center
        if (!handModel.palmCenter)
        {
            GameObject palmCenter = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
            palmCenter.name = "PalmCenter";

            palmCenter.transform.position = meanPoint;

            if (side == Side.Left)
            {
                palmCenter.transform.position += new Vector3(0.0f, fingerRadius, 0.0f);
                palmCenter.transform.localRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f));
            }
            else
            {
                palmCenter.transform.position -= new Vector3(0.0f, fingerRadius, 0.0f);
                palmCenter.transform.localRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 180.0f));
            }

            handModel.palmCenter = palmCenter.transform;
        }

        // Palm normal (depends on palmCenter)
        if (!handModel.palmNormal)
        {
            GameObject palmNormal = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
            palmNormal.name = "PalmNormal";

            palmNormal.transform.position = handModel.palmCenter.position;

            if (side == Side.Left)
                palmNormal.transform.localRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f));
            else
                palmNormal.transform.localRotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));

            handModel.palmNormal = palmNormal.transform;
        }

        // Palm interior/exterior (depend on PalmCenter and palmRadius)
        if (!handModel.palmInterior)
        {
            GameObject palmInterior = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
            palmInterior.name = "PalmInterior";

            palmInterior.transform.position = handModel.palmCenter.position;
            palmInterior.transform.rotation = handModel.palmCenter.rotation;

            palmInterior.transform.position += handModel.palmCenter.up * palmRadius;

            handModel.palmInterior = palmInterior.transform;
        }
        if (!handModel.palmExterior)
        {
            GameObject palmExterior = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
            palmExterior.name = "PalmExterior";

            palmExterior.transform.position = handModel.palmCenter.position;
            palmExterior.transform.rotation = handModel.palmCenter.rotation;

            palmExterior.transform.position -= handModel.palmCenter.up * palmRadius;

            handModel.palmExterior = palmExterior.transform;
        }

        // Ray line (depend on PalmCenter)
        if (!handModel.ray && generateRays)
        {
            GameObject ray = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
            ray.name = "Ray";

            ray.transform.position = handModel.palmCenter.position;

            LineRenderer lineRenderer = ray.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPositions(new Vector3[] { Vector3.zero, new Vector3(0.0f, 0.0f, 0.5f) });
            lineRenderer.material = rayMat;
            lineRenderer.startWidth = rayWidth;
            lineRenderer.endWidth = rayWidth;

            handModel.ray = ray.transform;
        }

        if (handModel is SlaveHandModel)
        {
            SlaveHandModel slaveHandModel = handModel as SlaveHandModel;

            // Palm trigger (depend on PalmCenter and palmRadius)
            if (!slaveHandModel.palmTrigger)
            {
                GameObject palmTrigger = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
                palmTrigger.name = "PalmTrigger";

                palmTrigger.transform.position = handModel.palmCenter.position;

                SphereCollider trigger = palmTrigger.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = palmRadius;

                TriggerNotifier notifier = palmTrigger.AddComponent<TriggerNotifier>();
                notifier.ignoreChildren = handModel.wrist.transformRef.parent;

                slaveHandModel.palmTrigger = notifier;
            }

            // Hand trigger (depend on PalmCenter and palmRadius)
            if (!slaveHandModel.handTrigger)
            {
                GameObject handTrigger = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
                handTrigger.name = "HandTrigger";

                handTrigger.transform.position = handModel.palmCenter.position;

                SphereCollider trigger = handTrigger.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = Vector3.Distance(meanPoint, BasicHelpers.FurthestPoint(meanPoint, GetFingerTips(slaveHandModel))) + 0.02f;

                TriggerNotifier notifier = handTrigger.AddComponent<TriggerNotifier>();
                notifier.ignoreChildren = handModel.wrist.transformRef.parent;

                slaveHandModel.handTrigger = notifier;
            }
        }

        // Colliders, Rigidbodies and Joitsn if needed
        if (handModel is SlaveHandModel)
        {
            // Setup colliders, rbs and joints for slave
            Transform fingersRoot = wrist;
            if (wristOffset)
                fingersRoot = wristOffset.transform;

            SetupPhysics(handModel as SlaveHandModel, fingersRoot, meanPoint, palmRadius, fingerRadius);
        }

    }

    Vector3[] GetFingerTips(HandModel handModel)
    {
        List<Vector3> tipPositions = new List<Vector3>();
        for (int f = 0; f < handModel.fingers.Length; f++)
        {
            tipPositions.Add(handModel.fingers[f].fingerTip.position);
        }
        return tipPositions.ToArray();
    }


    bool AllSystemsNominal()
    {
        if (!HandModelIsValid(wrist))
        {
            Debug.LogError("Wrist children should be clean branches (one child per bone)");
            return false;
        }

        if (!thumbRootBone.IsChildOf(wrist) || !indexRootBone.IsChildOf(wrist))
        {
            Debug.LogError("Thumb root bone and index root bone have to be direct children of Wrist");
            return false;
        }

        if (!thumbRootBone || !indexRootBone)
        {
            Debug.LogError("Thumb root bone and index root bone are required!");
            return false;
        }

        return true;
    }

    bool HandModelIsValid(Transform wrist)
    {
        for (int i = 0; i < wrist.childCount; i++)
        {
            if (!BasicHelpers.IsCleanBranch(wrist.GetChild(i)))
                return false;
        }

        return true;
    }


    Transform[] GetFingerTransforms(Transform fingerRoot)
    {
        List<Transform> bones = new List<Transform>();

        Transform bone = fingerRoot;

        bones.Add(bone);

        int iterationLimit = 100;
        while (bone.childCount > 0 && iterationLimit > 0)
        {
            bone = bone.GetChild(0);
            bones.Add(bone);

            iterationLimit--;
        }

        return bones.ToArray();
    }

    void SetupPhysics(SlaveHandModel slaveHand, Transform fingersRoot, Vector3 meanPoint, float palmRadius, float fingerRadius)
    {
        // Wrist collider
        GameObject palmObj = BasicHelpers.InstantiateEmptyChild(fingersRoot.gameObject);
        palmObj.name = "Palm.Collider";
        palmObj.transform.position = meanPoint;

        MeshCollider palm = palmObj.AddComponent<MeshCollider>();
        palm.sharedMesh = defaultPalmMesh;
        palm.convex = true;
        palm.material = skinPhysMat;

        Vector3 localScale = new Vector3(2.0f * palmRadius, fingerRadius, 2.0f * palmRadius);
        palmObj.transform.localScale = localScale;

        // Wrist rb
        Rigidbody wristRb = fingersRoot.gameObject.AddComponent<Rigidbody>();
        wristRb.useGravity = false;
        wristRb.interpolation = RigidbodyInterpolation.Interpolate;
        wristRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Wrist joint
        ConfigurableJoint wristJoint = wristRb.gameObject.AddComponent<ConfigurableJoint>();
        ConfigureJoint(wristJoint, Space.World);

        // Palm collision notifier
        CollisionNotifier palmNotifier = wristRb.gameObject.AddComponent<CollisionNotifier>();

        // Add refs
        (slaveHand.wrist as SlaveBoneModel).colliderRef = palm;
        (slaveHand.wrist as SlaveBoneModel).rigidbodyRef = wristRb;
        (slaveHand.wrist as SlaveBoneModel).jointRef = wristJoint;
        slaveHand.palmCollisionNotifier = palmNotifier;

        // Bones
        for (int f = 0; f < slaveHand.fingers.Length; f++)
        {
            for (int b = 0; b < slaveHand.fingers[f].bones.Length; b++)
            {
                Transform parent = slaveHand.fingers[f].bones[b].transformRef;
                Transform child;
                if (b == slaveHand.fingers[f].bones.Length - 1)
                {
                    child = slaveHand.fingers[f].fingerTip;
                }
                else
                {
                    child = slaveHand.fingers[f].bones[b + 1].transformRef;
                }

                // Bone collider
                GameObject capsuleObj = BasicHelpers.InstantiateEmptyChild(parent.gameObject);
                capsuleObj.transform.position = (parent.position + child.position) / 2.0f;
                capsuleObj.transform.name = parent.name + ".Collider";

                // Collider alignment
                capsuleObj.transform.LookAt(child.position);

                CapsuleCollider capsule = capsuleObj.AddComponent<CapsuleCollider>();
                capsule.direction = 2; // Along Z axis (as we use .lookAt())
                capsule.height = Vector3.Distance(parent.position, child.position);
                capsule.radius = fingerRadius;

                // Phys material
                capsule.material = skinPhysMat;

                // Bone rb
                Rigidbody boneRb = slaveHand.fingers[f].bones[b].transformRef.gameObject.AddComponent<Rigidbody>();
                boneRb.useGravity = false;
                boneRb.interpolation = RigidbodyInterpolation.Interpolate;
                boneRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Bone joint
                ConfigurableJoint boneJoint = boneRb.gameObject.AddComponent<ConfigurableJoint>();

                if (b == 0)
                    boneJoint.connectedBody = (slaveHand.wrist as SlaveBoneModel).rigidbodyRef;
                else
                    boneJoint.connectedBody = (slaveHand.fingers[f].bones[b - 1] as SlaveBoneModel).rigidbodyRef;

                ConfigureJoint(boneJoint, Space.Self);

                // Add refs
                (slaveHand.fingers[f].bones[b] as SlaveBoneModel).colliderRef = capsule;
                (slaveHand.fingers[f].bones[b] as SlaveBoneModel).rigidbodyRef = boneRb;
                (slaveHand.fingers[f].bones[b] as SlaveBoneModel).jointRef = boneJoint;
            }

            //Generate CollisionNotifiers
            CollisionNotifier notifier = slaveHand.fingers[f].distal.transformRef.gameObject.AddComponent<CollisionNotifier>();
            slaveHand.fingers[f].fingerTipCollisionNotifier = notifier;
        }
    }

    void ConfigureJoint(ConfigurableJoint joint, Space space)
    {
        if (space == Space.Self)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.autoConfigureConnectedAnchor = true;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.configuredInWorldSpace = false;
            joint.enablePreprocessing = false;
        }
        else
        {
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.configuredInWorldSpace = true;
            joint.enablePreprocessing = false;
        }
    }

    void AddLines(HandModel hand, Color color)
    {
        BoneLineDrawer bld = hand.wrist.transformRef.parent.gameObject.AddComponent<BoneLineDrawer>();
        bld.hand = hand;
        bld.color = color;

        for (int f = 0; f < hand.fingers.Length; f++)
        {
            LineRenderer lr = hand.fingers[f].bones[0].transformRef.gameObject.AddComponent<LineRenderer>();
            lr.material = rayMat;
            lr.startWidth = fingerRadius;
            lr.endWidth = fingerRadius;

            hand.fingers[f].lineRenderer = lr;

            DrawLineBetween dlb = hand.fingers[f].bones[0].transformRef.gameObject.AddComponent<DrawLineBetween>();
            dlb.isActive = true;

            for (int b = 0; b < hand.fingers[f].bones.Length; b++)
            {
                dlb.objs.Add(hand.fingers[f].bones[b].transformRef);
            }

            dlb.objs.Add(hand.fingers[f].fingerTip);
        }
    }

    void FixHumanBodyBones(HandModel hand, Side side)
    {
        if (side == Side.Left)
        {
            hand.wrist.humanBodyBone = HumanBodyBones.LeftHand;
            if (hand.forearm) hand.forearm.humanBodyBone = HumanBodyBones.LeftLowerArm;

        }
        else
        {
            hand.wrist.humanBodyBone = HumanBodyBones.RightEye;
            if (hand.forearm) hand.forearm.humanBodyBone = HumanBodyBones.RightLowerArm;

        }

        for (int f = 0; f < hand.fingers.Length; f++)
        {
            FixHumanFinger(hand.fingers[f], side);
        }
    }

    void FixHumanFinger(FingerModel finger, Side side)
    {
        HumanFinger fingerEnum = HumanFinger.None;
        HumanFingerBone fingerBoneEnum = HumanFingerBone.None;

        // Get finger enum
        if (finger == finger.hand.thumb)
            fingerEnum = HumanFinger.Thumb;
        else if (finger == finger.hand.index)
            fingerEnum = HumanFinger.Index;
        else if (finger == finger.hand.middle)
            fingerEnum = HumanFinger.Middle;
        else if (finger == finger.hand.ring)
            fingerEnum = HumanFinger.Ring;
        else if (finger == finger.hand.pinky)
            fingerEnum = HumanFinger.Pinky;

        for (int b = 0; b < finger.bones.Length; b++)
        {
            // Get finger bone enum
            if (b == finger.bones.Length - 1)
                fingerBoneEnum = HumanFingerBone.Distal;
            else if (b == finger.bones.Length - 2)
                fingerBoneEnum = HumanFingerBone.Intermediate;
            else if (b == finger.bones.Length - 3)
                fingerBoneEnum = HumanFingerBone.Proximal;
            else if (b == finger.bones.Length - 4)
                fingerBoneEnum = HumanFingerBone.Metacarpal;

            // If it's possible, fix BoneModel.humanBodyBone
            if (fingerEnum != HumanFinger.None && fingerBoneEnum != HumanFingerBone.None)
                FixHumanFingerBone(finger.bones[b], side, fingerEnum, fingerBoneEnum);
        }
    }

    void FixHumanFingerBone(BoneModel bone, Side side, HumanFinger finger, HumanFingerBone fingerbone)
    {
        switch (side)
        {
            case Side.Left:
                switch (finger)
                {
                    case HumanFinger.Thumb:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.LeftThumbProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.LeftThumbIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.LeftThumbDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Index:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.LeftIndexProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.LeftIndexIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.LeftIndexDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Middle:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.LeftMiddleProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.LeftMiddleIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.LeftMiddleDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Ring:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.LeftRingProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.LeftRingIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.LeftRingDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Pinky:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.LeftLittleProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.LeftLittleIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.LeftLittleDistal;
                                break;
                        }
                        break;
                }
                break;
            case Side.Right:
                switch (finger)
                {
                    case HumanFinger.Thumb:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.RightThumbProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.RightThumbIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.RightThumbDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Index:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.RightIndexProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.RightIndexIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.RightIndexDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Middle:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.RightMiddleProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.RightMiddleIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.RightMiddleDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Ring:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.RightRingProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.RightRingIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.RightRingDistal;
                                break;
                        }
                        break;
                    case HumanFinger.Pinky:
                        switch (fingerbone)
                        {
                            case HumanFingerBone.Proximal:
                                bone.humanBodyBone = HumanBodyBones.RightLittleProximal;
                                break;
                            case HumanFingerBone.Intermediate:
                                bone.humanBodyBone = HumanBodyBones.RightLittleIntermediate;
                                break;
                            case HumanFingerBone.Distal:
                                bone.humanBodyBone = HumanBodyBones.RightLittleDistal;
                                break;
                        }
                        break;
                }
                break;
        }

    }

    private void OnDrawGizmos()
    {
        if (wrist != null && showGizmos)
        {
            // Get world directions
            Vector3 fingerDir;
            Vector3 palmNormalDir;
            Vector3 thumbDir;

            if (handType == Side.Left)
            {
                fingerDir = new Vector3(-gizmoDirSize, 0.0f, 0.0f);
                palmNormalDir = new Vector3(0.0f, gizmoDirSize, 0.0f);
                thumbDir = new Vector3(0.0f, 0.0f, -gizmoDirSize);
            }
            else
            {
                fingerDir = new Vector3(gizmoDirSize, 0.0f, 0.0f);
                palmNormalDir = new Vector3(0.0f, -gizmoDirSize, 0.0f);
                thumbDir = new Vector3(0.0f, 0.0f, gizmoDirSize);
            }

# if UNITY_EDITOR
            // Draw finger direction
            Handles.color = Color.blue;
            Handles.Label(wrist.position + fingerDir, "Fingers");
            Handles.ArrowHandleCap(
                0,
                wrist.position,
                Quaternion.LookRotation(fingerDir),
                gizmoDirSize,
                EventType.Repaint
            );

            // Draw palm normal direction
            Handles.color = Color.black;
            Handles.Label(wrist.position + palmNormalDir, "Palm normal");
            Handles.ArrowHandleCap(
                0,
                wrist.position,
                Quaternion.LookRotation(palmNormalDir),
                gizmoDirSize,
                EventType.Repaint
            );

            // Draw thumb direction
            Handles.color = Color.white;
            Handles.Label(wrist.position + (thumbDir / 2) + fingerDir, "Thumb");
            Handles.ArrowHandleCap(
                0,
                wrist.position + (thumbDir / 2),
                Quaternion.LookRotation(fingerDir),
                gizmoDirSize,
                EventType.Repaint
            );
#endif
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(AutomaticHandSetup))]
public class HandSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutomaticHandSetup myScript = (AutomaticHandSetup)target;
        if (GUILayout.Button("SETUP"))
        {
            myScript.Setup();
        }
    }
}
#endif
