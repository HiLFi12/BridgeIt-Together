using System.Collections;
using UnityEngine;

/// <summary>
/// Vehículo que dispara un efecto de "agua explosiva" cuando la tirada de probabilidad tiene éxito.
/// Hereda de BaseProbabilitySkill y usa su detección/probabilidad/ammo.
/// Además del prefab que instancie la clase base, agrega su PROPIO efecto adicional.
/// NOTA: deja el campo 'prefab' del BaseProbabilitySkill ASIGNADO si quieres ver ambos (prefab + efecto).
/// </summary>
public class ExplosiveWaterCar : BaseProbabilitySkill
{
    [Header("Explosive Water - Efecto")] 
    [Tooltip("Prefab del efecto a spawnear al éxito de la probabilidad (partículas/VFX).")]
    [SerializeField] private GameObject effectPrefab;

    [Tooltip("Punto de spawn (Empty). Si no se asigna, usa este transform.")]
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Opciones de Instancia")] 
    [Tooltip("Parentar el efecto al spawn point.")]
    [SerializeField] private bool parentToSpawnPoint = true;
    [Tooltip("Offset local aplicado al spawn point.")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [Tooltip("Rotación adicional en euler aplicada al efecto instanciado.")]
    [SerializeField] private Vector3 additionalEulerRotation = Vector3.zero;
    [Tooltip("Auto-destruir el efecto tras X segundos (0 = no destruir automáticamente).")]
    [SerializeField] private float autoDestroyAfter = 0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private Color gizmoColor = new Color(0.3f, 0.8f, 1f, 0.5f);
    [SerializeField] private float gizmoSphereRadius = 0.2f;

    private GameObject currentEffectInstance;

    private void Awake()
    {
        if (!effectSpawnPoint) effectSpawnPoint = transform;
    }

    private void OnDisable()
    {
        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
            currentEffectInstance = null;
        }
    }

    // Hook llamado por la clase base cuando la tirada fue exitosa
    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        // Conservamos el prefab instanciado por la base (spawnedInstance) y además agregamos nuestro efecto.
        SpawnEffect();
    }

    private void SpawnEffect()
    {
        if (effectPrefab == null)
        {
            if (debugLogs) Debug.LogWarning("[ExplosiveWaterCar] No hay 'effectPrefab' asignado.", this);
            return;
        }
        if (!effectSpawnPoint) effectSpawnPoint = transform;

        Vector3 worldPos = effectSpawnPoint.TransformPoint(localOffset);
        Quaternion worldRot = effectSpawnPoint.rotation * Quaternion.Euler(additionalEulerRotation);

        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
            currentEffectInstance = null;
        }

        currentEffectInstance = Instantiate(effectPrefab, worldPos, worldRot);
        if (parentToSpawnPoint && currentEffectInstance)
        {
            currentEffectInstance.transform.SetParent(effectSpawnPoint, true);
        }
        if (autoDestroyAfter > 0f && currentEffectInstance)
        {
            Destroy(currentEffectInstance, autoDestroyAfter);
        }

        if (debugLogs && currentEffectInstance)
        {
            Debug.Log($"[ExplosiveWaterCar] Efecto '{currentEffectInstance.name}' instanciado en {worldPos}.", this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!effectSpawnPoint) effectSpawnPoint = transform;
        Gizmos.color = gizmoColor;
        Vector3 worldPos = effectSpawnPoint.TransformPoint(localOffset);
        Gizmos.DrawSphere(worldPos, gizmoSphereRadius);
    }
#endif
}
