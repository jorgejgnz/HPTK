using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Models.Avatar;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HandModelCaster : MonoBehaviour
{
    public HandModel copyThis;
    public MasterHandModel overwriteThis;

    public void Overwrite()
    {
        overwriteThis.thumb = copyThis.thumb;
        overwriteThis.index = copyThis.index;
        overwriteThis.middle = copyThis.middle;
        overwriteThis.ring = copyThis.ring;
        overwriteThis.pinky = copyThis.pinky;

        for (int i = 0; i < overwriteThis.thumb.bones.Length; i++)
        {
            overwriteThis.thumb.bones[i] = CastBone(overwriteThis.thumb.bones[i]);
            if (i == overwriteThis.thumb.bones.Length - 1)
                overwriteThis.thumb.distal = overwriteThis.thumb.bones[i];
        }

        for (int i = 0; i < overwriteThis.index.bones.Length; i++)
        {
            overwriteThis.index.bones[i] = CastBone(overwriteThis.index.bones[i]);
            if (i == overwriteThis.index.bones.Length - 1)
                overwriteThis.index.distal = overwriteThis.index.bones[i];
        }

        for (int i = 0; i < overwriteThis.middle.bones.Length; i++)
        {
            overwriteThis.middle.bones[i] = CastBone(overwriteThis.middle.bones[i]);
            if (i == overwriteThis.middle.bones.Length - 1)
                overwriteThis.middle.distal = overwriteThis.middle.bones[i];
        }

        for (int i = 0; i < overwriteThis.ring.bones.Length; i++)
        {
            overwriteThis.ring.bones[i] = CastBone(overwriteThis.ring.bones[i]);
            if (i == overwriteThis.ring.bones.Length - 1)
                overwriteThis.ring.distal = overwriteThis.ring.bones[i];
        }

        for (int i = 0; i < overwriteThis.pinky.bones.Length; i++)
        {
            overwriteThis.pinky.bones[i] = CastBone(overwriteThis.pinky.bones[i]);
            if (i == overwriteThis.pinky.bones.Length - 1)
                overwriteThis.pinky.distal = overwriteThis.pinky.bones[i];
        }

        overwriteThis.palmNormal = copyThis.palmNormal;
        overwriteThis.palmCenter = copyThis.palmCenter;
        overwriteThis.palmExterior = copyThis.palmExterior;
        overwriteThis.palmInterior = copyThis.palmInterior;
        overwriteThis.pinchCenter = copyThis.pinchCenter;
        overwriteThis.throatCenter = copyThis.throatCenter;

        overwriteThis.ray = copyThis.ray;

        overwriteThis.skinnedMR = copyThis.skinnedMR;

        overwriteThis.wrist = CastBone(copyThis.wrist);
        overwriteThis.forearm = CastBone(copyThis.forearm);
    }

    public MasterBoneModel CastBone(BoneModel bone)
    {
        MasterBoneModel masterBone = bone.gameObject.AddComponent<MasterBoneModel>();
        masterBone.transformRef = bone.transformRef;
        Destroy(bone);
        return masterBone;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HandModelCaster))]
public class HandModelCasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HandModelCaster myScript = (HandModelCaster)target;
        if (GUILayout.Button("OVERWRITE"))
        {
            myScript.Overwrite();
        }
    }
}
#endif

