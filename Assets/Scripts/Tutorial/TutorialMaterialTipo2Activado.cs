using UnityEngine;

[CreateAssetMenu(fileName = "TutorialMaterialTipo2Activado", menuName = "Tutorial/TutorialMaterialTipo2Activado", order = 12)]
public class TutorialMaterialTipo2Activado : TutorialSO
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
                Debug.Log($"[TutorialMaterialTipo2Activado] Inicializado para el jugador '{player.name}'.");
                // Chequear al inicializar por si ya tiene el material activado
                CheckAndComplete();
            }
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
            Debug.Log($"[TutorialMaterialTipo2Activado] Jugador '{player.name}' tiene material con isReady=true en mano.");
            CompleteTutorial();
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        holder = null;
    }
}

