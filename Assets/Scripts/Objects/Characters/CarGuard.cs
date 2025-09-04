using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// CarGuard: soldado que viaja sobre una carroza pero puede comportarse como vehículo al tocar el piso.
/// Estados clave:
/// - IsOnGround: true si toca el piso -> anima Walk/Run (marchando) y puede contar muerte individual al caer.
/// - IsOnCarriage: true si está sobre la carroza -> anima Idle y es "decoración" (no cuenta muerte extra si cae el vehículo).
/// - IsFlying: true si no toca nada -> anima Fly/Launch y se desactiva movimiento.
/// Reglas:
/// - Prioridad de estados: OnGround > OnCarriage > Flying.
/// - Cuando IsOnGround: activa como vehículo (AutoController) con velocidad configurada.
/// - Cuando IsOnCarriage: velocidad 0; no cuenta muerte individual.
/// - Cuando IsFlying: velocidad 0; anima volando.
/// </summary>
[DisallowMultipleComponent]
public class CarGuard : AutoController
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [Tooltip("Referencia a la carroza para detección por tag/layer")]
    [SerializeField] private Transform carriageRoot;

    [Header("Anim Params")]
    [SerializeField] private string animParamIsWalking = "IsWalking";
    [SerializeField] private string animParamIsIdle = "IsIdle";
    [SerializeField] private string animParamIsFlying = "IsFlying";

    [Header("Detección por tags/layers")]
    [Tooltip("Si toca alguno de estos tags, se considera suelo para moverse")]
    [SerializeField] private string[] groundTags = new[] { "Platform", "BridgeQuadrant" };
    [Tooltip("Tag alternativo para detectar carroza")]
    [SerializeField] private string caravanTag = "Caravan";
    [Tooltip("LayerMask alternativo para detectar carroza")]
    [SerializeField] private LayerMask caravanLayer;

    [Header("Sensor de pies (SphereCollider)")]
    [Tooltip("SphereCollider usado como sensor en los pies (isTrigger)")]
    [SerializeField] private SphereCollider footSensor;
    [SerializeField] private float footRadius = 0.15f;
    [SerializeField] private Vector3 footLocalCenter = new Vector3(0, -0.1f, 0);

    // Estado
    public bool IsOnGround { get; private set; }
    public bool IsOnCarriage { get; private set; }
    public bool IsFlying { get; private set; }

    // Cache
    private readonly System.Collections.Generic.List<Collider> _overlaps = new System.Collections.Generic.List<Collider>(8);

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    protected void Awake()
    {
        //base.Awake();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        // Asegurar el sensor de pies
        if (footSensor == null)
        {
            footSensor = GetComponent<SphereCollider>();
        }
        if (footSensor == null)
        {
            footSensor = gameObject.AddComponent<SphereCollider>();
        }
        footSensor.isTrigger = true;
        footSensor.center = footLocalCenter;
        footSensor.radius = Mathf.Max(0.01f, footRadius);
    }

    protected void Update()
    {
        UpdateStateByFootSensor();
        ApplyAnimatorBools();
        //base.Update();
    }

    private void UpdateStateByFootSensor()
    {
        // Posición y radio del sensor en espacio mundo
        Vector3 worldCenter = transform.TransformPoint(footSensor != null ? footSensor.center : footLocalCenter);
        float radius = footSensor != null ? footSensor.radius : footRadius;

        // OverlapSphere para detectar contactos bajo los pies
        int hits = Physics.OverlapSphereNonAlloc(worldCenter, radius, _tempBuffer, ~0, QueryTriggerInteraction.Collide);
        bool ground = false;
        bool caravan = false;
        for (int i = 0; i < hits; i++)
        {
            var col = _tempBuffer[i]; if (col == null || col.gameObject == this.gameObject) continue;
            // Ignorar nuestro propio sensor si aplicara
            if (col == footSensor) continue;
            // Detectar por tags de suelo
            if (!ground && col != null)
            {
                for (int t = 0; t < groundTags.Length; t++)
                {
                    var tag = groundTags[t];
                    if (!string.IsNullOrEmpty(tag) && col.CompareTag(tag)) { ground = true; break; }
                }
            }
            // Detectar carroza por tag o capa
            if (!caravan && col != null)
            {
                if (!string.IsNullOrEmpty(caravanTag) && col.CompareTag(caravanTag)) caravan = true;
                else if (caravanLayer.value != 0 && ((caravanLayer.value & (1 << col.gameObject.layer)) != 0)) caravan = true;
            }
            if (ground && caravan) break;
        }

        // Prioridad: suelo > carroza > volando
        IsOnGround = ground;
        IsOnCarriage = !ground && caravan;
        IsFlying = !ground && !caravan;
    }

    private void ApplyAnimatorBools()
    {
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(animParamIsWalking)) animator.SetBool(animParamIsWalking, IsOnGround);
            if (!string.IsNullOrEmpty(animParamIsIdle)) animator.SetBool(animParamIsIdle, IsOnCarriage);
            if (!string.IsNullOrEmpty(animParamIsFlying)) animator.SetBool(animParamIsFlying, IsFlying);
        }
    }
    // Buffer estático para evitar allocs en OverlapSphere
    private static readonly Collider[] _tempBuffer = new Collider[16];

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 worldCenter = Application.isPlaying
            ? transform.TransformPoint(footSensor != null ? footSensor.center : footLocalCenter)
            : transform.TransformPoint(footLocalCenter);
        float radius = Application.isPlaying && footSensor != null ? footSensor.radius : footRadius;
        Gizmos.DrawWireSphere(worldCenter, radius);
    }

    // API pública para wiring dinámico desde acciones/spawners
    public void SetCarriageRoot(Transform root) => carriageRoot = root;

}
