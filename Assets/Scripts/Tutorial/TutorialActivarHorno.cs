using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarHorno", menuName = "Tutorial/TutorialActivarHorno", order = 16)]
public class TutorialActivarHorno : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento de activación del horno
        Furnace.OnFurnaceTurnedOn += OnFurnaceTurnedOn;
        
        if (player != null)
        {
            Debug.Log($"[TutorialActivarHorno] Inicializado para el jugador '{player.name}'. Esperando que active un horno (TurnOn).");
        }
        else
        {
            Debug.LogWarning("[TutorialActivarHorno] No hay jugador asignado.");
        }
    }

    private void OnFurnaceTurnedOn(Furnace furnace, GameObject interactor)
    {
        if (TutorialFinished) return;
        
        // Verificar que el interactor sea el jugador asignado a este tutorial
        if (player != null && interactor == player.gameObject)
        {
            Debug.Log($"[TutorialActivarHorno] Jugador '{player.name}' activó el horno '{furnace.name}' (TurnOn llamado, HeatSphere activada). Completando tutorial.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        Furnace.OnFurnaceTurnedOn -= OnFurnaceTurnedOn;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        Furnace.OnFurnaceTurnedOn -= OnFurnaceTurnedOn;
    }
}

