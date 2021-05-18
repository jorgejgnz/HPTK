using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Avatar
{
    [Serializable]
    public class SerializableRepr
    {
        public string key;
        public ReprModel repr;

        public SerializableRepr(KeyValuePair<string, ReprModel> pair)
        {
            key = pair.Key;
            repr = pair.Value;
        }
    }

    public class PointModel : HPTKModel
    {
        [Header("Read Only")]
        [ReadOnly]
        public BoneModel bone;

        [ReadOnly]
        [SerializeField]
        List<SerializableRepr> _dictionary = new List<SerializableRepr>();
        public Dictionary<string, ReprModel> reprs = new Dictionary<string, ReprModel>();

        public ReprModel master {
            get
            {
                if (!reprs.ContainsKey(AvatarModel.key))
                {
                    Debug.LogError(transform.name + " does not have a master representation");
                    return null;
                }

                return reprs[AvatarModel.key];
            }
        }

        public Transform transformRef
        {
            get { return master.transformRef; }
            set { master.transformRef = value; }
        }

        public MeshRenderer meshRenderer
        {
            get { return master.meshRenderer; }
            set { master.meshRenderer = value; }
        }

        public SkinnedMeshRenderer skinnedMeshRenderer
        {
            get { return master.skinnedMeshRenderer; }
            set { master.skinnedMeshRenderer = value; }
        }

        PointView _view;
        public PointView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<PointView>();
                    if (!_view) _view = gameObject.AddComponent<PointView>();
                }

                return _view;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();

            if (!bone) bone = GetComponent<BoneModel>();
            if (!bone) bone = BasicHelpers.GetComponentInParents<BoneModel>(transform);

            if (!bone.points.Contains(this)) bone.points.Add(this);
            if (bone.transform == transform) bone.point = this;
            if (!bone.point) bone.point = this;
        }

        private void OnDrawGizmosSelected()
        {
            _dictionary.Clear();

            foreach (KeyValuePair<string, ReprModel> entry in reprs)
            {
                if (entry.Key == AvatarModel.key) Gizmos.color = Color.blue;
                else if (entry.Key == PuppetModel.key) Gizmos.color = Color.black;
                else Gizmos.color = Color.white;

                Gizmos.DrawSphere(entry.Value.transformRef.position, 0.005f);

                _dictionary.Add(new SerializableRepr(entry));
            }
        }
    }
}