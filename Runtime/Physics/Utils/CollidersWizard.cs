using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Utils;
using System;
using System.Linq;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Physics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Physics
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoneSearchEngine))]
    public class CollidersWizard : MonoBehaviour
    {
        public bool updateInEditor = false;

        [Range(0.01f,0.3f)]
        public float boneDensity = 0.15f;

        [Range(0.001f, 0.2f)]
        public float maxBoneRadius = 0.007f;

        [ReadOnly]
        public BoneSearchEngine source;

        List<CapsuleEditor> editors = new List<CapsuleEditor>();

#if UNITY_EDITOR
        private void Start()
        {
            if (!source) source = GetComponent<BoneSearchEngine>();
            
            if (source is BodySearchEngine)
            {
                boneDensity = 0.09f;
                maxBoneRadius = 0.12f;
            }
            else if (source is HandSearchEngine)
            {
                boneDensity = 0.15f;
                maxBoneRadius = 0.007f;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying && updateInEditor) UpdateAll();
        }
#endif

        public void UpdateAll()
        {
            if (!source) source = GetComponent<BoneSearchEngine>();
            source.bones.ForEach(b => UpdateBone(b));
        }

        public void UpdateBone(Bone b)
        {
            if (!b.parent.tsf || !b.child.tsf)
            {
                Debug.LogWarning("CollidersTree cannot update bone " + b.name + ". Missing parent or child Transforms.");
                return;
            }

            if (b.editor == null || b.editor.boneStart != b.parent.tsf || b.editor.boneEnd != b.child.tsf)
            {
                b.parent.tsf.GetComponentsFromDirectChildren<CapsuleEditor>(editors);

                if (b.editor == null)
                {
                    editors.ForEach(e => { if (e.boneStart == b.parent.tsf && e.boneEnd == b.child.tsf) b.editor = e; });
                }

                if (b.editor == null)
                {
                    GameObject child = BasicHelpers.InstantiateEmptyChild(b.parent.tsf.gameObject, b.parent.tsf.name + ".Collider");
                    b.editor = child.AddComponent<CapsuleEditor>();
                }

                b.editor.boneStart = b.parent.tsf;
                b.editor.boneEnd = b.child.tsf;
                b.editor.radiusRatio = b.defaultRadiusRatio;

                b.editor.transform.parent = b.parent.tsf;
            }

            b.editor.UpdateCollider();
            b.editor.UpdateRadius(boneDensity, maxBoneRadius);
        }

        public void DestroyColliders()
        {
            updateInEditor = false;
            source.bones.ForEach(b => { if (b.editor) DestroyImmediate(b.editor.gameObject); });
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CollidersWizard))]
public class CollidersGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CollidersWizard myScript = (CollidersWizard)target;

        if (GUILayout.Button("UPDATE COLLIDERS"))
        {
            myScript.UpdateAll();
        }

        if (GUILayout.Button("DESTROY COLLIDERS"))
        {
            myScript.DestroyColliders();
        }
    }
}
#endif
