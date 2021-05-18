using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Physics
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleEditor : MonoBehaviour
    {
        public Transform boneStart;
        public Transform boneEnd;

        public bool updateInEditor = true;
        public float radiusRatio = 1.0f;

        [ReadOnly]
        public CapsuleCollider capsule;

        Vector3 relBoneStartPos, relBoneEndPos;

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying || !updateInEditor)
                return;

            UpdateCollider();
        }
#endif

        public void UpdateCollider()
        {
            if (!capsule) capsule = GetComponent<CapsuleCollider>();

            if (!boneStart) boneStart = transform;
            if (!boneEnd) boneEnd = transform;

            relBoneStartPos = transform.InverseTransformPoint(boneStart.position);
            relBoneEndPos = transform.InverseTransformPoint(boneEnd.position);

            capsule.center = (relBoneStartPos + relBoneEndPos) / 2.0f;
            capsule.height = Vector3.Distance(relBoneStartPos, relBoneEndPos);

            capsule.direction = 2;

            transform.position = boneStart.position;
            transform.LookAt(boneEnd);
        }

        public void UpdateRadius(float boneDensity, float maxBoneRadius)
        {
            float radius = Vector3.Distance(boneEnd.position, boneStart.position) * boneDensity * radiusRatio;
            radius = Mathf.Clamp(radius, 0.0f, maxBoneRadius);
            capsule.radius = capsule.transform.InverseTransformVector(Vector3.one * radius).magnitude;
        }
    }
}
