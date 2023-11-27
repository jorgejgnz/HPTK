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
    public string repr = AvatarModel.key;
    public string gestureName = "";
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
        PoseHelpers.WritePointBasedGesture(script.hand, script.repr, asset);

        string path = "Assets/" + asset.gestureName + ".asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
#endif