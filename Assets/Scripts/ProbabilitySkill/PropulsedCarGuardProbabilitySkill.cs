using System.Collections;
using UnityEngine;

/// <summary>
/// Nueva versión simplificada:
/// - Elimina la lógica de "drop zones" / comprobaciones externas de caída.
/// - Usa un objeto de referencia (en escena) al que se le desactivan sus mallas al activarse la probabilidad.
/// - Instancia un prefab en la misma posición/rotación de ese objeto y lo lanza en parábola hacia atrás ( -transform.forward ).
/// </summary>
public class PropulsedCarGuardProbabilitySkill : BaseProbabilitySkill
{
    [Header("Objeto de Referencia (en escena)")]
    [Tooltip("Objeto ya colocado en la escena. Se desactivan sus MeshRenderers/SkinnedMeshRenderers al disparar.")]
    [SerializeField] private GameObject referenceObject;

    [Header("Prefab a Instanciar (será lanzado)")]
    [Tooltip("Prefab que se instanciará sustituyendo visualmente al objeto de referencia.")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Lanzamiento hacia atrás (Parámetros parabólicos)")]
    [SerializeField, Range(10f, 80f)] private float launchAngleDeg = 45f;
    [SerializeField, Min(0.1f)] private float launchSpeed = 12f;
    [SerializeField, Min(0.1f)] private float launchGravity = 9.81f;
    [SerializeField] private bool kinematicDuringLaunch = true;
    [SerializeField] private float minLaunchDistance = 0.05f;
    [SerializeField, Min(0.1f)] private float backwardDistance = 4f;

    [Header("Debug")] 
    [SerializeField] private bool debugLogs = false;
    [SerializeField, Tooltip("Si está activo, al volver a habilitar el objeto (pooling) se restaura el estado para permitir un nuevo disparo.")]
    private bool autoResetOnEnable = true;

    [Header("Ammo Reset (Pooling)")]
    [Tooltip("Si está activo, al reactivarse el objeto se restaura el ammo inicial (sin modificar la clase base).")]
    [SerializeField] private bool resetAmmoOnEnable = true;
    [Tooltip("Forzar un ammo inicial específico (>=0). Si es -1 se toma el ammo actual la primera vez como referencia.")]
    [SerializeField] private int initialAmmoOverride = -1;

    private int capturedInitialAmmo;
    private bool initialAmmoCaptured;

    private bool executed;

    private void OnEnable()
    {
        if (autoResetOnEnable)
        {
            executed = false;
            if (referenceObject) RestoreRenderers(referenceObject);
            if (resetAmmoOnEnable) RestoreAmmoIfNeeded();
            if (debugLogs) Debug.Log("[PropulsedCarGuardProbabilitySkill] Reset en OnEnable (pool reuse).", this);
        }
    }

    /// <summary>
    /// Llamar manualmente desde un sistema de pooling antes de reutilizar si se desactiva autoResetOnEnable.
    /// </summary>
    public void PrepareForReuse(GameObject newReference = null)
    {
        executed = false;
        if (newReference)
            referenceObject = newReference;
        if (referenceObject) RestoreRenderers(referenceObject);
        if (resetAmmoOnEnable) RestoreAmmoIfNeeded();
        if (debugLogs) Debug.Log("[PropulsedCarGuardProbabilitySkill] PrepareForReuse llamado.", this);
    }

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        if (executed) return;
        executed = true;

        if (!referenceObject)
        {
            if (debugLogs) Debug.LogWarning("[PropulsedCarGuardProbabilitySkill] referenceObject no asignado.", this);
            return;
        }

        if (!projectilePrefab)
        {
            if (debugLogs) Debug.LogWarning("[PropulsedCarGuardProbabilitySkill] projectilePrefab no asignado.", this);
            return;
        }

        // 1) Desactivar renderizadores del objeto de referencia para 'ocultarlo'.
        DisableRenderers(referenceObject);

        // 2) Instanciar el nuevo prefab en la misma posición y rotación (sin ser hijo).
        Transform refT = referenceObject.transform;
        GameObject instance = Instantiate(projectilePrefab, refT.position, refT.rotation);
        if (debugLogs) Debug.Log("[PropulsedCarGuardProbabilitySkill] Instanciado: " + instance.name, instance);

        // 3) Preparar destino: hacia atrás del transform de este script (carro) a una distancia fija.
        Vector3 destino = refT.position - transform.forward * backwardDistance;
        destino.y = refT.position.y; // Mantener altura inicial (puedes ajustar si quieres caída vertical progresiva)

        StartCoroutine(LaunchRoutine(instance.transform, destino));
    }

