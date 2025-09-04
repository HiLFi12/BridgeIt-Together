using UnityEngine;

public class BatterySlotInteractable : MonoBehaviour, IInteractable
{
    public PowerUpConstructorHolografico powerUp;
    public int slotIndex;

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        powerUp.InsertBattery(slotIndex);
        // Feedback visual/sonoro de inserción
    }
} 