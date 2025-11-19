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

    [Header("UI Vida del Cuadrante")]
    [SerializeField] private UnityEngine.UI.Image lifeBarImage;
    [SerializeField] private UnityEngine.UI.Image lifeBarBackground;

    [Header("Modo de Umbrales")]
    [Tooltip("Si está activo usa valores ABSOLUTOS de vida (puntos) para colores; si no, usa ratios 0..1.")]
    [SerializeField] private bool useAbsoluteLifeThresholds = true;

    [Header("Umbrales Absolutos (puntos de vida)")]
    [Tooltip("Vida >= este valor → color alto (verde). Ej: 43.")]
    [SerializeField] private float greenLifeAbsolute = 43f;
    [Tooltip("Vida < greenLifeAbsolute y >= este valor → color medio (amarillo). Ej: 3.")]
    [SerializeField] private float yellowLifeAbsolute = 3f;

    [Header("Umbrales por Ratio (solo si useAbsoluteLifeThresholds = false)")]
    [Range(0f,1f)] [SerializeField] private float greenLifeThreshold = 0.51f;
    [Range(0f,1f)] [SerializeField] private float yellowLifeThreshold = 0.26f;

    [Header("Colores de la barra")]
    [SerializeField] private Color highLifeColor = Color.green;
    [SerializeField] private Color mediumLifeColor = Color.yellow;
    [SerializeField] private Color lowLifeColor = Color.red;

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

        if (lifeBarImage != null) lifeBarImage.gameObject.SetActive(false);
        if (lifeBarBackground != null) lifeBarBackground.gameObject.SetActive(false); // ocultar fondo también
    }

    private void Update()
    {
        if (!Application.isPlaying || !validateHeatByProbing) return;
        if (quadrantSO == null) return;

        if (Time.time < _nextProbeTime) return;
        _nextProbeTime = Time.time + probeInterval;

        bool anyHeat = ProbeAnyHeat();
        if (anyHeat)
        {
            // Si detectamos calor y aún no está activo en el SO, encenderlo proactivamente
            if (!quadrantSO.heatActive)
            {
                if (debugHeatProbe)
                    Debug.Log($"[BridgeQuadrantInstance] Detected HeatSphere cerca. Activando calor en '{name}'.");
                quadrantSO.ApplyHeat();
            }
            _lastHeatSeenTime = Time.time;
        }
        else
        {
            // Si no vemos calor por un tiempo de gracia, apagar
            if (quadrantSO.heatActive && Time.time - _lastHeatSeenTime > heatLoseGraceSeconds)
            {
                if (debugHeatProbe)
                    Debug.Log($"[BridgeQuadrantInstance] Apagando calor por ausencia de HeatSphere en '{name}'.");
                quadrantSO.RemoveHeat();
            }
        }

        // Actualización de barra de vida (independiente de calor)
        UpdateLifeBar();
    }

    private void UpdateLifeBar()
    {
        if (lifeBarImage == null || quadrantSO == null) return;

        if (!AreAllLayersBuilt())
        {
            if (lifeBarImage.gameObject.activeSelf) lifeBarImage.gameObject.SetActive(false);
            if (lifeBarBackground != null && lifeBarBackground.gameObject.activeSelf)
                lifeBarBackground.gameObject.SetActive(false);
            return;
        }

        if (!lifeBarImage.gameObject.activeSelf) lifeBarImage.gameObject.SetActive(true);
        if (lifeBarBackground != null && !lifeBarBackground.gameObject.activeSelf)
            lifeBarBackground.gameObject.SetActive(true);

        float life = quadrantSO.currentLife;   // puntos actuales
        float maxLife = quadrantSO.maxLife;    // puntos máximos
        float ratio = maxLife > 0f ? life / maxLife : 0f;

        // El fill sigue usando ratio para que funcione con cualquier maxLife
        lifeBarImage.fillAmount = ratio;

        // Selección de color según modo
        if (useAbsoluteLifeThresholds)
        {
            // Absoluto: compara puntos de vida directamente
            if (life >= greenLifeAbsolute)
                lifeBarImage.color = highLifeColor;
            else if (life >= yellowLifeAbsolute)
                lifeBarImage.color = mediumLifeColor;
            else
                lifeBarImage.color = lowLifeColor;
        }
        else
        {
            // Ratio tradicional
            if (ratio >= greenLifeThreshold)
                lifeBarImage.color = highLifeColor;
            else if (ratio >= yellowLifeThreshold)
                lifeBarImage.color = mediumLifeColor;
            else
                lifeBarImage.color = lowLifeColor;
        }
    }

    private bool AreAllLayersBuilt()
    {
        if (quadrantSO == null || quadrantSO.requiredLayers == null || quadrantSO.requiredLayers.Length == 0)
            return false;
        for (int i = 0; i < quadrantSO.requiredLayers.Length; i++)
        {
            var layer = quadrantSO.requiredLayers[i];
            if (layer == null || !layer.isCompleted)
                return false;
        }
        return true;
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

        if (useAbsoluteLifeThresholds)
        {
            // Asegura orden lógico (verde > amarillo)
            if (greenLifeAbsolute < yellowLifeAbsolute)
                greenLifeAbsolute = yellowLifeAbsolute;
        }
        else
        {
            if (greenLifeThreshold < yellowLifeThreshold)
                greenLifeThreshold = yellowLifeThreshold;
        }
    }
#endif
}