    private void DisableRenderers(GameObject go)
    {
        if (!go) return;
        var meshR = go.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < meshR.Length; i++) meshR[i].enabled = false;
        var skinned = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinned.Length; i++) skinned[i].enabled = false;
    }

    private void RestoreRenderers(GameObject go)
    {
        if (!go) return;
        // Habilita cualquier tipo de Renderer (MeshRenderer, SkinnedMeshRenderer, etc.)
        var renders = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renders.Length; i++)
            renders[i].enabled = true;
    }

    private void RestoreAmmoIfNeeded()
    {
        // Capturar ammo inicial la primera vez (o usar override si se definió)
        if (!initialAmmoCaptured)
        {
            capturedInitialAmmo = initialAmmoOverride >= 0 ? initialAmmoOverride : GetAmmo();
            initialAmmoCaptured = true;
            if (debugLogs) Debug.Log($"[PropulsedCarGuardProbabilitySkill] Ammo inicial capturado = {capturedInitialAmmo}.", this);
        }
        int current = GetAmmo();
        int diff = capturedInitialAmmo - current;
        if (diff > 0)
        {
            AddAmmo(diff);
            if (debugLogs) Debug.Log($"[PropulsedCarGuardProbabilitySkill] Ammo restaurado (+{diff}) => {GetAmmo()}.", this);
        }
        else if (diff < 0)
        {
            // Caso raro: si por algún motivo el ammo actual supera al inicial, actualizamos el inicial para no forzar reducciones (la base no permite restar ammo)
            capturedInitialAmmo = current;
            if (debugLogs) Debug.Log($"[PropulsedCarGuardProbabilitySkill] Ammo actual ({current}) supera al inicial; nuevo inicial = {capturedInitialAmmo}.", this);
        }
    }

    private IEnumerator LaunchRoutine(Transform target, Vector3 destino)
    {
        if (!target) yield break;

        Vector3 start = target.position;
        Vector3 flat = new Vector3(destino.x - start.x, 0f, destino.z - start.z);
        float distance = flat.magnitude;

        if (distance < minLaunchDistance)
        {
            target.position = destino;
            yield break;
        }

        Vector3 dir = flat / Mathf.Max(distance, 0.0001f);
        float angleRad = launchAngleDeg * Mathf.Deg2Rad;
        float v = Mathf.Max(0.01f, launchSpeed);
        float cos = Mathf.Cos(angleRad);
        float tan = Mathf.Tan(angleRad);
        float g = Mathf.Max(0.01f, launchGravity);
        float totalTime = distance / (v * cos);

        Rigidbody trb = target.GetComponent<Rigidbody>();
        bool hadRB = trb;
        bool prevKin = false;
        if (hadRB && kinematicDuringLaunch)
        {
            prevKin = trb.isKinematic;
            trb.isKinematic = true;
        }

        float t = 0f;
        while (t < totalTime && target)
        {
            t += Time.deltaTime;
            float ct = Mathf.Min(t, totalTime);
            float x = v * cos * ct;
            float yOffset = x * tan - (g * x * x) / (2f * v * v * cos * cos);

            Vector3 pos = start + dir * x;
            pos.y = start.y + yOffset;
            target.position = pos;
            yield return null;
        }

        if (target)
            target.position = destino;

        if (hadRB && kinematicDuringLaunch && trb)
            trb.isKinematic = prevKin;

        if (debugLogs && target) Debug.Log("[PropulsedCarGuardProbabilitySkill] Lanzamiento finalizado en: " + destino, target.gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!referenceObject) return;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.9f);
        Gizmos.DrawWireSphere(referenceObject.transform.position, 0.15f);
        Vector3 destino = referenceObject.transform.position - transform.forward * backwardDistance;
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(destino, 0.15f);
        Gizmos.DrawLine(referenceObject.transform.position + Vector3.up * 0.05f, destino + Vector3.up * 0.05f);
    }
#endif
}