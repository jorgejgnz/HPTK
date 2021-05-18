using HandPhysicsToolkit.Helpers;
using JorgeJGnz.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class LerpPath : MonoBehaviour
    {
        public bool loop = true;
        public bool onlyInEditor = false;
        public bool drawLines = true;

        public Transform moveThis;
        public List<AnimationHelpers.Animation> path;

        public float stepTime = 0.5f;
        public float waitTime = 0.5f;

        int index = 0;
        bool readyForNextStep = true;

        private void Start()
        {
            for (int p = 0; p < path.Count; p++)
            {
                path[p].receiver = moveThis;
                path[p].duration = stepTime;
            }
        }

        private void OnEnable()
        {
            readyForNextStep = true;
        }

        private void Update()
        {
            if (onlyInEditor)
            {
#if !UNITY_EDITOR
            return;
#endif
            }

            if (readyForNextStep)
            {
                readyForNextStep = false;

                AnimationHelpers.Play(this, path[index], () =>
                {
                    AsyncHelpers.DoAfterTime(this, waitTime, () =>
                    {
                        if (index == path.Count - 1 && loop)
                        {
                            index = 0;
                            readyForNextStep = true;
                        }
                        else if (index < path.Count - 1)
                        {
                            index++;
                            readyForNextStep = true;
                        }
                    });
                });
            }
        }

        public void UpdateFromChildren()
        {
            List<AnimationHelpers.Animation> tmpPath = new List<AnimationHelpers.Animation>();

            AnimationHelpers.Animation tmpPoint;

            int nextIndex = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                nextIndex++;

                if (nextIndex == transform.childCount)
                    nextIndex = 0;

                tmpPoint = new AnimationHelpers.Animation(moveThis, transform.GetChild(i), transform.GetChild(nextIndex), stepTime, true, true, false);

                tmpPath.Add(tmpPoint);
            }

            path = tmpPath;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawLines && path.Count > 0)
            {
                for (int p = 0; p < path.Count; p++)
                {
                    if (path[p] == null) continue;

                    if (!path[p].start || !path[p].destination) continue;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(path[p].start.position, path[p].start.position + path[p].start.up * 0.05f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(path[p].start.position, path[p].start.position + path[p].start.right * 0.05f);

                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(path[p].start.position, 0.01f);
                    Gizmos.DrawLine(path[p].start.position, path[p].destination.position);
                }
            }
        }
#endif

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LerpPath)), CanEditMultipleObjects]
    public class LerpPathEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LerpPath myScript = (LerpPath)target;
            if (GUILayout.Button("UPDATE FROM CHILDREN"))
            {
                myScript.UpdateFromChildren();
            }
        }
    }
#endif
}

