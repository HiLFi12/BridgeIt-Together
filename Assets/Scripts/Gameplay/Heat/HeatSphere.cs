using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de calor reutilizable para hornos / players (power-up).
/// Administrar vida (duración) del fuego, recarga con "carbones" y activación de objetos ITurnable dentro de un radio.
/// Colocar este script en un GameObject esférico (puede tener un SphereCollider trigger) como hijo del Horno o directamente en el jugador.
/// </summary>
[DisallowMultipleComponent]
public class HeatSphere : MonoBehaviour
{
    [Header("Vida del Fuego")]
    [Tooltip("Vida máxima (tiempo en segundos) que puede tener el fuego al recargar completamente.")]
    [SerializeField] private float vidaMaxima = 30f;
    [Tooltip("Vida actual del fuego (decrece con el tiempo si activo).")]
    [SerializeField] private float vidaActual = 0f;
    [Tooltip("Velocidad de consumo por segundo de la vida.")]
    [SerializeField] private float consumoPorSegundo = 1f;

    [Header("Recarga (Carbones)")]
    [Tooltip("Cantidad de carbones necesarios para activar/recargar completamente el fuego.")]
    [SerializeField] private int carbonesNecesarios = 3;
    [Tooltip("Carbones acumulados actualmente.")]
    [SerializeField] private int carbonesActuales = 0;
    [Tooltip("Prefab de carbón (munición) que se consume al interactuar para recargar.")]
    [SerializeField] private GameObject prefabCarbon; // Referencia opcional para validación visual

    [Header("Cooldown / Estado")]
    [Tooltip("Indica si el sistema está en cooldown activo (true mientras vidaActual > 0). Se vuelve false cuando llega a 0.")]
    [SerializeField] private bool cooldownActivo = false;

    [Header("Detección ITurnable")]
    [Tooltip("Usar un SphereCollider como trigger. Si se deja null se intentará obtener del mismo objeto.")]
    [SerializeField] private SphereCollider detectionSphere;
    [Tooltip("Capas a considerar para detección de objetos ITurnable.")]
    [SerializeField] private LayerMask turnableLayerMask = ~0; // por defecto todo

    [Header("Efecto de Calor")]
    [Tooltip("GameObject (por ejemplo un VFX) que se activa mientras el fuego está encendido.")]
    [SerializeField] private GameObject efectoCalor;

    [Header("Debug")] 
    [SerializeField] private bool mostrarDebugLogs = false;
    [SerializeField] private Color gizmoColorEncendido = new Color(1f, 0.4f, 0f, 0.35f);
    [SerializeField] private Color gizmoColorApagado = new Color(0.3f, 0.3f, 0.3f, 0.25f);

    // Objetos ITurnable dentro del radio
    private readonly HashSet<ITurnable> turnablesDentro = new();

    // Propiedades públicas de solo lectura
    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;
    public int CarbonesActuales => carbonesActuales;
    public int CarbonesNecesarios => carbonesNecesarios;
    public bool EstaEncendido => vidaActual > 0f;
    public bool CooldownActivo => cooldownActivo;

    private void Reset()
    {
        detectionSphere = GetComponent<SphereCollider>();
        if (detectionSphere != null)
        {
            detectionSphere.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (!detectionSphere) detectionSphere = GetComponent<SphereCollider>();
        if (detectionSphere) detectionSphere.isTrigger = true;
        ActualizarEfectoCalor();
    }

    private void Update()
    {
        if (vidaActual > 0f)
        {
            vidaActual -= consumoPorSegundo * Time.deltaTime;
            if (vidaActual <= 0f)
            {
                vidaActual = 0f;
                cooldownActivo = false; // apagado
                ApagarTurnables();
                ActualizarEfectoCalor();
                if (mostrarDebugLogs)
                    Debug.Log($"[HeatSphere] Fuego agotado en {name}");
            }
        }
        else
        {
            cooldownActivo = false;
        }
    }

    /// <summary>
    /// Intenta añadir un carbón (desde interacción del jugador). Devuelve true si se agregó.
    /// </summary>
    public bool AgregarCarbon(GameObject carbonGO)
    {
        // Validar referencia opcional
        if (carbonGO == null)
        {
            if (mostrarDebugLogs) Debug.Log("[HeatSphere] Objeto carbón nulo.");
            return false;
        }

        carbonesActuales++;
        if (mostrarDebugLogs)
            Debug.Log($"[HeatSphere] Carbón agregado {carbonesActuales}/{carbonesNecesarios} en {name}");

        if (carbonesActuales >= carbonesNecesarios)
        {
            // Reset contador y activar fuego completo
            carbonesActuales = 0;
            vidaActual = vidaMaxima;
            cooldownActivo = true;
            EncenderTurnables();
            ActualizarEfectoCalor();
            if (mostrarDebugLogs)
                Debug.Log($"[HeatSphere] Fuego recargado a máximo en {name}");
        }

        return true;
    }

    private void EncenderTurnables()
    {
        foreach (var t in turnablesDentro)
        {
            if (t != null && !t.IsTurn)
            {
                t.TurnOn(gameObject);
            }
        }
    }

    private void ApagarTurnables()
    {
        foreach (var t in turnablesDentro)
        {
            if (t != null && t.IsTurn)
            {
                t.TurnOff(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EstaEnLayer(other.gameObject.layer)) return;
        var turnable = other.GetComponent<ITurnable>();
        if (turnable == null) return;
        turnablesDentro.Add(turnable);
        if (EstaEncendido)
        {
            // activar inmediatamente
            if (!turnable.IsTurn)
                turnable.TurnOn(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!EstaEnLayer(other.gameObject.layer)) return;
        var turnable = other.GetComponent<ITurnable>();
        if (turnable == null) return;
        if (turnablesDentro.Contains(turnable))
        {
            // salida: apagar
            if (turnable.IsTurn)
                turnable.TurnOff(gameObject);
            turnablesDentro.Remove(turnable);
        }
    }

    private bool EstaEnLayer(int layer)
    {
        return (turnableLayerMask & (1 << layer)) != 0;
    }

    private void ActualizarEfectoCalor()
    {
        if (efectoCalor != null)
        {
            efectoCalor.SetActive(EstaEncendido);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var col = detectionSphere ? detectionSphere : GetComponent<SphereCollider>();
        if (!col) return;
        Gizmos.color = EstaEncendido ? gizmoColorEncendido : gizmoColorApagado;
        Gizmos.DrawSphere(col.bounds.center, col.radius * transform.lossyScale.x);
    }
#endif
}
