using UnityEngine;

[DisallowMultipleComponent]
public class QuadrantDamageVisualizer : MonoBehaviour
{
    [Header("Fuente de Estado")]
    [Tooltip("SO del cuadrante. Asignar desde el Grid o el prefab del cuadrante.")]
    [SerializeField] private BridgeQuadrantSO quadrantSO;

    [Header("Renderers a tintar")]
    [Tooltip("Si está vacío, intentará encontrar los renderers de la última capa (hijos que empiecen con 'Layer_2_').")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Color")]
    [Tooltip("Color base del material DAMAGED (0% rojo). Si está vacío, se lee del material al iniciar.")]
    [SerializeField] private Color baseDamagedColor = Color.white;
    [Tooltip("Color objetivo (100% rojo).")]
    [SerializeField] private Color fullRedColor = Color.red;

    [Tooltip("Nombre del property de color del shader. URP usa _BaseColor. Built-in usa _Color.")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Intensidad escalonada (inspector)")]
    [Tooltip("Intensidad hacia rojo cuando la vida está entre 30% y 40%.")]
    [Range(0f, 1f)] [SerializeField] private float intensityAt30 = 0.10f;
    [Tooltip("Intensidad hacia rojo cuando la vida está entre 20% y 30%.")]
    [Range(0f, 1f)] [SerializeField] private float intensityAt20 = 0.20f;
    [Tooltip("Intensidad hacia rojo cuando la vida está en 10% o menos.")]
    [Range(0f, 1f)] [SerializeField] private float intensityAt10 = 1.00f;

    [Tooltip("Multiplica las intensidades definidas arriba para afinar rápido (1 = sin cambios).")]
    [Range(0f, 2f)] [SerializeField] private float intensityScale = 1f;

    [Header("Debug")]
    [SerializeField] private bool autoFindLastLayerRenderers = true;
    [SerializeField] private bool logMissing = false;

    private int _colorPropId;
    private MaterialPropertyBlock _mpb;
    private bool _hasBaseColor;
    private float _lastAppliedT = -1f;
    private BridgeQuadrantSO.LastLayerState _lastState;

    private void Awake()
    {
        _colorPropId = Shader.PropertyToID(colorPropertyName);
        _mpb = new MaterialPropertyBlock();

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            if (autoFindLastLayerRenderers)
                targetRenderers = FindLastLayerRenderers();
        }

        // Si no hay color base explícito, tomarlo del primer renderer/material
        if (targetRenderers != null && targetRenderers.Length > 0 && targetRenderers[0] != null)
        {
            var mat = targetRenderers[0].sharedMaterial;
            if (mat != null)
            {
                if (!mat.HasProperty(_colorPropId))
                {
                    // fallback a _Color si _BaseColor no existe
                    colorPropertyName = "_Color";
                    _colorPropId = Shader.PropertyToID(colorPropertyName);
                }

                if (mat.HasProperty(_colorPropId))
                {
                    baseDamagedColor = mat.GetColor(_colorPropId);
                    _hasBaseColor = true;
                }
            }
        }
    }

    private void OnEnable()
    {
        // Forzar primer actualización
        _lastAppliedT = -1f;
        _lastState = (BridgeQuadrantSO.LastLayerState)(-1);
        ApplyTintImmediate();
    }

    private void Update()
    {
        ApplyTintImmediate();
    }

    private void ApplyTintImmediate()
    {
        if (quadrantSO == null || targetRenderers == null || targetRenderers.Length == 0)
            return;

        var state = quadrantSO.lastLayerState;
        float life = Mathf.Clamp01(quadrantSO.GetLifeRatio());

        // Si no está dañado, restaurar color base
        if (state != BridgeQuadrantSO.LastLayerState.Damaged)
        {
            if (_lastState == BridgeQuadrantSO.LastLayerState.Damaged)
                ApplyColorToAll(baseDamagedColor);
            _lastState = state;
            _lastAppliedT = -1f;
            return;
        }

        // Escalonado por umbrales relativos al damagedThreshold (por defecto 40%)
        float thr = quadrantSO != null ? quadrantSO.damagedThreshold01 : 0.40f; // ~40%
        float a = Mathf.Max(0f, thr - 0.10f); // ~30%
        float b = Mathf.Max(0f, thr - 0.20f); // ~20%
        float c = Mathf.Max(0f, thr - 0.30f); // ~10%

        float t; // intensidad hacia rojo (0..1)
        if (life > thr)          t = 0.00f;           // > 40% -> base
        else if (life >= a)       t = intensityAt30;   // 30–40%
        else if (life >=  c)       t = intensityAt20;   // 10–20% y 20–30% => 20–30% entra aquí (life > c)
        else                      t = intensityAt10;   // <= 10%

        // Escala global y clamp
        t = Mathf.Clamp01(t * Mathf.Max(0f, intensityScale));

        if (Mathf.Approximately(t, _lastAppliedT) && state == _lastState)
            return;

        Color final = Color.Lerp(baseDamagedColor, fullRedColor, t);
        ApplyColorToAll(final);

        _lastAppliedT = t;
        _lastState = state;
    }

    private void ApplyColorToAll(Color c)
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            var r = targetRenderers[i];
            if (r == null) continue;

            // Usar MPB para no instanciar materiales
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorPropId, c);
            r.SetPropertyBlock(_mpb);
        }
    }

    private Renderer[] FindLastLayerRenderers()
    {
        var trs = GetComponentsInChildren<Transform>(true);
        System.Collections.Generic.List<Renderer> list = new System.Collections.Generic.List<Renderer>();
        for (int i = 0; i < trs.Length; i++)
        {
            var t = trs[i];
            if (t == transform) continue;
            if (t.name.StartsWith("Layer_2_"))
            {
                var rs = t.GetComponentsInChildren<Renderer>(true);
                if (rs != null && rs.Length > 0)
                    list.AddRange(rs);
            }
        }

        if (list.Count == 0 && logMissing)
            Debug.LogWarning("[QuadrantDamageVisualizer] No se encontraron renderers de Layer_2_. Asigna manualmente los Renderers.", this);

        return list.ToArray();
    }

    // API pública opcional para enlazar en runtime (si el Grid instancia capas y querés fijar targets desde allí)
    public void Bind(BridgeQuadrantSO so, Renderer[] renderers = null)
    {
        quadrantSO = so;
        if (renderers != null && renderers.Length > 0)
            targetRenderers = renderers;

        // Recalcular property id por si cambió el shader del renderer
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            var mat = targetRenderers[0].sharedMaterial;
            if (mat != null)
            {
                if (!mat.HasProperty(_colorPropId))
                {
                    colorPropertyName = "_Color";
                    _colorPropId = Shader.PropertyToID(colorPropertyName);
                }
                if (mat.HasProperty(_colorPropId) && !_hasBaseColor)
                {
                    baseDamagedColor = mat.GetColor(_colorPropId);
                    _hasBaseColor = true;
                }
            }
        }

        _lastAppliedT = -1f;
        _lastState = (BridgeQuadrantSO.LastLayerState)(-1);
        ApplyTintImmediate();
    }
}