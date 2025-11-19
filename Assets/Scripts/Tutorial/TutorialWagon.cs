using UnityEngine;

[CreateAssetMenu(fileName = "TutorialWagon", menuName = "Tutorial/TutorialWagon", order = 18)]
public class TutorialWagon : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático del Wagon
        Wagon.OnWagonInteracted += OnWagonInteracted;
        
        if (player != null)
        {
            Debug.Log($"[TutorialWagon] Inicializado para el jugador '{player.name}'. Esperando interacción con el vagón.");
        }
        else
        {
            Debug.LogWarning("[TutorialWagon] No hay jugador asignado.");
        }
    }

    private void OnWagonInteracted(Wagon wagon, GameObject interactor)
    {
        // Si el tutorial ya está terminado, no hacer nada
        if (TutorialFinished) return;
        
        // Verificar que el interactor sea el jugador asignado a este tutorial
        if (player != null && interactor == player.gameObject)
        {
            Debug.Log($"[TutorialWagon] El jugador '{player.name}' interactuó con el vagón '{wagon.name}'. Tutorial completado.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        Wagon.OnWagonInteracted -= OnWagonInteracted;
    }
    
    // Desuscribirse también cuando se destruye el ScriptableObject
    private void OnDestroy()
    {
        Wagon.OnWagonInteracted -= OnWagonInteracted;
    }
}

