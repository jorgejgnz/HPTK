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
    public class BodySearchEngine : BoneSearchEngine
    {
        [Header("Body")]
        public Body body;

        [Header("Debug")]
        public bool showGizmos = true;

        public sealed override void Search()
        {
            if (!body.IsValid())
            {
                Debug.LogError("Body has missing references. Bones cannot be searched from body.");
                return;
            }

            bones.Clear();

            bones.Add(new Bone(body.hipsPoint, body.spinePoint, 2.8f));
            bones.Add(new Bone(body.hipsPoint, body.leftThighPoint, 5.0f));
            bones.Add(new Bone(body.hipsPoint, body.rightThighPoint, 5.0f));

            bones.Add(new Bone(body.leftThighPoint, body.leftCalfPoint, 1.0f));
            bones.Add(new Bone(body.leftCalfPoint, body.leftFootPoint, 0.8f));
            bones.Add(new Bone(body.leftFootPoint, body.leftToesPoint, 2.0f));

            bones.Add(new Bone(body.rightThighPoint, body.rightCalfPoint, 1.0f));
            bones.Add(new Bone(body.rightCalfPoint, body.rightFootPoint, 0.8f));
            bones.Add(new Bone(body.rightFootPoint, body.rightToesPoint, 2.0f));

            bones.Add(new Bone(body.spinePoint, body.chestPoint, 6.0f));
            bones.Add(new Bone(body.chestPoint, body.neckPoint, 5.0f));

            bones.Add(new Bone(body.chestPoint, body.leftUpperArmPoint, 2.2f));     
            bones.Add(new Bone(body.chestPoint, body.rightUpperArmPoint, 2.2f));

            //bones.Add(new Bone(body.neckPoint, body.leftUpperArmPoint, 1.5f));
            //bones.Add(new Bone(body.neckPoint, body.rightUpperArmPoint, 1.5f));

            bones.Add(new Bone(body.neckPoint, body.headPoint, 5.0f));
            bones.Add(new Bone(body.headPoint, body.headTopPoint, 2.5f));
            // Head to eyes is not a bone

            bones.Add(new Bone(body.leftShoulderPoint, body.leftUpperArmPoint, 2.0f));
            bones.Add(new Bone(body.leftUpperArmPoint, body.leftForearmPoint, 1.0f));
            bones.Add(new Bone(body.leftForearmPoint, body.leftHandPoint, 0.8f));

            bones.Add(new Bone(body.rightShoulderPoint, body.rightUpperArmPoint, 2.0f));
            bones.Add(new Bone(body.rightUpperArmPoint, body.rightForearmPoint, 1.0f));
            bones.Add(new Bone(body.rightForearmPoint, body.rightHandPoint, 0.8f));

            bool valid = true;
            bones.ForEach(b => { if (!b.IsValid()) valid = false; });
            if (!valid) Debug.LogError("Some bones were badly generated");
        }

        public void AutoFill()
        {
            if (body.leftShoulderPoint.original && !body.leftUpperArmPoint.original)
            {
                body.leftUpperArmPoint.original = body.leftShoulderPoint.original.GetFirstChild();
                body.leftForearmPoint.original = body.leftUpperArmPoint.original.GetFirstChild();
                body.leftHandPoint.original = body.leftForearmPoint.original.GetFirstChild();
            }

            if (body.rightShoulderPoint.original && !body.rightUpperArmPoint.original)
            {
                body.rightUpperArmPoint.original = body.rightShoulderPoint.original.GetFirstChild();
                body.rightForearmPoint.original = body.rightUpperArmPoint.original.GetFirstChild();
                body.rightHandPoint.original = body.rightForearmPoint.original.GetFirstChild();
            }

            if (body.leftThighPoint.original && !body.leftCalfPoint.original)
            {
                body.leftCalfPoint.original = body.leftThighPoint.original.GetFirstChild();
                body.leftFootPoint.original = body.leftCalfPoint.original.GetFirstChild();
                body.leftToesPoint.original = body.leftFootPoint.original.GetFirstChild();
            }

            if (body.rightThighPoint.original && !body.rightCalfPoint.original)
            {
                body.rightCalfPoint.original = body.rightThighPoint.original.GetFirstChild();
                body.rightFootPoint.original = body.rightCalfPoint.original.GetFirstChild();
                body.rightToesPoint.original = body.rightFootPoint.original.GetFirstChild();
            }
        }

        protected sealed override void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            bones.ForEach(b => DrawBone(b));

            if (body.eyesPoint.original)
            {
                Gizmos.DrawSphere(body.eyesPoint.original.position, 0.01f);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BodySearchEngine))]
public class BodySearchEngineEditor : BoneSearchEngineEditor
{
    private SerializedProperty szdBody, szdBones;

    private void OnEnable()
    {
        szdBody = serializedObject.FindProperty("body");
        szdBones = serializedObject.FindProperty("bones");
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        // DrawDefaultInspector();

        BodySearchEngine myScript = (BodySearchEngine)target;

        GUILayout.Label("Body", EditorStyles.boldLabel);

        OriginalField(myScript.body.hipsPoint, "Hips");
        OriginalField(myScript.body.spinePoint, "Spine");
        OriginalField(myScript.body.chestPoint, "Chest");
        OriginalField(myScript.body.neckPoint, "Neck");
        OriginalField(myScript.body.headPoint, "Head");
        OriginalField(myScript.body.eyesPoint, "Eyes");
        OriginalField(myScript.body.headTopPoint, "Head Top");

        OriginalField(myScript.body.leftShoulderPoint, "Left Shoulder");
        OriginalField(myScript.body.leftUpperArmPoint, "Left Upper Arm");
        OriginalField(myScript.body.leftForearmPoint, "Left Forearm");
        OriginalField(myScript.body.leftHandPoint, "Left Hand Wrist");

        OriginalField(myScript.body.rightShoulderPoint, "Right Shoulder");
        OriginalField(myScript.body.rightUpperArmPoint, "Right Upper Arm");
        OriginalField(myScript.body.rightForearmPoint, "Right Forearm");
        OriginalField(myScript.body.rightHandPoint, "Right Hand Wrist");

        OriginalField(myScript.body.leftThighPoint, "Left Thigh");
        OriginalField(myScript.body.leftCalfPoint, "Left Calf");
        OriginalField(myScript.body.leftFootPoint, "Left Foot");
        OriginalField(myScript.body.leftToesPoint, "Left Toes");

        OriginalField(myScript.body.rightThighPoint, "Right Thigh");
        OriginalField(myScript.body.rightCalfPoint, "Right Calf");
        OriginalField(myScript.body.rightFootPoint, "Right Foot");
        OriginalField(myScript.body.rightToesPoint, "Right Toes");

        myScript.AutoFill();

        EditorGUILayout.PropertyField(szdBones);

        GUILayout.Label("Debug", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Show Gizmos");

        myScript.showGizmos = EditorGUILayout.Toggle(myScript.showGizmos);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(szdBody);

        GUI.enabled = myScript.canSearch;
        if (GUILayout.Button("SEARCH BONES"))
        {
            myScript.Search();
        }
        GUI.enabled = true;
    }
}
#endif
