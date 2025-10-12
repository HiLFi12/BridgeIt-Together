using UnityEngine;

public class HeavyVehicleDamage : SimpleVehicleDamage
{
    [Header("Daño extra para vehículos pesados")]
    [SerializeField] private int damageMultiplier = 20;

    protected override void ApplyDamageToQuadrant(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;
        
        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            return;
        }
        
        // Aplica daño multiplicado llamando OnVehicleImpact múltiples veces
        for (int i = 0; i < damageMultiplier; i++)
        {
            grid.OnVehicleImpact(x, z);
        }
    }
}
