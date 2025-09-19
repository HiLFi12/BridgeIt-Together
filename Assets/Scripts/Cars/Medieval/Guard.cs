using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

public class Guard : AutoController
{
    [Header("Feet Detection")]
    [SerializeField] private Transform feetPoint;
    [SerializeField, Min(0f)] private float feetRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask caravanLayer;

    [Header("Animation (optional)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isIdleParam = "IsIdle";
    [SerializeField] private string isWalkingParam = "IsWalking";
    [SerializeField] private string isFlyingParam = "IsFlying";

    [Header("Debug")]
    [SerializeField] private bool drawFeetGizmos = true;

    public bool IsIdle { get; private set; }
    public bool IsWalking { get; private set; }
    public bool IsFlying { get; private set; }

    [SerializeField, Tooltip("True si su muerte debe contar (cuando no está Idle).")]
    private bool suMuerteCuenta;
    public bool SuMuerteCuenta => suMuerteCuenta;

    private readonly Collider[] hits = new Collider[8];

    private void Reset()
    {
        feetPoint = transform;
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (!feetPoint) feetPoint = transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Auto-inicializar usando la dirección configurada en el inspector
        Vector3 dir = direccionInicial;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right;
        Initialize(dir.normalized);
    }

    private void OnValidate()
    {
        if (!feetPoint) feetPoint = transform;
        feetRadius = Mathf.Max(0f, feetRadius);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateState();
        suMuerteCuenta = !IsIdle || IsWalking || IsFlying;
        UpdateAnimator();
    }

    private void UpdateState()
    {
        bool onCaravan = OverlapAny(feetPoint.position, feetRadius, caravanLayer);
        bool onGround = OverlapAny(feetPoint.position, feetRadius, groundLayer);

        if (onCaravan)
            SetState(idle: true, walking: true, flying: false);
        else if (onGround)
            SetState(idle: false, walking: true, flying: false);
        else
            SetState(idle: false, walking: false, flying: true);
    }

    private void UpdateAnimator()
    {
        if (!animator) return;
        if (!string.IsNullOrEmpty(isIdleParam)) animator.SetBool(isIdleParam, IsIdle);
        if (!string.IsNullOrEmpty(isWalkingParam)) animator.SetBool(isWalkingParam, IsWalking);
        if (!string.IsNullOrEmpty(isFlyingParam)) animator.SetBool(isFlyingParam, IsFlying);
    }

    private void SetState(bool idle, bool walking, bool flying)
    {
        IsIdle = idle;
        IsWalking = walking;
        IsFlying = flying;
    }

    private bool OverlapAny(Vector3 center, float radius, LayerMask mask)
    {
        int count = Physics.OverlapSphereNonAlloc(center, radius, hits, mask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
            if (hits[i] != null) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawFeetGizmos) return;
        if (!feetPoint) feetPoint = transform;
        Gizmos.color = new Color(1f, 1f, 0.25f, 0.65f);
        Gizmos.DrawWireSphere(feetPoint.position, feetRadius);
    }
#endif
}