using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialActivarMaterialTipo2", menuName = "Tutorial/TutorialActivarMaterialTipo2", order = 10)]
public class TutorialActivarMaterialTipo2 : TutorialSO
{
    private HashSet<MaterialTipo2Ready> _initialReadyMaterials;
    private bool _hasInitialized;

    public override void Initialize()
    {
        base.Initialize();
        
        // Guardar referencias a los materiales que YA están ready al inicio
        _initialReadyMaterials = new HashSet<MaterialTipo2Ready>();
        
        MaterialTipo2Ready[] allMaterials = Object.FindObjectsByType<MaterialTipo2Ready>(FindObjectsSortMode.None);
        foreach (var material in allMaterials)
        {
            if (material != null && material.IsReady)
            {
                _initialReadyMaterials.Add(material);
            }
        }
        
        _hasInitialized = true;
        
        Debug.Log($"[TutorialActivarMaterialTipo2] Inicializado. {_initialReadyMaterials.Count} materiales ya estaban ready. Esperando activación de un material nuevo.");
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished || !_hasInitialized) return;

        // Buscar materiales que estén ready AHORA
        MaterialTipo2Ready[] allMaterials = Object.FindObjectsByType<MaterialTipo2Ready>(FindObjectsSortMode.None);
        
        foreach (var material in allMaterials)
        {
            if (material == null) continue;
            
            // Si este material está ready Y NO estaba en el set inicial → es nuevo!
            if (material.IsReady && !_initialReadyMaterials.Contains(material))
            {
                Debug.Log($"[TutorialActivarMaterialTipo2] Material '{material.name}' activado (no estaba ready al inicio). Completando tutorial.");
                CompleteTutorial();
                return;
            }
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        _hasInitialized = false;
        if (_initialReadyMaterials != null)
        {
            _initialReadyMaterials.Clear();
        }
    }
}

