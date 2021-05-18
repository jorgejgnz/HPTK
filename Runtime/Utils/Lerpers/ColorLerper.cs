using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class ColorLerper : MonoBehaviour
    {
        public SkinnedMeshRenderer smr;

        public Color mincolor;
        public Color maxColor;
        public string shaderColorParamName;

        public void UpdateColor(float lerp)
        {
            smr.material.SetColor(shaderColorParamName, Color.Lerp(mincolor, maxColor, lerp));
        }
    }
}
