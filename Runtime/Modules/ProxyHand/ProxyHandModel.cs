using HPTK.Models.Interaction;
using HPTK.Settings;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class ProxyHandModel : HPTKModel
    {
        [HideInInspector]
        public ProxyHandHandler handler;

        [HideInInspector]
        public AvatarModel avatar;

        [Header("Models")]
        public MasterHandModel master;
        public SlaveHandModel slave;
        public HandModel ghost;

        [Header("Refs")]

        public Transform shoulderTip;

        public float scale = 1.0f;

        [Header("Module registry")]
        public List<HPTKHandler> relatedHandlers = new List<HPTKHandler>();

        [Header("Configuration (CoreModel's if empty)")]
        public CoreConfiguration configuration;
        public bool updateHandValues = true;

        [Header("Updated by Controller")]
        public float error;
        public float errorLerp;

        private void Awake()
        {
            master.proxyHand = this;
            if (slave) slave.proxyHand = this;
            if (ghost) ghost.proxyHand = this;
        }
    }
}