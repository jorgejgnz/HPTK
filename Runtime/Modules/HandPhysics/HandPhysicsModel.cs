using HPTK.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class HandPhysicsModel : HPTKModel
    {
        public ProxyHandModel proxyHand;
       
        public HandPhysicsConfigurationAsset asset;
        [HideInInspector]
        public HandPhysicsConfiguration configuration;

        public bool isActive = true;
        public bool applyConfOnUpdate = false;
        public bool updateConfFromScale = true;
    }
}
