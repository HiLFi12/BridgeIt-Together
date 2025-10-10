using UnityEngine;

public class HeavyVehicleDamage : SimpleVehicleDamage
{
    [Header("Daño extra")]
    [SerializeField] private int heavyDamageAmount = 1000;

    public override void ApplyDamageToQuadrant(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;
        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            return;
        }
        // Aplica mucho más daño
        for (int i = 0; i < heavyDamageAmount; i++)
            grid.OnVehicleImpact(x, z);
    }
}

