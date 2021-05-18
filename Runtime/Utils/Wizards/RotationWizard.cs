using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public enum Axis
    {
        None,
        PositiveX,
        PositiveY,
        PositiveZ,
        NegativeX,
        NegativeY,
        NegativeZ
    }

    [RequireComponent(typeof(HandSearchEngine))]
    public class RotationWizard : MonoBehaviour
    {
        public Axis boneToInterior = Axis.PositiveZ;
        public Axis wristToFingers = Axis.PositiveY;

        public bool requiredManualRotation = false;

        [ReadOnly]
        public HandSearchEngine source;

        [ReadOnly]
        public List<Transform> corrections = new List<Transform>();

        List<Transform> overwritableChildren = new List<Transform>();

        public void GenerateMissingCorrections()
        {
            if (!source) source = GetComponent<HandSearchEngine>();

            if (source && !source.hand.IsValid())
            {
                Debug.LogError("Left hand has missing references. Rotation cannot be generated");
                return;
            }
            else
            {
                GenerateCorrections(source, source.side);
                source.UpdateFingerBones();
                source.canSearch = false;
            }
        }

        void GenerateCorrections(HandSearchEngine source, Side s)
        {
            source.bones.ForEach(bone =>
            {
                Vector3 parentToChildDir;

                // Debug.Log("Updating " + bone.parent.original.name);

                if (bone.parent.original == source.hand.wristPoint.original) parentToChildDir = AxisToDir(wristToFingers, bone.parent.original);
                else parentToChildDir = (bone.child.original.position - bone.parent.original.position).normalized;

                GenerateCorrection(bone.parent, parentToChildDir, AxisToDir(boneToInterior, bone.parent.original), s);

                // Leaf bones
                if (source.bones.Find(b => b.parent == bone.child) == null)
                {
                    GenerateCorrection(bone.child, parentToChildDir, AxisToDir(boneToInterior, bone.child.original), s);
                }
            });
        }

        void GenerateCorrection(Point point, Vector3 worldParentToChildDir, Vector3 worldPalmNormalDir, Side s)
        {
            // Get correct world rotation
            Quaternion correctRot;
            switch (s)
            {
                // Left: X(-worldParentToChildDir), Y(+worldPalmNormalDir)
                case Side.Left:

                    correctRot = Quaternion.LookRotation(Vector3.Cross(-worldParentToChildDir, worldPalmNormalDir), worldPalmNormalDir);

                    break;

                // Right: X(+worldParentToChildDir), Y(-worldPalmNormalDir)
                case Side.Right:
                default:

                    correctRot = Quaternion.LookRotation(Vector3.Cross(worldParentToChildDir, -worldPalmNormalDir), -worldPalmNormalDir);

                    break;
            }

            // Unparent, generate/modify correction and reparent
            if (point.corrected == null)
            {
                Debug.Log("Instantiating correction for " + point.original.name);

                // Set to rotation zero
                if (!requiredManualRotation) point.original.localRotation = Quaternion.identity;

                // Correction is parent. Raw is child
                point.corrected = BasicHelpers.InstantiateEmptyChild(point.original.gameObject, point.original.name + ".Correction").transform;

                point.corrected.parent = point.original.parent;
                point.corrected.rotation = correctRot;

                point.original.name += ".Original";
                point.original.parent = point.corrected;

                if (!corrections.Contains(point.corrected)) corrections.Add(point.corrected);
            }
        }

        public void DestroyGenerated()
        {
            List<Point> points = source.hand.ToList();
            points.AddRange(source.specialPoints.ToList());
            points.Add(source.hand.wristPoint);

            points.ForEach(p =>
            {
                if (p.original && p.corrected)
                {
                    if (p.original.name.EndsWith(".Original")) p.original.name = p.original.name.Replace(".Original", "");

                    Debug.Log("Reverting " + p.original.name);

                    RevertParenting(p.corrected, p.original, overwritableChildren);

                    corrections.Remove(p.corrected);

                    DestroyImmediate(p.corrected.gameObject);

                    p.corrected = null;
                }
            });

            source.UpdateFingerBones();

            source.canSearch = true;
        }

        void RevertParenting(Transform corrected, Transform raw, List<Transform> childrenToOverwrite)
        {
            if (!corrected || !raw)
                return;

            raw.parent = corrected.parent;

            corrected.GetDirectChildren(childrenToOverwrite);
            childrenToOverwrite.ForEach(c => c.parent = raw);
        }

        Vector3 AxisToDir(Axis axis, Transform t)
        {
            switch (axis)
            {
                case Axis.PositiveX:
                    return t.rotation * Vector3.right;

                case Axis.PositiveY:
                    return t.rotation * Vector3.up;

                case Axis.PositiveZ:
                default:
                    return t.rotation * Vector3.forward;

                case Axis.NegativeX:
                    return t.rotation * Vector3.right * -1.0f;

                case Axis.NegativeY:
                    return t.rotation * Vector3.up * -1.0f;

                case Axis.NegativeZ:
                    return t.rotation * Vector3.forward * -1.0f;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RotationWizard))]
public class RotationWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RotationWizard myScript = (RotationWizard)target;

        if (GUILayout.Button("GENERATE MISSING CORRECTIONS"))
        {
            myScript.GenerateMissingCorrections();
        }

        if (GUILayout.Button("DESTROY GENERATED CORRECTIONS"))
        {
            myScript.DestroyGenerated();
        }
    }
}
#endif
