using System.Collections.Generic;
using UnityEngine;

public class SimpleVehicleDamage : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask bridgeLayer;
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";
    [SerializeField] private string vehicleTag = "Vehicle";
    [SerializeField] private bool debugMode = true;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<Collider> previousInside = new();
    private readonly HashSet<Collider> insideNow = new();
    private readonly HashSet<string> damagedQuadrants = new(); // Cambiado a string con coordenadas

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
        insideNow.Clear();

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
            if (!col) continue;
            if (!PassesFilters(col)) continue;

            insideNow.Add(col);

            bool isNewEntry = !previousInside.Contains(col);
            if (!isNewEntry) continue;

            ProcessQuadrant(col);
        }

        previousInside.Clear();
        foreach (var c in insideNow)
            previousInside.Add(c);
    }

    private bool PassesFilters(Collider col)
    {
        if (!col) return false;
        if (!string.IsNullOrEmpty(bridgeQuadrantTag) && !col.CompareTag(bridgeQuadrantTag))
            return false;
        return true;
    }

    private void ProcessQuadrant(Collider collider)
    {
        if (TryGetQuadrantInfo(collider, out BridgeConstructionGrid grid, out int x, out int z))
        {
            // Crear clave única con grid + coordenadas
            string key = $"{grid.GetInstanceID()}_{x}_{z}";
            
            if (damagedQuadrants.Contains(key))
                return;

            ApplyDamageToQuadrant(grid, x, z);
            damagedQuadrants.Add(key);
        }
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

        // CAMBIO: Usar el centro del collider en lugar de ClosestPoint
        Vector3 worldPoint = (collider is Collider col)
            ? col.bounds.center  // <-- AQUÍ ESTÁ EL CAMBIO
            : collider.transform.position;

        Vector3 localPos = grid.transform.InverseTransformPoint(worldPoint);
        x = Mathf.FloorToInt(localPos.x / grid.quadrantSize);
        z = Mathf.FloorToInt(localPos.z / grid.quadrantSize);

        return true;
    }


    public virtual void ApplyDamageToQuadrant(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;

        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            if (debugMode)
                Debug.LogWarning($"Cuadrante [{x},{z}] fuera de límites");
            return;
        }

        grid.OnVehicleImpact(x, z);

        if (debugMode)
            Debug.Log($"Daño aplicado al cuadrante [{x},{z}] en {grid.name}");
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
