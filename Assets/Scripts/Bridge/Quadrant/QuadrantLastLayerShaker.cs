using UnityEngine;

[DisallowMultipleComponent]
public class QuadrantLastLayerShaker : MonoBehaviour
{
    [Header("Fuente de Estado")]
    [SerializeField] private BridgeQuadrantSO quadrantSO;

    [Header("Objetivo a agitar")]
    [Tooltip("Root visual de la última capa (se moverá todo el árbol de hijos, incluidas las grietas).")]
    [SerializeField] private Transform targetRoot;

    [Header("Parámetros de temblor")]
    [Tooltip("Amplitud de translación (metros)")]
    [SerializeField] private float positionAmplitude = 0.005f;
    [Tooltip("Amplitud de rotación (grados)")]
    [SerializeField] private float rotationAmplitudeDeg = 0f;
    [Tooltip("Frecuencia base (Hz)")]
    [SerializeField] private float frequency = 4f;

    [Header("Umbral de Vida para Temblor")]
    [Tooltip("El temblor inicia cuando la vida absoluta restante es menor o igual a este valor.")]
    public float shakeThresholdLifePoints = 1f;

    [Header("Debug")]
    [SerializeField] private bool autoFromRenderers = true;
    [SerializeField] private bool drawGizmos = false;
    [Tooltip("Si está activo, al pausar (timeScale=0) se detiene el temblor y se restauran las poses base.")]
    [SerializeField] private bool stopWhenPaused = true;

    private Vector3 _baseLocalPos;
    private Quaternion _baseLocalRot;
    private float _phase;
    private bool _initialized;

    private void Awake()
    {
        CaptureBases();
    }

    private void OnEnable()
    {
        CaptureBases();
    }

    private void OnDisable()
    {
        RestoreBases();
    }

    private void Update()
    {
        if (quadrantSO == null || targetRoot == null)
            return;

        // Evitar artefactos cuando el juego está pausado: restaurar y no aplicar offsets
        if (stopWhenPaused && Mathf.Approximately(Time.timeScale, 0f))
        {
            RestoreBases();
            return;
        }

        // Solo temblar si la última capa está construida y la vida es muy baja (<= 1 punto)
        int lastIdx = (quadrantSO.requiredLayers != null && quadrantSO.requiredLayers.Length > 0)
            ? quadrantSO.requiredLayers.Length - 1 : 2;
        bool lastBuilt = lastIdx >= 0 && lastIdx < quadrantSO.requiredLayers.Length && quadrantSO.requiredLayers[lastIdx].isCompleted;

        float lifePoints = GetLifePoints(quadrantSO);
    bool shouldShake = lastBuilt && quadrantSO.lastLayerState != BridgeQuadrantSO.LastLayerState.Destroyed && lifePoints <= shakeThresholdLifePoints;

        if (!shouldShake)
        {
            // Restaurar si dejó de temblar
            RestoreBases();
            return;
        }

        if (!_initialized) CaptureBases();

        // Usar tiempo escalado normal; si quisieras animar aún en pausa, cambia a Time.unscaledTime
        float t = Time.time;
        float w = 2f * Mathf.PI * frequency;
        float s1 = Mathf.Sin(w * t + _phase);
        float s2 = Mathf.Sin(w * 0.87f * t + _phase * 1.37f);

        // Posición: pequeño jitter
        Vector3 offset = new Vector3(s1, 0.35f * s2, -0.8f * s1) * positionAmplitude;
        targetRoot.localPosition = _baseLocalPos + offset;

        // Rotación: leve oscilación (componer sobre la base para evitar drift de euler)
        Vector3 eulerOffset = new Vector3(0.35f * s2, 0.0f, 1.0f * s1) * rotationAmplitudeDeg;
        targetRoot.localRotation = _baseLocalRot * Quaternion.Euler(eulerOffset);
    }

    private float GetLifePoints(BridgeQuadrantSO so)
    {
        switch (so.era)
        {
            case BridgeQuadrantSO.EraType.Industrial: return so.currentLife;      // Industrial migrado a vida unificada
            case BridgeQuadrantSO.EraType.Futuristic: return so.batteryLife;      // 0..100
            default: return so.currentLife;                                       // 0..maxLife(100)
        }
    }

    private void CaptureBases()
    {
        if (targetRoot == null)
        {
            if (autoFromRenderers)
            {
                // Si no se asignó root explícito, usamos el propio transform de este componente
                targetRoot = transform;
            }
            else
            {
                return;
            }
        }

        _baseLocalPos = targetRoot.localPosition;
        _baseLocalRot = targetRoot.localRotation;
        _phase = Hash01(targetRoot.GetInstanceID()) * Mathf.PI * 2f;
        _initialized = true;
    }

    private void RestoreBases()
    {
        if (!_initialized || targetRoot == null) return;
        targetRoot.localPosition = _baseLocalPos;
        targetRoot.localRotation = _baseLocalRot;
    }

    private float Hash01(int seed)
    {
        uint u = (uint)seed;
        u ^= 2747636419u;
        u *= 2654435769u;
        u ^= u >> 16;
        u *= 2654435769u;
        u ^= u >> 16;
        u *= 2654435769u;
        return (u & 0x00FFFFFF) / (float)0x01000000; // [0,1)
    }

    // API pública para enlazar desde el Grid
    public void Bind(BridgeQuadrantSO so, Transform[] targets = null)
    {
        quadrantSO = so;
        if (targets != null && targets.Length > 0)
        {
            // Tomamos el root común (primer transform) como objetivo de temblor global
            targetRoot = targets[0];
        }
        _initialized = false;
        CaptureBases();
    }

    public void ConfigureShakeThreshold(float value)
    {
        shakeThresholdLifePoints = Mathf.Max(0f, value);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || targetRoot == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetRoot.position, 0.04f);
    }
#endif
}
