using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable, IGrababble
{
    public InteractPriority InteractPriority => InteractPriority.Medium;

    private Transform player;
    private Vector3 objectPosition;
    private Quaternion objectRotation; // Changed type to Quaternion to match transform.localRotation

    private void Start()
    {
        Player playerComponent = FindObjectOfType<Player>();
        if (playerComponent != null)
        {
            player = playerComponent.transform;
        }
    }

    public void Interact(GameObject interactor)
    {
        if (interactor != null)
        {
            transform.SetParent(interactor.transform);
        }
    }

    public void GrabPosition(Vector3 position)
    {
        objectPosition = transform.position;
    }

    public void GrabRotation(Quaternion rotation)
    {
        objectRotation = transform.localRotation; 
    }
}
