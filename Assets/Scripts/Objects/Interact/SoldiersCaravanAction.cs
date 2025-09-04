using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Caravana de soldados:
/// - 1 guardia en el piso (frontal) y 3 guardias arriba de la carreta.
/// - Habilidad (ProbabilityAction): destruir la carreta, instanciar humo y soltar 3 guardias que caen y continúan según su propio controlador.
/// - Gestión de colisión de "ESFERA del piso":
///   * Mientras la carreta está viva: desactivar esfera del guardia1 y activar la de la carreta.
///   * Al destruir la carreta: desactivar esfera de la carreta y activar la del guardia1.
/// - No depende de CarGuard; usa GameObject genérico con Rigidbody para los guardias.
/// </summary>
[DisallowMultipleComponent]
public class SoldiersCaravanAction : AutoController
{
    [Header("Referencias de Guardia")]
    [SerializeField] private GameObject groundGuard; // Guardia delantero en el piso
    [SerializeField] private List<GameObject> topGuards = new List<GameObject>(); // 3 guardias sobre la carreta

    [Header("Carreta")]
    [SerializeField] private GameObject carriageObject;       // Objeto visual/físico de la carreta a destruir
    [SerializeField] private Collider carriageFloorSphere; // Sensor de piso de la carreta (Sphere o Capsule, trigger)

    [Header("Colisión del Guardia1 (frontal)")]
    [SerializeField] private Collider groundGuardFloorSphere; // Sensor de piso del guardia frontal (Sphere o Capsule, trigger)

    [Header("Efecto de humo")]
    [SerializeField] private GameObject smokePrefab;          // Shader/VFX de humo a instanciar antes de destruir la carreta
    [SerializeField] private Vector3 smokeOffset = Vector3.up * 0.5f;

    [Header("Dirección de avance")]
    [SerializeField] private Transform forwardReference; // Si es null usa este transform

    [Header("Impulso al soltar guardias superiores")]
    [SerializeField] private float forwardDetachImpulse = 0f; // 0 para mantener comportamiento previo (solo vertical)
    [SerializeField] private float dettachUpImpulse = 2f;     // Impulso vertical suave al soltar guardias superiores
    [SerializeField] private ForceMode dettachForceMode = ForceMode.VelocityChange;

    [Header("Comportamiento de separación")]
    [SerializeField] private bool detachGuardsOnDestroy = true;
    [SerializeField] private float hostCollisionIgnoreDuration = 0.75f;

    [Header("Notas")]
    [SerializeField] private bool logBecomesVehicleHints = true; // Los prefabs deben gestionar su estado al caer

    private bool isDestroyed = false;
    // Referencias dinámicas al vehículo/auto-controller
    private Rigidbody hostRigidbody;
    private Component hostAutoController; // Nombre de tipo: "AutoController"

    protected void Awake()
    {
        //base.Awake();
        if (forwardReference == null) forwardReference = transform;
        //TryCacheHostComponents();
        // Estado inicial de colisiones: carreta activa, guardia1 desactivada
        SetCarriageFloorSphereActive(true);
        SetGroundGuardFloorSphereActive(false);
        if (logBecomesVehicleHints)
        {
            Debug.Log("[SoldiersCaravanAction] Asegúrate de que los prefabs de guardias tengan su controlador para comportarse como vehículo al caer.", this);
        }
    }

