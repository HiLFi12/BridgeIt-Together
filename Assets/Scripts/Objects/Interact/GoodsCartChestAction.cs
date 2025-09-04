using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Carro de mercancías: habilidad que hereda de ProbabilityAction.
/// Al ejecutarse, instancia un cofre detrás del vehículo para que impacte el cuadrante posterior.
/// El cofre se encarga de aplicar el daño al cuadrante y generar humo al destruirse.
/// Adjuntar este componente al prefab del carro de mercancías.
/// </summary>
[DisallowMultipleComponent]
public class GoodsCartChestAction : AutoController
{
    [Header("Prefabs")]
    [SerializeField] private GameObject chestPrefab;  // Prefab del cofre (debe tener ChestProjectile)
    [SerializeField] private GameObject smokePrefab;  // Efecto de humo al destruir el cofre

    [Header("Spawn del Cofre")]
    [Tooltip("Distancia hacia atrás (respecto al forward del carro) donde se spawnea el cofre")]
    [SerializeField] private float backOffset = 1.0f;
    [Tooltip("Offset vertical del spawn")]
    [SerializeField] private float upOffset = 0.3f;

    [Header("Impulso Inicial del Cofre")]
    [Tooltip("Impulso hacia atrás aplicado al cofre (Velocidad Cambio)")]
    [SerializeField] private float backwardLaunch = 6f;
    [SerializeField] private float downwardBias = 0.5f;
    [SerializeField] private ForceMode launchForceMode = ForceMode.VelocityChange;

    [Header("Referencias")]
    [SerializeField] private Transform forwardReference; // Si es null, se intentará usar la dirección real de movimiento
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // Se autollenará si está vacío

    // Referencias dinámicas al vehículo/auto-controller
    private Rigidbody hostRigidbody;
    private Component hostAutoController; // No dependemos del tipo concreto para no acoplar

    private void Reset()
    {
        forwardReference = transform;
    }

    protected void Awake()
    {
        //base.Awake();
        if (forwardReference == null) forwardReference = transform;
        if (bridgeGrid == null) bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();

        // Intentar capturar el contexto del vehículo (AutoController/Rigidbody) si el prefab vive dentro de uno
        //TryCacheHostComponents();
    }

    public void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        SpawnChestBehind();
    }

    private void SpawnChestBehind()
    {
        if (chestPrefab == null) return;

        Vector3 backDir = GetBackDirection();
        Vector3 spawnPos = transform.position - backDir * backOffset + Vector3.up * upOffset;
        Quaternion spawnRot = Quaternion.LookRotation(-backDir, Vector3.up);

        GameObject chest = Instantiate(chestPrefab, spawnPos, spawnRot);

        // Configurar script del cofre
        var chestScript = chest.GetComponent<ChestProjectile>();


        // Evitar colisión inmediata con el vehículo anfitrión
        IgnoreCollisionsWithHost(chest);

        // Aplicar impulso inicial hacia atrás
        var rb = chest.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 impulse = -backDir * backwardLaunch + Vector3.down * downwardBias;
            rb.AddForce(impulse, launchForceMode);
        }
    }

    // Determina la dirección "hacia atrás" priorizando la orientación real de movimiento
    private Vector3 GetBackDirection()
    {
        // 1) Si hay una referencia explícita, úsala
        if (forwardReference != null)
        {
            Vector3 fwd = forwardReference.forward;
            if (fwd.sqrMagnitude > 1e-6f) return fwd.normalized;
        }

        // 2) Si el vehículo tiene Rigidbody y está moviéndose, usa su velocidad como forward
        if (hostRigidbody != null)
        {
            Vector3 vel = hostRigidbody.linearVelocity;
            vel.y = 0f; // ignorar componente vertical para un spawn más estable
            if (vel.sqrMagnitude > 1e-4f)
            {
                return vel.normalized; // backDir será -vel posteriormente
            }
        }

        // 3) Fallback: usar este transform.forward
        return transform.forward.normalized;
    }

    // Intenta localizar Rigidbody del host y usar este AutoController como referencia de host
    //private void TryCacheHostComponents()
    //{
    //    // Buscar Rigidbody en este objeto o padres (el carro suele tener uno)
    //    hostRigidbody = (rb != null) ? rb : GetComponentInParent<Rigidbody>();
    //    // Este propio componente hereda AutoController; usar self como host
    //    hostAutoController = this;
    //}

    // Ignora colisiones entre el cofre recién creado y el vehículo anfitrión
    private void IgnoreCollisionsWithHost(GameObject chest)
    {
        if (chest == null) return;

        var hostRoot = hostAutoController != null ? ((Component)hostAutoController).gameObject : gameObject;
        var hostCols = hostRoot.GetComponentsInChildren<Collider>(true);
        var chestCols = chest.GetComponentsInChildren<Collider>(true);
        if (hostCols == null || chestCols == null) return;

        for (int i = 0; i < hostCols.Length; i++)
        {
            var hc = hostCols[i];
            if (hc == null || !hc.enabled) continue;
            if (hc.isTrigger) continue;
            for (int j = 0; j < chestCols.Length; j++)
            {
                var cc = chestCols[j];
                if (cc == null || !cc.enabled) continue;
                if (cc.isTrigger) continue;
                Physics.IgnoreCollision(hc, cc, true);
            }
        }
    }
}
