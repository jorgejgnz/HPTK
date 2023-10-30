using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    [RequireComponent(typeof(ConstraintModel))]
    public class ConstraintView : HPTKView
    {
        ConstraintModel model;

        public PartView part { get { return model.part.view; } }
        public List<Constraint> constraints { get { return model.constraints; } }

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<ConstraintModel>();
        }
    }
}
