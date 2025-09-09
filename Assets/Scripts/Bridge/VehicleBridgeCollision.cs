using System.Collections.Generic;
using UnityEngine;

public class VehicleBridgeCollision : MonoBehaviour
{
    [Header("Configuración")]
    public string bridgeQuadrantTag = "BridgeQuadrant";
    public string vehicleTag = "Vehicle";
    public bool debugMode = true;

    [Header("Control de Colisiones")]
    public float collisionCooldown = 1.0f;

    [Header("Compatibilidad (legacy)")]
    [Tooltip("Campo mantenido solo para compatibilidad con scripts que asignaban el grid manualmente. El sistema ahora detecta dinámicamente el BridgeConstructionGrid. Se usa como fallback si no se puede resolver desde el collider.")]
    public BridgeConstructionGrid bridgeGrid; // legacy

    // Key: "<gridID>_<x>_<z>"
    private Dictionary<string, float> recentCollisions = new Dictionary<string, float>();

    private void Start()
    {
        // Validación de tags (mantiene tu lógica original)
        ValidateTag(bridgeQuadrantTag, "cuadrantes del puente");
        ValidateTag(vehicleTag, "vehículos");
    }

    private void Update()
    {
        if (Time.time % 5.0f < Time.deltaTime)
            CleanupOldCollisions();
    }

    private void ValidateTag(string tagName, string desc)
    {
        bool exists = true;
        try
        {
            var go = new GameObject();
            go.tag = tagName;
            Object.Destroy(go);
        }
        catch { exists = false; }

        if (!exists)
            Debug.LogError($"El tag '{tagName}' no existe. Afecta a {desc}.");
        else if (debugMode) Debug.Log($"Tag '{tagName}' verificado.");
    }

    private void CleanupOldCollisions()
    {
        float now = Time.time;
        List<string> toRemove = new List<string>();
        foreach (var kv in recentCollisions)
            if (now - kv.Value > collisionCooldown * 2f)
                toRemove.Add(kv.Key);
        foreach (var k in toRemove) recentCollisions.Remove(k);
        if (debugMode && toRemove.Count > 0)
            Debug.Log($"Limpiadas {toRemove.Count} entradas antiguas.");
    }

    private bool IsVehicleOrChildOfVehicle()
    {
        if (CompareTag(vehicleTag)) return true;
        Transform p = transform.parent;
        while (p)
        {
            if (p.CompareTag(vehicleTag)) return true;
            p = p.parent;
        }
        return false;
    }

    private bool IsCollisionValid(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return false;
        string key = $"{grid.GetInstanceID()}_{x}_{z}";
        float now = Time.time;
        if (recentCollisions.TryGetValue(key, out float last))
        {
            float dt = now - last;
            if (dt < collisionCooldown)
            {
                if (debugMode)
                    Debug.Log($"Cooldown cuadrante [{x},{z}] en {grid.name}. Restante: {collisionCooldown - dt:F2}s");
                return false;
            }
        }
        recentCollisions[key] = now;
        return true;
    }

    private void ProcessVehicleImpact(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null) return;
        if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
        {
            if (debugMode) Debug.LogWarning($"Cuadrante [{x},{z}] fuera de límites en {grid.name}");
            return;
        }

        if (!IsCollisionValid(grid, x, z)) return;

        grid.OnVehicleImpact(x, z);
        if (debugMode) Debug.Log($"Impacto cuadrante [{x},{z}] en grid {grid.name}");

        ProbabilityAction prob = GetComponentInParent<ProbabilityAction>();
        if (prob != null)
            prob.TryExecuteOnQuadrant(x, z);
    }

    // -------- Obtención dinámica del grid + coordenadas ----------
    private bool TryResolveQuadrantFromCollider(Component hitComp, out BridgeConstructionGrid grid, out int qx, out int qz)
    {
        qx = qz = -1;
        grid = null;

        // 1) Componente informativo directo
        BridgeQuadrantInfo info = hitComp.GetComponent<BridgeQuadrantInfo>() ?? hitComp.GetComponentInParent<BridgeQuadrantInfo>();
        if (info != null && info.grid != null)
        {
            grid = info.grid;
            qx = info.x;
            qz = info.z;
            return true;
        }

        // 2) Fallback: buscar grid padre y calcular por posición
        grid = hitComp.GetComponentInParent<BridgeConstructionGrid>();
        if (grid == null)
        {
            // Fallback adicional: usar grid legacy si está asignado (compatibilidad)
            if (bridgeGrid != null)
            {
                grid = bridgeGrid;
                Vector3 fallbackWorldPoint = transform.position;
                Vector3 fallbackLocal = grid.transform.InverseTransformPoint(fallbackWorldPoint);
                qx = Mathf.FloorToInt(fallbackLocal.x / grid.quadrantSize);
                qz = Mathf.FloorToInt(fallbackLocal.z / grid.quadrantSize);
                return true;
            }
            return false;
        }

        // Usar el punto más cercano o posición del collider
        Vector3 worldPoint = (hitComp is Collider col)
            ? col.ClosestPoint(transform.position)
            : hitComp.transform.position;

        Vector3 local = grid.transform.InverseTransformPoint(worldPoint);
        qx = Mathf.FloorToInt(local.x / grid.quadrantSize);
        qz = Mathf.FloorToInt(local.z / grid.quadrantSize);
        return true;
    }

    // ---------- Colisiones físicas ----------
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    public void HandleCollision(Collision collision)
    {
        if (!IsVehicleOrChildOfVehicle()) return;

        // Verificar si el objeto golpeado es un cuadrante (por tag o por componente)
        if (!collision.gameObject.CompareTag(bridgeQuadrantTag) &&
            collision.gameObject.GetComponentInParent<BridgeQuadrantInfo>() == null)
            return;

        if (TryResolveQuadrantFromCollider(collision.collider, out var grid, out int x, out int z))
            ProcessVehicleImpact(grid, x, z);
        else if (debugMode)
            Debug.LogWarning("No se pudo resolver cuadrante desde colisión.");
    }

    // ---------- Triggers ----------
    private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other);
    }

    public void HandleTrigger(Collider other)
    {
        if (!IsVehicleOrChildOfVehicle()) return;

        if (!other.CompareTag(bridgeQuadrantTag) &&
            other.GetComponentInParent<BridgeQuadrantInfo>() == null)
            return;

        if (TryResolveQuadrantFromCollider(other, out var grid, out int x, out int z))
            ProcessVehicleImpact(grid, x, z);
        else if (debugMode)
            Debug.LogWarning("No se pudo resolver cuadrante desde trigger.");
    }

    // Métodos estáticos para hijos (sin cambios sustanciales)
    public static void HandleCollisionFromChild(GameObject child, Collision c)
    {
        var script = child.GetComponentInParent<VehicleBridgeCollision>();
        if (script) script.HandleCollision(c);
    }

    public static void HandleTriggerFromChild(GameObject child, Collider other)
    {
        var script = child.GetComponentInParent<VehicleBridgeCollision>();
        if (script) script.HandleTrigger(other);
    }
}