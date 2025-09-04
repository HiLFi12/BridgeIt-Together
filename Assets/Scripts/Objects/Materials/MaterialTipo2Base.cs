using UnityEngine;

/// <summary>
/// Script padre para Material Tipo 2: maneja construcción y estado PuedeConstruirse.
/// </summary>
public class MaterialTipo2Base : MaterialBaseInteractable
{
    protected override int LayerIndex => 1; 

    protected override void Awake()
    {
        base.Awake();
        if (era == BridgeQuadrantSO.EraType.Medieval)
        {
            puedeConstruirse = false; // Restricción específica Medieval
        }
    }

}
