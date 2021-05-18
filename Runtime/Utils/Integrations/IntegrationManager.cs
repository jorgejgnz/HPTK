using System;
using System.Collections.Generic;
using UnityEngine;
using static HandPhysicsToolkit.Utils.IntegrationConfiguration;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Utils
{
#if UNITY_EDITOR
    public class IntegrationManager : EditorWindow
    {
        static string integrationConfPath = "Packages/com.jorgejgnz.hptk/Samples/Integrations/Integrations.asset";

        static List<Integration> integrations = new List<Integration>();

        [MenuItem("HPTK/Integration Manager")]
        public static void ShowWindow()
        {
            if (integrations.Count == 0)
            {
                Debug.Log(integrationConfPath);
                IntegrationConfiguration intConf = (IntegrationConfiguration)AssetDatabase.LoadAssetAtPath(integrationConfPath, typeof(IntegrationConfiguration));
                integrations = intConf.integrations;
            }

            EditorWindow.GetWindow(typeof(IntegrationManager));
        }

        void OnGUI()
        {
            GUILayout.Label("Built-in integrations", EditorStyles.boldLabel);

            for (int i = 0; i < integrations.Count; i++)
            {
                GUI.enabled = integrations[i].package.Length > 0;

                if (GUILayout.Button(integrations[i].name))
                {
                    AssetDatabase.ImportPackage(integrations[i].package, true);
                }
            }
        }
    }
#endif
}