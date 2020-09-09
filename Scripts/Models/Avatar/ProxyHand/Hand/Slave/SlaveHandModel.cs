using HPTK.Models.Interaction;
using HPTK.Views.Handlers;
using HPTK.Views.Notifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class SlaveHandModel : HandModel
    {
        [Header("Interactor")]
        public InteractorModel interactor;

        [Header("Notifiers")]
        public TriggerNotifier palmTrigger;
        public TriggerNotifier handTrigger;
        public CollisionNotifier palmCollisionNotifier;

        private new void Awake()
        {
            base.Awake();
        }
    }
}
