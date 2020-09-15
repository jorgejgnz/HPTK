using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractableRef : MonoBehaviour
{
    public InteractableHandler interactable;

    private void Awake()
    {
        if (!interactable)
            interactable = GetComponent<InteractableHandler>();

        if (!interactable)
            interactable = GetComponentInChildren<InteractableHandler>();
    }
}
