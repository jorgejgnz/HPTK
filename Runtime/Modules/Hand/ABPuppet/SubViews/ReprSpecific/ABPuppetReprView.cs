using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    public sealed class ABPuppetReprView : ReprView
    {
        ABPuppetReprModel puppet { get { return model as ABPuppetReprModel; } }
    }
}
