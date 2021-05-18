using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Utils;
using System;
using System.Linq;
using HandPhysicsToolkit.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    [ExecuteInEditMode]
    public class HandSearchEngine : BoneSearchEngine
    {
        [Header("Hand")]
        public Side side = Side.Left;

        public Hand hand;
        public SpecialPoints specialPoints;

        [HideInInspector]
        public List<Bone> correctedThumb = new List<Bone>();
        [HideInInspector]
        public List<Bone> correctedIndex = new List<Bone>();
        [HideInInspector]
        public List<Bone> correctedMiddle = new List<Bone>();
        [HideInInspector]
        public List<Bone> correctedRing = new List<Bone>();
        [HideInInspector]
        public List<Bone> correctedPinky = new List<Bone>();
        [HideInInspector]
        public List<Bone> otherBones = new List<Bone>();

        [Header("Debug")]
        public bool showGizmos = true;

        public void AutoFill()
        {
            if (hand.thumb0Point.original && !hand.thumb1Point.original)
            {
                hand.thumb1Point.original = hand.thumb0Point.original.GetFirstChild();
            }

            if (hand.thumb1Point.original && !hand.thumb2Point.original)
            {
                hand.thumb2Point.original = hand.thumb1Point.original.GetFirstChild();
                hand.thumb3Point.original = hand.thumb2Point.original.GetFirstChild();
                hand.thumbTipPoint.original = hand.thumb3Point.original.GetFirstChild();
            }

            if (hand.index1Point.original && !hand.index2Point.original)
            {
                hand.index2Point.original = hand.index1Point.original.GetFirstChild();
                hand.index3Point.original = hand.index2Point.original.GetFirstChild();
                hand.indexTipPoint.original = hand.index3Point.original.GetFirstChild();
            }

            if (hand.middle1Point.original && !hand.middle2Point.original)
            {
                hand.middle2Point.original = hand.middle1Point.original.GetFirstChild();
                hand.middle3Point.original = hand.middle2Point.original.GetFirstChild();
                hand.middleTipPoint.original = hand.middle3Point.original.GetFirstChild();
            }

            if (hand.ring1Point.original && !hand.ring2Point.original)
            {
                hand.ring2Point.original = hand.ring1Point.original.GetFirstChild();
                hand.ring3Point.original = hand.ring2Point.original.GetFirstChild();
                hand.ringTipPoint.original = hand.ring3Point.original.GetFirstChild();
            }

            if (hand.pinky0Point.original && !hand.pinky1Point.original)
            {
                hand.pinky1Point.original = hand.pinky0Point.original.GetFirstChild();
            }

            if (hand.pinky1Point.original && !hand.pinky2Point.original)
            {
                hand.pinky2Point.original = hand.pinky1Point.original.GetFirstChild();
                hand.pinky3Point.original = hand.pinky2Point.original.GetFirstChild();
                hand.pinkyTipPoint.original = hand.pinky3Point.original.GetFirstChild();
            }
        }

        public sealed override void Search()
        {
            if (!hand.IsValid())
            {
                Debug.LogError("Hand has missing references. Bones cannot be searched from hand.");
                return;
            }

            bones.Clear();

            if (hand.thumb0Point.original) bones.Add(new Bone(hand.wristPoint, hand.thumb0Point, 1.0f));
            else bones.Add(new Bone(hand.wristPoint, hand.thumb1Point, 1.0f));

            bones.Add(new Bone(hand.wristPoint, hand.index1Point, 1.0f));
            bones.Add(new Bone(hand.wristPoint, hand.middle1Point, 1.0f));
            bones.Add(new Bone(hand.wristPoint, hand.ring1Point, 1.0f));

            if (hand.pinky0Point.original) bones.Add(new Bone(hand.wristPoint, hand.pinky0Point, 1.0f));
            else bones.Add(new Bone(hand.wristPoint, hand.pinky1Point, 1.0f));

            if (hand.thumb0Point.original) bones.Add(new Bone(hand.thumb0Point, hand.thumb1Point, 1.0f));
            bones.Add(new Bone(hand.thumb1Point, hand.thumb2Point, 1.0f));
            bones.Add(new Bone(hand.thumb2Point, hand.thumb3Point, 1.0f));
            bones.Add(new Bone(hand.thumb3Point, hand.thumbTipPoint, 1.0f));

            bones.Add(new Bone(hand.index1Point, hand.index2Point, 1.0f));
            bones.Add(new Bone(hand.index2Point, hand.index3Point, 1.0f));
            bones.Add(new Bone(hand.index3Point, hand.indexTipPoint, 1.0f));

            bones.Add(new Bone(hand.middle1Point, hand.middle2Point, 1.0f));
            bones.Add(new Bone(hand.middle2Point, hand.middle3Point, 1.0f));
            bones.Add(new Bone(hand.middle3Point, hand.middleTipPoint, 1.0f));

            bones.Add(new Bone(hand.ring1Point, hand.ring2Point, 1.0f));
            bones.Add(new Bone(hand.ring2Point, hand.ring3Point, 1.0f));
            bones.Add(new Bone(hand.ring3Point, hand.ringTipPoint, 1.0f));

            if (hand.pinky0Point.original) bones.Add(new Bone(hand.pinky0Point, hand.pinky1Point, 1.0f));
            bones.Add(new Bone(hand.pinky1Point, hand.pinky2Point, 1.0f));
            bones.Add(new Bone(hand.pinky2Point, hand.pinky3Point, 1.0f));
            bones.Add(new Bone(hand.pinky3Point, hand.pinkyTipPoint, 1.0f));

            UpdateFingerBones();

            bool valid = true;
            bones.ForEach(b => { if (!b.IsValid()) valid = false; });
            if (!valid) Debug.LogError("Some bones were badly generated");
        }

        public void UpdateFingerBones()
        {
            if (hand.wristPoint.corrected != null && bones.Find(b => b.parent.corrected == hand.wristPoint.corrected) == null) Debug.LogError("Corrected transforms are inconsistent");

            correctedThumb = bones.FindAll(b => b.parent.corrected != null && (b.parent.corrected == hand.thumb0Point.corrected || b.parent.corrected == hand.thumb1Point.corrected || b.parent.corrected == hand.thumb2Point.corrected || b.parent.corrected == hand.thumb3Point.corrected));
            correctedIndex = bones.FindAll(b => b.parent.corrected != null && (b.parent.corrected == hand.index1Point.corrected || b.parent.corrected == hand.index2Point.corrected || b.parent.corrected == hand.index3Point.corrected));
            correctedMiddle = bones.FindAll(b => b.parent.corrected != null && (b.parent.corrected == hand.middle1Point.corrected || b.parent.corrected == hand.middle2Point.corrected || b.parent.corrected == hand.middle3Point.corrected));
            correctedRing = bones.FindAll(b => b.parent.corrected != null && (b.parent.corrected == hand.ring1Point.corrected || b.parent.corrected == hand.ring2Point.corrected || b.parent.corrected == hand.ring3Point.corrected));
            correctedPinky = bones.FindAll(b => b.parent.corrected != null && (b.parent.corrected == hand.pinky0Point.corrected || b.parent.corrected == hand.pinky1Point.corrected || b.parent.corrected == hand.pinky2Point.corrected || b.parent.corrected == hand.pinky3Point.corrected));
            otherBones = bones.FindAll(b => !correctedThumb.Contains(b) && !correctedIndex.Contains(b) && !correctedMiddle.Contains(b) && !correctedRing.Contains(b) && !correctedPinky.Contains(b));
        }

        protected sealed override void OnDrawGizmos()
        {
            if (!showGizmos)
                return;

            Gizmos.color = Color.red;
            correctedThumb.ForEach(b => DrawBone(b));

            Gizmos.color = Color.yellow;
            correctedIndex.ForEach(b => DrawBone(b));

            Gizmos.color = Color.blue;
            correctedMiddle.ForEach(b => DrawBone(b));

            Gizmos.color = Color.green;
            correctedRing.ForEach(b => DrawBone(b));

            Gizmos.color = Color.magenta;
            correctedPinky.ForEach(b => DrawBone(b));

            Gizmos.color = Color.white;
            otherBones.ForEach(b => DrawBone(b));

            Gizmos.color = Color.black;
            if (specialPoints.palmCenterPoint.original) DrawPoint(specialPoints.palmCenterPoint.original);
            if (specialPoints.palmNormalPoint.original) DrawPoint(specialPoints.palmNormalPoint.original);
            if (specialPoints.palmExteriorPoint.original) DrawPoint(specialPoints.palmExteriorPoint.original);
            if (specialPoints.palmInteriorPoint.original) DrawPoint(specialPoints.palmInteriorPoint.original);
            if (specialPoints.pinchCenterPoint.original) DrawPoint(specialPoints.pinchCenterPoint.original);
            if (specialPoints.throatCenterPoint.original) DrawPoint(specialPoints.throatCenterPoint.original);
            if (specialPoints.rayPoint.original)
            {
                Gizmos.DrawSphere(specialPoints.rayPoint.original.position, 0.005f);
                Gizmos.DrawLine(specialPoints.rayPoint.original.position, specialPoints.rayPoint.original.position + specialPoints.rayPoint.original.forward * 0.08f);
            }
        }

        void DrawPoint(Transform point)
        {
            Gizmos.DrawSphere(point.position, 0.005f);
            Gizmos.DrawLine(point.position, point.position + point.forward * 0.02f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HandSearchEngine))]
