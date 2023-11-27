
namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    using HandPhysicsToolkit.Helpers;
    using HandPhysicsToolkit.Modules.Avatar;
    using System.Collections;
    using UnityEngine;

    public class ArticulationBodyFollower : MonoBehaviour
    {
        public ArticulationBody ArticulationBody;
        public Transform Target;

        [Header("Prismatic Joint Settings")]
        public ArticulationBody XAxisPositionBody;
        public ArticulationBody YAxisPositionBody;
        public ArticulationBody ZAxisPositionBody;

        [Header("Revolute Joint Settings")]
        public ArticulationBody YAxisRotationBody;
        public ArticulationBody XAxisRotationBody;
        public ArticulationBody ZAxisRotationBody;
        public bool UseWorldRotation;

        [Header("Spherical Joint Settings")]
        public ArticulationBody AllAxesRotationBody;
        public ArticulationBody Root;

        Vector3 StartingPosition, PositionDelta;
        Vector3 StartingRotation, RotationDelta, PreviousParentAnchorRotation;
        float CurrentAngleInRange;
        float CurrentAngleDifferenceInRange;
        int OperationSign = 1;

        void Start()
        {
            SetStartingPositionAndRotation();
        }

        void Update()
        {
            // UpdateArticulationBodyScale();
        }

        void FixedUpdate()
        {
            UpdatePositionBodies();

            UpdateRotationBodies();
        }

        void SetStartingPositionAndRotation()
        {
            StartingPosition = transform.position;

            StartingRotation = UseWorldRotation ? Target.rotation.eulerAngles : Target.localRotation.eulerAngles;

            PreviousParentAnchorRotation = ArticulationBody.parentAnchorRotation.eulerAngles;
        }

        void UpdatePositionBodies()
        {
            if (XAxisPositionBody || YAxisPositionBody || ZAxisPositionBody)
            {
                PositionDelta = MathHelpers.InverseTransformPoint(StartingPosition, Quaternion.Euler(StartingRotation), Target.position);
            }

            SetArticulationBodyXDrive(XAxisPositionBody, PositionDelta.x);
            SetArticulationBodyYDrive(YAxisPositionBody, PositionDelta.y);
            SetArticulationBodyZDrive(ZAxisPositionBody, PositionDelta.z);
        }

        void UpdateRotationBodies()
        {
            if (Root)
            {
                ArticulationBody.parentAnchorRotation = Quaternion.Inverse(Root.transform.rotation) * Target.rotation;
                return;
            }

            if (YAxisRotationBody || XAxisRotationBody || ZAxisRotationBody || AllAxesRotationBody)
            {
                if (UseWorldRotation)
                {
                    RotationDelta = Target.rotation.eulerAngles;
                }
                else
                {
                    RotationDelta = Target.localRotation.eulerAngles - StartingRotation;
                }
            }
            else
            {
                return;
            }

            if (AllAxesRotationBody)
            {
                SetArticulationBodyYDrive(AllAxesRotationBody, FixAngleJump(AllAxesRotationBody.yDrive, RotationDelta.y, AllAxesRotationBody.yDrive.target));
                SetArticulationBodyXDrive(AllAxesRotationBody, FixAngleJump(AllAxesRotationBody.xDrive, RotationDelta.x, AllAxesRotationBody.xDrive.target));
                SetArticulationBodyZDrive(AllAxesRotationBody, FixAngleJump(AllAxesRotationBody.zDrive, RotationDelta.z, AllAxesRotationBody.zDrive.target));

                return;
            }

            // The axes have to be rotated in a specific order to work properly: Y->X->Z
            if (YAxisRotationBody)
            {
                SetArticulationBodyXDrive(YAxisRotationBody, FixAngleJump(YAxisRotationBody.xDrive, RotationDelta.y, YAxisRotationBody.xDrive.target));
            }
            else
            {
                if (transform.localRotation.eulerAngles.y != Target.localRotation.eulerAngles.y)
                {
                    AdjustParentAnchorRotation(Quaternion.Euler(
                        PreviousParentAnchorRotation.x,
                        PreviousParentAnchorRotation.y +
                        (Target.localRotation.eulerAngles.y - transform.localRotation.eulerAngles.y),
                        PreviousParentAnchorRotation.z
                    ));
                }
            }

            if (XAxisRotationBody)
            {
                SetArticulationBodyXDrive(XAxisRotationBody, FixAngleJump(XAxisRotationBody.xDrive, RotationDelta.x, XAxisRotationBody.xDrive.target));
            }
            else
            {
                if (transform.localRotation.eulerAngles.x != Target.localRotation.eulerAngles.x)
                {
                    AdjustParentAnchorRotation(Quaternion.Euler(
                        PreviousParentAnchorRotation.x,
                        PreviousParentAnchorRotation.y,
                        PreviousParentAnchorRotation.z  +
                        (Target.localRotation.eulerAngles.x - transform.localRotation.eulerAngles.x)
                    ));
                }
            }

            if (ZAxisRotationBody)
            {
                SetArticulationBodyXDrive(ZAxisRotationBody, FixAngleJump(ZAxisRotationBody.xDrive, RotationDelta.z, ZAxisRotationBody.xDrive.target));
            }
            else
            {
                if (transform.localRotation.eulerAngles.z != Target.localRotation.eulerAngles.z)
                {
                    AdjustParentAnchorRotation(Quaternion.Euler(
                        PreviousParentAnchorRotation.x  +
                        (Target.localRotation.eulerAngles.z - transform.localRotation.eulerAngles.z),
                        PreviousParentAnchorRotation.y,
                        PreviousParentAnchorRotation.z
                    ));
                }
            }
        }

        void SetArticulationBodyXDrive(ArticulationBody ab, float targetValue)
        {
            if (ab)
            {
                var xDrive = ab.xDrive;
                xDrive.target = targetValue;
                ab.xDrive = xDrive;
            }
        }

        void SetArticulationBodyYDrive(ArticulationBody ab, float targetValue)
        {
            if (ab)
            {
                var yDrive = ab.yDrive;
                yDrive.target = targetValue;
                ab.yDrive = yDrive;
            }
        }

        void SetArticulationBodyZDrive(ArticulationBody ab, float targetValue)
        {
            if (ab)
            {
                var zDrive = ab.zDrive;
                zDrive.target = targetValue;
                ab.zDrive = zDrive;
            }
        }

        void AdjustParentAnchorRotation(Quaternion newRotation)
        {
            ArticulationBody.matchAnchors = false;

            ArticulationBody.parentAnchorRotation = newRotation;

            PreviousParentAnchorRotation = ArticulationBody.parentAnchorRotation.eulerAngles;
        }

        float FixAngleJump(ArticulationDrive drive, float targetAngleInRange, float currentAngle)
        {
            CurrentAngleInRange = currentAngle % 360f;
            if (CurrentAngleInRange < 0)
            {
                CurrentAngleInRange += 360;
            }
            if (targetAngleInRange < 0)
            {
                targetAngleInRange += 360;
            }
            CurrentAngleDifferenceInRange = Mathf.Abs(CurrentAngleInRange - targetAngleInRange);

            if (CurrentAngleDifferenceInRange < 0.1f)
            {
                return currentAngle;
            }

            if (CurrentAngleDifferenceInRange > 180f)
            {
                if (CurrentAngleInRange <= targetAngleInRange)
                {
                    OperationSign = -1;
                    CurrentAngleDifferenceInRange = CurrentAngleInRange + (360f - targetAngleInRange);
                }
                else
                {
                    OperationSign = 1;
                    CurrentAngleDifferenceInRange = targetAngleInRange + (360f - CurrentAngleInRange);
                }
            }
            else
            {
                if (CurrentAngleInRange <= targetAngleInRange)
                {
                    OperationSign = 1;
                }
                else
                {
                    OperationSign = -1;
                }
            }

            currentAngle += (float)OperationSign * CurrentAngleDifferenceInRange;

            return currentAngle;
        }

        void AdjustParentAnchorPositionOnRescale(ArticulationBody ab)
        {
            ab.matchAnchors = false;

            Vector3 newParentAnchorPosition = Target.localPosition;

            if (!ab.transform.parent.GetComponent<ArticulationBody>())
            {
                newParentAnchorPosition += Target.parent.localPosition;
            }

            ab.parentAnchorPosition = newParentAnchorPosition;
        }

        public void OnScaleChange()
        {
            if (YAxisRotationBody)
            {
                AdjustParentAnchorPositionOnRescale(YAxisRotationBody);

                YAxisRotationBody.parentAnchorRotation = Quaternion.Euler(
                    YAxisRotationBody.anchorRotation.eulerAngles.x,
                    Target.localRotation.eulerAngles.y,
                    YAxisRotationBody.anchorRotation.eulerAngles.z
                );
            }
            else if (ZAxisRotationBody)
            {
                AdjustParentAnchorPositionOnRescale(ZAxisRotationBody);

                ZAxisRotationBody.parentAnchorRotation = Quaternion.Euler(
                    ZAxisRotationBody.anchorRotation.eulerAngles.x,
                    ZAxisRotationBody.anchorRotation.eulerAngles.y,
                    Target.localRotation.eulerAngles.z
                );
            }
        }
    }
}

