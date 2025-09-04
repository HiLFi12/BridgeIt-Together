using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractPriority
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

public interface IInteractable
{
    InteractPriority InteractPriority { get; }
    void Interact(GameObject interactor);
}

