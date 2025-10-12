using UnityEngine;
using System.Collections.Generic;

public class ChestProjectile : MonoBehaviour
{
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";
    [SerializeField] private GameObject shaderPrefab;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask bridgeLayer;
    [SerializeField] private float medievalDamageAmount = 10f;

    private readonly Collider[] overlap = new Collider[16];
    private readonly HashSet<string> damagedQuadrants = new();

    private void OnCollisionEnter(Collision collision)
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            overlap,
            bridgeLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            var col = overlap[i];
            if (col == null) continue;
            if (!string.IsNullOrEmpty(bridgeQuadrantTag) && !col.CompareTag(bridgeQuadrantTag))
                continue;

            // Buscar BridgeQuadrantInstance (solo para medieval)
            BridgeQuadrantInstance quadrantInstance = col.GetComponent<BridgeQuadrantInstance>();
            if (quadrantInstance != null && quadrantInstance.quadrantSO != null)
            {
                string key = $"{quadrantInstance.GetInstanceID()}";
                
                if (!damagedQuadrants.Contains(key))
                {
                    // SOLO afecta a cuadrantes medievales
                    if (quadrantInstance.quadrantSO.era == BridgeQuadrantSO.EraType.Medieval)
                    {
                        // Daño directo a batteryLife para medieval
                        quadrantInstance.quadrantSO.batteryLife -= medievalDamageAmount;
                    }
                    
                    damagedQuadrants.Add(key);
                }
                continue;
            }

            // Si no es BridgeQuadrantInstance, usar método antiguo con BridgeConstructionGrid
            if (TryGetQuadrantInfo(col, out BridgeConstructionGrid grid, out int x, out int z))
            {
                string key = $"{grid.GetInstanceID()}_{x}_{z}";
                if (damagedQuadrants.Contains(key)) continue;
                ApplyDamageToQuadrant(grid, x, z);
                damagedQuadrants.Add(key);
            }
        }

        if (shaderPrefab != null)
            Instantiate(shaderPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private bool TryGetQuadrantInfo(Component collider, out BridgeConstructionGrid grid, out int x, out int z)
    {
        grid = null;
        x = z = -1;

        BridgeQuadrantInfo info = collider.GetComponent<BridgeQuadrantInfo>();
        if (info == null)
            info = collider.GetComponentInParent<BridgeQuadrantInfo>();

        if (info != null && info.grid != null)
        {
            grid = info.grid;
            x = info.x;
            z = info.z;
            return true;
        }

        if (!collider.CompareTag(bridgeQuadrantTag))
            return false;

        grid = collider.GetComponentInParent<BridgeConstructionGrid>();
        if (grid == null) return false;

        Vector3 worldPoint = (collider is Collider col)
            ? col.bounds.center
            : collider.transform.position;

        Vector3 localPos = grid.transform.InverseTransformPoint(worldPoint);
        x = Mathf.FloorToInt(localPos.x / grid.quadrantSize);
        z = Mathf.FloorToInt(localPos.z / grid.quadrantSize);

        return true;
    }

    private void ApplyDamageToQuadrant(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;
        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            Debug.LogWarning($"Cuadrante [{x},{z}] fuera de límites");
            return;
        }
        grid.OnVehicleImpact(x, z);
        Debug.Log($"Daño aplicado al cuadrante [{x},{z}] en {grid.name}");
    }
}
