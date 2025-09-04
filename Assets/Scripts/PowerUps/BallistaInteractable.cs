using UnityEngine;

public class BallistaInteractable : MonoBehaviour, IInteractable
{
    public PowerUpMotivacionEstatua powerUp;
    public StatueInteractable statue;

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        // Simular disparo
        if (statue != null && Vector3.Distance(transform.position, statue.transform.position) < 10f)
        {
            powerUp.OnBallistaHit();
            // Feedback visual/sonoro de disparo
        }
    }
} 