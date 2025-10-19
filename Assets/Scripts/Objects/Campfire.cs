using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class Campfire : MonoBehaviour, IInteractable
{
    public InteractPriority InteractPriority => InteractPriority.Medium;

    public void Interact(GameObject interactor)
    {
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder != null && holder.GetHeldObject() != null)
        {
            PaloIgnifugo palo = holder.GetHeldObject().GetComponent<PaloIgnifugo>();
            if (palo != null && !palo.EstaEncendido())
            {
                palo.SetEncendido(true);
            }
        }
    }
}
