using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SimpleVehicleDamage : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";
    [SerializeField] private LayerMask bridgeLayer;
    [SerializeField] private string vehicleTag = "Vehicle";

    [Header("Damage")]
    [FormerlySerializedAs("futuristicDamageAmount")]
    [SerializeField] private float damageAmount = 1f;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<string> damagedQuadrants = new HashSet<string>();

    private void Awake()
    {
        if (!detectionPoint) detectionPoint = transform;
    }

    private void OnValidate()
    {
        if (!detectionPoint) detectionPoint = transform;
        detectionRadius = Mathf.Max(0.01f, detectionRadius);
    }

    private void FixedUpdate()
    {
        if (!IsVehicle()) return;
        RunDetection();
    }

    private void RunDetection()
    {
        int count = Physics.OverlapSphereNonAlloc(
            detectionPoint.position,
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

            // Preferir BridgeQuadrantInstance para daño directo unificado
            BridgeQuadrantInstance quadrantInstance = col.GetComponent<BridgeQuadrantInstance>();
            if (quadrantInstance != null && quadrantInstance.quadrantSO != null)
            {
                string key = $"{quadrantInstance.GetInstanceID()}";

                if (!damagedQuadrants.Contains(key))
                {
                    quadrantInstance.quadrantSO.ApplyGenericDamage(damageAmount);

                    Debug.Log($"[SimpleVehicleDamage] Daño {damageAmount} aplicado (era {quadrantInstance.quadrantSO.era})");

                    damagedQuadrants.Add(key);
                }
                continue;
            }

            // Fallback: usar BridgeConstructionGrid
            if (TryGetQuadrantInfo(col, out BridgeConstructionGrid grid, out int x, out int z))
            {
                string key = $"{grid.GetInstanceID()}_{x}_{z}";

                if (!damagedQuadrants.Contains(key))
                {
                    ApplyDamageToQuadrant(grid, x, z);
                    damagedQuadrants.Add(key);
                }
            }
        }
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

    protected virtual void ApplyDamageToQuadrant(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;

        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            Debug.LogWarning($"[SimpleVehicleDamage] Cuadrante [{x},{z}] fuera de límites");
            return;
        }

        grid.OnVehicleImpact(x, z);

        Debug.Log($"[SimpleVehicleDamage] Daño aplicado al cuadrante [{x},{z}] en {grid.name}");
    }

    private bool IsVehicle()
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.CompareTag(vehicleTag)) return true;
            current = current.parent;
        }
        return false;
    }

    public void Reset()
    {
        damagedQuadrants.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!detectionPoint) detectionPoint = transform;
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
    }
#endif
}
