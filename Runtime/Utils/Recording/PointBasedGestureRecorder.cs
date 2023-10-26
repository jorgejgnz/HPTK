using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PointBasedGestureRecorder : MonoBehaviour
{
    public HandModel hand;
    public string gestureName = "";

    private Vector3 GetRelativePoint(HandModel hand, HumanFinger finger)
    {
        FingerModel fingerModel = null;
        switch (finger)
        {
            case HumanFinger.Thumb: fingerModel = hand.thumb; break;
            case HumanFinger.Index: fingerModel = hand.index; break;
            case HumanFinger.Middle: fingerModel = hand.middle; break;
            case HumanFinger.Ring: fingerModel = hand.ring; break;
            case HumanFinger.Pinky: fingerModel = hand.pinky; break;
        }

        if (fingerModel != null)
        {
            return hand.wrist.transformRef.InverseTransformPoint(fingerModel.tip.transformRef.position);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void AssignPoints(HandModel hand, PointBasedGestureAsset asset)
    {
        asset.thumbTip = GetRelativePoint(hand, HumanFinger.Thumb);
        asset.indexTip = GetRelativePoint(hand, HumanFinger.Index);
        asset.middleTip = GetRelativePoint(hand, HumanFinger.Middle);
        asset.ringTip = GetRelativePoint(hand, HumanFinger.Ring);
        asset.pinkyTip = GetRelativePoint(hand, HumanFinger.Pinky);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PointBasedGestureRecorder))]
public class PointBasedGestureRecorderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PointBasedGestureRecorder script = (PointBasedGestureRecorder)target;
        if (GUILayout.Button("Save"))
        {
            if (!script.hand)
            {
                Debug.LogError("Hand is not assigned. Gesture can't be recorded.");
                return;
            }
            InstantiateGesture(script);
        }
    }

    public void InstantiateGesture(PointBasedGestureRecorder script)
    {
        PointBasedGestureAsset asset = ScriptableObject.CreateInstance<PointBasedGestureAsset>();
        asset.gestureName = script.gestureName;
        asset.handSide = script.hand.side;
        script.AssignPoints(script.hand, asset);

        string path = "Assets/" + asset.gestureName + ".asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
#endif