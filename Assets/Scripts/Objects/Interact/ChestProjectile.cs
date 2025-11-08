using UnityEngine;
using System.Collections.Generic;

public class ChestProjectile : MonoBehaviour
{
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";
    [SerializeField] private GameObject shaderPrefab;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask bridgeLayer;
    [Header("Damage")]
    [Tooltip("Daño genérico aplicado al cuadrante, igual que SimpleVehicleDamage (ApplyGenericDamage).")]
    [SerializeField] private float damageAmount = 1f;
    [Tooltip("Si > 0, sobreescribe damageAmount SOLO para era Medieval.")]
    [SerializeField] private float medievalDamageAmount = 0f;

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

            // Buscar BridgeQuadrantInstance (daño unificado como SimpleVehicleDamage)
            BridgeQuadrantInstance quadrantInstance = col.GetComponent<BridgeQuadrantInstance>();
            if (quadrantInstance != null && quadrantInstance.quadrantSO != null)
            {
                // Intentar usar BridgeQuadrantInfo para una clave consistente (grid+x+z) y refrescar visuales
                var info = quadrantInstance.GetComponent<BridgeQuadrantInfo>() ?? quadrantInstance.GetComponentInParent<BridgeQuadrantInfo>();
                string key;
                if (info != null && info.grid != null)
                {
                    key = $"{info.grid.GetInstanceID()}_{info.x}_{info.z}";
                }
                else
                {
                    key = $"{quadrantInstance.GetInstanceID()}";
                }
                
                if (!damagedQuadrants.Contains(key))
                {
                    // Seleccionar cantidad de daño (igual que SimpleVehicleDamage → ApplyGenericDamage)
                    float amt = damageAmount;
                    if (quadrantInstance.quadrantSO.era == BridgeQuadrantSO.EraType.Medieval && medievalDamageAmount > 0f)
                        amt = medievalDamageAmount;

                    quadrantInstance.quadrantSO.ApplyGenericDamage(amt);

                    // Si tenemos info de grid, forzar refresco visual
                    if (info != null && info.grid != null)
                    {
                        info.grid.RefreshQuadrantVisuals(info.x, info.z);
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
    // Usar pasos por eje si están configurados en el grid (consistente con PlayerBridgeInteraction/SimpleVehicleDamage)
    float stepX = grid.QuadrantStepX;
    float stepZ = grid.QuadrantStepZ;
    x = Mathf.FloorToInt(localPos.x / Mathf.Max(0.0001f, stepX));
    z = Mathf.FloorToInt(localPos.z / Mathf.Max(0.0001f, stepZ));

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
