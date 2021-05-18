using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    [RequireComponent(typeof(ReprModel))]
    public class ReprView : HPTKView
    {
        protected ReprModel model;

        public string reprName { get { return model.name; } }

        // Parent
        public PointView point { get { return model.point.view; } }

        // Refs
        public Transform transformRef { get { return model.transformRef; } }
        public MeshRenderer meshRenderer { get { return model.meshRenderer; } }
        public SkinnedMeshRenderer skinnedMeshRenderer { get { return model.skinnedMeshRenderer; } }

        // Angle
        public float localrotZ { get { return model.localRotZ; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<ReprModel>();
        }
    }
}
