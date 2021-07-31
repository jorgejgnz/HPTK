using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HandPhysicsToolkit.Utils.IntegrationConfiguration;

namespace HandPhysicsToolkit.Utils
{
    [CreateAssetMenu(menuName = "HPTK/IntegrationConfigurationExtension", order = 2)]
    public class IntegrationExtension : ScriptableObject
    {
        public bool autoIntegration = true;

        public IntegrationConfiguration main;
        public List<Integration> integrations = new List<Integration>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (main != null)
            {
                for (int i = 0; i < integrations.Count; i++)
                {
                    if (main.integrations.Find(mainIntegration => mainIntegration.name == integrations[i].name) == null)
                        main.integrations.Add(integrations[i]);
                }
            }
        }
#endif
    }
}