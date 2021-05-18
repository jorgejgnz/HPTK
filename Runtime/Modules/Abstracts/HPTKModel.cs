using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit
{
    public abstract class HPTKModel : HPTKElement
    {
        [HideInInspector]
        public bool awaked = false;

        public virtual void Awake()
        {
            awaked = true;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HPTKModel), editorForChildClasses: true), CanEditMultipleObjects]
public class HPTKModelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HPTKModel myScript = (HPTKModel)target;

        if (GUILayout.Button("AWAKE THIS"))
        {
            Debug.Log("Awaking only one model...");
            myScript.Awake();
            Debug.Log("Done");
        }
    }
}
#endif
