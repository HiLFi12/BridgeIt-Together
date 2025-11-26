using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HeatSpherent
/// Coloca este script en un GameObject vacío con un SphereCollider (isTrigger = true).
/// Comportamiento:
///  - Al entrar en contacto (OnTriggerEnter) con cualquier HeatSphere:
///      * Llama CooldownOff() a TODOS los HeatSphere actualmente solapados con el volumen del SphereCollider.
///      * Se destruye (opcionalmente con un pequeño retardo para permitir efectos visuales).
///  - Si no hubiera HeatSphere en ese frame (raro), no hace nada hasta que uno entre.
/// Afecta a "todos" en un solo ciclo usando un barrido Physics.OverlapSphere para no depender de múltiples OnTriggerEnter secuenciales.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class HeatSpherent : MonoBehaviour
{
    [Header("Cooldown Auto-destrucción")]
    [Tooltip("Tiempo en segundos antes de que el HeatSpherent se autodestruya.")]
    [SerializeField] private float destructionCooldown = 5f;

    [Header("Debug")] 
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.7f, 1f, 0.25f);

    private bool _consumed = false;
    private SphereCollider _sphere;
    private float _currentCooldown;
    private static readonly Collider[] _overlap = new Collider[128];

    private void Awake()
    {
        _sphere = GetComponent<SphereCollider>();
        if (_sphere != null) _sphere.isTrigger = true;
        _currentCooldown = destructionCooldown;
    }

    private void Update()
    {
        // Detectar HeatSphere activamente
        if (!_consumed)
        {
            DetectAndActivate();
        }

        // Sistema de cooldown auto-destrucción
        if (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.deltaTime;

            if (_currentCooldown <= 0f)
            {
                if (debugLogs)
                {
                    Debug.Log($"[HeatSpherent] Cooldown alcanzado, autodestruyendo.", this);
                }
                Destroy(gameObject);
            }
        }
    }

    private void DetectAndActivate()
    {
        if (_sphere == null) return;

        float effectiveRadius = _sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        int count = Physics.OverlapSphereNonAlloc(transform.position + _sphere.center, effectiveRadius, _overlap, Physics.AllLayers, QueryTriggerInteraction.Collide);

        // Buscar si hay alguna HeatSphere en el área
        for (int i = 0; i < count; i++)
        {
            var col = _overlap[i];
            if (col == null) continue;
            
            var hs = col.GetComponentInParent<HeatSphere>();
            if (hs != null)
            {
                // Encontramos al menos una HeatSphere, activamos
                ActivateAndConsume();
                return;
            }
        }
    }

    private void ActivateAndConsume()
    {
        _consumed = true;

        int affected = AffectAllOverlappingHeatSpheres();
        if (debugLogs)
        {
            Debug.Log($"[HeatSpherent] HeatSphere afectados: {affected}", this);
        }
    }

    /// <summary>
    /// Llama CooldownOff() a todos los HeatSphere que estén actualmente solapados con el volumen del SphereCollider.
    /// </summary>
    private int AffectAllOverlappingHeatSpheres()
    {
        if (_sphere == null)
        {
            if (debugLogs) Debug.LogWarning("[HeatSpherent] Sin SphereCollider.", this);
            return 0;
        }

        float effectiveRadius = _sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        int count = Physics.OverlapSphereNonAlloc(transform.position + _sphere.center, effectiveRadius, _overlap, Physics.AllLayers, QueryTriggerInteraction.Collide);

        int affected = 0;
        HashSet<HeatSphere> processed = new HashSet<HeatSphere>();

        for (int i = 0; i < count; i++)
        {
            var col = _overlap[i];
            if (col == null) continue;
            var hs = col.GetComponentInParent<HeatSphere>();
            if (hs == null) continue;
            if (processed.Contains(hs)) continue; // evitar duplicados si varios colliders del mismo heat sphere

            hs.CooldownOff();
            processed.Add(hs);
            affected++;
        }

        return affected;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_sphere == null) _sphere = GetComponent<SphereCollider>();
        if (_sphere == null) return;
        Gizmos.color = gizmoColor;
        float effectiveRadius = _sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        Gizmos.DrawWireSphere(transform.position + _sphere.center, effectiveRadius);
    }
#endif
}
