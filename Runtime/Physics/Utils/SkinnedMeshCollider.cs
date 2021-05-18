using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshCollider : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public MeshCollider meshCollider;
    public bool updateEveryFrame = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!skinnedMeshRenderer) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (!meshCollider) meshCollider = GetComponent<MeshCollider>();
    }
#endif

    private void Update()
    {
        if (updateEveryFrame) UpdateCollider();
    }

    public void UpdateCollider()
    {
        Mesh mesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(mesh);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SkinnedMeshCollider)), CanEditMultipleObjects]
public class SkinnedMeshColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SkinnedMeshCollider myScript = (SkinnedMeshCollider)target;
        if (GUILayout.Button("UPDATE MESH COLLIDER"))
        {
            myScript.UpdateCollider();
        }
    }
}
#endif
