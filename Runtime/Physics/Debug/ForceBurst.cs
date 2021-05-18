using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceBurst : MonoBehaviour
{
    public float everySeconds = 0.5f;
    public float strength = 1.0f;
    public ForceMode mode;
    public Rigidbody rb;
    public bool gizmos = true;
    public float gizmoSize = 0.1f;

    float timeRemaining = 0.0f;

    private void FixedUpdate()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0.0f)
        {
            rb.AddForce(transform.forward * strength, mode);
            timeRemaining = everySeconds;
        }
    }

    public void Activate()
    {
        rb.AddForce(transform.forward * strength, mode);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!gizmos)
            return;

        // Draw finger direction
        Handles.color = Color.blue;
        Handles.ArrowHandleCap(
            0,
            transform.position,
            Quaternion.LookRotation(transform.forward),
            gizmoSize,
            EventType.Repaint
        );
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ForceBurst), editorForChildClasses: true), CanEditMultipleObjects]
public class ForceBurstEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ForceBurst myScript = (ForceBurst)target;

        if (GUILayout.Button("ACTIVATE"))
        {
            myScript.Activate();
        }
    }
}
#endif