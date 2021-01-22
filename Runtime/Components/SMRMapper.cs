using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
