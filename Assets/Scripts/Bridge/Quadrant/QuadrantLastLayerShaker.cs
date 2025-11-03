using UnityEngine;

[DisallowMultipleComponent]
public class QuadrantLastLayerShaker : MonoBehaviour
{
    [Header("Fuente de Estado")]
    [SerializeField] private BridgeQuadrantSO quadrantSO;

    [Header("Objetivos a agitar (solo visuals)")]
    [Tooltip("Si está vacío, toma todos los Renderers hijos y usa sus transforms (sin mover colliders del root).")]
    [SerializeField] private Transform[] targetTransforms;

    [Header("Parámetros de temblor")]
    [Tooltip("Amplitud de translación (metros)")]
    [SerializeField] private float positionAmplitude = 0.005f;
    [Tooltip("Amplitud de rotación (grados)")]
    [SerializeField] private float rotationAmplitudeDeg = 0f;
    [Tooltip("Frecuencia base (Hz)")]
    [SerializeField] private float frequency = 4f;

    [Header("Debug")]
    [SerializeField] private bool autoFromRenderers = true;
    [SerializeField] private bool drawGizmos = false;

    private Vector3[] _baseLocalPos;
    private Quaternion[] _baseLocalRot;
    private float[] _phase;
    private bool _initialized;

    private void Awake()
    {
        EnsureTargets();
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
        if (quadrantSO == null || targetTransforms == null || targetTransforms.Length == 0)
            return;

        // Solo temblar si la última capa está construida y la vida es muy baja (<= 1 punto)
        int lastIdx = (quadrantSO.requiredLayers != null && quadrantSO.requiredLayers.Length > 0)
            ? quadrantSO.requiredLayers.Length - 1 : 2;
        bool lastBuilt = lastIdx >= 0 && lastIdx < quadrantSO.requiredLayers.Length && quadrantSO.requiredLayers[lastIdx].isCompleted;

        float lifePoints = GetLifePoints(quadrantSO);
        bool shouldShake = lastBuilt && quadrantSO.lastLayerState != BridgeQuadrantSO.LastLayerState.Destroyed && lifePoints <= 1f;

        if (!shouldShake)
        {
            // Restaurar si dejó de temblar
            RestoreBases();
            return;
        }

        if (!_initialized) CaptureBases();

        float t = Time.time;
        for (int i = 0; i < targetTransforms.Length; i++)
        {
            var tf = targetTransforms[i];
            if (tf == null) continue;

            float ph = (_phase != null && i < _phase.Length) ? _phase[i] : 0f;
            float w = 2f * Mathf.PI * frequency;
            float s1 = Mathf.Sin(w * t + ph);
            float s2 = Mathf.Sin(w * 0.87f * t + ph * 1.37f);

            // Posición: pequeño jitter
            Vector3 offset = new Vector3(s1, 0.35f * s2, -0.8f * s1) * positionAmplitude;
            tf.localPosition = _baseLocalPos[i] + offset;

            // Rotación: leve oscilación
            Vector3 eulerOffset = new Vector3(0.35f * s2, 0.0f, 1.0f * s1) * rotationAmplitudeDeg;
            tf.localRotation = Quaternion.Euler(_baseLocalRot[i].eulerAngles + eulerOffset);
        }
    }

    private float GetLifePoints(BridgeQuadrantSO so)
    {
        switch (so.era)
        {
            case BridgeQuadrantSO.EraType.Industrial: return so.currentTemperature; // 0..maxTemperature(100)
            case BridgeQuadrantSO.EraType.Futuristic: return so.batteryLife;       // 0..100
            default: return so.currentLife;                                        // 0..maxLife(100)
        }
    }

    private void EnsureTargets()
    {
        if ((targetTransforms == null || targetTransforms.Length == 0) && autoFromRenderers)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers != null && renderers.Length > 0)
            {
                targetTransforms = new Transform[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                    targetTransforms[i] = renderers[i].transform;
            }
        }
    }

    private void CaptureBases()
    {
        EnsureTargets();
        if (targetTransforms == null) return;
        int n = targetTransforms.Length;
        _baseLocalPos = new Vector3[n];
        _baseLocalRot = new Quaternion[n];
        _phase = new float[n];
        for (int i = 0; i < n; i++)
        {
            var tf = targetTransforms[i];
            if (tf == null) continue;
            _baseLocalPos[i] = tf.localPosition;
            _baseLocalRot[i] = tf.localRotation;
            // Fase pseudo-única por transform
            _phase[i] = Hash01(tf.GetInstanceID()) * Mathf.PI * 2f;
        }
        _initialized = true;
    }

    private void RestoreBases()
    {
        if (!_initialized || targetTransforms == null) return;
        for (int i = 0; i < targetTransforms.Length; i++)
        {
            var tf = targetTransforms[i];
            if (tf == null) continue;
            tf.localPosition = _baseLocalPos[i];
            tf.localRotation = _baseLocalRot[i];
        }
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
            targetTransforms = targets;
        _initialized = false;
        CaptureBases();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || targetTransforms == null) return;
        Gizmos.color = Color.yellow;
        foreach (var t in targetTransforms)
        {
            if (t == null) continue;
            Gizmos.DrawWireSphere(t.position, 0.02f);
        }
    }
#endif
}
