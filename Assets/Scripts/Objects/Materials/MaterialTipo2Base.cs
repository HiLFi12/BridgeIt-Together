using BridgeItTogether.Gameplay.Abstractions;
using UnityEngine;

/// <summary>
/// Script padre para Material Tipo 2: maneja construcción y estado PuedeConstruirse.
/// </summary>
public class MaterialTipo2Base : MaterialBaseInteractable, IUIActivatable
{
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 0;
    public int UIIndex => uiIndex;

    protected override int LayerIndex => 1; 

    protected override void Awake()
    {
        base.Awake();
        if (era == BridgeQuadrantSO.EraType.Medieval)
        {
            puedeConstruirse = false; // Restricción específica Medieval
        }
    }

    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }
}
