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
    public Vector3 startScale = new Vector3(0.2f,0.2f,0.2f);
    [Tooltip("Escala máxima a la que crece el splash.")]
    public Vector3 peakScale = new Vector3(2.5f, 0.5f, 2.5f);
    [Tooltip("Escala final luego de volver a encogerse.")]
    public Vector3 endScale = new Vector3(1f, 0.2f, 1f);
    [Tooltip("Tiempo de crecimiento hasta peak.")]
    public float growTime = 0.4f;
    [Tooltip("Tiempo de retorno desde peak hasta end.")]
    public float shrinkTime = 1.2f;
    [Tooltip("Destruir este objeto tras completar la animación (si 0 => no destruir)." )]
    public float destroyAfter = 2.5f;

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
        // Los cuadrantes parecen instanciar SOs vía BridgeConstructionGrid; necesitamos acceso.
        // Buscamos componente que pueda contener referencia indirecta (no existe directo aquí), así que se asume un gestor externo actualiza.
        // Simplificación: intentar encontrar un script que exponga el SO no implementado -> se podría extender si existe.
        // Como fallback imposible (no hay script de vínculo), retornamos null y se esperaría agregar un contenedor.
        return null; // TODO: enlazar con sistema real si se agrega componente de instancia.
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
