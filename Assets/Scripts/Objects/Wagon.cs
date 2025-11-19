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

    [Header("Objetos Estáticos")]
    [SerializeField, Tooltip("GameObjects que no se moverán con el wagon (como canvas UI)")]
    private GameObject[] staticObjects;

    [Header("Estado / Debug")] 
    [SerializeField] private bool debugGizmos = true;

    [Header("Audio - Movimiento (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir en loop mientras el vagón se mueve. -1 desactiva.")]
    [SerializeField] private int moveLoopSfxIndex = -1;

    [Header("Requisitos para iniciar")]
    [Tooltip("Si está activo, se requiere calor (isTurned) para iniciar el viaje. Por defecto desactivado para eliminar dependencia del HeatSphere.")]
    [SerializeField] private bool requireHeatToStart = false;

    // ITurnable
    public bool isTurned { get; private set; } = false;

    // IInteractable
    public InteractPriority InteractPriority => interactPriority;

    private bool traveling = false;
    private bool atPointA = true; // indica si está estacionado exactamente en A (si es false asumimos está en B)

    // Holder
    private GameObject heldObject; // único objeto
    private Rigidbody heldRigidbody;

    private Coroutine travelRoutine;

    // Audio
    private AudioSource moveLoopSource;

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
        StartMoveLoopSfx();
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
        // Dependencia de calor eliminada por defecto. Si se desea, se puede reactivar con 'requireHeatToStart'.
        if (requireHeatToStart && !isTurned)
        {
            Debug.Log("Wagon: se requiere calor para iniciar y no hay calor activo.");
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

        // Guardar posiciones world de objetos estáticos
        Vector3[] staticPositions = null;
        Quaternion[] staticRotations = null;
        if (staticObjects != null && staticObjects.Length > 0)
        {
            staticPositions = new Vector3[staticObjects.Length];
            staticRotations = new Quaternion[staticObjects.Length];
            for (int i = 0; i < staticObjects.Length; i++)
            {
                if (staticObjects[i] != null)
                {
                    staticPositions[i] = staticObjects[i].transform.position;
                    staticRotations[i] = staticObjects[i].transform.rotation;
                }
            }
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
            
            // Restaurar posiciones de objetos estáticos
            if (staticPositions != null)
            {
                for (int i = 0; i < staticObjects.Length; i++)
                {
                    if (staticObjects[i] != null)
                    {
                        staticObjects[i].transform.position = staticPositions[i];
                        staticObjects[i].transform.rotation = staticRotations[i];
                    }
                }
            }
            
            yield return null;
        }

        // Restaurar posiciones al finalizar
        if (staticPositions != null)
        {
            for (int i = 0; i < staticObjects.Length; i++)
            {
                if (staticObjects[i] != null)
                {
                    staticObjects[i].transform.position = staticPositions[i];
                    staticObjects[i].transform.rotation = staticRotations[i];
                }
            }
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

        StopMoveLoopSfx();
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
        // Llamado por HeatSphere cuando el wagon entra en su radio de detección.
        // El wagon inmediatamente obtiene capacidad de moverse.
        isTurned = true;
    }

    public void TurnOff()
    {
        // Llamado por HeatSphere cuando el wagon sale de su radio de detección.
        // El wagon inmediatamente pierde capacidad de iniciar nuevos viajes.
        // Si está viajando, continúa hasta llegar al destino.
        isTurned = false;
    }

    private void StartMoveLoopSfx()
    {
        if (moveLoopSfxIndex < 0) return;
        if (moveLoopSource != null && moveLoopSource.isPlaying) return;

        var audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null || audioManager.soundEffects == null) return;
        if (moveLoopSfxIndex < 0 || moveLoopSfxIndex >= audioManager.soundEffects.Count) return;

        if (moveLoopSource == null)
        {
            moveLoopSource = gameObject.AddComponent<AudioSource>();
            moveLoopSource.loop = true;
            moveLoopSource.playOnAwake = false;
            moveLoopSource.spatialBlend = 1f; // 3D
        }

        moveLoopSource.clip = audioManager.soundEffects[moveLoopSfxIndex];
        if (moveLoopSource.clip != null)
        {
            moveLoopSource.Play();
        }
    }

    private void StopMoveLoopSfx()
    {
        if (moveLoopSource != null && moveLoopSource.isPlaying)
        {
            moveLoopSource.Stop();
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
