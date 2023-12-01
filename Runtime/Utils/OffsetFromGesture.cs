using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OffsetFromGesture : MonoBehaviour
{
    [Header("Left")]
    public HandView leftHand;
    public Gesture leftGesture;
    public Transform leftOffset;

    [Header("Right")]
    public HandView rightHand;
    public Gesture rightGesture;
    public Transform rightOffset;

    [Header("Control")]
    public Side calibrateSide = Side.None;
    public bool modifyPos = true;
    public bool modifyRot = false;

    [Header("Debug")]
    public TextMeshProUGUI instructions;

    HandView hand;
    Gesture gesture;
    Transform moveThis;

    Vector3 initWristPos, initObjLocalPos, offsetPos;
    Quaternion initWristRot, initObjLocalRot, offsetRot;

    bool isMoving = false;

    private void Start()
    {
        SetCalibrationSide(calibrateSide, true);
    }

    private void Update()
    {
        if (!leftOffset || !rightOffset)
        {
            instructions.text = "Assign offset references first";
            return;
        }

        if (isMoving)
        {
            offsetPos = MathHelpers.InverseTransformPoint(initWristPos, initWristRot, hand.wrist.master.transformRef.position);
            offsetRot = Quaternion.Inverse(initWristRot) * hand.wrist.master.transformRef.rotation;

            if (modifyPos) moveThis.localPosition = initObjLocalPos + offsetPos;
            if (modifyRot) moveThis.localRotation = initObjLocalRot * offsetRot;
        }
    }

    void OnActivation()
    {
        initWristPos = hand.wrist.master.transformRef.position;
        initWristRot = hand.wrist.master.transformRef.rotation;

        initObjLocalPos = moveThis.localPosition;
        initObjLocalRot = moveThis.localRotation;

        isMoving = true;
    }

    void OnDeactivation()
    {
        isMoving = false;
    }

    void SetCalibrationSide(Side side, bool force = false)
    {
        if (side == calibrateSide && !force) return;

        if (gesture != null)
        {
            gesture.onActivation.RemoveListener(OnActivation);
            gesture.onDeactivation.RemoveListener(OnDeactivation);
        }

        isMoving = false;
        calibrateSide = side;

        switch(side)
        {
            case Side.Left:
                hand = rightHand;
                gesture = rightGesture;
                moveThis = leftOffset;
                instructions.text = "RIGHT pinch to modify LEFT offset";
                break;
            case Side.Right:
                hand = leftHand;
                gesture = leftGesture;
                moveThis = rightOffset;
                instructions.text = "LEFT pinch to modify RIGHT offset";
                break;
            case Side.None:
            default:
                hand = null;
                gesture = null;
                moveThis = null;
                instructions.text = "Select a hand to calibrate...";
                break;
        }

        if (gesture != null)
        {
            gesture.onActivation.AddListener(OnActivation);
            gesture.onDeactivation.AddListener(OnDeactivation);
        }
    }

    public void SetCalibrationSide(string side)
    {
        switch (side.ToLower())
        {
            case "left":
            case "l":
                SetCalibrationSide(Side.Left);
                break;
            case "right":
            case "r":
                SetCalibrationSide(Side.Right);
                break;
            default:
                SetCalibrationSide(Side.None);
                break;
        }
    }

    public void SetPosition(bool modifyPos)
    {
        this.modifyPos = modifyPos;
    }

    public void SetRotation(bool modifyRot)
    {
        this.modifyRot = modifyRot;
    }
}
