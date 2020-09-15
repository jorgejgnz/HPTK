using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRGroup : MonoBehaviour
{
    public MeshRenderer[] meshRenderers;

    public void SetMaterial(Material mat)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = mat;
        }
    }
}
