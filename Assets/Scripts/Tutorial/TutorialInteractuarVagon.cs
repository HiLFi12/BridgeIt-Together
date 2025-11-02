using UnityEngine;

[CreateAssetMenu(fileName = "TutorialInteractuarVagon", menuName = "Tutorial/TutorialInteractuarVagon", order = 17)]
public class TutorialInteractuarVagon : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático del Wagon
        Wagon.OnWagonInteracted += OnWagonInteracted;
        
        if (player != null)
        {
            Debug.Log($"[TutorialInteractuarVagon] Inicializado para el jugador '{player.name}'. Esperando que interactúe con un vagón.");
        }
        else
        {
            Debug.LogWarning("[TutorialInteractuarVagon] No hay jugador asignado.");
        }
    }

    private void OnWagonInteracted(Wagon wagon, GameObject interactor)
    {
        if (TutorialFinished) return;
        
        // Verificar que el interactor sea el jugador asignado a este tutorial
        if (player != null && interactor == player.gameObject)
        {
            Debug.Log($"[TutorialInteractuarVagon] Jugador '{player.name}' interactuó con el vagón '{wagon.name}'. Completando tutorial.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        Wagon.OnWagonInteracted -= OnWagonInteracted;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        Wagon.OnWagonInteracted -= OnWagonInteracted;
    }
}

