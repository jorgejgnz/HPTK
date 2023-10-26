using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "HPTK/PointBasedGesture")]
public class PointBasedGestureAsset : ScriptableObject
{
    public string gestureName;
    public Side handSide;
    public Vector3 thumbTip;
    public Vector3 indexTip;
    public Vector3 middleTip;
    public Vector3 ringTip;
    public Vector3 pinkyTip;

    public PointBasedGestureAsset(string name)
    {
        gestureName = name;
    }
}
