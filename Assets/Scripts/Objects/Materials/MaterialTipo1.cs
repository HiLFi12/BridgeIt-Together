using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class MaterialTipo1 : MaterialBaseInteractable, IUIActivatable
{
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 0;
    
    public int UIIndex => uiIndex;
    
    protected override int LayerIndex => 0; // capa base

    protected override void PostEnsure()
    {
        // Aseguramos el tipo de material en el BridgeMaterialInfo creado por la base
        var info = GetComponent<BridgeMaterialInfo>();
        if (info != null)
        {
            info.materialType = BridgeQuadrantSO.MaterialType.Wood; // MaterialTipo1 es madera
        }
    }
    
    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }
 
    public void CombinarConResina()
    {
        Debug.Log("Palo de madera listo para ser combinado con resina.");
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}