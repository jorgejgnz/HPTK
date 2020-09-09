using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class DrawLineBetween : MonoBehaviour
{
    LineRenderer lr;

    public List<Transform> objs = new List<Transform>();

    public bool isActive = false;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = objs.Count;
    }

    void Update()
    {
        if (!isActive)
            return;

        for (int i = 0; i < objs.Count; i++)
        {
            lr.SetPosition(i, objs[i].position);
        }
    }
}
