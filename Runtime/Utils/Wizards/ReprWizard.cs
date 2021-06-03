using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public enum Representation
    {
        Master,
        Slave
    }

    [RequireComponent(typeof(BoneSearchEngine))]
    public class ReprWizard : MonoBehaviour
    {
        public Representation representation;
        public BodyModel body;

        [Header("Hand-specific")]
        public bool requiresCorrected = true;

        [Header("Read Only")]
        [ReadOnly]
        public BoneSearchEngine source;

        [ReadOnly]
        public List<ReprModel> reprModels = new List<ReprModel>();

        public void GenerateMissingReprModels()
        {
            if (!source) source = GetComponent<BoneSearchEngine>();

            if (source is BodySearchEngine && !(source as BodySearchEngine).body.IsValid())
            {
                Debug.LogError("Body has missing references. ReprModels cannot be generated");
                return;
            }
            else if (source is HandSearchEngine && !(source as HandSearchEngine).hand.IsValid())
            {
                Debug.LogError("Hand has missing references. ReprModels cannot be generated");
                return;
            }
            else if (source is HandSearchEngine && !(source as HandSearchEngine).hand.wristPoint.corrected && requiresCorrected)
            {
                Debug.LogError("Hand wrist has not been corrected. Fix rotations first");
                return;
            }
            else
            {
                GenerateMissingReprModels(source);
            }
        }

        public void DestroyGenerated()
        {
            reprModels.ForEach(r => DestroyImmediate(r));
            reprModels.Clear();
        }

        List<Point> GetPoints(BoneSearchEngine source)
        {
            List<Point> points; //ToList

            if (source is HandSearchEngine)
            {
                HandSearchEngine sourceAsHand = source as HandSearchEngine;
                points = sourceAsHand.hand.ToList();
                points.AddRange(sourceAsHand.specialPoints.ToList());
            }
            else if (source is BodySearchEngine)
            {
                BodySearchEngine sourceAsBody = source as BodySearchEngine;
                points = sourceAsBody.body.ToList();

                // Don't generate reprmodels for hands. They can only be generated when attached under a HandSearchEngine (requires rotation fix)
                points.RemoveAll(p => p.original == sourceAsBody.body.leftHandPoint.original || p.original == sourceAsBody.body.rightHandPoint.original);
            }
            else
            {
                points = source.GetPointsFromBones();
            }

            return points;
        }

        void GenerateMissingReprModels(BoneSearchEngine source)
        {
            List<Point> points = GetPoints(source);
            points.ForEach(p => GenerateReprModel(p, source.bones.Find(b => b.parent.original == p.original) == null));
        }

        void GenerateReprModel(Point point, bool isLeaf)
        {
            if (point.original == null)
                return; 

            if (point.repr == null)
            {
                switch (representation)
                {
                    case Representation.Slave:

                        // Avoid generating multiple ReprModels for the same point
                        point.repr = point.tsf.GetComponent<PuppetReprModel>();

                        point.repr = point.tsf.gameObject.AddComponent<PuppetReprModel>();

                        PuppetReprModel puppetRepr = point.repr as PuppetReprModel;

                        if (isLeaf) puppetRepr.usePhysics = false;
                        else puppetRepr.usePhysics = true;

                        if (source is BodySearchEngine)
                        {
                            Body body = (source as BodySearchEngine).body;
                            puppetRepr.isSpecial = (point == body.hipsPoint || point == body.spinePoint || point == body.chestPoint || point == body.neckPoint);
                        }
                        else if (source is HandSearchEngine)
                        {
                            Hand hand = (source as HandSearchEngine).hand;
                            puppetRepr.isSpecial = (point == hand.thumb0Point || point == hand.thumb1Point || point == hand.thumb2Point || point == hand.thumb3Point);
                        }

                        break;

                    case Representation.Master:
                    default:

                        // Avoid generating multiple ReprModels for the same point
                        point.repr = point.tsf.GetComponent<ReprModel>();

                        point.repr = point.tsf.gameObject.AddComponent<ReprModel>();

                        break;
                }

                point.repr.originalTsfRef = point.original;
            }

            if (!reprModels.Contains(point.repr)) reprModels.Add(point.repr);
        }

        public void LinkPointReprs()
        {
            List<Point> points = GetPoints(source);

            Point invalidPoint = points.Find(p => p.repr == null);

            if (invalidPoint != null)
            {
                Debug.LogError("There are points, like " + invalidPoint.tsf.name + ", that have no ReprModel. ReprModels may have not been generated.");
                return;
            }

            if (source is BodySearchEngine)
            {
                WizardHelpers.LinkBodyPointReprs(source as BodySearchEngine, body, false);
            }
            else if (source is HandSearchEngine)
            {
                WizardHelpers.LinkHandPointReprs(source as HandSearchEngine, body, true, requiresCorrected);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ReprWizard))]
public class ReprWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ReprWizard myScript = (ReprWizard)target;

        if (GUILayout.Button("GENERATE MISSING REPRMODELS"))
        {
            myScript.GenerateMissingReprModels();
        }

        if (GUILayout.Button("DESTROY GENERATED REPRMODELS"))
        {
            myScript.DestroyGenerated();
        }

        if (GUILayout.Button("LINK POINTS-REPRS"))
        {
            myScript.LinkPointReprs();
        }
    }
}
#endif
