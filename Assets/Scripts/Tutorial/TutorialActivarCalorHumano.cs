using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarCalorHumano", menuName = "Tutorial/TutorialActivarCalorHumano", order = 19)]
public class TutorialActivarCalorHumano : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento estático del PowerUpCalorHumano
        PowerUpCalorHumano.OnCalorHumanoActivated += OnCalorHumanoActivated;
        
        Debug.Log("[TutorialActivarCalorHumano] Inicializado. Esperando que se active el PowerUp Calor Humano.");
    }

    private void OnCalorHumanoActivated(PowerUpCalorHumano calorHumano)
    {
        if (TutorialFinished) return;
        
        Debug.Log($"[TutorialActivarCalorHumano] PowerUpCalorHumano '{calorHumano.name}' fue activado (TurnOn llamado). Completando tutorial para todos los jugadores.");
        CompleteTutorial();
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        // Desuscribirse del evento para evitar memory leaks
        PowerUpCalorHumano.OnCalorHumanoActivated -= OnCalorHumanoActivated;
    }
    
    // Desuscribirse también cuando se destruye el SO
    private void OnDestroy()
    {
        PowerUpCalorHumano.OnCalorHumanoActivated -= OnCalorHumanoActivated;
    }
}

