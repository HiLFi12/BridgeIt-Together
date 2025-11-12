using UnityEngine;
using System.Collections;

public class Wagon : MonoBehaviour, IInteractable, ITurnable
{
    // Evento estático para notificar cuando un jugador interactúa con el vagón
    public static event System.Action<Wagon, GameObject> OnWagonInteracted;
    
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
    [SerializeField] private GameObject shadow;

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
        shadow.SetActive(false);
        
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
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    public void Interact(GameObject interactor)
    {
        // Notificar que el jugador interactuó con el vagón (para tutoriales)
        OnWagonInteracted?.Invoke(this, interactor);
        
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