    public void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        if (!isDestroyed)
        {
            DestroyCaravan();
        }
    }

    /// <summary>
    /// Destruye la carreta: activa humo, suelta guardias de arriba y conmuta colisiones.
    /// </summary>
    public void DestroyCaravan()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 1) Instanciar humo
        if (smokePrefab != null && carriageObject != null)
        {
            Instantiate(smokePrefab, carriageObject.transform.position + smokeOffset, Quaternion.identity);
        }

        // 2) Soltar guardias superiores al mundo para que caigan y cuenten como vehículos al tocar el piso
        if (topGuards != null)
        {
            Vector3 dir = GetForwardDirection();
            foreach (var g in topGuards)
            {
                if (g == null) continue;
                // Desparentar si están como hijos de la carreta
                if (detachGuardsOnDestroy)
                    g.transform.SetParent(null, true);

                // Asegurar Rigidbody para que puedan caer
                var rb = g.GetComponent<Rigidbody>();
                if (rb == null) rb = g.GetComponentInChildren<Rigidbody>(true);
                if (rb != null)
                {
                    // Impulso hacia adelante (opcional) + vertical para separarlos visualmente
                    Vector3 impulse = dir * forwardDetachImpulse + Vector3.up * dettachUpImpulse;
                    rb.AddForce(impulse, dettachForceMode);
                }

                // Ignorar colisiones con la carroza por un breve lapso
                if (hostCollisionIgnoreDuration > 0f)
                {
                    IgnoreCollisionsWithHostTemporarily(g, hostCollisionIgnoreDuration);
                }
            }
        }

        // 3) Conmutar esfera de colisión del piso: desactivar de carreta y activar la del guardia frontal
        SetCarriageFloorSphereActive(false);
        SetGroundGuardFloorSphereActive(true);
        // Nota: El comportamiento del guardia frontal al caer lo gestiona su propio prefab/controlador.

        // 4) Destruir/ocultar la carreta
        if (carriageObject != null)
        {
            Destroy(carriageObject);
        }
    }

    private void SetCarriageFloorSphereActive(bool active)
    {
        if (carriageFloorSphere != null)
        {
            carriageFloorSphere.enabled = active;
        }
    }

    private void SetGroundGuardFloorSphereActive(bool active)
    {
        if (groundGuardFloorSphere != null)
        {
            groundGuardFloorSphere.enabled = active;
        }
    }

    // Dirección priorizando la velocidad real del host
    private Vector3 GetForwardDirection()
    {
        if (forwardReference != null)
        {
            Vector3 f = forwardReference.forward;
            if (f.sqrMagnitude > 1e-6f) return f.normalized;
        }
        if (hostRigidbody != null)
        {
            Vector3 v = hostRigidbody.linearVelocity;
            v.y = 0f;
            if (v.sqrMagnitude > 1e-4f) return v.normalized;
        }
        return transform.forward.normalized;
    }

    // Detecta Rigidbody del host y referencia al AutoController para contexto
    //private void TryCacheHostComponents()
    //{
    //    // Si este componente está en el root del vehículo, usar rb del AutoController si existe
    //    hostRigidbody = (rb != null) ? rb : GetComponentInParent<Rigidbody>();
    //    // Este propio componente hereda AutoController; usar self como host
    //    hostAutoController = this;
    //}

    // Ignora colisiones guardia↔host por un tiempo
    private void IgnoreCollisionsWithHostTemporarily(GameObject obj, float seconds)
    {
        if (obj == null) return;
        var hostRoot = hostAutoController != null ? ((Component)hostAutoController).gameObject : gameObject;
        var hostCols = hostRoot.GetComponentsInChildren<Collider>(true);
        var objCols = obj.GetComponentsInChildren<Collider>(true);
        if (hostCols == null || objCols == null) return;

        for (int i = 0; i < hostCols.Length; i++)
        {
            var hc = hostCols[i];
            if (hc == null || !hc.enabled || hc.isTrigger) continue;
            for (int j = 0; j < objCols.Length; j++)
            {
                var oc = objCols[j];
                if (oc == null || !oc.enabled || oc.isTrigger) continue;
                Physics.IgnoreCollision(hc, oc, true);
            }
        }
        StartCoroutine(ReenableCollisions(hostCols, objCols, seconds));
    }

    private System.Collections.IEnumerator ReenableCollisions(Collider[] hostCols, Collider[] objCols, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (hostCols == null || objCols == null) yield break;
        for (int i = 0; i < hostCols.Length; i++)
        {
            var hc = hostCols[i];
            if (hc == null || !hc.enabled || hc.isTrigger) continue;
            for (int j = 0; j < objCols.Length; j++)
            {
                var oc = objCols[j];
                if (oc == null || !oc.enabled || oc.isTrigger) continue;
                Physics.IgnoreCollision(hc, oc, false);
            }
        }
    }
}
