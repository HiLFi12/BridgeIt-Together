using UnityEngine;

[CreateAssetMenu(fileName = "TutorialLever", menuName = "Tutorial/TutorialLever", order = 19)]
public class TutorialLever : TutorialSO
{
    // Variable estática para rastrear si algún jugador ya completó esta tarea
    private static bool anyPlayerCompletedLever = false;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Resetear el estado estático al inicializar el primer tutorial
        anyPlayerCompletedLever = false;
        
        // Suscribirse al evento estático de la palanca
        WagonLeverInteractable.OnLeverInteracted += OnLeverInteracted;
        
        if (player != null)
        {
            Debug.Log($"[TutorialLever] Inicializado para el jugador '{player.name}'. Esperando que alguien tire de la palanca.");
        }
        else
        {
            Debug.LogWarning("[TutorialLever] No hay jugador asignado.");
        }
    }

    private void OnLeverInteracted(WagonLeverInteractable lever, GameObject interactor)
    {
        // Si el tutorial ya está terminado, no hacer nada
        if (TutorialFinished) return;
        
        // Marcar que algún jugador completó la tarea
        if (!anyPlayerCompletedLever)
        {
            anyPlayerCompletedLever = true;
            
            // Obtener el nombre del jugador que interactuó
            string interactorName = interactor != null ? interactor.name : "Desconocido";
            Debug.Log($"[TutorialLever] El jugador '{interactorName}' tiró de la palanca '{lever.name}'. Tutorial completado para todos.");
        }
        
        // Completar el tutorial (se completará para todos los jugadores)
        CompleteTutorial();
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        WagonLeverInteractable.OnLeverInteracted -= OnLeverInteracted;
    }
    
    // Desuscribirse también cuando se destruye el ScriptableObject
    private void OnDestroy()
    {
        WagonLeverInteractable.OnLeverInteracted -= OnLeverInteracted;
    }
}

