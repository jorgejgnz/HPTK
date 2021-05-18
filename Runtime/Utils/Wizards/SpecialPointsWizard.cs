using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace HandPhysicsToolkit.Utils
{
    [RequireComponent(typeof(HandSearchEngine))]
    public class SpecialPointsWizard : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float centerRange = 0.25f;

        public float centerToSurfaceDistance = 6.0f;

        [ReadOnly]
        public HandSearchEngine source;

        Side side { get { return source.side; } }
        Hand hand { get { return source.hand; } }
        SpecialPoints specialPoints { get { return source.specialPoints; } }

        public void FixSpecialPoints()
        {
            if (!source) source = GetComponent<HandSearchEngine>();

            Transform wrist = hand.wristPoint.original;
            Transform index1 = hand.index1Point.original;
            Transform middle1 = hand.middle1Point.original;
            Transform ring1 = hand.middle1Point.original;
            Transform pinky1 = hand.pinky1Point.original;
            Transform thumb1 = hand.thumb1Point.original;
            Transform indexTip = hand.indexTipPoint.original;
            Transform thumbTip = hand.thumbTipPoint.original;

            Vector3 wristToRing1 = ring1.position - wrist.position;
            Vector3 wristToIndex1 = index1.position - wrist.position;

            Vector3 worldPalmNormalDir;

            if (side == Side.Left) worldPalmNormalDir = Vector3.Cross(wristToIndex1, wristToRing1);
            else worldPalmNormalDir = Vector3.Cross(wristToRing1, wristToIndex1);

            Vector3 frontFingerCenter = (index1.position + middle1.position + ring1.position + pinky1.position) / 4.0f;
            Vector3 worldPalmCenterPos = (wrist.position * centerRange + frontFingerCenter * (1.0f - centerRange));

            Vector3 centerToWristDir = wrist.position - worldPalmCenterPos;
            Vector3 frontFingerCenterToIndex1Dir = index1.position - frontFingerCenter;

            Vector3 frontFingerCenterToIndex = index1.position - frontFingerCenter;

            if (!specialPoints.palmCenterPoint.original)
            {
                specialPoints.palmCenterPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "PalmCenter." + side.ToString()).transform;
                specialPoints.palmCenterPoint.original.position = worldPalmCenterPos + (worldPalmNormalDir * centerToSurfaceDistance);
                specialPoints.palmCenterPoint.original.rotation = Quaternion.LookRotation(Vector3.Cross(centerToWristDir, frontFingerCenterToIndex1Dir), frontFingerCenterToIndex1Dir);
            }

            if (!specialPoints.palmNormalPoint.original)
            {
                specialPoints.palmNormalPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "PalmNormal." + side.ToString()).transform;
                specialPoints.palmNormalPoint.original.position = worldPalmCenterPos + (worldPalmNormalDir * centerToSurfaceDistance);
                specialPoints.palmNormalPoint.original.rotation = Quaternion.LookRotation(worldPalmNormalDir, frontFingerCenterToIndex1Dir);
            }

            if (!specialPoints.palmInteriorPoint.original)
            {
                specialPoints.palmInteriorPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "PalmInterior." + side.ToString()).transform;
                specialPoints.palmInteriorPoint.original.rotation = specialPoints.palmCenterPoint.original.rotation;
                specialPoints.palmInteriorPoint.original.position = specialPoints.palmCenterPoint.original.position + frontFingerCenterToIndex;
            }

            if (!specialPoints.palmExteriorPoint.original)
            {
                specialPoints.palmExteriorPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "PalmExterior." + side.ToString()).transform;
                specialPoints.palmExteriorPoint.original.rotation = specialPoints.palmCenterPoint.original.rotation;
                specialPoints.palmExteriorPoint.original.position = specialPoints.palmCenterPoint.original.position - frontFingerCenterToIndex;
            }

            if (!specialPoints.rayPoint.original)
            {
                specialPoints.rayPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "Ray." + side.ToString()).transform;
                specialPoints.rayPoint.original.position = specialPoints.palmCenterPoint.original.position;
                specialPoints.rayPoint.original.rotation = specialPoints.palmNormalPoint.original.rotation;
            }

            if (!specialPoints.pinchCenterPoint.original)
            {
                specialPoints.pinchCenterPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "PinchCenter." + side.ToString()).transform;
            }

            if (!specialPoints.throatCenterPoint.original)
            {
                specialPoints.throatCenterPoint.original = BasicHelpers.InstantiateEmptyChild(wrist.gameObject, "ThroatCenter." + side.ToString()).transform;
            }

            PositionConstraint pinchPosConstraint = specialPoints.pinchCenterPoint.original.GetComponent<PositionConstraint>();
            LookAtConstraint pinchRotConstraint = specialPoints.pinchCenterPoint.original.GetComponent<LookAtConstraint>();

            PositionConstraint throatPosConstraint = specialPoints.throatCenterPoint.original.GetComponent<PositionConstraint>();
            LookAtConstraint throatRotConstraint = specialPoints.throatCenterPoint.original.GetComponent<LookAtConstraint>();

            ConstraintSource indexTipSource = new ConstraintSource();
            indexTipSource.sourceTransform = indexTip;
            indexTipSource.weight = 1.0f;

            ConstraintSource index1Source = new ConstraintSource();
            index1Source.sourceTransform = index1;
            index1Source.weight = 1.0f;

            ConstraintSource thumbTipSource = new ConstraintSource();
            thumbTipSource.sourceTransform = thumbTip;
            thumbTipSource.weight = 1.0f;

            ConstraintSource thumb1Source = new ConstraintSource();
            thumb1Source.sourceTransform = thumb1;
            thumb1Source.weight = 1.0f;

            if (pinchPosConstraint == null)
            {
                pinchPosConstraint = specialPoints.pinchCenterPoint.original.gameObject.AddComponent<PositionConstraint>();
                pinchPosConstraint.constraintActive = true;
                pinchPosConstraint.AddSource(indexTipSource);
                pinchPosConstraint.AddSource(thumbTipSource);

                pinchPosConstraint.locked = false;
                pinchPosConstraint.translationOffset = pinchPosConstraint.translationAtRest = Vector3.zero;
                pinchPosConstraint.locked = true;
            }

            if (pinchRotConstraint == null)
            {
                pinchRotConstraint = specialPoints.pinchCenterPoint.original.gameObject.AddComponent<LookAtConstraint>();
                pinchRotConstraint.constraintActive = true;
                pinchRotConstraint.AddSource(indexTipSource);

                pinchRotConstraint.locked = false;
                pinchRotConstraint.rotationOffset = pinchRotConstraint.rotationAtRest = Vector3.zero;
                pinchRotConstraint.locked = true;
            }

            if (throatPosConstraint == null)
            {
                throatPosConstraint = specialPoints.throatCenterPoint.original.gameObject.AddComponent<PositionConstraint>();
                throatPosConstraint.constraintActive = true;
                throatPosConstraint.AddSource(index1Source);
                throatPosConstraint.AddSource(thumb1Source);

                throatPosConstraint.locked = false;
                throatPosConstraint.translationOffset = throatPosConstraint.translationAtRest = Vector3.zero;
                throatPosConstraint.locked = true;
            }

            if (throatRotConstraint == null)
            {
                throatRotConstraint = specialPoints.throatCenterPoint.original.gameObject.AddComponent<LookAtConstraint>();
                throatRotConstraint.constraintActive = true;
                throatRotConstraint.AddSource(indexTipSource);

                throatRotConstraint.locked = false;
                throatRotConstraint.rotationOffset = throatRotConstraint.rotationAtRest = Vector3.zero;
                throatRotConstraint.locked = true;
            }
        }

        public void DestroySpecialPoints()
        {
            if (!source) source = GetComponent<HandSearchEngine>();

            if (specialPoints.palmCenterPoint.original) DestroyImmediate(specialPoints.palmCenterPoint.original.gameObject);
            if (specialPoints.palmExteriorPoint.original) DestroyImmediate(specialPoints.palmExteriorPoint.original.gameObject);
            if (specialPoints.palmInteriorPoint.original) DestroyImmediate(specialPoints.palmInteriorPoint.original.gameObject);
            if (specialPoints.palmNormalPoint.original) DestroyImmediate(specialPoints.palmNormalPoint.original.gameObject);
            if (specialPoints.pinchCenterPoint.original) DestroyImmediate(specialPoints.pinchCenterPoint.original.gameObject);
            if (specialPoints.throatCenterPoint.original) DestroyImmediate(specialPoints.throatCenterPoint.original.gameObject);
            if (specialPoints.rayPoint.original) DestroyImmediate(specialPoints.rayPoint.original.gameObject);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpecialPointsWizard))]
public class SpecialPointsWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpecialPointsWizard myScript = (SpecialPointsWizard)target;

        if (GUILayout.Button("FIX SPECIAL POINTS"))
        {
            myScript.FixSpecialPoints();
        }

        if (GUILayout.Button("DESTROY SPECIAL POINTS"))
        {
            myScript.DestroySpecialPoints();
        }
    }
}
#endif
