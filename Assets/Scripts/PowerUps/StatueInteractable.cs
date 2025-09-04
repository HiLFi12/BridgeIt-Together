using UnityEngine;

public class StatueInteractable : MonoBehaviour, IInteractable
{
    public PowerUpMotivacionEstatua powerUp;
    public Transform destinationPoint;
    private bool isCarried = false;

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        // Aquí puedes implementar la lógica de recogida/transporte
        isCarried = true;
        // Asignar como hijo del jugador, por ejemplo
        transform.SetParent(interactor.transform);
        // Feedback visual/sonoro
    }

    private void Update()
    {
        if (isCarried && Vector3.Distance(transform.position, destinationPoint.position) < 1f)
        {
            powerUp.OnStatueArrived();
            // Feedback visual/sonoro de llegada
        }
    }
} 