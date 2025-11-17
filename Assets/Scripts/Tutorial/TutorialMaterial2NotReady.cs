using UnityEngine;

[CreateAssetMenu(fileName = "TutorialMaterial2NotReady", menuName = "Tutorial/TutorialMaterial2NotReady", order = 11)]
public class TutorialMaterial2NotReady : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[TutorialMaterial2NotReady] Inicializado. Esperando que algún MaterialTipo2Ready tenga isReady = true.");
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished) return;

        // Buscar todos los MaterialTipo2Ready en la escena
        MaterialTipo2Ready[] materials = Object.FindObjectsByType<MaterialTipo2Ready>(FindObjectsSortMode.None);
        
        foreach (var material in materials)
        {
            if (!material.IsReady)
            {
                Debug.Log($"[TutorialMaterial2NotReady] Encontrado MaterialTipo2Ready '{material.name}' con isReady = true. Completando tutorial.");
                CompleteTutorial();
                return;
            }
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        Debug.Log("[TutorialMaterial2NotReady] Tutorial reseteado.");
    }
}

