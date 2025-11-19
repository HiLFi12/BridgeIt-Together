using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI de estado (encima de la última capa)")]
    [Tooltip("Canvas en World Space ya colocado encima del cuadrante (opcional, solo referencia).")]
    [SerializeField] private Canvas worldCanvas;
    [Tooltip("Image que usaremos para mostrar el icono de estado de vida.")]
    [SerializeField] private Image stateImage;

    [Header("Umbrales de vida (ratio 0..1)")]
    [Tooltip("Vida >= highThreshold → estado ALTO (sprite alto).")]
    [Range(0f, 1f)] [SerializeField] private float highThreshold = 0.51f;
    [Tooltip("Vida >= mediumThreshold y < highThreshold → estado MEDIO (sprite medio).")]
    [Range(0f, 1f)] [SerializeField] private float mediumThreshold = 0.26f;
    [Tooltip("Vida < mediumThreshold → estado BAJO (sprite bajo + temblor opcional).")]
    [Range(0f, 1f)] [SerializeField] private float lowThreshold = 0.0f;

    [Header("Sprites por estado (genérico)")]
    [SerializeField] private Sprite highLifeSprite;
    [SerializeField] private Sprite mediumLifeSprite;
    [SerializeField] private Sprite lowLifeSprite;

    [Header("Modos por era")] 
    [Tooltip("Si está activo, usa presets específicos por era en vez de los umbrales genéricos.")]
    [SerializeField] private bool useEraSpecificMode = true;

    [System.Serializable]
    public class EraMode
    {
        public BridgeQuadrantSO.EraType era;
        [Range(0f, 1f)] public float highThreshold = 0.51f;
        [Range(0f, 1f)] public float mediumThreshold = 0.26f;
        [Range(0f, 1f)] public float lowThreshold = 0.0f;
        public Sprite highSprite;
        public Sprite mediumSprite;
        public Sprite lowSprite;
        public bool enableShakeOnLow = true;
    }

    [SerializeField] private EraMode[] eraModes;


    [Header("Debug")]
    [SerializeField] private bool autoFromRenderers = true;
    [SerializeField] private bool drawGizmos = false;
    [Tooltip("Si está activo, al pausar (timeScale=0) se detiene el temblor y se restauran las poses base.")]
    [SerializeField] private bool stopWhenPaused = true;

    private Vector3[] _baseLocalPos;
    private Quaternion[] _baseLocalRot;
    private float[] _phase;
    private bool _initialized;
    private EraMode _activeMode;

    private void Awake()
    {
        EnsureTargets();
        CaptureBases();

        // Resolver preset por era
        if (quadrantSO != null && useEraSpecificMode && eraModes != null)
        {
            foreach (var m in eraModes)
            {
                if (m != null && m.era == quadrantSO.era)
                {
                    _activeMode = m;
                    break;
                }
            }
        }

        if (_activeMode == null)
        {
            _activeMode = new EraMode
            {
                era = quadrantSO != null ? quadrantSO.era : BridgeQuadrantSO.EraType.Medieval,
                highThreshold = highThreshold,
                mediumThreshold = mediumThreshold,
                lowThreshold = lowThreshold,
                highSprite = highLifeSprite,
                mediumSprite = mediumLifeSprite,
                lowSprite = lowLifeSprite,
                enableShakeOnLow = true
            };
        }
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

        // Evitar artefactos cuando el juego está pausado: restaurar y no aplicar offsets
        if (stopWhenPaused && Mathf.Approximately(Time.timeScale, 0f))
        {
            RestoreBases();
            return;
        }

        // Solo considerar efectos si la última capa está construida y el cuadrante no está destruido
        int lastIdx = (quadrantSO.requiredLayers != null && quadrantSO.requiredLayers.Length > 0)
            ? quadrantSO.requiredLayers.Length - 1 : 2;
        bool lastBuilt = lastIdx >= 0 && lastIdx < quadrantSO.requiredLayers.Length && quadrantSO.requiredLayers[lastIdx].isCompleted;
        if (!lastBuilt || quadrantSO.lastLayerState == BridgeQuadrantSO.LastLayerState.Destroyed)
        {
            if (stateImage != null) stateImage.enabled = false;
            RestoreBases();
            return;
        }

        float lifePoints = GetLifePoints(quadrantSO);
        float ratio = quadrantSO.GetLifeRatio(); // 0..1

        // Elegir sprite y si debe temblar según el modo activo
        Sprite spriteToUse = null;
        bool shouldShake = false;

        if (ratio >= _activeMode.highThreshold)
        {
            spriteToUse = _activeMode.highSprite;
            shouldShake = false;
        }
        else if (ratio >= _activeMode.mediumThreshold)
        {
            spriteToUse = _activeMode.mediumSprite;
            shouldShake = false;
        }
        else
        {
            spriteToUse = _activeMode.lowSprite;
            shouldShake = _activeMode.enableShakeOnLow;
        }

        // Actualizar imagen de estado
        if (stateImage != null)
        {
            if (spriteToUse != null)
            {
                stateImage.enabled = true;
                stateImage.sprite = spriteToUse;
            }
            else
            {
                stateImage.enabled = false;
            }
        }

        if (!shouldShake)
        {
            RestoreBases();
            return;
        }

        if (!_initialized) CaptureBases();

        // Usar tiempo escalado normal; si quisieras animar aún en pausa, cambia a Time.unscaledTime
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

            // Rotación: leve oscilación (componer sobre la base para evitar drift de euler)
            Vector3 eulerOffset = new Vector3(0.35f * s2, 0.0f, 1.0f * s1) * rotationAmplitudeDeg;
            tf.localRotation = _baseLocalRot[i] * Quaternion.Euler(eulerOffset);
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
