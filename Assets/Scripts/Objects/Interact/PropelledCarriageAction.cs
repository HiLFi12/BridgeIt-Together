using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Orquestador para "carro con guardia propulsado" (genérico).
/// - Hereda de ProbabilityAction: con cierta probabilidad el caballo patea la carroza y el guardia sale volando.
/// - No depende de un tipo específico (p.ej. CarGuard); funciona con cualquier prefab con Rigidbody.
/// - No crea flags de estado; sólo aplica lanzamiento físico para que los detectores propios del prefab reaccionen.
/// Adjuntar al root del vehículo (la carroza).
/// </summary>
[DisallowMultipleComponent]
public class PropelledCarriageAction : AutoController
{
    [Header("Referencias")]
    [SerializeField] private GameObject guardPrefab; // Prefab del guardia (puede tener CarGuard en la raíz o en un hijo)
    [SerializeField] private Animator horseAnimator;       // Animator del caballo (opcional)
    [SerializeField] private Transform forwardReference;   // Referencia para dirección forward (por defecto este transform)
    [SerializeField] private Transform guardSpawnPoint;    // Punto de spawn opcional para el guardia prefab
    [SerializeField] private Vector3 guardSpawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Animación Caballo (opcional)")]
    [SerializeField] private string horseKickTrigger = "Kick"; // Trigger o nombre de anim

