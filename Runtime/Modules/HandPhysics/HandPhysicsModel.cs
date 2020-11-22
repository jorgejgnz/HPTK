using HPTK.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class HandPhysicsModel : HPTKModel
    {
        public ProxyHandModel proxyHand;
        public HandPhysicsConfiguration configuration;
        public bool isActive = true;
    }
}
