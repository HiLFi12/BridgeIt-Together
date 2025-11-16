using UnityEngine;

[CreateAssetMenu(fileName = "TutorialMaterialTipo2ProcessedReady", menuName = "Tutorial/TutorialMaterialTipo2ProcessedReady", order = 10)]
public class TutorialMaterialTipo2ProcessedReady : TutorialSO
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
                Debug.Log($"[TutorialMaterialTipo2ProcessedReady] Inicializado para el jugador '{player.name}'.");
                // Chequear al inicializar por si ya tiene el material
                CheckAndComplete();
            }
            else
            {
                Debug.LogWarning("[TutorialMaterialTipo2ProcessedReady] No se encontró PlayerObjectHolder en el jugador asignado.");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialMaterialTipo2ProcessedReady] No hay jugador asignado.");
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

        // Buscar el componente MaterialTipo2Ready
        var materialReady = held.GetComponent<MaterialTipo2Ready>();
        if (materialReady != null && materialReady.IsReady)
        {
            Debug.Log($"[TutorialMaterialTipo2ProcessedReady] Jugador '{player.name}' tiene un MaterialTipo2Ready con IsReady=true en mano.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        holder = null;
    }
}

