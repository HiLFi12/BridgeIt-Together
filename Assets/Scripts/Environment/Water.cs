using System.Collections;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other || !other.gameObject) return;
        if (other.CompareTag("BridgeQuadrant"))
        {
            BridgeQuadrantSO so = GetQuadrantSO(other.gameObject);
            if (so != null)
            {
                so.AddWaterBlocker();
                if (debugLogs) Debug.Log($"[Water] Añadido waterBlocker a {other.name}", this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other || !other.gameObject) return;
        if (other.CompareTag("BridgeQuadrant"))
        {
            BridgeQuadrantSO so = GetQuadrantSO(other.gameObject);
            if (so != null)
            {
                so.RemoveWaterBlocker();
                if (debugLogs) Debug.Log($"[Water] Removido waterBlocker de {other.name}", this);
            }
        }
    }

    private BridgeQuadrantSO GetQuadrantSO(GameObject quadrantGO)
    {
        // Buscar el vínculo de instancia -> SO
        var link = quadrantGO.GetComponent<BridgeQuadrantInstance>();
        if (link != null && link.quadrantSO != null)
            return link.quadrantSO;
        // Si no está en el mismo GO, intentar en padres (por si el collider es hijo)
        link = quadrantGO.GetComponentInParent<BridgeQuadrantInstance>();
        if (link != null && link.quadrantSO != null)
            return link.quadrantSO;
        return null;
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
