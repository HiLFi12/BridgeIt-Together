using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class MaterialTipo3 : MaterialBaseInteractable, IUIActivatable
{
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 0;
    
    public int UIIndex => uiIndex;

    // Índice de capa para este material (capa superior, índice 2)
    protected override int LayerIndex => 2;

    protected override void PostEnsure()
    {
        // MaterialBaseInteractable ya creó/actualizó BridgeMaterialInfo con era y layerIndex
        var materialInfo = GetComponent<BridgeMaterialInfo>();
        if (materialInfo != null)
        {
            materialInfo.materialType = BridgeQuadrantSO.MaterialType.Metal; // MaterialTipo3 es metal
        }
    }

    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}