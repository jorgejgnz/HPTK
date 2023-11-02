using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    public class ConstraintModel : HPTKModel
    {
        public static string key = "constrained";

        public PartModel part;   
        public bool mimicScale = true;
        public bool mimicRootInWorldSpace = true;

        public List<Constraint> constraints = new List<Constraint>();

        ConstraintController _controller;
        public ConstraintController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<ConstraintController>();
                    if (!_controller) _controller = gameObject.AddComponent<ConstraintController>();
                }

                return _controller;
            }
        }

        ConstraintView _view;
        public ConstraintView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<ConstraintView>();
                    if (!_view) _view = gameObject.AddComponent<ConstraintView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();

            foreach (Constraint constraint in constraints)
            {
                constraint.model = this;
            }
        }
    }
}
