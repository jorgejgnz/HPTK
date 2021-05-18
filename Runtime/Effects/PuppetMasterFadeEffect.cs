using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ErrorMeasure
{
    Constraint,
    Position
}

public class PuppetMasterFadeEffect : MonoBehaviour
{
    public PuppetView puppet;
    public BoneView errorFrom;
    public ErrorMeasure errorMeasure = ErrorMeasure.Constraint;
    public string shaderParam = "_BaseColor";
    public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    [Range(0.0f, 2.0f)]
    public float maxError = 0.5f;
    [Range(0.0f, 0.5f)]
    public float minError = 0.05f;

    [Header("Read Only")]
    [ReadOnly]
    [SerializeField]
    Color color;
    [ReadOnly]
    [SerializeField]
    bool ready = false;

    TargetConstraint constraint;

    float error;
    float lerp;

    private void Start()
    {
        if (ValidStart())
        {
            // Wait for puppet to instantiate missing components
            if (puppet.ready) OnPhysicsReady();
            else puppet.onPhysicsReady.AddListener(() => OnPhysicsReady());
        }
        else
        {
            Debug.LogWarning("Fade effect cannot start");
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (ready)
        {
            if (errorMeasure == ErrorMeasure.Constraint) error = constraint.error;
            else error = Vector3.Distance(errorFrom.reprs[AvatarModel.key].transformRef.position, errorFrom.reprs[PuppetModel.key].transformRef.position);
            
            skinnedMeshRenderers.ForEach(smr => { if (smr != null) UpdateEffect(smr, error); });
        }
    }

    void OnPhysicsReady()
    {
        constraint = (errorFrom.reprs[PuppetModel.key] as PuppetReprView).constraint;
        ready = true;
    }

    void UpdateEffect(SkinnedMeshRenderer smr, float error)
    {
        color = smr.material.GetColor(shaderParam);
        lerp = Mathf.InverseLerp(minError, maxError, error);
        color.a = lerp;

        for (int m = 0; m < smr.materials.Length; m++)
        {
            smr.materials[m].SetColor(shaderParam, color);
        }
    }

    bool ValidStart()
    {
        if (!errorFrom.point.reprs.ContainsKey(PuppetModel.key))
        {
            Debug.LogWarning("Bone " + errorFrom.boneName + " does not have a " + PuppetModel.key + " representation");
            return false;
        }

        if (!puppet)
        {
            Debug.LogWarning("Missing reference to Puppet module. The effect cannot start");
            return false;
        }

        return true;
    }
}
