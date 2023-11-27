using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLogger : MonoBehaviour
{
    public string prefix;
    public Transform parent;
    public Transform child;

    public string format = "F2";

    private void Start()
    {
        if (child == null) child = transform;
        if (prefix == null || prefix.Length == 0) prefix = gameObject.name;
    }

    private void Update()
    {
        // Pos
        Vector3 pos = child.position;
        if (parent) pos = parent.InverseTransformPoint(pos);

        // Rot
        Quaternion rot = child.rotation;
        if (parent) rot = Quaternion.Inverse(parent.rotation) * child.rotation;

        // Print if possible
        if (RealtimeLog.singleton) RealtimeLog.singleton.Write($"[{prefix.PadRight(15)}]\nPOS:{PrettyPrint(pos,format)}\nROT:{PrettyPrint(rot.eulerAngles,format)}");
    }

    string PrettyPrint(Vector3 v, string format)
    {
        return $"({v.x.ToString(format)},{v.y.ToString(format)},{v.z.ToString(format)})";
    }
}
