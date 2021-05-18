using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    public class PuppetReprModel : ReprModel
    {
        [Header("Puppet")]
        public Pheasy pheasy;
        public Transform goal;
        public bool usePhysics = true;
        public bool isSpecial = false;

        [Header("Locking")]
        public float minLocalRotZ = PuppetModel.minLocalRotZ;
        public float maxLocalRotZ = PuppetModel.maxLocalRotZ;
        public Quaternion fixedLocalRot = Quaternion.identity;

        [Header("Read Only")]
        [ReadOnly]
        [SerializeField]
        bool _constrained;
        [ReadOnly]
        public bool ready = false;
        [ReadOnly]
        public PuppetModel puppet;

        [HideInInspector]
        public TargetConstraint constraint;

        public PuppetReprView specificView { get { return view as PuppetReprView; } }

        protected sealed override ReprView GetView()
        {
            ReprView view = GetComponent<PuppetReprView>();
            if (!view) view = gameObject.AddComponent<PuppetReprView>();

            return view;
        }

        protected sealed override string FindKey()
        {
            return PuppetModel.key;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            minLocalRotZ = PuppetModel.minLocalRotZ;
            maxLocalRotZ = PuppetModel.maxLocalRotZ;
        }

        private void Update()
        {
            _constrained = constraint != null;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PuppetReprModel)), CanEditMultipleObjects]
    public class PuppetReprModelEditor : ReprModelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}