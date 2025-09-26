using UnityEngine;

/// <summary>
/// Palanca que inicia el movimiento del Wagon si hay calor activo.
/// </summary>
public class WagonLeverInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Wagon targetWagon;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private bool requirePlayerHasNoObject = false;

    public InteractPriority InteractPriority => interactPriority;

    public void Interact(GameObject interactor)
    {
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
