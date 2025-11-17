using UnityEngine;

[CreateAssetMenu(fileName = "TutorialAgarrarEstatua", menuName = "Tutorial/TutorialAgarrarEstatua", order = 14)]
public class TutorialAgarrarEstatua : TutorialSO
{
    private bool _isSubscribed;

    public override void Initialize()
    {
        base.Initialize();
        
        // Suscribirse al evento de StatueInteractable
        if (!_isSubscribed)
        {
            StatueInteractable.OnStatuePickedUp += OnStatuePickedUp;
            _isSubscribed = true;
            Debug.Log("[TutorialAgarrarEstatua] Inicializado. Esperando que cualquier player agarre una estatua.");
        }
    }

    private void OnStatuePickedUp(StatueInteractable statue, GameObject interactor)
    {
        if (TutorialFinished) return;
        
        // Verificar si el interactor es un player (cualquiera)
        Player interactorPlayer = interactor.GetComponent<Player>();
        if (interactorPlayer != null)
        {
            Debug.Log($"[TutorialAgarrarEstatua] Player '{interactorPlayer.name}' agarró la estatua '{statue.name}'. Completando tutorial para todos.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        Debug.Log("[TutorialAgarrarEstatua] Tutorial reseteado.");
    }
    
    private void OnDestroy()
    {
        if (_isSubscribed)
        {
            StatueInteractable.OnStatuePickedUp -= OnStatuePickedUp;
            _isSubscribed = false;
        }
    }
}

