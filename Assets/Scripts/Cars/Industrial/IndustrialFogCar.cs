using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Skill de niebla industrial: al cumplirse la tirada de probabilidad (BaseProbabilitySkill)
/// instancia un efecto de humo durante un tiempo limitado (modificable) y luego lo destruye.
/// Puede volver a intentarlo mientras haya "ammo" (del sistema base).
/// </summary>
public class IndustrialFogCar : BaseProbabilitySkill
{
    [Header("Industrial Fog - Efecto Único")] 
    [Tooltip("Prefab de efecto (partículas / VFX) que se activará cuando la probabilidad sea exitosa.")] 
    [SerializeField] private GameObject effectPrefab;

    [Tooltip("Punto desde el que se instancian los efectos (Empty). Si no se asigna, usa el transform del vehículo.")] 
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Duración del Efecto")]
    [Tooltip("Duración en segundos que el efecto permanece activo tras una tirada exitosa.")]
    [Min(0.1f)] [SerializeField] private float effectDuration = 5f;

    [Header("Opciones de Instancia")] 
    [Tooltip("Parentar el efecto al vehículo para que lo siga.")] [SerializeField] private bool parentEffectToCar = true;
    [Tooltip("Offset local aplicado al spawn point.")] [SerializeField] private Vector3 localOffset = Vector3.zero;
    [Tooltip("Rotación adicional (en euler) aplicada al efecto instanciado.")] [SerializeField] private Vector3 additionalEulerRotation = Vector3.zero;

    [Header("Debug")] 
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private Color gizmoColor = new Color(0.8f, 0.9f, 1f, 0.6f);
    [SerializeField] private float gizmoSphereRadius = 0.25f;

    private GameObject currentEffectInstance;
    private Coroutine activeEffectRoutine;
    private bool effectActive;

    [Header("Compatibilidad Base Spawn")]
    [Tooltip("Si está activo, se suprime programáticamente el prefab del BaseProbabilitySkill para que no spawnee nada.")]
    [SerializeField] private bool suppressBasePrefabSpawn = true;

    private void Reset()
    {
        effectSpawnPoint = transform;
    }

    private void Awake()
    {
        if (!effectSpawnPoint) effectSpawnPoint = transform;
        // Suprimir el prefab del BaseProbabilitySkill si así se configura
        if (suppressBasePrefabSpawn)
        {
            try
            {
                var field = typeof(BaseProbabilitySkill).GetField("prefab", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(this, null);
                }
            }
            catch { /* Ignorar si reflexión falla */ }
        }
    }

    private void OnDisable()
    {
        StopAndDestroyEffect();
    }

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        // Reemplazamos el spawn normal de la base: si la base instanció algo, lo destruimos.
        if (spawnedInstance != null)
        {
            Destroy(spawnedInstance);
        }
        if (effectActive) return; // Evitar duplicar mientras activo
        StartEffect();
    }

    private void StartEffect()
    {
        if (effectActive) return;
        var prefab = effectPrefab;
        if (!prefab)
        {
            if (debugLogs)
                Debug.LogWarning("[IndustrialFogCar] No hay prefab de efecto para instanciar.", this);
            return;
        }
        if (!effectSpawnPoint) effectSpawnPoint = transform;
        Vector3 worldPos = effectSpawnPoint.TransformPoint(localOffset);
        Quaternion rot = effectSpawnPoint.rotation * Quaternion.Euler(additionalEulerRotation);
        currentEffectInstance = Instantiate(prefab, worldPos, rot);
        if (parentEffectToCar && currentEffectInstance != null)
            currentEffectInstance.transform.SetParent(effectSpawnPoint, true);
        effectActive = true;
        if (activeEffectRoutine != null) StopCoroutine(activeEffectRoutine);
        activeEffectRoutine = StartCoroutine(EffectLifetimeRoutine());
        if (debugLogs && currentEffectInstance)
            Debug.Log($"[IndustrialFogCar] Efecto '{currentEffectInstance.name}' iniciado por {effectDuration} s.", this);
    }

    private IEnumerator EffectLifetimeRoutine()
    {
        float t = 0f;
        while (t < effectDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        StopAndDestroyEffect();
    }

    private void StopAndDestroyEffect()
    {
        if (activeEffectRoutine != null)
        {
            StopCoroutine(activeEffectRoutine);
            activeEffectRoutine = null;
        }
        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
            if (debugLogs)
                Debug.Log("[IndustrialFogCar] Efecto destruido tras duración configurada.", this);
        }
        currentEffectInstance = null;
        effectActive = false;
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
