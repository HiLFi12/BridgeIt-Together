using UnityEngine;

[CreateAssetMenu(fileName = "TutorialActivarEstatua", menuName = "Tutorial/TutorialActivarEstatua", order = 13)]
public class TutorialActivarEstatua : TutorialSO
{
    private BridgeConstructionGrid _bridgeGrid;
    private bool _hasInitialized;

    public override void Initialize()
    {
        base.Initialize();
        
        // Buscar el grid del puente en la escena
        if (_bridgeGrid == null)
        {
            _bridgeGrid = Object.FindFirstObjectByType<BridgeConstructionGrid>();
            if (_bridgeGrid == null)
            {
                Debug.LogWarning("[TutorialActivarEstatua] No se encontró BridgeConstructionGrid en la escena.");
                return;
            }
        }
        
        _hasInitialized = true;
        
        Debug.Log($"[TutorialActivarEstatua] Inicializado. Esperando que todos los cuadrantes estén completos hasta layer 1.");
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished) return;
        if (!_hasInitialized) return;
        if (_bridgeGrid == null) return;
        
        // Verificar si TODOS los cuadrantes tienen las capas 0 y 1 completadas
        if (AreAllQuadrantsCompleteUpToLayer1())
        {
            Debug.Log($"[TutorialActivarEstatua] ¡Todos los cuadrantes completos hasta layer 1! Completando tutorial.");
            CompleteTutorial();
        }
    }

    private bool AreAllQuadrantsCompleteUpToLayer1()
    {
        if (_bridgeGrid == null) return false;
        
        for (int x = 0; x < _bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < _bridgeGrid.gridLength; z++)
            {
                var so = _bridgeGrid.GetQuadrantSO(x, z);
                if (so == null) continue;
                
                // Verificar que layer 0 y layer 1 estén completadas
                if (so.requiredLayers.Length > 0 && !so.requiredLayers[0].isCompleted)
                {
                    return false; // Layer 0 no está completa
                }
                
                if (so.requiredLayers.Length > 1 && !so.requiredLayers[1].isCompleted)
                {
                    return false; // Layer 1 no está completa
                }
            }
        }
        
        // Todos los cuadrantes tienen layer 0 y 1 completas
        return true;
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        
        _hasInitialized = false;
        
        Debug.Log("[TutorialActivarEstatua] Tutorial reseteado.");
    }
}

