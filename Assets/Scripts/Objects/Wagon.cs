using UnityEngine;
using System.Collections;

/// <summary>
/// Wagon: vagon que se mueve entre dos puntos (A <-> B) usando Translate.
/// Requiere: 
/// - Dos referencias Transform (pointA, pointB) asignadas en el inspector (empty objects en la escena).
/// - Implementa IInteractable: el jugador lo inicia (toggle de viaje) solo si hay calor activo (isTurned==true).
/// - Implementa ITurnable: encendido/apagado controlado por HeatSphere. Si se apaga durante el trayecto, completa el tramo actual y luego se queda inactivo hasta que vuelva a haber calor.
/// - Posee un simple holder para un único item (similar a PlayerObjectHolder) para colocar/quitar un objeto con la misma tecla de interacción.
/// Flujo:
///   1) Idle en punto actual (comienza en pointA o en su posición inicial más cercana a A/B).
///   2) Jugador interactúa (Interact) -> si isTurned && !viajando inicia viaje hacia el otro punto.
///   3) Durante el viaje si Heat se pierde (TurnOff) se marca flag heatLostDuringTrip. El movimiento NO se cancela: termina el tramo.
///   4) Al llegar: si heatLostDuringTrip == true el vagón queda inactivo (requiere que HeatSphere vuelva a activarlo para poder iniciar el viaje inverso).
///   5) Si el jugador interactúa mientras sostiene un objeto y el vagón no tiene uno -> lo coloca. Si el vagón tiene y el jugador está libre -> se lo da.
/// Priorización de interacción configurable.
/// </summary>
public class Wagon : MonoBehaviour, IInteractable, ITurnable
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f; // unidades por segundo
    [SerializeField] private bool startAtPointA = true;
    [SerializeField, Tooltip("Si está activo, se toman las posiciones iniciales de los puntos y luego se usan fijas incluso si los transforms son hijos y se mueven.")] private bool freezePointPositions = true;

    private Vector3 worldPointA;
    private Vector3 worldPointB;
    private bool worldPointsInitialized = false;

    [Header("Interacción")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // por si se necesita referencia (no se usa directamente aquí)
    // El vagón ya no inicia viaje por interacción directa; se usa una palanca externa.


    [Header("Holder del Vagón")]
    [SerializeField] private Transform holderAnchor;

    [Header("Estado / Debug")] 
    [SerializeField] private bool debugGizmos = true;

    // ITurnable
    public bool isTurned { get; private set; } = false;

    // IInteractable
    public InteractPriority InteractPriority => interactPriority;

    private bool traveling = false;
    private bool atPointA = true; // indica si está estacionado exactamente en A (si es false asumimos está en B)
    private bool heatLostDuringTrip = false;

    // Holder
    private GameObject heldObject; // único objeto
    private Rigidbody heldRigidbody;


    private Coroutine travelRoutine;

    private void Awake()
    {
        // Validaciones suaves (permitimos nulos hasta asignar en inspector)
        if (holderAnchor == null)
        {
            holderAnchor = this.transform; // fallback
        }
    }

    private void Start()
    {
        // Inicializar posición
        if (pointA != null && pointB != null)
        {
            if (freezePointPositions || !worldPointsInitialized)
            {
                worldPointA = pointA.position;
                worldPointB = pointB.position;
                worldPointsInitialized = true;
            }
            if (startAtPointA)
            {
                transform.position = freezePointPositions ? worldPointA : pointA.position;
                atPointA = true;
            }
            else
            {
                transform.position = freezePointPositions ? worldPointB : pointB.position;
                atPointA = false;
            }
        }
    }

    public void Interact(GameObject interactor)
    {
        // 1. Intentar intercambio de objeto de carga primero
        var playerHolder = interactor.GetComponent<PlayerObjectHolder>();
        if (playerHolder != null)
        {
            // Dar prioridad a colocar si player tiene objeto y vagón no
            if (playerHolder.HasObjectInHand() && heldObject == null)
            {
                RecibirObjetoDeJugador(playerHolder);
                return; // una sola acción por interacción
            }
            // Si el jugador no sostiene nada y el vagón tiene, entregarlo
            if (!playerHolder.HasObjectInHand() && heldObject != null)
            {
                EntregarObjetoAlJugador(playerHolder);
                return;
            }
        }

        // Ya no se inicia el viaje aquí; solo la palanca externa llama AttemptStart().
    }

    private void IniciarViaje()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Wagon: faltan referencias a pointA/pointB.");
            return;
        }
        if (travelRoutine != null) StopCoroutine(travelRoutine);
        heatLostDuringTrip = false; // se limpia al comenzar
        travelRoutine = StartCoroutine(Viajar());
    }

    // Método público para ser invocado por otros interactuables/prefabs
    public bool AttemptStart()
    {
        if (traveling)
        {
            Debug.Log("Wagon: ya está viajando.");
            return false;
        }
        if (!isTurned)
        {
            Debug.Log("Wagon: no hay calor activo, no se puede iniciar el viaje.");
            return false;
        }
        IniciarViaje();
        return true;
    }

    private IEnumerator Viajar()
    {
        traveling = true;
        Vector3 target;
        if (freezePointPositions && worldPointsInitialized)
        {
            target = atPointA ? worldPointB : worldPointA;
        }
        else
        {
            target = atPointA ? pointB.position : pointA.position;
        }

        while (true)
        {
            Vector3 dir = (target - transform.position);
            float dist = dir.magnitude;
            if (dist <= 0.01f)
            {
                transform.position = target; // asegurar exactitud
                break;
            }
            Vector3 step = dir.normalized * speed * Time.deltaTime;
            if (step.magnitude >= dist)
            {
                transform.position = target;
                break;
            }
            transform.Translate(step, Space.World); // movement
            yield return null;
        }

        // Llegó al destino
        traveling = false;
        // Actualizar estado de posición alcanzada
        float distA;
        float distB;
        if (freezePointPositions && worldPointsInitialized)
        {
            distA = Vector3.Distance(transform.position, worldPointA);
            distB = Vector3.Distance(transform.position, worldPointB);
        }
        else
        {
            distA = pointA ? Vector3.Distance(transform.position, pointA.position) : Mathf.Infinity;
            distB = pointB ? Vector3.Distance(transform.position, pointB.position) : Mathf.Infinity;
        }
        if (distA <= distB)
        {
            transform.position = (freezePointPositions && worldPointsInitialized) ? worldPointA : pointA.position; // snap
            atPointA = true;
        }
        else
        {
            transform.position = (freezePointPositions && worldPointsInitialized) ? worldPointB : pointB.position;
            atPointA = false;
        }

        // Si se perdió el calor durante el viaje, no vuelve a poder iniciar hasta nuevo TurnOn
        if (heatLostDuringTrip)
        {
            isTurned = false; // forzamos estado off hasta nuevo TurnOn real por HeatSphere
        }
    }

    // --- Holder Lógica ---
    private void RecibirObjetoDeJugador(PlayerObjectHolder playerHolder)
    {
        GameObject obj = playerHolder.GetHeldObject();
        if (obj == null) return;

        // Soltar del jugador pero sin aplicar física -> tomamos control
        playerHolder.DropObject(); // esto suelta con física; queremos re-posicionar. Alternativa: replicar lógica pick.
        // Para mantener sencillo: si el drop causa reposicionamiento, teletransportamos al holder.
        heldObject = obj;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
        }
    heldObject.transform.SetParent(holderAnchor, true);
    heldObject.transform.localPosition = Vector3.zero;
    heldObject.transform.localRotation = Quaternion.identity; // Usa la rotación del anchor
    }

    private void EntregarObjetoAlJugador(PlayerObjectHolder playerHolder)
    {
        if (heldObject == null) return;
        // Crear flujo similar al PickUpExistingInstance
        var obj = heldObject;
        heldObject = null;
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true; // player holder vuelve a ponerlo cinemático
            heldRigidbody.useGravity = false;
        }
        obj.transform.SetParent(null, true);
        playerHolder.PickUpExistingInstance(obj);
        heldRigidbody = null;
    }

    // --- ITurnable ---
    public void TurnOn()
    {
        // Puede llamarse múltiples veces mientras está dentro de HeatSphere.
        // Si el calor regresa antes de finalizar el viaje, queremos que NO se invalide al llegar.
        isTurned = true;
        if (traveling && heatLostDuringTrip)
        {
            // Se recuperó el calor antes de terminar: limpiar la marca para que no se apague al llegar.
            heatLostDuringTrip = false;
        }
    }

    public void TurnOff()
    {
        if (!isTurned) return;
        if (traveling)
        {
            // Marcar que se perdió calor en medio del viaje; se evaluará al llegar.
            heatLostDuringTrip = true;
        }
        else
        {
            isTurned = false;
        }
    }

    // Gizmos para debug
    private void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;
        Gizmos.color = Color.cyan;
        if (pointA != null) Gizmos.DrawSphere(pointA.position, 0.15f);
        if (pointB != null) Gizmos.DrawSphere(pointB.position, 0.15f);
        if (holderAnchor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(holderAnchor.position, 0.1f);
        }
        if (freezePointPositions && worldPointsInitialized)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldPointA, 0.12f);
            Gizmos.DrawWireSphere(worldPointB, 0.12f);
        }
    }
}
