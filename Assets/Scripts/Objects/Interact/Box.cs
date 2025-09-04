using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IInteractable
{
    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();

        if (playerObjectHolder != null)
        {
            if (playerObjectHolder.HasObjectInHand())
            {
                Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
            }
            
            // Usar PickUpExistingInstance para recoger este objeto directamente
            playerObjectHolder.PickUpExistingInstance(gameObject);
        }
        else
        {
            Debug.Log("El interactor no tiene el componente PlayerObjectHolder.");
        }
    }
}
