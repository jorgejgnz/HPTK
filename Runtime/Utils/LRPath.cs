using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class LRPath : MonoBehaviour
    {
        LineRenderer lr;

        public List<Transform> points = new List<Transform>();

        void Start()
        {
            lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = true;
        }

        private void Update()
        {
            UpdateLR();
        }

        void UpdateLR()
        {
            if (!lr || points.Find(p => p== null)) return;

            lr.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null) continue;

                lr.SetPosition(i, points[i].position);
            }
        }
    }
}
