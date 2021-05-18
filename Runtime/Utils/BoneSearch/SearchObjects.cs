using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    [Serializable]
    public class Body
    {
        public Point hipsPoint;
        public Point spinePoint;
        public Point chestPoint;
        public Point neckPoint;
        public Point headPoint;
        public Point eyesPoint;
        public Point headTopPoint;
        public Point leftShoulderPoint;
        public Point leftUpperArmPoint;
        public Point leftForearmPoint;
        public Point leftHandPoint;
        public Point rightShoulderPoint;
        public Point rightUpperArmPoint;
        public Point rightForearmPoint;
        public Point rightHandPoint;
        public Point leftThighPoint;
        public Point leftCalfPoint;
        public Point leftFootPoint;
        public Point leftToesPoint;
        public Point rightThighPoint;
        public Point rightCalfPoint;
        public Point rightFootPoint;
        public Point rightToesPoint;

        public bool IsValid()
        {
            // Mandatory bones
            List<Point> torsoRefs = new List<Point> { hipsPoint, spinePoint, chestPoint, neckPoint, headPoint, eyesPoint, headTopPoint };
            List<Point> leftArm = new List<Point> { leftShoulderPoint, leftUpperArmPoint, leftForearmPoint, leftHandPoint };
            List<Point> rightArm = new List<Point> { rightShoulderPoint, rightUpperArmPoint, rightForearmPoint, rightHandPoint };
            List<Point> leftLeg = new List<Point> { leftThighPoint, leftCalfPoint, leftFootPoint, leftToesPoint };
            List<Point> rightLeg = new List<Point> { rightThighPoint, rightCalfPoint, rightFootPoint, rightToesPoint };

            bool valid = true;

            torsoRefs.ForEach(p => { if (!p.original) valid = false; });
            leftArm.ForEach(p => { if (!p.original) valid = false; });
            rightArm.ForEach(p => { if (!p.original) valid = false; });
            leftLeg.ForEach(p => { if (!p.original) valid = false; });
            rightLeg.ForEach(p => { if (!p.original) valid = false; });

            return valid;
        }

        public List<Point> ToList()
        {
            List<Point> list = new List<Point>() {
                hipsPoint,
                spinePoint,
                chestPoint,
                neckPoint,
                headPoint,
                eyesPoint,
                headTopPoint,
                leftShoulderPoint,
                leftUpperArmPoint,
                leftForearmPoint,
                leftHandPoint,
                rightShoulderPoint,
                rightUpperArmPoint,
                rightForearmPoint,
                rightHandPoint,
                leftThighPoint,
                leftCalfPoint,
                leftFootPoint,
                leftToesPoint,
                rightThighPoint,
                rightCalfPoint,
                rightFootPoint,
                rightToesPoint,
            };

            list.RemoveAll(p => p == null || p.original == null);

            return list;
        }
    }

    [Serializable]
    public class Hand
    {
        public Point wristPoint;
        public Point thumb0Point;
        public Point thumb1Point;
        public Point thumb2Point;
        public Point thumb3Point;
        public Point thumbTipPoint;
        public Point index1Point;
        public Point index2Point;
        public Point index3Point;
        public Point indexTipPoint;
        public Point middle1Point;
        public Point middle2Point;
        public Point middle3Point;
        public Point middleTipPoint;
        public Point ring1Point;
        public Point ring2Point;
        public Point ring3Point;
        public Point ringTipPoint;
        public Point pinky0Point;
        public Point pinky1Point;
        public Point pinky2Point;
        public Point pinky3Point;
        public Point pinkyTipPoint;

        public bool IsValid()
        {
            // Mandatory bones
            List<Point> thumb = new List<Point> { /*thumb0Point,*/ thumb1Point, thumb2Point, thumb3Point, thumbTipPoint };
            List<Point> index = new List<Point> { index1Point, index2Point, index3Point, indexTipPoint };
            List<Point> middle = new List<Point> { middle1Point, middle2Point, middle3Point, middleTipPoint };
            List<Point> ring = new List<Point> { ring1Point, ring2Point, ring3Point, ringTipPoint };
            List<Point> pinky = new List<Point> { /*pinky0Point,*/ pinky1Point, pinky2Point, pinky3Point, pinkyTipPoint };

            bool valid = true;

            if (!wristPoint.original) valid = false;
            thumb.ForEach(p => { if (!p.original) valid = false; });
            index.ForEach(p => { if (!p.original) valid = false; });
            middle.ForEach(p => { if (!p.original) valid = false; });
            ring.ForEach(p => { if (!p.original) valid = false; });
            pinky.ForEach(p => { if (!p.original) valid = false; });

            return valid;
        }

        /// <summary>
        /// WARNING: Can include NULL elements
        /// </summary>
        /// <returns></returns>
        public List<Point> ToList()
        {
            List<Point> list = new List<Point>() {
                wristPoint,
                thumb0Point,
                thumb1Point,
                thumb2Point,
                thumb3Point,
                thumbTipPoint,
                index1Point,
                index2Point,
                index3Point,
                indexTipPoint,
                middle1Point,
                middle2Point,
                middle3Point,
                middleTipPoint,
                ring1Point,
                ring2Point,
                ring3Point,
                ringTipPoint,
                pinky0Point,
                pinky1Point,
                pinky2Point,
                pinky3Point,
                pinkyTipPoint
            };

            list.RemoveAll(p => p == null || p.original == null);

            return list;
        }
    }

    [Serializable]
    public class SpecialPoints
    {
        public Point pinchCenterPoint;
        public Point throatCenterPoint;
        public Point palmCenterPoint;
        public Point palmNormalPoint;
        public Point palmInteriorPoint;
        public Point palmExteriorPoint;
        public Point rayPoint;

        public List<Point> ToList()
        {
            List<Point> list = new List<Point>() {
                pinchCenterPoint,
                throatCenterPoint,
                palmCenterPoint,
                palmNormalPoint,
                palmInteriorPoint,
                palmExteriorPoint,
                rayPoint
            };

            list.RemoveAll(p => p == null || p.original == null);

            return list;
        }
    }

    [Serializable]
    public class Point
    {
        public Transform original;

        [ReadOnly]
        public Transform corrected;

        [ReadOnly]
        public ReprModel repr;

        public Transform tsf { get { if (corrected != null) return corrected; else return original; } }

        public Point() { }

        public Point(Transform original)
        {
            this.original = original;
        }

        public bool IsValid()
        {
            return original;
        }
    }

    [Serializable]
    public class Bone
    {
        public string name { get { return parent.tsf.name + "->" + child.tsf.name; } }

        [HideInInspector]
        public float defaultRadiusRatio = 1.0f;

        public Point parent;
        public Point child;

        [Header("Collider")]
        [ReadOnly]
        public CapsuleEditor editor;

        public Bone(Point parent, Point child, float defaultRadiusRatio)
        {
            this.parent = parent;
            this.child = child;
            this.defaultRadiusRatio = defaultRadiusRatio;
        }

        public bool IsValid()
        {
            return parent != null && child != null && parent.IsValid() && child.IsValid();
        }
    }
}
