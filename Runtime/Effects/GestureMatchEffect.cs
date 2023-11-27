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
    public string repr;

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
        if (repr == null || repr.Length == 0)
        {
            if (bone.reprs.ContainsKey(PuppetModel.key)) repr = PuppetModel.key;
            else if (bone.reprs.ContainsKey("ab.puppet")) repr = "ab.puppet";
            else repr = AvatarModel.key;
        }

        if (!bone.point.reprs.ContainsKey(repr) && !bone.point.reprs[repr].skinnedMeshRenderer) return;

        colorLerp = Mathf.InverseLerp(minColorAt, maxColorAt, gesture.lerp);

        bone.point.reprs[repr].skinnedMeshRenderer.material.SetColor(shaderParam, Color.Lerp(minColor, maxColor, colorLerp));
    }
}
