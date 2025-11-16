using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarEstatua", menuName = "Tutorial/TutorialActivarEstatua", order = 13)]
public class TutorialActivarEstatua : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático de la estatua
        PowerUpMotivacionEstatua.OnEstatuaActivated += OnEstatuaActivated;

        Debug.Log("[TutorialActivarEstatua] Inicializado. Esperando que se active una estatua con flecha.");
    }

    private void OnEstatuaActivated(PowerUpMotivacionEstatua estatua)
    {
        if (TutorialFinished) return;
        
        Debug.Log($"[TutorialActivarEstatua] Estatua '{estatua.name}' fue activada (isActive=true). Completando tutorial.");
        CompleteTutorial();
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        PowerUpMotivacionEstatua.OnEstatuaActivated -= OnEstatuaActivated;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        PowerUpMotivacionEstatua.OnEstatuaActivated -= OnEstatuaActivated;
    }
}

