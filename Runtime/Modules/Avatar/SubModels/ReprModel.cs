using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class ReprModel : HPTKModel
    {
        public PointModel point;

        [Header("Refs")]
        public Transform transformRef;
        public MeshRenderer meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public LineRenderer lineRenderer;

        ReprModel _parent;
        public ReprModel parent
        {
            get
            {
                if (!_parent && point.bone.parent && point.bone.parent.point.reprs.ContainsKey(key)) _parent = point.bone.parent.point.reprs[key];
                return _parent;
            }
        }

        [Header("Control")]
        public bool relativeToParentBone = true;

        public Vector3 localPosition
        {
            get { return controller.GetLocalPosition(this); }
            set { transformRef.position = controller.GetWorldFromLocalPoition(value, this); }
        }

        public Quaternion localRotation
        {
            get { return controller.GetLocalRotation(this); }
            set { transformRef.rotation = controller.GetWorldFromLocalRotation(value, this); }
        }

        public float localRotZ
        {
            get { return controller.GetProcessedAngleZ(localRotation); }
        }

        [Header("Armature")]
        [ReadOnly]
        public Transform originalTsfRef;

        [Header("Read Only")]
        [ReadOnly]
        public string key;

        ReprView _view;
        public ReprView view
        {
            get
            {
                if (!_view) _view = GetView();
                return _view;
            }
        }

        // Shortcut to controller
        AvatarController controller { get { return point.bone.part.body.avatar.controller; } }

        public override sealed void Awake()
        {
            base.Awake();

            if (!transformRef) transformRef = transform;
            if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
            if (!skinnedMeshRenderer) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();

            if (!point) point = GetComponent<PointModel>();
            if (!point) point = BasicHelpers.GetComponentInParents<PointModel>(transform);

            if (!point)
            {
                Debug.LogError("ReprModel " + transform.name + " has no point!");
                return;
            }
            else if (!point.reprs.ContainsValue(this))
            {
                if (key == null || key == "") key = FindKey();

                if (key != null && key != "")
                {
                    if (!point.reprs.ContainsKey(key)) point.reprs.Add(key, this);
                    else key = null;
                }
            }
        }

        protected virtual ReprView GetView()
        {
            ReprView view = GetComponent<ReprView>();
            if (!view) view = gameObject.AddComponent<ReprView>();

            return view;
        }

        protected virtual string FindKey()
        {
            return AvatarModel.key;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ReprModel)), CanEditMultipleObjects]
    public class ReprModelEditor : HPTKModelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ReprModel myScript = (ReprModel)target;

            if (GUILayout.Button("AWAKE CHILDREN REPRMODELS"))
            {
                Debug.Log("Awaking children models...");

                ReprModel[] childrenModels = myScript.transform.GetComponentsInChildren<ReprModel>();

                for (int c = 0; c < childrenModels.Length; c++)
                {
                    if (childrenModels[c] == myScript) continue;

                    childrenModels[c].Awake();
                }

                Debug.Log("Done");
            }
        }
    }
#endif
}