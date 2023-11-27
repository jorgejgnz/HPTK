using HandPhysicsToolkit.Utils;
using HandPhysicsToolkit.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HandPhysicsToolkit.Modules.Hand.ABPuppet;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
    [RequireComponent(typeof(HandSearchEngine))]
    public class ABHierarchyWizard : MonoBehaviour
    {
        [Serializable]
        public class ParentConfig
        {
            public string suffix;
            public Transform tsf;
        }
        
        [Serializable]
        public class BoneConfig
        {
            public string name;
            public Transform transform;
            public ABPuppetReprModel reprModel;

            public List<ParentConfig> parents = new List<ParentConfig>();
            public bool originalAsLastParent = true;

            public BoneConfig(Transform t)
            {
                this.name = t.name;
                this.transform = t;
                this.reprModel = t.GetComponent<ABPuppetReprModel>();
            }
        }

        public List<BoneConfig> boneConfigs = new List<BoneConfig>();

        private HandSearchEngine engine;

        private void OnValidate()
        {
            foreach (BoneConfig config in boneConfigs)
            {
                if (config.transform && !config.reprModel)
                {
                    config.reprModel = config.transform.GetComponent<ABPuppetReprModel>();
                }
            }
        }

        public void LoadBoneConfigs()
        {
            if (!engine) engine = GetComponent<HandSearchEngine>();

            boneConfigs.Clear();
            foreach(Point point in engine.hand.ToList())
            {
                if (point != null && point.original != null)
                {
                    boneConfigs.Add(new BoneConfig(point.original));
                }
            }
        }

        public void ModifyHierarchy()
        {
            for (int i = 0; i < boneConfigs.Count; i++)
            {
                ModifyBone(boneConfigs[i]);
            }
        }

        void ModifyBone(BoneConfig config)
        {
            GameObject gObj = config.transform.gameObject;
            int max = config.parents.Count;
            for (int i = 0; i < max; i++)
            {
                GameObject parent = gObj;
                if (i < max - 1) parent = BasicHelpers.InstantiateParent(gObj, "TemporalName");
                parent.name = $"{gObj.name}.{config.parents[i].suffix}";
                config.parents[i].tsf = parent.transform;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ABHierarchyWizard))]
public class ABHierarchyWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ABHierarchyWizard myScript = (ABHierarchyWizard)target;

        if (GUILayout.Button("LOAD BONE CONFIGS"))
        {
            myScript.LoadBoneConfigs();
        }

        if (GUILayout.Button("MODIFY HIERARCHY (!)"))
        {
            myScript.ModifyHierarchy();
        }
    }
}
#endif

