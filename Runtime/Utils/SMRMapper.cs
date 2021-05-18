using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    public class SMRMapper : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Transform[] bones;

        public void Read()
        {
            if (!skinnedMeshRenderer) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            bones = skinnedMeshRenderer.bones;
        }

        public void Write()
        {
            if (!skinnedMeshRenderer) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.bones = bones;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!skinnedMeshRenderer) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }
#endif

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SMRMapper)), CanEditMultipleObjects]
public class SMRMapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SMRMapper myScript = (SMRMapper)target;
        if (GUILayout.Button("READ"))
        {
            myScript.Read();
        }
        if (GUILayout.Button("WRITE"))
        {
            myScript.Write();
        }
    }
}
#endif
