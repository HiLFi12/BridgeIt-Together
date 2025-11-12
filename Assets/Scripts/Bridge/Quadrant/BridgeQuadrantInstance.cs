using UnityEngine;

/// <summary>
/// Vincula un objeto de cuadrante físico con su ScriptableObject correspondiente.
/// Añádelo al GameObject del cuadrante (etiquetado como "BridgeQuadrant").
/// </summary>
[DisallowMultipleComponent]
public class BridgeQuadrantInstance : MonoBehaviour, ITurnable
{
    [Tooltip("Referencia al ScriptableObject que representa el estado lógico de este cuadrante.")]
    public BridgeQuadrantSO quadrantSO;

    // ITurnable delega al SO para que HeatSphere pueda activar/desactivar el calor
    public bool isTurned => quadrantSO != null && quadrantSO.isTurned;
    public void TurnOn()
    {
        if (quadrantSO != null)
        {
            // Bloquear encendido si hay agua activa sobre este cuadrante (gestión central en Water)
            if (Water.HasWaterOn(this)) return;
            quadrantSO.TurnOn();
            // Failsafe: refrescar último calor visto para evitar apagado inmediato
            _lastHeatSeenTime = Time.time;
        }
    }
    public void TurnOff()
    {
        if (quadrantSO != null) quadrantSO.TurnOff();
    }

    // ===== Failsafe: validar calor por proximidad sin modificar HeatSphere =====
    [Header("Validación de calor por proximidad (failsafe)")]
    [Tooltip("Sondea si hay un HeatSphere activo cerca. Si no hay por un tiempo de gracia, apaga el calor del cuadrante.")]
    [SerializeField] private bool validateHeatByProbing = true;

    [Tooltip("Radio de chequeo. Ajústalo similar (o un poco menor) al radio del HeatSphere.")]
    [SerializeField] private float heatCheckRadius = 1.6f;

    [Tooltip("Capas que se consideran al buscar HeatSphere.")]
    [SerializeField] private LayerMask heatLayerMask = ~0;

    [Tooltip("Cada cuánto sondear (segundos).")]
    [SerializeField] private float probeInterval = 0.15f;

    [Tooltip("Tiempo de gracia sin ver HeatSphere antes de apagar (segundos).")]
    [SerializeField] private float heatLoseGraceSeconds = 0.5f;

    [Header("Debug (failsafe)")]
    [SerializeField] private bool debugHeatProbe = false;
    [SerializeField] private Color probeGizmoColor = new Color(1f, 0.5f, 0f, 0.25f);

    private float _nextProbeTime;
    private float _lastHeatSeenTime;
    private static readonly Collider[] _probe = new Collider[128];

    private void OnEnable()
    {
        _nextProbeTime = Time.time + probeInterval;
        if (quadrantSO != null && quadrantSO.isTurned)
            _lastHeatSeenTime = Time.time;
    }

    private void Update()
    {
        if (!Application.isPlaying || !validateHeatByProbing) return;
        if (quadrantSO == null) return;

        // Solo validar si creemos que hay calor aplicado
        if (!quadrantSO.heatActive) return;

        if (Time.time < _nextProbeTime) return;
        _nextProbeTime = Time.time + probeInterval;

        bool anyHeat = ProbeAnyHeat();
        if (anyHeat)
        {
            _lastHeatSeenTime = Time.time;
        }
        else
        {
            if (Time.time - _lastHeatSeenTime > heatLoseGraceSeconds)
            {
                if (debugHeatProbe)
                    Debug.Log($"[BridgeQuadrantInstance] Apagando calor por ausencia de HeatSphere en '{name}'.");
                quadrantSO.RemoveHeat();
            }
        }
    }

    private bool ProbeAnyHeat()
    {
        // Primero: intentar por colliders (si algún HeatSphere decide tenerlos)
        int mask = (heatLayerMask.value == 0) ? Physics.AllLayers : heatLayerMask.value;
        int count = Physics.OverlapSphereNonAlloc(transform.position, heatCheckRadius, _probe, mask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            var col = _probe[i];
            if (col == null) continue;

            var hs = col.GetComponentInParent<HeatSphere>();
            if (hs != null && hs.isActiveAndEnabled && hs.gameObject.activeInHierarchy)
                return true;
        }

        // Fallback: no dependas de colliders; busca HeatSphere activos por distancia
        var spheres = Object.FindObjectsOfType<HeatSphere>();
        float r2 = heatCheckRadius * heatCheckRadius;
        Vector3 pos = transform.position;

        for (int i = 0; i < spheres.Length; i++)
        {
            var hs = spheres[i];
            if (hs == null || !hs.isActiveAndEnabled || !hs.gameObject.activeInHierarchy) continue;

            // Centro del HeatSphere (raíz)
            float d2 = (hs.transform.position - pos).sqrMagnitude;
            if (d2 <= r2)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reevaluar calor tras la salida de agua: si hay HeatSphere cercana se re-aplica calor; de lo contrario se asegura apagado.
    /// </summary>
    public void ReevaluateHeatAfterWater()
    {
        if (quadrantSO == null) return;
        bool hasHeat = ProbeAnyHeat();
        if (hasHeat)
        {
            quadrantSO.ApplyHeat();
            _lastHeatSeenTime = Time.time;
        }
        else
        {
            quadrantSO.RemoveHeat();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (validateHeatByProbing)
        {
            Gizmos.color = probeGizmoColor;
            Gizmos.DrawWireSphere(transform.position, heatCheckRadius);
        }
    }

    private void OnValidate()
    {
        if (heatCheckRadius < 0.1f) heatCheckRadius = 0.1f;
        if (probeInterval < 0.02f) probeInterval = 0.02f;
        if (heatLoseGraceSeconds < 0f) heatLoseGraceSeconds = 0f;
    }
#endif
}
