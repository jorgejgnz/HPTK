using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BidirectionalAssignation : MonoBehaviour
{
    public Transform value;
}

#if UNITY_EDITOR
[CustomEditor(typeof(BidirectionalAssignation))]
public class BidirectionalAssignationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BidirectionalAssignation myScript = (BidirectionalAssignation)target;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value (Custom)");
        myScript.value = EditorGUILayout.ObjectField(myScript.value, typeof(Transform), true) as Transform;

        EditorGUILayout.EndHorizontal();
    }
}
#endif
