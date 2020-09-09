using HPTK.Models.Interaction;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableDebugger : MonoBehaviour
{
    public InteractableModel interactable;
    public TextMeshPro tmpro;

    private void Update()
    {
        tmpro.text = "totalHovering: " + interactable.totalHovering + "\n";
        tmpro.text += "totalTouching: " + interactable.totalTouching + "\n";
        tmpro.text += "totalGrabbing: " + interactable.totalGrabbing + "\n";
    }
}
