using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Constraint;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    [Serializable]
    public class ColorEndpoint
    {
        public Renderer renderer;
        public string shaderParam = "_BaseColor";
        public int matIdx = 0;
    }

    public string fromRepr = ConstraintModel.key;
    public string toRepr;
    public BoneView bone;
    
    public List<ColorEndpoint> colorEndpoints = new List<ColorEndpoint>();

    [Range(0.0f, 2.0f)]
    public float maxError = 0.5f;
    [Range(0.0f, 0.5f)]
    public float minError = 0.05f;

    Color color;
    float error;
    float lerp;

    private void Update()
    {
        if (toRepr == null || toRepr.Length == 0)
        {
            if (bone.reprs.ContainsKey(PuppetModel.key)) toRepr = PuppetModel.key;
            else if (bone.reprs.ContainsKey("ab.puppet")) toRepr = "ab.puppet";
            else toRepr = AvatarModel.key;
        }

        if (!bone.point.reprs.ContainsKey(fromRepr))
        {
            Debug.LogWarning("Bone " + bone.boneName + " does not have a " + fromRepr + " representation");
            return;
        }

        if (!bone.point.reprs.ContainsKey(toRepr))
        {
            Debug.LogWarning("Bone " + bone.boneName + " does not have a " + toRepr + " representation");
            return;
        }

        error = Vector3.Distance(bone.reprs[fromRepr].transformRef.position, bone.reprs[toRepr].transformRef.position);

        foreach (ColorEndpoint colorEndpoint in colorEndpoints)
        {
            if (colorEndpoint != null) UpdateEffect(colorEndpoint, error);
        }
    }

    void UpdateEffect(ColorEndpoint ep, float error)
    {
        color = ep.renderer.materials[ep.matIdx].GetColor(ep.shaderParam);
        lerp = Mathf.InverseLerp(minError, maxError, error);
        color.a = lerp;
        ep.renderer.materials[ep.matIdx].SetColor(ep.shaderParam, color);
    }
}
