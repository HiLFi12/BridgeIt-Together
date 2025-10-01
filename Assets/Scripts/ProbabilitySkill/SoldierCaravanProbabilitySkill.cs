using UnityEngine;

/// <summary>
/// Versión adaptada: al cumplirse la probabilidad oculta el objeto de referencia (desactiva sus renderers)
/// y crea un prefab sustituto en la misma posición/rotación. No hay lógica de lanzamiento.
/// Opcionalmente soporta pooling restaurando estado en OnEnable.
/// </summary>
public class SoldierCaravanProbabilitySkill : BaseProbabilitySkill
{
    [Header("Objetos de Referencia (en escena)")]
    [Tooltip("Objetos ya colocados en la escena. Se desactivan sus Renderers al disparar.")]
    [SerializeField] private GameObject[] referenceObjects = new GameObject[4];

    [Header("Prefabs de Reemplazo (visual)")]
    [Tooltip("Prefabs a instanciar en la misma posición/rotación. Si la cantidad no coincide, se reutiliza el último válido.")]
    [SerializeField] private GameObject[] replacementPrefabs = new GameObject[4];

    [Header("Comportamiento")]
    [Tooltip("Si true: instancia TODOS los reemplazos para cada referencia válida. Si false: sólo el primero que encuentre válido.")]
    [SerializeField] private bool instantiateAll = true;

    [Tooltip("Si true: ignora referencias nulas en vez de cancelar todo.")]
    [SerializeField] private bool skipNullReferences = true;

    [Header("Destrucción de Caravan")]
    [Tooltip("Objeto 'caravan' a destruir tras instanciar los reemplazos.")]
    [SerializeField] private GameObject caravanObject;
    [Tooltip("Si true, se destruirá el objeto caravan al cumplirse la probabilidad.")]
    [SerializeField] private bool destroyCaravan = true;
    [Tooltip("Delay (segundos) antes de destruir la caravan. 0 = inmediato.")]
    [SerializeField, Min(0f)] private float destroyCaravanDelay = 0f;
    [Tooltip("Si no se asignó caravanObject y esto está activo, se destruye este GameObject (donde vive el skill)." )]
    [SerializeField] private bool destroySelfIfNoCaravan = false;

    [Header("Pooling / Reset")] 
    [SerializeField, Tooltip("Si está activo, al reactivar el objeto se restauran renderers y ammo.")] private bool autoResetOnEnable = true;

    [Header("Debug")] 
    [SerializeField] private bool debugLogs = false;

    private bool executed;

    private void OnEnable()
    {
        if (autoResetOnEnable)
        {
            executed = false;
            RestoreAllReferenceRenderers();
            if (debugLogs) Debug.Log("[SoldierCaravanProbabilitySkill] Reset en OnEnable.", this);
        }
    }

    /// <summary>
    /// Reset manual si se usa un pool custom con autoResetOnEnable desactivado.
    /// </summary>
    public void PrepareForReuse(GameObject[] newReferences = null)
    {
        executed = false;
        if (newReferences != null && newReferences.Length > 0)
            referenceObjects = newReferences;
        RestoreAllReferenceRenderers();
        if (debugLogs) Debug.Log("[SoldierCaravanProbabilitySkill] PrepareForReuse ejecutado.", this);
    }

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        if (executed) return;
        executed = true;

        if (referenceObjects == null || referenceObjects.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[SoldierCaravanProbabilitySkill] No hay referenceObjects asignados.", this);
            return;
        }
        if (replacementPrefabs == null || replacementPrefabs.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[SoldierCaravanProbabilitySkill] No hay replacementPrefabs asignados.", this);
            return;
        }

        int replacementsLen = replacementPrefabs.Length;
        int spawnedCount = 0;

        for (int i = 0; i < referenceObjects.Length; i++)
        {
            var refObj = referenceObjects[i];
            if (!refObj)
            {
                if (skipNullReferences)
                {
                    if (debugLogs) Debug.LogWarning($"[SoldierCaravanProbabilitySkill] referenceObjects[{i}] es null. Se omite.", this);
                    continue;
                }
                else
                {
                    if (debugLogs) Debug.LogWarning($"[SoldierCaravanProbabilitySkill] referenceObjects[{i}] es null. Cancelando.", this);
                    return;
                }
            }

            // Desactivar renderers de la referencia
            DisableRenderers(refObj);

            // Determinar qué prefab usar (si hay menos prefabs que referencias, reutiliza el último válido)
            GameObject prefab = replacementPrefabs[Mathf.Clamp(i, 0, replacementsLen - 1)];
            if (!prefab)
            {
                if (debugLogs) Debug.LogWarning($"[SoldierCaravanProbabilitySkill] replacementPrefabs[{Mathf.Clamp(i,0,replacementsLen-1)}] es null. Se omite.", this);
                continue;
            }

            Transform rt = refObj.transform;
            GameObject inst = Instantiate(prefab, rt.position, rt.rotation);
            spawnedCount++;
            if (debugLogs) Debug.Log($"[SoldierCaravanProbabilitySkill] Instanciado reemplazo {inst.name} para referencia {refObj.name} (index {i}).", inst);

            if (!instantiateAll)
                break; // sólo el primero
        }

        if (debugLogs)
            Debug.Log($"[SoldierCaravanProbabilitySkill] Proceso completado. Reemplazos instanciados: {spawnedCount}.", this);

        // Destruir caravan después de spawnear
        if (destroyCaravan)
        {
            if (caravanObject)
            {
                if (debugLogs) Debug.Log($"[SoldierCaravanProbabilitySkill] Destruyendo caravan '{caravanObject.name}' (delay={destroyCaravanDelay}).", caravanObject);
                if (destroyCaravanDelay <= 0f) Destroy(caravanObject); else Destroy(caravanObject, destroyCaravanDelay);
            }
            else if (destroySelfIfNoCaravan)
            {
                if (debugLogs) Debug.Log("[SoldierCaravanProbabilitySkill] caravanObject no asignado. Destruyendo self.", this);
                if (destroyCaravanDelay <= 0f) Destroy(gameObject); else Destroy(gameObject, destroyCaravanDelay);
            }
        }
    }

    private void DisableRenderers(GameObject go)
    {
        if (!go) return;
        var rends = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++) rends[i].enabled = false;
    }

    private void RestoreRenderers(GameObject go)
    {
        if (!go) return;
        var rends = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++) rends[i].enabled = true;
    }

    private void RestoreAllReferenceRenderers()
    {
        if (referenceObjects == null) return;
        for (int i = 0; i < referenceObjects.Length; i++)
            RestoreRenderers(referenceObjects[i]);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (referenceObjects == null) return;
        Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.9f);
        for (int i = 0; i < referenceObjects.Length; i++)
        {
            var r = referenceObjects[i];
            if (!r) continue;
            Gizmos.DrawWireSphere(r.transform.position, 0.15f);
        }
    }
#endif
}