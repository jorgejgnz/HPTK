using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class IntegrationConfiguration : ScriptableObject
    {
        [Serializable]
        public class Integration
        {
            public string name = "None";
            public string package;
        }

        public List<Integration> integrations = new List<Integration>();
    }
}
