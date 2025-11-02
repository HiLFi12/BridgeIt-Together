using UnityEngine;

/// <summary>
/// Palanca que inicia el movimiento del Wagon si hay calor activo.
/// </summary>
public class WagonLeverInteractable : MonoBehaviour, IInteractable
{
    // Evento estático para notificar cuando un jugador interactúa con la palanca
    public static event System.Action<WagonLeverInteractable, GameObject> OnLeverInteracted;
    
    [SerializeField] private Wagon targetWagon;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private bool requirePlayerHasNoObject = false;

    public InteractPriority InteractPriority => interactPriority;

    public void Interact(GameObject interactor)
    {
        // Notificar que el jugador interactuó con la palanca (para tutoriales)
        OnLeverInteracted?.Invoke(this, interactor);
        
        if (targetWagon == null)
        {
            Debug.LogWarning("WagonLeverInteractable: no hay Wagon asignado.");
            return;
        }
        if (requirePlayerHasNoObject)
        {
            var holder = interactor.GetComponent<PlayerObjectHolder>();
            if (holder != null && holder.HasObjectInHand())
            {
                Debug.Log("WagonLever: el jugador debe tener las manos libres para iniciar.");
                return;
            }
        }
        bool started = targetWagon.AttemptStart();
        if (started)
        {
            Debug.Log("WagonLever: viaje iniciado.");
        }
    }
}
