using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    public class Constraint : MonoBehaviour
    {
        [ReadOnly]
        public ConstraintModel model;

        private void Awake()
        {
            if (!model) model = GetComponentInParent<ConstraintModel>();
            if (model && !model.constraints.Contains(this)) model.constraints.Add(this);
        }

        public virtual void OnMimicCompleted(PartModel part) { }
    }
}
