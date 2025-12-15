using UnityEngine;

/// <summary>
/// Tutorial que se completa cuando el jugador interactúa exitosamente con una TorchInteractable
/// usando un PaloIgnifugo encendido.
/// </summary>
[CreateAssetMenu(fileName = "TutorialEncenderAntorcha", menuName = "Tutorial/TutorialEncenderAntorcha", order = 11)]
public class TutorialEncenderAntorcha : TutorialSO
{
    private bool eventSubscribed;

    public override void Initialize()
    {
        base.Initialize();

        // Suscribirse al evento estático de TorchInteractable
        if (!eventSubscribed)
        {
            TorchInteractable.OnSuccessfulTorchLit += OnTorchLitSuccessfully;
            eventSubscribed = true;
        }
    }

    private void OnTorchLitSuccessfully()
    {
        // Completar el tutorial cuando se enciende exitosamente una antorcha
        if (!TutorialFinished)
        {
            Debug.Log("[TutorialEncenderAntorcha] ¡Antorcha encendida exitosamente! Tutorial completado.");
            CompleteTutorial();
        }
    }

    // Cleanup cuando el ScriptableObject se destruye
    private void OnDisable()
    {
        if (eventSubscribed)
        {
            TorchInteractable.OnSuccessfulTorchLit -= OnTorchLitSuccessfully;
            eventSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (eventSubscribed)
        {
            TorchInteractable.OnSuccessfulTorchLit -= OnTorchLitSuccessfully;
            eventSubscribed = false;
        }
    }
}

