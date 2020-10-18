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

public enum Side
{
    L,
    R
}

public class AutomaticHandSetup : MonoBehaviour
{
    public Side handType;
    public float fingerRadius = 0.01f;
    
    public Transform wrist;
    public Transform thumbRootBone;
    public Transform indexRootBone;
    public GameObject proxyHandModulePrefab;

    [Header("Optional")]
    public Transform wristOffset;

    GameObject mainRoot;
    GameObject objects;
    GameObject modules;

    int indexSiblingIndex;
    int thumbSiblingIndex;

    GameObject masterRootObject;
    GameObject masterWrist;
    Transform masterWristOffset;

    GameObject slaveRootObject;
    GameObject slaveWrist;

    GameObject ghostRootObject;
    GameObject ghostWrist;

    ProxyHandModel phModel;
    MasterHandModel masterHandModel;
    SlaveHandModel slaveHandModel;
    HandModel ghostHandModel;

    [Header("Special points, triggers and ray")]
    public float rayWidth = 0.005f;
    public Material rayMat;

    Vector3 meanPoint;
    float palmRadius;

    [Header("Collider Generation")]
    public Mesh defaultPalmMesh;
    public PhysicMaterial skinPhysMat;

    public void Setup()
    {
        // Check errors
        if (!AllSystemsNominal())
            return;

        // Set hand pose to open
        BasicHelpers.SetLocalRotForHierarchy(wrist, Vector3.zero);

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
        indexSiblingIndex = indexRootBone.GetSiblingIndex();
        thumbSiblingIndex = thumbRootBone.GetSiblingIndex();

        // Initialize phModel, masterhandModel and slaveHandModel
        SetupProxyHandModule();

        // Initialize masterWrist and slaveWrist
        SetupMasterObjects();
        SetupSlaveObjects();
        SetupGhostObjects();

        // Setup HPTK models
        SetupMasterHandModel(masterHandModel, masterWrist.transform, masterWristOffset);
        SetupSlaveHandModel(slaveHandModel, slaveWrist.transform); // Depends on phModel.master for automatic rig mapping
        SetupGhostHandModel(ghostHandModel, ghostWrist.transform);

        // Setup special points, triggers, ray (and colliders, rbs and joints if needed)
        FixHand(phModel.master, handType, masterWristOffset);
        FixHand(phModel.slave, handType, null);
        FixHand(phModel.ghost, handType, null);

        // Add lines
        AddLines(phModel.master, Color.blue);
        AddLines(phModel.slave, Color.black);
        AddLines(phModel.ghost, Color.white);

        // Remove .fingers arrays (have been using during generation for automatic rig mapping) but
        // as handRoot children can have fingers in different orders, HandModel fingers have to be set in Inspector and then fingers array will be initialized on Awake
        phModel.master.fingers = null;
        phModel.slave.fingers = null;
        phModel.ghost.fingers = null;
    }

    void SetupMasterObjects()
    {
        // CustomHand > Objects > Master
        masterRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        masterRootObject.name = "Master." + handType.ToString();

        // CustomHand > Objects > Master > (content)
        masterWrist = Instantiate(wrist.gameObject, masterRootObject.transform.position, masterRootObject.transform.rotation);
        masterWrist.transform.parent = masterRootObject.transform;
        masterWrist.transform.name = "Wrist";

        // Store wristOffset
        if (wristOffset)
        {
            masterWristOffset = masterWrist.transform.GetChild(0);
        }

        // Set SkinnedMR material to master material
        /*
        SkinnedMeshRenderer masterSMR = mmasterWrist.GetComponent<SkinnedMeshRenderer>();
        masterSMR.material = masterMat;
        */
    }

    // 1. Wrist tiene SMR
    // 2. Offset tiene rb y joint
    // 3. wristBone.transformRef apunta a Wrist

    void SetupSlaveObjects()
    {
        // CustomHand > Objects > Slave
        slaveRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        slaveRootObject.name = "Slave." + handType.ToString();

        // Move it to avoid overlay
        slaveRootObject.transform.position += new Vector3(0.0f, 0.2f, 0.0f);

        // CustomHand > Objects > Slave > (content)
        Transform handRoot;
        if (wristOffset)
            handRoot = wristOffset;
        else
            handRoot = wrist;

        slaveWrist = Instantiate(handRoot.gameObject, slaveRootObject.transform.position, slaveRootObject.transform.rotation);
        slaveWrist.transform.parent = slaveRootObject.transform;
        slaveWrist.transform.name = "Wrist";

        // Set SkinnedMR material to salve material
        /*
        SkinnedMeshRenderer slaveSMR = slaveWrist.GetComponent<SkinnedMeshRenderer>();
        slaveSMR.material = slaveMat;
        */
    }

