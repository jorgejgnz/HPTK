using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawLineRenderer : MonoBehaviour
{
    public Transform Point1;
    public Transform Point2;
    public Transform Point3;

    public LineRenderer linerenderer;
    public float vertexCount = 12;
    List<Vector3> pointList = new List<Vector3>();

    void Update()
    {
        pointList.Clear();

        for(float ratio = 0;ratio<=1;ratio+= 1/vertexCount)
        {
            var tangent1 = Vector3.Lerp(Point1.position, Point2.position, ratio);
            var tangent2 = Vector3.Lerp(Point2.position, Point3.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        linerenderer.positionCount = pointList.Count;
        linerenderer.SetPositions(pointList.ToArray());
    }
}