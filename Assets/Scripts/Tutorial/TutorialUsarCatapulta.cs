using UnityEngine;

[CreateAssetMenu(fileName = "TutorialUsarCatapulta", menuName = "Tutorial/TutorialUsarCatapulta", order = 11)]
public class TutorialUsarCatapulta : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático de la catapulta
        Catapult.OnCatapultUsed += OnCatapultUsed;
        
        Debug.Log("[TutorialUsarCatapulta] Inicializado. Esperando que se use una catapulta.");
    }

    private void OnCatapultUsed(Catapult catapult)
    {
        if (TutorialFinished) return;
        
        Debug.Log($"[TutorialUsarCatapulta] Catapulta '{catapult.name}' fue usada. Completando tutorial.");
        CompleteTutorial();
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        Catapult.OnCatapultUsed -= OnCatapultUsed;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        Catapult.OnCatapultUsed -= OnCatapultUsed;
    }
}

