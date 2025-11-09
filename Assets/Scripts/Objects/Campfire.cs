using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class Campfire : MonoBehaviour, IInteractable
{
    public InteractPriority InteractPriority => InteractPriority.Medium;

    [Header("Audio")]
    [SerializeField] private int igniteSfxIndex = -1;

    public void Interact(GameObject interactor)
    {
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder != null && holder.GetHeldObject() != null)
        {
            PaloIgnifugo palo = holder.GetHeldObject().GetComponent<PaloIgnifugo>();
            if (palo != null && !palo.EstaEncendido())
            {
                palo.SetEncendido(true);

                // Reproducir SFX de encendido si está configurado
                if (igniteSfxIndex >= 0)
                {
                    var audio = FindFirstObjectByType<AudioManager>();
                    if (audio != null)
                    {
                        audio.PlaySFX(igniteSfxIndex);
                    }
                }
            }
        }
    }
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }
}
