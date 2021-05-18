using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Part.Puppet
{
    public sealed class PuppetReprView : ReprView
    {
        PuppetReprModel puppet { get { return model as PuppetReprModel; } }

        public Pheasy pheasy { get { return puppet.pheasy; } }
        public Transform goal { get { return puppet.goal; } }
        public TargetConstraint constraint { get { return puppet.constraint; } }
        public bool isSpecial { get { return puppet.isSpecial; } }

        public float minLocalRotZ { get { return puppet.minLocalRotZ; } }
        public float maxLocalRotZ { get { return puppet.maxLocalRotZ; } }
        public Quaternion fixedLocalRot { get { return puppet.fixedLocalRot; } }

        public bool ready { get { return puppet.ready; } }

        public UnityEvent onPhysicsReady = new UnityEvent();
    }
}