    [Header("Lanzamiento del Guardia")]
    [Tooltip("Magnitud horizontal del lanzamiento. Se aplicará en la dirección contraria por eje X.")]
    [SerializeField] private float forwardLaunch = 7f;
    [Tooltip("Impulso vertical aplicado al guardia (en unidades de velocidad cambio)")]
    [SerializeField] private float upwardLaunch = 5f;
    [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;

    [Header("Al caer (opcional)")]
    [Tooltip("Hacer que el guardia cuente como vehículo al tocar el piso (usa su propio controlador para tags)")]
    [SerializeField] private bool countAsVehicleOnGround = true;
    [Tooltip("Velocidad del guardia al tocar el piso, como vehículo básico")]
    [SerializeField] private float groundVehicleSpeed = 5f;
    [Tooltip("Tiempo aproximado en el aire antes de reactivarse como vehículo")]
    [SerializeField] private float airTime = 1.0f;

    [Header("Comportamiento de Lanzamiento")]
    [Tooltip("Si es true, desparenta al guardia antes de lanzarlo para que sea independiente")]
    [SerializeField] private bool detachGuardOnLaunch = true;
    [Tooltip("Tiempo durante el cual se ignoran colisiones guardia↔carroza tras el lanzamiento (0=desactivado)")]
    [SerializeField] private float hostCollisionIgnoreDuration = 0.75f;

    [Header("Guardia sentado sobre la carroza (previo al lanzamiento)")]
    [Tooltip("Parentar el guardia al host para que no se caiga antes del lanzamiento")]
    [SerializeField] private bool parentGuardToHost = true;
    [Tooltip("Hacer al guardia cinemático y sin gravedad mientras está sobre la carroza (evita que tumbe el vehículo)")]
    [SerializeField] private bool makeGuardKinematicWhileOnCarriage = true;
    [Tooltip("Deshabilitar colliders del guardia mientras está sentado para que no interfiera con la física del vehículo")]
    [SerializeField] private bool disableGuardCollidersWhileOnCarriage = true;

    // Referencias dinámicas al vehículo (host)
    private Rigidbody hostRigidbody;
    private bool hasLaunched = false; // one-shot

    private void Reset()
    {
        forwardReference = transform;
    }

    private void Awake()
    {
        if (forwardReference == null) forwardReference = transform;
        TryCacheHostComponents();
    }

    private void Start()
    {
        // Si ya hay un guardia en hijos (prefab compuesto), configurarlo para permanecer sobre la carroza
        SetupGuardOnCarriageIfPresent();
    }

    // Compat opcional: método público con misma firma (ya no override)
    public void Execute(int quadrantX = -1, int quadrantZ = -1) => TryLaunchGuard();

    /// <summary>
    /// Intenta lanzar al guardia: dispara anim del caballo y aplica impulso físico para que el guardia salga volando.
    /// El guardia/prefab debe tener sus propios detectores para reaccionar al lanzamiento.
    /// </summary>
    public void TryLaunchGuard()
    {
        if (hasLaunched) return; // evitar doble spawn
        hasLaunched = true;
        // Animación del caballo
        if (horseAnimator != null && !string.IsNullOrEmpty(horseKickTrigger))
        {
            horseAnimator.SetTrigger(horseKickTrigger);
        }

        // Reutilizar guardia si ya existe como hijo (para conservar configuraciones), si no, instanciar
        GameObject targetGuardGO = null;
        GameObject spawnedRoot = null;
        var existingGuard = GetComponentInChildren<CarGuard>(true);
        if (existingGuard != null)
        {
            targetGuardGO = existingGuard.gameObject;
            // Asegurar posición/rotación al punto de spawn si se definió
            Transform spawnRef = guardSpawnPoint != null ? guardSpawnPoint : transform;
            targetGuardGO.transform.position = spawnRef.position + guardSpawnOffset;
            targetGuardGO.transform.rotation = Quaternion.LookRotation(GetForwardDirection(), Vector3.up);
            targetGuardGO.SetActive(true);
        }
        else if (guardPrefab != null)
        {
            Transform spawnRef = guardSpawnPoint != null ? guardSpawnPoint : transform;
            Vector3 spawnPos = spawnRef.position + guardSpawnOffset;
            Quaternion spawnRot = Quaternion.LookRotation(GetForwardDirection(), Vector3.up);
            spawnedRoot = Instantiate(guardPrefab, spawnPos, spawnRot);
            // El objetivo es el root instanciado (prefab genérico)
            if (spawnedRoot != null) targetGuardGO = spawnedRoot;

            // Mantener sentado hasta el lanzamiento
            if (targetGuardGO != null && parentGuardToHost)
            {
                targetGuardGO.transform.SetParent(transform, true);
                // Configurar CarGuard si existe
                var cg = targetGuardGO.GetComponentInChildren<CarGuard>(true);
                if (cg != null)
                {
                    cg.SetCarriageRoot(transform);
                    // Ya no se requiere configurar velocidad ni forward en CarGuard (solo bools por sensor)
                }

                var grb = targetGuardGO.GetComponent<Rigidbody>();
                if (grb == null) grb = targetGuardGO.GetComponentInChildren<Rigidbody>(true);
                if (grb != null && makeGuardKinematicWhileOnCarriage)
                {
                    grb.isKinematic = true;
                    grb.useGravity = false;
                    grb.linearVelocity = Vector3.zero;
                    grb.angularVelocity = Vector3.zero;
                }

                if (disableGuardCollidersWhileOnCarriage)
                {
                    var gcols = targetGuardGO.GetComponentsInChildren<Collider>(true);
                    for (int i = 0; i < gcols.Length; i++)
                    {
                        var c = gcols[i];
                        if (c == null) continue;
                        c.enabled = false;
                    }
                }
            }

            // Nota: Si deseas comportamiento especial "al caer", configúralo en el propio prefab
            // (por ejemplo, con AutoControllerBasic). Este script no impone flags custom.
            if (countAsVehicleOnGround)
            {
                Debug.Log("[PropelledCarriageAction] countAsVehicleOnGround activo. Asegúrate de que el prefab tenga su propio controlador para el estado en suelo.", this);
            }
        }

        if (targetGuardGO == null)
        {
            Debug.LogWarning("[PropelledCarriageAction] No hay guardPrefab asignado; no se puede lanzar el guardia.", this);
            return;
        }
        // Obtener Rigidbody en raíz o en hijos
        Rigidbody rb = targetGuardGO.GetComponent<Rigidbody>();
        if (rb == null) rb = targetGuardGO.GetComponentInChildren<Rigidbody>(true);
        if (rb == null)
        {
            Debug.LogWarning("[PropelledCarriageAction] El prefab del guardia no tiene Rigidbody; no se puede aplicar impulso.", this);
            return;
        }

        // Revertir estado "sentado": re-habilitar colliders, quitar kinematic, etc.
        if (disableGuardCollidersWhileOnCarriage && targetGuardGO != null)
        {
            var gcols = targetGuardGO.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < gcols.Length; i++)
            {
                var c = gcols[i];
                if (c == null) continue;
                c.enabled = true;
            }
        }
        if (targetGuardGO != null)
        {
            var grb2 = targetGuardGO.GetComponent<Rigidbody>();
            if (grb2 == null) grb2 = targetGuardGO.GetComponentInChildren<Rigidbody>(true);
            if (grb2 != null && makeGuardKinematicWhileOnCarriage)
            {
                grb2.isKinematic = false;
                grb2.useGravity = true;
            }
        }

        // Desparentar si se solicita (aplica tanto a guardia existente como instanciado)
        bool guardDetached = false;
        if (detachGuardOnLaunch)
        {
            Transform guardT = null;
            if (targetGuardGO != null) guardT = targetGuardGO.transform;
            else if (spawnedRoot != null) guardT = spawnedRoot.transform;
            if (guardT != null)
            {
                guardT.SetParent(null, true);
                guardDetached = true;
            }
        }

        // Dirección contraria por eje X
        Vector3 oppX = GetOppositeXDirection();
        Vector3 impulse = oppX * forwardLaunch + Vector3.up * upwardLaunch;

        // Borrar velocidad lateral previa mínima para efecto más claro (opcional)
        // rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.AddForce(impulse, forceMode);

        // Ignorar colisiones con la carroza por un breve lapso para evitar choques inmediatos
        Collider[] hostCols = null; Collider[] guardCols = null;
        if (hostCollisionIgnoreDuration > 0f)
        {
            var hostRoot = this.gameObject; // somos el AutoController del host
            hostCols = hostRoot.GetComponentsInChildren<Collider>(true);
            guardCols = targetGuardGO.GetComponentsInChildren<Collider>(true);
            if (hostCols != null && guardCols != null)
            {
                for (int i = 0; i < hostCols.Length; i++)
                {
                    var hc = hostCols[i];
                    if (hc == null || !hc.enabled || hc.isTrigger) continue;
                    for (int j = 0; j < guardCols.Length; j++)
                    {
                        var gc = guardCols[j];
                        if (gc == null || !gc.enabled || gc.isTrigger) continue;
                        Physics.IgnoreCollision(hc, gc, true);
                    }
                }
            }
        }

        // Preparar helper en el guardia para reactivar movimiento tras el vuelo y re-habilitar colisiones
        var helper = targetGuardGO.AddComponent<GuardResumeHelper>();
        helper.Setup(hostCols, guardCols, hostCollisionIgnoreDuration, airTime, groundVehicleSpeed, GetForwardXDirection(), FindFirstObjectByType<BridgeConstructionGrid>());

        // En lugar de destruir, desactivar el host solo si el guardia fue desanclado
        // para no apagar también al guardia cuando era hijo del host.
        if (guardDetached)
        {
            gameObject.SetActive(false);
        }
    }

