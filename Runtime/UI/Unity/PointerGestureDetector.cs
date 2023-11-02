using HandPhysicsToolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PointerGestureDetector : MonoBehaviour
{
    public PointPlaneWidget widget;
    public PointerHandler pointerHandler;

    public float minDistToDrag = 0.01f;

    public bool isHovering;
    public bool isSelecting;
    public bool isDragging;

    private Vector3 latestWorldPlanePos;

    private void Start()
    {
        widget.onHover.AddListener(OnPointerHover);
        widget.onFirstPress.AddListener(OnPointerDown);
        widget.onLastRelease.AddListener(OnPointerUp);
        widget.onUnhover.AddListener(OnPointerUnhover);
    }

    private void Update()
    {
        if (isDragging)
        {
            if (widget.pressed.Count > 0)
            {
                latestWorldPlanePos = widget.pressed[0].worldPlanePos;
                pointerHandler.OnDrag(latestWorldPlanePos);
            }
            else
            {
                isDragging = false;
                pointerHandler.OnEndDrag(latestWorldPlanePos);
            }
        }
        else
        {
            if (widget.pressed.Count > 0)
            {
                // Comprueba si está arrastrando
                Vector3 currentWorldPlanePos = widget.pressed[0].worldPlanePos;
                if (Vector3.Distance(currentWorldPlanePos, latestWorldPlanePos) > minDistToDrag)
                {
                    // Está arrastrando, llama a OnBeginDrag
                    isDragging = true;
                    latestWorldPlanePos = currentWorldPlanePos;
                    pointerHandler.OnBeginDrag(currentWorldPlanePos);
                }
            }
        }
        
        if (isHovering && widget.pressed.Count == 0)
        {
            // Si sigue habiendo hovers y no hay seleccion envía OnEnter OTRA VEZ
            if (widget.pressed.Count > 0)
            {
                Vector3 hoverWorldPlanePos = widget.pressed[0].worldPlanePos;
                pointerHandler.OnPointerHover(hoverWorldPlanePos);
            }
        }
    }

    public void OnPointerDown(PointTrack pointTrack)
    {
        pointerHandler.OnPointerDown(pointTrack.worldPlanePos);
        pointerHandler.OnSubmit(pointTrack.worldPlanePos);
        latestWorldPlanePos = pointTrack.worldPlanePos;
        isSelecting = true;
    }

    public void OnPointerUp(PointTrack pointTrack)
    {
        pointerHandler.OnPointerUp(pointTrack.worldPlanePos);
        latestWorldPlanePos = pointTrack.worldPlanePos;
        isSelecting = false;
    }

    public void OnPointerHover(PointTrack pointTrack)
    {
        pointerHandler.OnPointerHover(pointTrack.worldPlanePos);
        latestWorldPlanePos = pointTrack.worldPlanePos;
        isHovering = true;
    }

    public void OnPointerUnhover(PointTrack pointTrack)
    {
        pointerHandler.OnPointerUnhover(pointTrack.worldPlanePos);
        latestWorldPlanePos = pointTrack.worldPlanePos;
        isHovering = false;
    }
}
