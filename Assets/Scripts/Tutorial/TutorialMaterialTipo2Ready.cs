using UnityEngine;

[CreateAssetMenu(fileName = "TutorialMaterialTipo2Ready", menuName = "Tutorial/TutorialMaterialTipo2Ready", order = 9)]
public class TutorialMaterialTipo2Ready : TutorialSO
{
    private PlayerObjectHolder[] allHolders;

    public override void Initialize()
    {
        base.Initialize();
        
        // Encontrar todos los PlayerObjectHolder en la escena (ambos jugadores)
        allHolders = Object.FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
        
        if (allHolders == null || allHolders.Length == 0)
        {
            Debug.LogWarning("[TutorialMaterialTipo2Ready] No se encontraron PlayerObjectHolder en la escena.");
        }
        else
        {
            Debug.Log($"[TutorialMaterialTipo2Ready] Inicializado con {allHolders.Length} jugador(es).");
            // Chequear al inicializar por si ya tiene el material
            CheckAndComplete();
        }
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished || allHolders == null || allHolders.Length == 0) return;

        CheckAndComplete();
    }

    private void CheckAndComplete()
    {
        // Revisar todos los jugadores
        foreach (var holder in allHolders)
        {
            if (holder == null || !holder.HasObjectInHand()) continue;

            var held = holder.GetHeldObject();
            if (held == null) continue;

            // Buscar el componente MaterialTipo2Ready
            var materialReady = held.GetComponent<MaterialTipo2Ready>();
            if (materialReady != null && !materialReady.IsReady)
            {
                Debug.Log($"[TutorialMaterialTipo2Ready] Material encontrado con startReady=false en jugador.");
                CompleteTutorial();
                return;
            }
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        allHolders = null;
    }
}