    private void SetupGuardOnCarriageIfPresent()
    {
        var cg = GetComponentInChildren<CarGuard>(true);
        if (cg == null) return;
        var guardGO = cg.gameObject;
        // Wiring para conservar sus configuraciones propias (groundMask etc.)
        cg.SetCarriageRoot(transform);
        // No configurar velocidad ni forward: CarGuard solo maneja bools por sensor de pies

        // Mantenerlo sentado/anclado para que no caiga ni empuje al vehículo
        if (parentGuardToHost)
        {
            guardGO.transform.SetParent(transform, true);
        }
        var grb = guardGO.GetComponent<Rigidbody>();
        if (grb == null) grb = guardGO.GetComponentInChildren<Rigidbody>(true);
        if (grb != null && makeGuardKinematicWhileOnCarriage)
        {
            grb.isKinematic = true;
            grb.useGravity = false;
            grb.linearVelocity = Vector3.zero;
            grb.angularVelocity = Vector3.zero;
        }
        if (disableGuardCollidersWhileOnCarriage)
        {
            var gcols = guardGO.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < gcols.Length; i++)
            {
                var c = gcols[i];
                if (c == null) continue;
                c.enabled = false;
            }
        }
    }

    // Determina la dirección forward priorizando la velocidad real del host
    private Vector3 GetForwardDirection()
    {
        // 1) Referencia explícita
        if (forwardReference != null)
        {
            Vector3 f = forwardReference.forward;
            if (f.sqrMagnitude > 1e-6f) return f.normalized;
        }
        // 2) Movimiento real del host
        if (hostRigidbody != null)
        {
            Vector3 v = hostRigidbody.linearVelocity;
            v.y = 0f;
            if (v.sqrMagnitude > 1e-4f) return v.normalized;
        }
        // 3) Fallback
        return transform.forward.normalized;
    }

    // Dirección X del movimiento del host (solo +X o -X)
    private Vector3 GetForwardXDirection()
    {
        float sx = 0f;
        if (forwardReference != null)
        {
            sx = Mathf.Sign(forwardReference.forward.x);
        }
        if (Mathf.Approximately(sx, 0f) && hostRigidbody != null)
        {
            sx = Mathf.Sign(hostRigidbody.linearVelocity.x);
        }
        if (Mathf.Approximately(sx, 0f))
        {
            sx = Mathf.Sign(transform.forward.x);
            if (Mathf.Approximately(sx, 0f)) sx = 1f;
        }
        return sx > 0 ? Vector3.right : Vector3.left;
    }

    // Dirección opuesta por eje X
    private Vector3 GetOppositeXDirection()
    {
        return -GetForwardXDirection();
    }

    // Detecta Rigidbody y un componente llamado "AutoController" en padres para contexto
    private void TryCacheHostComponents()
    {
        // Como heredamos de AutoController, somos el host; tomar el Rigidbody local
        hostRigidbody = GetComponent<Rigidbody>();
        if (hostRigidbody == null)
        {
            hostRigidbody = gameObject.AddComponent<Rigidbody>();
            hostRigidbody.useGravity = true;
            hostRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }


    // Fin clase
}
