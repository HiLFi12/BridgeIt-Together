using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agua temporal que salpica: al instanciarse crece (splash) y luego vuelve a reducirse.
/// Mientras su collider (trigger) toca cuadrantes de puente, fuerza isTurned=false (vía AddWaterBlocker/RemoveWaterBlocker en BridgeQuadrantSO).
/// </summary>
[RequireComponent(typeof(Collider))]
public class Water : MonoBehaviour
{
    [Header("Escala Splash")] 
    [Tooltip("Escala inicial relativa al spawn (aplicada inmediatamente al instanciar)." )]
    public Vector3 startScale = new Vector3(0.15f,0.1f,0.15f);
    [Tooltip("Escala máxima a la que crece el splash.")]
    public Vector3 peakScale = new Vector3(1.05f, 0.25f, 1.05f); // Aproximado a un cuadrante
    [Tooltip("Escala final luego de volver a encogerse.")]
    public Vector3 endScale = new Vector3(0.8f, 0.15f, 0.8f);
    [Tooltip("Tiempo de crecimiento hasta peak.")]
    public float growTime = 0.25f;
    [Tooltip("Tiempo de retorno desde peak hasta end.")]
    public float shrinkTime = 0.9f;
    [Tooltip("Destruir este objeto tras completar la animación (si 0 => no destruir)." )]
    public float destroyAfter = 2.2f;

    [Header("Bridge Quadrant Detección")] 
    [Tooltip("Radio extra para debug (no usado en lógica)." )]
    public float debugRadius = 0f;
    [Tooltip("Mostrar logs de depuración.")]
    public bool debugLogs = false;

    private Coroutine scaleRoutine;
    // Seguimiento por instancia de cuadrantes tocados (para limpieza robusta)
    private HashSet<BridgeQuadrantInstance> _currentContacts = new HashSet<BridgeQuadrantInstance>();

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col && !col.isTrigger) col.isTrigger = true;
        transform.localScale = startScale;
    }

    /// <summary>
    /// Permite configurar dinámicamente el splash tras instanciarlo (ej. desde un spawner probabilístico).
    /// Cualquier parámetro puede dejarse en null para no modificarlo.
    /// </summary>
    public void ConfigureSplash(Vector3? start = null, Vector3? peak = null, Vector3? end = null, float? grow = null, float? shrink = null, float? life = null)
    {
        if (start.HasValue) startScale = start.Value;
        if (peak.HasValue) peakScale = peak.Value;
        if (end.HasValue) endScale = end.Value;
        if (grow.HasValue) growTime = grow.Value;
        if (shrink.HasValue) shrinkTime = shrink.Value;
        if (life.HasValue) destroyAfter = life.Value;
        // Reiniciar estado inicial si se llama antes de OnEnable
        transform.localScale = startScale;
    }

    private void OnEnable()
    {
        scaleRoutine = StartCoroutine(ScaleSequence());
        if (destroyAfter > 0f) Destroy(gameObject, destroyAfter);
    }

    private IEnumerator ScaleSequence()
    {
        // Crecer
        float t = 0f;
        while (t < growTime)
        {
            t += Time.deltaTime;
            float lerp = growTime > 0 ? Mathf.Clamp01(t / growTime) : 1f;
            transform.localScale = Vector3.Lerp(startScale, peakScale, lerp);
            yield return null;
        }
        // Encoger
        t = 0f;
        while (t < shrinkTime)
        {
            t += Time.deltaTime;
            float lerp = shrinkTime > 0 ? Mathf.Clamp01(t / shrinkTime) : 1f;
            transform.localScale = Vector3.Lerp(peakScale, endScale, lerp);
            yield return null;
        }
        transform.localScale = endScale;
        scaleRoutine = null;
    }

    private static Dictionary<BridgeQuadrantInstance, int> _activeWaterContacts = new Dictionary<BridgeQuadrantInstance, int>();
    private static readonly Collider[] _overlap = new Collider[256];

    private void OnTriggerEnter(Collider other)
    {
        if (!other || !other.gameObject) return;
        if (other.CompareTag("BridgeQuadrant"))
        {
            var inst = other.GetComponent<BridgeQuadrantInstance>() ?? other.GetComponentInParent<BridgeQuadrantInstance>();
            if (inst != null)
            {
                // Evitar incrementar múltiples veces por sub-colliders: solo primera vez que este water toca el cuadrante
                if (_currentContacts.Add(inst))
                {
                    if (!_activeWaterContacts.ContainsKey(inst)) _activeWaterContacts[inst] = 0;
                    _activeWaterContacts[inst]++;
                    if (debugLogs) Debug.Log($"[Water] Contacto NUEVO con {inst.name} (countGlobal={_activeWaterContacts[inst]})", this);
                    inst.TurnOff();
                }
                else if (debugLogs)
                {
                    Debug.Log($"[Water] Contacto duplicado ignorado con {inst.name}", this);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other || !other.gameObject) return;
        if (other.CompareTag("BridgeQuadrant"))
        {
            var inst = other.GetComponent<BridgeQuadrantInstance>() ?? other.GetComponentInParent<BridgeQuadrantInstance>();
            if (inst != null && _activeWaterContacts.ContainsKey(inst))
            {
                // Solo procesar salida si realmente lo teníamos registrado
                if (_currentContacts.Remove(inst))
                {
                    _activeWaterContacts[inst]--;
                    if (_activeWaterContacts[inst] <= 0)
                    {
                        _activeWaterContacts.Remove(inst);
                        inst.ReevaluateHeatAfterWater();
                        if (debugLogs) Debug.Log($"[Water] Última salida de {inst.name}. Re-evaluando calor.", this);
                    }
                    else if (debugLogs)
                    {
                        Debug.Log($"[Water] Salida parcial de {inst.name} (restanteGlobal={_activeWaterContacts[inst]})", this);
                    }
                }
                else if (debugLogs)
                {
                    Debug.Log($"[Water] Salida ignorada (no estaba en contactos locales) de {inst.name}", this);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Limpiar cualquier cuadrante que siga contado si el agua desaparece sin disparar exit (destroy inmediato / pooling)
        if (_currentContacts.Count == 0) return;
        foreach (var inst in _currentContacts)
        {
            if (inst == null) continue;
            if (_activeWaterContacts.ContainsKey(inst))
            {
                _activeWaterContacts[inst]--;
                if (_activeWaterContacts[inst] <= 0)
                {
                    _activeWaterContacts.Remove(inst);
                    inst.ReevaluateHeatAfterWater();
                    if (debugLogs) Debug.Log($"[Water] OnDestroy liberó y re-evaluó calor de {inst.name}", this);
                }
                else if (debugLogs)
                {
                    Debug.Log($"[Water] OnDestroy decrementó {inst.name} (restanteGlobal={_activeWaterContacts[inst]})", this);
                }
            }
        }
        _currentContacts.Clear();
    }

    public static bool HasWaterOn(BridgeQuadrantInstance inst)
    {
        if (inst == null) return false;
        return _activeWaterContacts.ContainsKey(inst);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (debugRadius > 0f)
        {
            Gizmos.color = new Color(0f,0.5f,1f,0.3f);
            Gizmos.DrawWireSphere(transform.position, debugRadius);
        }
    }
#endif
}
