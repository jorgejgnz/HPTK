using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Utils;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    public class Constraint : MonoBehaviour
    {
        [ReadOnly]
        public ConstraintModel model;

        public virtual void OnMimicCompleted(PartModel part) { }
    }
}
