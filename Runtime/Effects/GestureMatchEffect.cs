using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureMatchEffect : MonoBehaviour
{
    public BoneView bone;
    public Gesture gesture;
    public string representation = PuppetModel.key;

    [Range(0.0f, 1.0f)]
    public float minColorAt = 0.0f;
    [Range(0.0f, 1.0f)]
    public float maxColorAt = 1.0f;

    public Color minColor = new Color(0.1f, 0.1f,0.1f,1.0f);
    public Color maxColor = new Color(1.0f, 0.1f, 0.1f, 1.0f);
    public string shaderParam = "_BaseColor";

    float colorLerp = 0.0f;

    private void Update()
    {
        if (!bone.point.reprs.ContainsKey(representation) && !bone.point.reprs[representation].skinnedMeshRenderer) return;

        colorLerp = Mathf.InverseLerp(minColorAt, maxColorAt, gesture.lerp);

        bone.point.reprs[representation].skinnedMeshRenderer.material.SetColor(shaderParam, Color.Lerp(minColor, maxColor, colorLerp));
    }
}
