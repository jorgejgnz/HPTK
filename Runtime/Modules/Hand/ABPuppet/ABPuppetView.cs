using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    [RequireComponent(typeof(ABPuppetModel))]
    public class ABPuppetView : HPTKView
    {
        ABPuppetModel model;

        public HandView hand { get { return model.hand.specificView; } }

        // Public properties

        // Public events

        // Public functions

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<ABPuppetModel>();
        }
    }
}
