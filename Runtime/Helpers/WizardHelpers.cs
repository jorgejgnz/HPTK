using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class WizardHelpers
    {
        public static void LinkBodyPointReprs(BodySearchEngine source, BodyModel body, bool includeHands)
        {
            // Torso
            LinkPointRepr(source.body.hipsPoint, body.torso.hips.point, false);
            LinkPointRepr(source.body.spinePoint, body.torso.spine.point, false);
            LinkPointRepr(source.body.chestPoint, body.torso.chest.point, false);
            LinkPointRepr(source.body.neckPoint, body.torso.neck.point, false);
            LinkPointRepr(source.body.headPoint, body.torso.head.point, false);
            LinkPointRepr(source.body.eyesPoint, body.torso.eyes, false);
            LinkPointRepr(source.body.headTopPoint, body.torso.headTop, false);

            // Left arm
            LinkPointRepr(source.body.leftShoulderPoint, body.leftArm.shoulder.point, false);
            LinkPointRepr(source.body.leftUpperArmPoint, body.leftArm.upper.point, false);
            LinkPointRepr(source.body.leftForearmPoint, body.leftArm.forearm.point, false);
            if (includeHands) LinkPointRepr(source.body.leftHandPoint, body.leftArm.hand.wrist.point, true);

            // Right arm
            LinkPointRepr(source.body.rightShoulderPoint, body.rightArm.shoulder.point, false);
            LinkPointRepr(source.body.rightUpperArmPoint, body.rightArm.upper.point, false);
            LinkPointRepr(source.body.rightForearmPoint, body.rightArm.forearm.point, false);
            if (includeHands) LinkPointRepr(source.body.rightHandPoint, body.rightArm.hand.wrist.point, true);

            // Left leg
            LinkPointRepr(source.body.leftThighPoint, body.leftLeg.thigh.point, false);
            LinkPointRepr(source.body.leftCalfPoint, body.leftLeg.calf.point, false);
            LinkPointRepr(source.body.leftFootPoint, body.leftLeg.foot.point, false);
            LinkPointRepr(source.body.leftToesPoint, body.leftLeg.toes, false);

            // Right leg
            LinkPointRepr(source.body.rightThighPoint, body.rightLeg.thigh.point, false);
            LinkPointRepr(source.body.rightCalfPoint, body.rightLeg.calf.point, false);
            LinkPointRepr(source.body.rightFootPoint, body.rightLeg.foot.point, false);
            LinkPointRepr(source.body.rightToesPoint, body.rightLeg.toes, false);
        }

        public static void LinkHandPointReprs(HandSearchEngine source, BodyModel body, bool includeWrist, bool requiresCorrected)
        {
            HandModel hand;
            if (source.side == Helpers.Side.Left) hand = body.leftHand;
            else hand = body.rightHand;

            // Prevent invalid model
            string msg = GetErrorMsg(hand);
            if (msg != "")
            {
                Debug.LogError(msg);
                return;
            }

            if (requiresCorrected && !source.hand.wristPoint.corrected)
            {
                Debug.LogError("Wrist has no corrected representation. Probably rotations have not been corrected yet (use RotationWizard)");
                return;
            }

            // Detect inconsistencies model-representation
            if ((!source.hand.thumb0Point.tsf && hand.thumb.threeUnderLast) || (source.hand.thumb0Point.tsf && !hand.thumb.threeUnderLast))
            {
                Debug.LogError("Thumb0 is not present in both model and correct representation. Representation? " + (source.hand.thumb0Point.tsf != null) + ". Model? " + (hand.thumb.threeUnderLast != null));
                return;
            }

            if ((!source.hand.pinky0Point.tsf && hand.pinky.threeUnderLast) || (source.hand.pinky0Point.tsf && !hand.pinky.threeUnderLast))
            {
                Debug.LogError("Pinky0 is not present in both model and correct representation. Representation?" + (source.hand.pinky0Point.tsf != null) + ". Model? " + (hand.pinky.threeUnderLast != null));
                return;
            }

            // Wrist
            if (includeWrist) LinkPointRepr(source.hand.wristPoint, hand.wrist.point, requiresCorrected);

            // Thumb
            if (source.hand.thumb0Point.original) LinkPointRepr(source.hand.thumb0Point, hand.thumb.threeUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.thumb1Point, hand.thumb.twoUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.thumb2Point, hand.thumb.oneUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.thumb3Point, hand.thumb.last.point, requiresCorrected);
            LinkPointRepr(source.hand.thumbTipPoint, hand.thumb.tip, requiresCorrected);

            // Index
            LinkPointRepr(source.hand.index1Point, hand.index.twoUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.index2Point, hand.index.oneUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.index3Point, hand.index.last.point, requiresCorrected);
            LinkPointRepr(source.hand.indexTipPoint, hand.index.tip, requiresCorrected);

            // Middle
            LinkPointRepr(source.hand.middle1Point, hand.middle.twoUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.middle2Point, hand.middle.oneUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.middle3Point, hand.middle.last.point, requiresCorrected);
            LinkPointRepr(source.hand.middleTipPoint, hand.middle.tip, requiresCorrected);

            // Ring
            LinkPointRepr(source.hand.ring1Point, hand.ring.twoUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.ring2Point, hand.ring.oneUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.ring3Point, hand.ring.last.point, requiresCorrected);
            LinkPointRepr(source.hand.ringTipPoint, hand.ring.tip, requiresCorrected);

            // Pinky
            if (source.hand.pinky0Point.original) LinkPointRepr(source.hand.pinky0Point, hand.pinky.threeUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.pinky1Point, hand.pinky.twoUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.pinky2Point, hand.pinky.oneUnderLast.point, requiresCorrected);
            LinkPointRepr(source.hand.pinky3Point, hand.pinky.last.point, requiresCorrected);
            LinkPointRepr(source.hand.pinkyTipPoint, hand.pinky.tip, requiresCorrected);

            // Specials
            LinkPointRepr(source.specialPoints.palmCenterPoint, hand.palmCenter, false);
            LinkPointRepr(source.specialPoints.palmNormalPoint, hand.palmNormal, false);
            LinkPointRepr(source.specialPoints.palmInteriorPoint, hand.palmInterior, false);
            LinkPointRepr(source.specialPoints.palmExteriorPoint, hand.palmExterior, false);
            LinkPointRepr(source.specialPoints.pinchCenterPoint, hand.pinchCenter, false);
            LinkPointRepr(source.specialPoints.throatCenterPoint, hand.throatCenter, false);
            LinkPointRepr(source.specialPoints.rayPoint, hand.ray, false);
        }

        public static void LinkPointRepr(Point pointSet, PointModel pointModel, bool requiresCorrected)
        {
            if (!pointSet.tsf && !pointModel)
            {
                Debug.LogError("Cannot link Point-Representation. Both are NULL");
                return;
            }
            else if (requiresCorrected && pointModel && !pointSet.corrected)
            {
                Debug.LogError("Cannot link point model " + pointModel.name + " as original " + pointSet.original + " was not corrected");
                return;
            }
            else if (pointSet.tsf != null && !pointModel)
            {
                Debug.LogError("Cannot link representation of original " + pointSet.original.name + " as there is no point model to represent");
                return;
            }
            else if (pointSet.repr == null)
            {
                Debug.LogError("Representation " + pointSet.tsf.name + " has no ReprModel attached. ReprModels might have not been generated");
                return;
            }

            pointSet.repr.point = pointModel;
        }

        public static string GetErrorMsg(HandModel hand)
        {
            if (!hand.wrist) return "Hand " + hand.name + " has missing wrist";
            if (!hand.palmCenter || !hand.palmExterior || !hand.palmInterior || !hand.palmNormal) return "Hand " + hand.name + " has missing special points";
            if (hand.thumb.bonesFromRootToTip.Count == 0 || hand.index.bonesFromRootToTip.Count == 0) return "Hand " + hand.name + " has zero bones in the ordered array of bones. Probably the model was not started";
            else return "";
        }
    }
}
