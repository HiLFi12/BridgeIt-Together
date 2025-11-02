using UnityEngine;

[CreateAssetMenu(fileName = "TutorialUsarCarbonEnHorno", menuName = "Tutorial/TutorialUsarCarbonEnHorno", order = 15)]
public class TutorialUsarCarbonEnHorno : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático del Furnace
        Furnace.OnCoalAdded += OnCoalAdded;
    }

    private void OnCoalAdded(Furnace furnace, GameObject interactor)
    {
        if (TutorialFinished) return;
        
        // Verificar que el interactor sea el jugador asignado a este tutorial
        if (player != null && interactor == player.gameObject)
        {
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        Furnace.OnCoalAdded -= OnCoalAdded;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        Furnace.OnCoalAdded -= OnCoalAdded;
    }
}

