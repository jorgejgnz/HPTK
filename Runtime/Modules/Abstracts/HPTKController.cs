using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HandPhysicsToolkit;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit
{
    public abstract class HPTKController : HPTKElement
    {
        public HPTKView genericView { get; protected set; }
        public HPTKModel genericModel { get; protected set; }

        [HideInInspector]
        public bool awaked = false;

        [HideInInspector]
        public bool started = false;

        public virtual void Awake()
        {
            awaked = true;

            if (HPTK.core == null)
            {
                Debug.LogWarning("HPTK singleton was not found in the scene, disabling " + transform.name);
                gameObject.SetActive(false);
            }
        }

        private void Update() { if (!HPTK.core.controlsUpdateCalls) ControllerUpdate(); }

        public virtual void ControllerStart()
        {
            started = true;
        }

        public virtual void ControllerUpdate()
        {
            if (!started) ControllerStart();
        }

        public void SetGeneric(HPTKView view, HPTKModel model) { genericView = view; genericModel = model; }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(HPTKController), editorForChildClasses: true), CanEditMultipleObjects]
public class HPTKControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HPTKController myScript = (HPTKController)target;

        if (GUILayout.Button("AWAKE THIS"))
        {
            Debug.Log("Awaking only one controller...");
            myScript.Awake();
            Debug.Log("Done");
        }
    }
}
#endif
