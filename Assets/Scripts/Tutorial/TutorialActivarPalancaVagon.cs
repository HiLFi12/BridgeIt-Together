using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarPalancaVagon", menuName = "Tutorial/TutorialActivarPalancaVagon", order = 18)]
public class TutorialActivarPalancaVagon : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático de la palanca
        WagonLeverInteractable.OnLeverInteracted += OnLeverInteracted;
        
        if (player != null)
        {
            Debug.Log($"[TutorialActivarPalancaVagon] Inicializado para el jugador '{player.name}'. Esperando que interactúe con la palanca del vagón.");
        }
        else
        {
            Debug.LogWarning("[TutorialActivarPalancaVagon] No hay jugador asignado.");
        }
    }

    private void OnLeverInteracted(WagonLeverInteractable lever, GameObject interactor)
    {
        if (TutorialFinished) return;
        
        // Verificar que el interactor sea el jugador asignado a este tutorial
        if (player != null && interactor == player.gameObject)
        {
            Debug.Log($"[TutorialActivarPalancaVagon] Jugador '{player.name}' interactuó con la palanca '{lever.name}'. Completando tutorial.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        WagonLeverInteractable.OnLeverInteracted -= OnLeverInteracted;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        WagonLeverInteractable.OnLeverInteracted -= OnLeverInteracted;
    }
}

