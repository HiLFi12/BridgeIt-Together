using UnityEngine;

[CreateAssetMenu(fileName = "TutorialAgarrarCarbon", menuName = "Tutorial/TutorialAgarrarCarbon", order = 14)]
public class TutorialAgarrarCarbon : TutorialSO
{
    private PlayerObjectHolder holder;

    public override void Initialize()
    {
        base.Initialize();
        
        if (player != null)
        {
            holder = player.GetComponent<PlayerObjectHolder>();
            if (holder != null)
            {
                Debug.Log($"[TutorialAgarrarCarbon] Inicializado para el jugador '{player.name}'.");
                // Chequear al inicializar por si ya tiene el carbón
                CheckAndComplete();
            }
            else
            {
                Debug.LogWarning("[TutorialAgarrarCarbon] No se encontró PlayerObjectHolder en el jugador asignado.");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialAgarrarCarbon] No hay jugador asignado.");
        }
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished || holder == null) return;

        CheckAndComplete();
    }

    private void CheckAndComplete()
    {
        if (holder == null) return;
        if (!holder.HasObjectInHand()) return;

        var held = holder.GetHeldObject();
        if (held == null) return;

        // Buscar el componente CoalItem
        var coalItem = held.GetComponent<CoalItem>();
        if (coalItem != null)
        {
            Debug.Log($"[TutorialAgarrarCarbon] Jugador '{player.name}' tiene un CoalItem en mano.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        holder = null;
    }
}

