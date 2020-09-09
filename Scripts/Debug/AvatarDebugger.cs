using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvatarDebugger : MonoBehaviour
{
    public AvatarModel avatar;
    public TextMeshPro handTmpro;

    public TextMeshPro LMfingersTmpro;
    public TextMeshPro LSfingersTmpro;
    public TextMeshPro RMfingersTmpro;
    public TextMeshPro RSfingersTmpro;

    private void Update()
    {
        handTmpro.text = "LEFT\n";
        handTmpro.text += "\tMASTER\n";
        handTmpro.text += "Fist: " + avatar.leftHand.master.fistLerp + "\n";
        handTmpro.text += "Grasp: " + avatar.leftHand.master.graspLerp + "\n";
        handTmpro.text += "Grasp speed: " + avatar.leftHand.master.graspSpeed + "\n";
        handTmpro.text += "ERROR: " + avatar.leftHand.error + "\n";

        handTmpro.text += "RIGHT\n";
        handTmpro.text += "\tMASTER\n";
        handTmpro.text += "Fist: " + avatar.rightHand.master.fistLerp + "\n";
        handTmpro.text += "Grasp: " + avatar.rightHand.master.graspLerp + "\n";
        handTmpro.text += "Grasp speed: " + avatar.rightHand.master.graspSpeed + "\n";
        handTmpro.text += "ERROR: " + avatar.rightHand.error + "\n";

        LMfingersTmpro.text = "";
        for (int i = 0; i < avatar.leftHand.master.fingers.Length; i++)
        {
            PrintFinger(avatar.leftHand.master.fingers[i],LMfingersTmpro);
        }

        LSfingersTmpro.text = "";
        for (int i = 0; i < avatar.leftHand.slave.fingers.Length; i++)
        {
            PrintFinger(avatar.leftHand.slave.fingers[i], LSfingersTmpro);
        }

        RMfingersTmpro.text = "";
        for (int i = 0; i < avatar.rightHand.master.fingers.Length; i++)
        {
            PrintFinger(avatar.rightHand.master.fingers[i], RMfingersTmpro);
        }

        RSfingersTmpro.text = "";
        for (int i = 0; i < avatar.rightHand.slave.fingers.Length; i++)
        {
            PrintFinger(avatar.rightHand.slave.fingers[i], RSfingersTmpro);
        }
    }

    void PrintFinger(FingerModel finger, TextMeshPro tmpro)
    {
        tmpro.text += "\n" + finger.name + "\n";
        tmpro.text += "Pinch: " + finger.pinchLerp + "\n";
        tmpro.text += "Pinch speed: " + finger.pinchSpeed + "\n";
        tmpro.text += "BaseRot: " + finger.baseRotationLerp + "\n";
        tmpro.text += "Flex: " + finger.flexLerp + "\n";
        tmpro.text += "Palm: " + finger.palmLineLerp + "\n";
        tmpro.text += "Strength: " + finger.strengthLerp + "\n";
    }
}
