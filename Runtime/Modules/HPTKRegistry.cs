using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit
{
    public class HPTKRegistry : List<HPTKController>
    {
        [HideInInspector]
        public HPTKViewEvent onRegistry = new HPTKViewEvent();
        [HideInInspector]
        public HPTKViewEvent onUnregistry = new HPTKViewEvent();

        public new void Add(HPTKController controller)
        {
            if (Contains(controller))
                return;

            base.Add(controller);

            onRegistry.Invoke(controller.genericView);

            return;
        }

        public new void Remove(HPTKController controller)
        {
            if (!Contains(controller))
                return;

            onUnregistry.Invoke(controller.genericView);

            base.Remove(controller);

            return;
        }
    }
}