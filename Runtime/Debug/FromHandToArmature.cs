using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Models.Avatar;
using HPTK.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FromHandToArmature : MonoBehaviour
{
    public bool onDrawGizmos = true;
    public bool onUpdate = true;

    public HandModel hand;
 
    public Space space;

    void Update()
    {
        if (onUpdate)
            ApplyToArmature();
    }

    public void ApplyToArmature()
    {
        if (!hand)
            return;

        if (!hand.proxyHand)
        {
            Debug.LogError("Hand.ProxyHand is NULL!");
            return;
        }

        if (hand.fingers.Length != hand.proxyHand.master.fingers.Length)
        {
            Debug.LogError("Hand.fingers and Hand.proxyHand.master.fingers have to have the same length!");
            return;
        }

        MasterBoneModel masterWristBone = hand.proxyHand.master.wrist as MasterBoneModel;

        if (masterWristBone.armatureBone)
        {         
            if (space == Space.World)
            {
                masterWristBone.armatureBone.rotation = hand.wrist.transformRef.rotation * masterWristBone.relativeToOriginalArmatureWorld;
            }
            else
            {
                /* unsupported */

                Debug.LogError("Local space is not supported!");
                return;
            }
        }


        for (int f = 0; f < hand.fingers.Length; f++)
        {
            for (int b = 0; b < hand.fingers[f].bones.Length; b++)
            {
                BoneModel bone = hand.fingers[f].bones[b];

                MasterBoneModel masterBone = hand.proxyHand.master.fingers[f].bones[b] as MasterBoneModel;

                if (masterBone.armatureBone)
                {
                    if (space == Space.World)
                    {
                        masterBone.armatureBone.rotation = bone.transformRef.rotation * masterBone.relativeToOriginalArmatureWorld;
                    }
                    else
                    {
                        /* unreachable */
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!onDrawGizmos || onUpdate)
            return;

        ApplyToArmature();
    }


}

#if UNITY_EDITOR
[CustomEditor(typeof(FromHandToArmature))]
public class FromMasterToArmatureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FromHandToArmature myScript = (FromHandToArmature)target;
        if (GUILayout.Button("APPLY"))
        {
            myScript.ApplyToArmature();
        }
    }
}
#endif
