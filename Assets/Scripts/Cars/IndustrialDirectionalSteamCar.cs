using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Variante direccional de la skill de vapor industrial.
/// Hereda de IndustrialFogCar pero ignora su punto único de spawn.
/// - Define dos empties: forwardSpawn y backwardSpawn.
/// - En OnProbabilitySuccess detecta hornos (Furnace / HeatSphere) cercanos.
/// - Si los hornos más cercanos están mayormente detrás del coche => instancia atrás; si no => adelante.
/// - Instancia un prefab grande de vapor (steamPrefab).
/// - El prefab debe incluir un collider (trigger) con el script SteamHeatDisabler para afectar hornos.
/// </summary>
public class IndustrialDirectionalSteamCar : IndustrialFogCar
{
    [Header("Directional Steam Settings")] 
    [Tooltip("Spawn point delantero.")] [SerializeField] private Transform forwardSpawn;
    [Tooltip("Spawn point trasero.")] [SerializeField] private Transform backwardSpawn;
    [Tooltip("Prefab del gran vapor direccional.")] [SerializeField] private GameObject steamPrefab;
    [Tooltip("Radio de búsqueda para localizar hornos / fuentes de calor.")] [SerializeField] private float furnaceSearchRadius = 25f;
    [Tooltip("Capas consideradas para la búsqueda de hornos.")] [SerializeField] private LayerMask furnaceLayerMask;
    [Tooltip("Cuántos colliders máximos evaluar por barrido.")] [SerializeField] private int maxFurnaceColliders = 32;
    [Tooltip("Mantener el vapor parentado al spawn elegido.")] [SerializeField] private bool parentSteam = true;
    [Tooltip("Destruir automáticamente el vapor tras esta duración (0 = infinito hasta que el prefab se auto-destruya)." )]
    [SerializeField] private float autoDestroyAfter = 8f;

    [Header("Debug")] 
    [FormerlySerializedAs("debugLogs")]
    [SerializeField] private bool debugLogsDirectional = false;
    [SerializeField] private Color searchGizmoColor = new Color(1f,0.8f,0.4f,0.15f);

    private readonly Collider[] _overlapResults = new Collider[64];
    private GameObject activeSteamInstance;

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        // Ignoramos la lógica de IndustrialFogCar y aplicamos la propia direccional.
        SpawnDirectionalSteam();
    }

    private void SpawnDirectionalSteam()
    {
        if (steamPrefab == null)
        {
            if (debugLogsDirectional) Debug.LogWarning("[IndustrialDirectionalSteamCar] No hay steamPrefab asignado.", this);
            return;
        }
        if (!forwardSpawn || !backwardSpawn)
        {
            if (debugLogsDirectional) Debug.LogWarning("[IndustrialDirectionalSteamCar] Falta asignar forwardSpawn o backwardSpawn.", this);
            return;
        }

        // Buscar hornos cercanos.
        Vector3 center = transform.position;
        int count = Physics.OverlapSphereNonAlloc(center, furnaceSearchRadius, _overlapResults, furnaceLayerMask, QueryTriggerInteraction.Collide);
        if (debugLogsDirectional) Debug.Log($"[IndustrialDirectionalSteamCar] Hornos detectados: {count}", this);

        int forwardVotes = 0;
        int backwardVotes = 0;

        Vector3 fwd = transform.forward;
        for (int i = 0; i < count && i < maxFurnaceColliders; i++)
        {
            var c = _overlapResults[i];
            if (c == null) continue;
            // Considerar objeto que tenga Furnace o HeatSphere
            bool isFurnace = c.GetComponentInParent<Furnace>() != null || c.GetComponentInParent<HeatSphere>() != null;
            if (!isFurnace) continue;
            Vector3 dir = (c.transform.position - center).normalized;
            float dot = Vector3.Dot(fwd, dir); // >0 delante, <0 detrás
            if (dot >= 0f) forwardVotes++; else backwardVotes++;
        }

        // Decidir spawn
        Transform chosen = forwardSpawn;
        if (backwardVotes > forwardVotes) chosen = backwardSpawn;

        if (activeSteamInstance != null)
        {
            Destroy(activeSteamInstance);
            activeSteamInstance = null;
        }

        activeSteamInstance = Instantiate(steamPrefab, chosen.position, chosen.rotation);
        if (parentSteam && activeSteamInstance) activeSteamInstance.transform.SetParent(chosen, true);

        if (debugLogsDirectional)
        {
            Debug.Log($"[IndustrialDirectionalSteamCar] Vapor instanciado en {(chosen == forwardSpawn ? "DELANTE" : "ATRÁS")} (FwdVotes={forwardVotes} / BackVotes={backwardVotes}).", this);
        }

        if (autoDestroyAfter > 0f && activeSteamInstance != null)
        {
            Destroy(activeSteamInstance, autoDestroyAfter);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = searchGizmoColor;
        Gizmos.DrawSphere(transform.position, furnaceSearchRadius);
        if (forwardSpawn)
        {
            Gizmos.color = Color.green; Gizmos.DrawWireCube(forwardSpawn.position, Vector3.one * 0.5f);
        }
        if (backwardSpawn)
        {
            Gizmos.color = Color.red; Gizmos.DrawWireCube(backwardSpawn.position, Vector3.one * 0.5f);
        }
    }
#endif
}
