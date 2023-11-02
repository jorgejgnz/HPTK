using HandPhysicsToolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HandPhysicsToolkit.Utils
{
    public class WidgetColorModifier : MonoBehaviour
    {
        public Widget widget;
        public Image image;
        public Renderer rend;
        public string shaderColorParamName = "_BaseColor";

        public Color minHoverColor = Color.gray;
        public Color maxHoverColor = Color.white;
        public Color pressedColor = Color.blue;

        private void Awake()
        {
            if (!widget) widget = GetComponent<Widget>();
            if (!rend) rend = GetComponent<Renderer>();
            if (!image) image = GetComponent<Image>();
        }

        private void Update()
        {
            if (widget)
            {
                if (widget.pressed.Count > 0)
                {
                    SetColor(pressedColor);
                }
                else
                {
                    SetColor(Color.Lerp(minHoverColor, maxHoverColor, widget.currentMaxDepth));
                }
            }
        }

        void SetColor(Color c)
        {
            if (rend) rend.material.SetColor(shaderColorParamName, c);
            if (image) image.color = c;
        }
    }
}