    void SetupGhostObjects()
    {
        // CustomHand > Objects > Ghost
        ghostRootObject = BasicHelpers.InstantiateEmptyChild(objects);
        ghostRootObject.name = "Ghost." + handType.ToString();

        // Move it to avoid overlay
        ghostRootObject.transform.position += new Vector3(0.0f, 0.4f, 0.0f);

        // CustomHand > Objects > Ghost > (content)
        Transform handRoot;
        if (wristOffset)
            handRoot = wristOffset;
        else
            handRoot = wrist;

        ghostWrist = Instantiate(handRoot.gameObject, ghostRootObject.transform.position, ghostRootObject.transform.rotation);
        ghostWrist.transform.parent = ghostRootObject.transform;
        ghostWrist.transform.name = "Wrist";

        // Set SkinnedMR material to ghost material
        /*
        SkinnedMeshRenderer ghostSMR = ghostWrist.GetComponent<SkinnedMeshRenderer>();
        ghostSMR.material = ghostMat;
        */
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

        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Slave
        GameObject slaveHandModelObj = BasicHelpers.InstantiateEmptyChild(phModel.gameObject);
        slaveHandModelObj.name = "Slave." + handType.ToString();
        slaveHandModel = slaveHandModelObj.AddComponent<SlaveHandModel>();

        // CustomHand > [Modules] > ProxyHandModule > ProxyHandModel > Ghost
        GameObject ghostHandModelObj = BasicHelpers.InstantiateEmptyChild(phModel.gameObject);
        ghostHandModelObj.name = "Ghost." + handType.ToString();
        ghostHandModel = ghostHandModelObj.AddComponent<HandModel>();

        // Make HandModels accessible from ProxyHandModel
        phModel.master = masterHandModel;
        phModel.slave = slaveHandModel;
        phModel.ghost = ghostHandModel;

        // Make ProxyHandModel accessible from HandModel
        masterHandModel.proxyHand = phModel;
        slaveHandModel.proxyHand = phModel;
        ghostHandModel.proxyHand = phModel;
    }

    void SetupMasterHandModel(MasterHandModel handModel, Transform masterWrist, Transform masterWristOffset)
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
            wristBone.offset = masterWristOffset;
        }

        /* 
        */

        // Set SkinnedMR
        handModel.skinnedMR = masterWrist.GetComponent<SkinnedMeshRenderer>();

        List<FingerModel> fingers = new List<FingerModel>();

        Transform handRoot;
        if (masterWristOffset != null)
            handRoot = masterWristOffset;
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

                bones.Add(masterBone);
            }

            fingerModel.fingerBase = bones[0].transformRef;
            fingerModel.distal = bones[bones.Count - 1];

            fingerModel.bones = bones.ToArray();

            /* MASTER BONE MODEL SPECIFIC
            */

            /*
             */

            fingers.Add(fingerModel);

            if (f == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }

            if (f == thumbSiblingIndex)
            {
                fingerModel.name = "Thumb";
                handModel.thumb = fingerModel;
            }
        }

        // Only available during generation (points generation and automatic rig mapping)
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

                /* SLAVE BONE MODEL SPECIFIC
                 */

                // Simple automatic rig mapping
                if (handModel.proxyHand.master.fingers[f] != null && handModel.proxyHand.master.fingers[f].bones[b] != null)
                    slaveBone.masterBone = handModel.proxyHand.master.fingers[f].bones[b] as MasterBoneModel;

                /* 
                */

                bones.Add(slaveBone);
            }

            fingerModel.fingerBase = bones[0].transformRef;
            fingerModel.distal = bones[bones.Count - 1];

            fingerModel.bones = bones.ToArray();

            fingers.Add(fingerModel);

            if (i == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }

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

            // Increase finger counter
            f++;
        }

        // Only available during generation (points generation and automatic rig mapping)
        handModel.fingers = fingers.ToArray();
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

            fingers.Add(fingerModel);

            if (f == indexSiblingIndex)
            {
                fingerModel.name = "Index";
                handModel.index = fingerModel;
            }

            if (f == thumbSiblingIndex)
            {
                fingerModel.name = "Thumb";
                handModel.thumb = fingerModel;
            }
        }

        // Only available during generation (points generation and automatic rig mapping)
        handModel.fingers = fingers.ToArray();
    }

    void FixHand(HandModel handModel, Side side, Transform wristOffset)
    {
        // Check errors
        if (handModel.fingers.Length == 0)
        {
            Debug.LogError("Fingers array is empty!");
            return;
        }

        // Not all hands need an offset
        if (!wristOffset)
            wristOffset = handModel.wrist.transformRef;

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

            if (side == Side.L)
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

            if (side == Side.L)
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
        if (!handModel.ray)
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
            SetupPhysics(handModel as SlaveHandModel, wristOffset, meanPoint, palmRadius, fingerRadius);
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
        if (wristOffset)
        {
            if(wrist.childCount > 1 || !HandModelIsValid(wristOffset))
            {
                Debug.LogError("Wrist can only have 1 child (wristOffset). wristOffset can have >1 children (fingers)");
                return false;
            }
            if (!thumbRootBone.IsChildOf(wristOffset) || !indexRootBone.IsChildOf(wristOffset))
            {
                Debug.LogError("Thumb root bone and index root bone have to be direct children of wristOffset");
                return false;
            }
            if (!wristOffset.IsChildOf(wrist))
            {
                Debug.LogError("wristOffset has to be son of Wrist");
                return false;
            }
        }
        else
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

    void SetupPhysics(SlaveHandModel slaveHand, Transform wristOffset, Vector3 meanPoint, float palmRadius, float fingerRadius)
    {
        // Wrist collider
        GameObject palmObj = BasicHelpers.InstantiateEmptyChild(wristOffset.gameObject);
        palmObj.name = "Palm.Collider";
        palmObj.transform.position = meanPoint;

        MeshCollider palm = palmObj.AddComponent<MeshCollider>();
        palm.sharedMesh = defaultPalmMesh;
        palm.convex = true;
        palm.material = skinPhysMat;

        Vector3 localScale = new Vector3(2.0f * palmRadius, fingerRadius, 2.0f * palmRadius);
        palmObj.transform.localScale = localScale;

        // Wrist rb
        Rigidbody wristRb = wristOffset.gameObject.AddComponent<Rigidbody>();
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
