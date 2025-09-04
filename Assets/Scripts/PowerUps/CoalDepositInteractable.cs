using UnityEngine;

public class CoalDepositInteractable : MonoBehaviour, IInteractable
{
    public PowerUpCalorHumano powerUp;

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        powerUp.InsertarCarbon();
        // Feedback visual/sonoro de inserción
    }
} 