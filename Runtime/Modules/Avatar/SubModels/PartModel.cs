using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class PartModel : HPTKModel
    {
        [Header("Refs")]
        public Side side;
        public HumanBodyPart part;

        [Header("Scaling")]
        public bool scaleRepresentations = true;

        [Header("Read Only")]
        [SerializeField]
        [ReadOnly]
        List<HPTKController> _registry;
        public HPTKRegistry registry = new HPTKRegistry();
        [ReadOnly]
        public List<PartModel> parts = new List<PartModel>();
        [ReadOnly]
        public List<BoneModel> bones = new List<BoneModel>();
        [ReadOnly]
        public BoneModel root;
        [ReadOnly]
        public PartModel parent;
        [ReadOnly]
        public BodyModel body;

        [ReadOnly]
        [SerializeField]
        float _realScale = 1.0f;
        public float realScale
        {
            get { return _realScale; }
            set
            {
                _realScale = value;
                view.UpdateScale();
            }
        }

        [ReadOnly]
        [SerializeField]
        float _extraScale = 1.0f;
        public float extraScale
        {
            get { return _extraScale; }
            set
            {
                _extraScale = value;
                view.UpdateScale();
            }
        }
        public float totalScale { get { return realScale * extraScale; } }

        protected AvatarController controller { get { return body.avatar.controller; } }

        PartView _view;
        public PartView view
        {
            get
            {
                if (!_view) _view = GetView();

                return _view;
            }
        }

        public override sealed void Awake()
        {
            base.Awake();

            GetView();

            if (!parent) parent = transform.parent.GetComponent<PartModel>();
            if (!body) body = transform.parent.GetComponent<BodyModel>();

            parts.RemoveAll(p => p == null);
            bones.RemoveAll(b => b == null);

            if (parent)
            {
                if (!parent.parts.Contains(this)) parent.parts.Add(this);
            }
            else
            {
                if (!body) Debug.LogError("Part without parent is interpreted as root part and it requires a body. Move " + transform.name + " as child of a BodyModel in hierarchy");
                else if (body.root != null && body.root != this) Debug.LogWarning("Multiple body roots were found. Preserving original root");
                else body.root = this;
            }

            _registry = registry;

            PartAwake();
        }

        protected virtual PartView GetView()
        {
            PartView view = GetComponent<PartView>();
            if (!view) view = gameObject.AddComponent<PartView>();

            return view;
        }

        protected virtual void PartAwake() { }
    }
}