public class HandSearchEngineEditor : BoneSearchEngineEditor
{
    private SerializedProperty szdHand, szdSpecialPoints, szdBones;

    private void OnEnable()
    {
        szdHand = serializedObject.FindProperty("hand");
        szdSpecialPoints = serializedObject.FindProperty("specialPoints");
        szdBones = serializedObject.FindProperty("bones");
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        // DrawDefaultInspector();

        HandSearchEngine myScript = (HandSearchEngine)target;

        GUILayout.Label("Side", EditorStyles.boldLabel);

        myScript.side = (Side)EditorGUILayout.EnumPopup(myScript.side);

        GUILayout.Label("Hand bones", EditorStyles.boldLabel);

        OriginalField(myScript.hand.wristPoint, "Wrist");

        OriginalField(myScript.hand.thumb0Point, "Thumb 0");
        OriginalField(myScript.hand.thumb1Point, "Thumb 1");
        OriginalField(myScript.hand.thumb2Point, "Thumb 2");
        OriginalField(myScript.hand.thumb3Point, "Thumb 3");
        OriginalField(myScript.hand.thumbTipPoint, "Thumb Tip");

        OriginalField(myScript.hand.index1Point, "Index 1");
        OriginalField(myScript.hand.index2Point, "Index 2");
        OriginalField(myScript.hand.index3Point, "Index 3");
        OriginalField(myScript.hand.indexTipPoint, "Index Tip");

        OriginalField(myScript.hand.middle1Point, "Middle 1");
        OriginalField(myScript.hand.middle2Point, "Middle 2");
        OriginalField(myScript.hand.middle3Point, "Middle 3");
        OriginalField(myScript.hand.middleTipPoint, "Middle Tip");

        OriginalField(myScript.hand.ring1Point, "Ring 1");
        OriginalField(myScript.hand.ring2Point, "Ring 2");
        OriginalField(myScript.hand.ring3Point, "Ring 3");
        OriginalField(myScript.hand.ringTipPoint, "Ring Tip");

        OriginalField(myScript.hand.pinky0Point, "Pinky 0");
        OriginalField(myScript.hand.pinky1Point, "Pinky 1");
        OriginalField(myScript.hand.pinky2Point, "Pinky 2");
        OriginalField(myScript.hand.pinky3Point, "Pinky 3");
        OriginalField(myScript.hand.pinkyTipPoint, "Pinky Tip");

        GUILayout.Label("Hand special points", EditorStyles.boldLabel);

        OriginalField(myScript.specialPoints.palmCenterPoint, "Palm Center");
        OriginalField(myScript.specialPoints.palmNormalPoint, "Palm Normal");
        OriginalField(myScript.specialPoints.palmExteriorPoint, "Palm Exterior");
        OriginalField(myScript.specialPoints.palmInteriorPoint, "Palm Interior");
        OriginalField(myScript.specialPoints.pinchCenterPoint, "Pinch Center");
        OriginalField(myScript.specialPoints.throatCenterPoint, "Throat Center");
        OriginalField(myScript.specialPoints.rayPoint, "Ray");

        myScript.AutoFill();

        EditorGUILayout.PropertyField(szdBones);

        GUILayout.Label("Debug", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Show Gizmos");

        myScript.showGizmos = EditorGUILayout.Toggle(myScript.showGizmos);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(szdHand);
        EditorGUILayout.PropertyField(szdSpecialPoints);

        GUI.enabled = myScript.canSearch;
        if (GUILayout.Button("SEARCH BONES"))
        {
            myScript.Search();
        }
        GUI.enabled = true;

        Repaint();
    }
}
#endif
