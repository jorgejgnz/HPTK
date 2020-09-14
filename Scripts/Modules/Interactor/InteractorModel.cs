using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Views.Notifiers;
using HPTK.Views.Handlers;

namespace HPTK.Models.Interaction
{
    public class InteractorModel : HPTKModel
    {
        [HideInInspector]
        public InteractorHandler handler;

        public ProxyHandModel proxyHand;

        public int totalHovering = 0;
        public int totalTouching = 0;
        public int totalGrabbing = 0;

        public bool isHovering = false;
        public bool isTouching = false;
        public bool isGrabbing = false;

        public List<InteractionModel> interactions = new List<InteractionModel>();

        private void Start()
        {
            proxyHand.relatedHandlers.Add(handler);
        }

    }
}
                                                                              