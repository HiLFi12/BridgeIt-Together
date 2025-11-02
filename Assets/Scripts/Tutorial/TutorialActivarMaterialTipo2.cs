using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarMaterialTipo2", menuName = "Tutorial/TutorialActivarMaterialTipo2", order = 10)]
public class TutorialActivarMaterialTipo2 : TutorialSO
{
    private MaterialTipo2Ready[] allMaterials;

    public override void Initialize()
    {
        base.Initialize();
        
        // Encontrar todos los MaterialTipo2Ready en la escena
        allMaterials = Object.FindObjectsByType<MaterialTipo2Ready>(FindObjectsSortMode.None);
        
        if (allMaterials == null || allMaterials.Length == 0)
        {
            Debug.LogWarning("[TutorialActivarMaterialTipo2] No se encontraron MaterialTipo2Ready en la escena.");
        }
        else
        {
            Debug.Log($"[TutorialActivarMaterialTipo2] Inicializado con {allMaterials.Length} material(es).");
            // Chequear al inicializar por si ya hay alguno activado
            CheckAndComplete();
        }
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished || allMaterials == null || allMaterials.Length == 0) return;

        CheckAndComplete();
    }

    private void CheckAndComplete()
    {
        // Revisar todos los materiales en la escena
        foreach (var material in allMaterials)
        {
            if (material == null) continue;

            // Verificar si isReady es true (usando la propiedad IsReady de la clase base)
            if (material.IsReady)
            {
                Debug.Log($"[TutorialActivarMaterialTipo2] Material '{material.name}' encontrado con isReady=true.");
                CompleteTutorial();
                return;
            }
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        allMaterials = null;
    }
}

