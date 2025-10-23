using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IHitable
{
    [SerializeField] private Transform interactionPoint;
    public float interactionRadius;

    private Collider[] interactables = new Collider[5];
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    [SerializeField] private KeyCode buildKey = KeyCode.F;
    
    [Header("Dash Settings")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool requireMovementForDash = true;
    [SerializeField] private bool dashOnlyWhenGrounded = true;
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private Transform dashSpawn;
    
    [Header("Interaction UI")]
    [SerializeField] private Image interactionUIImage;

    [Header("Build UI")]
    [SerializeField] private Image buildUIImage;
    
    private GameConditionManager gameConditionManager;
    private PlayerObjectHolder objectHolder;
    private PlayerBridgeInteraction bridgeInteraction;
    private PlayerAnimator playerAnimator;
    private CharacterController characterController;
    private PlayerController playerController;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction buildAction;
    private InputAction dashAction;
    private InputAction dropAction;
    private InputAction pauseAction;

    // Dash state
    private bool isDashing = false;
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private float dashTimer = 0f;

    private HashSet<IInteractable> ignoredInteractables = new HashSet<IInteractable>();

    [Header("Debug Bridge Hotkey")]
    [SerializeField] private bool enableFillBridgeHotkey = false;
    [SerializeField] private KeyCode fillBridgeKey = KeyCode.G;
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // puede asignarse manualmente, o se buscará

    void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        bridgeInteraction = GetComponent<PlayerBridgeInteraction>();
        playerAnimator = GetComponent<PlayerAnimator>();
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        gameConditionManager = FindObjectOfType<GameConditionManager>();
        
        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindAction("Interact");
            buildAction = playerInput.actions.FindAction("Build");
            dashAction = playerInput.actions.FindAction("Dash");
            dropAction = playerInput.actions.FindAction("Drop");
            pauseAction = playerInput.actions.FindAction("Pause");
        }
        
        interactionUIImage.gameObject.SetActive(false);
        // Inicializar BuildUI oculto
        if (buildUIImage != null) buildUIImage.gameObject.SetActive(false);

        // Intentar auto-asignar grid si no se arrastró en inspector
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }
    }

    void Update()
    {
        // Actualizar cooldown del dash
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
            }
        }

        // Ejecutar dash si está activo
        if (isDashing)
        {
            ExecuteDash();
            return; // No procesar otras acciones durante el dash
        }

        // Detectar input de dash
        if ((dashAction.triggered || Input.GetKeyDown(dashKey)) && canDash)
        {
            TryStartDash();
        }

        TryInteract();

        if (dropAction.triggered || Input.GetKeyDown(dropKey))
        {
            TryDropObject();
        }

        if (pauseAction.triggered)
        {
            gameConditionManager.PauseGame();
        }

        if ((buildAction.triggered || Input.GetKeyDown(buildKey)) && bridgeInteraction != null)
        {
            bridgeInteraction.TryInteractWithQuadrant();
            
            // Activar animación de construcción
            if (playerAnimator != null)
            {
                playerAnimator.TriggerBuildAnimation();
            }
        }

        // Mostrar/ocultar BuildUI según condiciones
        UpdateBuildUI();

        // Hotkey debug para rellenar todo el puente
        if (enableFillBridgeHotkey && Input.GetKeyDown(fillBridgeKey))
        {
            if (bridgeGrid != null)
            {
                // Llamar a método público que llena todo (asumiendo DebugRellenarTodoPuente es público)
                bridgeGrid.DebugRellenarTodoPuente();
            }
            else
            {
                Debug.LogWarning("[Player] Hotkey de rellenar puente presionado pero no se encontró BridgeConstructionGrid en la escena.");
            }
        }
    }

    private void TryStartDash()
    {
        // Verificar si tiene CharacterController
        if (characterController == null)
        {
            Debug.LogWarning("[Player] No se puede hacer dash sin CharacterController");
            return;
        }

        // Verificar si está en el suelo (si es requerido)
        if (dashOnlyWhenGrounded && !characterController.isGrounded)
        {
            return;
        }

        // Determinar dirección del dash
        Vector3 direction = Vector3.zero;
        
        if (requireMovementForDash && playerController != null)
        {
            // Usar el input de movimiento actual
            Vector2 movementInput = playerController.MovementInput;
            
            if (movementInput.magnitude < 0.1f)
            {
                // No hay input de movimiento suficiente
                return;
            }
            
            direction = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        }
        else
        {
            // Usar la dirección hacia donde mira el jugador
            direction = transform.forward;
        }

        // Iniciar el dash
        StartDash(direction);
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashDirection = direction;
        dashTimer = 0f;
        
        // Iniciar cooldown
        canDash = false;
        dashCooldownTimer = dashCooldown;
        
        // Instanciar efecto visual de dash si el prefab está asignado
        if (dashEffectPrefab != null)
        {
            Instantiate(dashEffectPrefab, dashSpawn.transform.position, Quaternion.identity);
        }
        
        Debug.Log($"[Player] Dash iniciado en dirección: {direction}");
    }

    private void ExecuteDash()
    {
        dashTimer += Time.deltaTime;
        
        // Calcular progreso del dash (0 a 1)
        float progress = Mathf.Clamp01(dashTimer / dashDuration);
        
        // Aplicar la curva de animación
        float curveValue = dashCurve.Evaluate(progress);
        
        // Calcular la velocidad instantánea del dash usando derivada de la curva
        float speed = dashDistance / dashDuration * GetCurveDerivative(progress);
        
        // Calcular el movimiento de este frame
        Vector3 movement = dashDirection * speed * Time.deltaTime;
        
        // Aplicar el movimiento usando CharacterController (respeta colisiones)
        if (characterController != null)
        {
            characterController.Move(movement);
        }
        
        // Verificar si el dash ha terminado
        if (progress >= 1f)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        
        Debug.Log("[Player] Dash completado");
    }

    // Calcula la derivada aproximada de la curva para velocidad suave
    private float GetCurveDerivative(float progress)
    {
        float epsilon = 0.01f;
        float nextProgress = Mathf.Min(progress + epsilon, 1f);
        
        float currentValue = dashCurve.Evaluate(progress);
        float nextValue = dashCurve.Evaluate(nextProgress);
        
        return (nextValue - currentValue) / epsilon;
    }

    // Propiedades públicas para verificar estado del dash
    public bool IsDashing => isDashing;
    public bool CanDash => canDash && !isDashing;

    private void TryInteract()
    {
        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer, QueryTriggerInteraction.Collide);

        if (elements == 0) 
        {
            HideInteractionUI();
            ignoredInteractables.Clear();
            return;
        }

        bool paloEncendido = false;
        var holder = GetComponent<PlayerObjectHolder>();
        if (holder != null && holder.HasObjectInHand())
        {
            var palo = holder.GetHeldObject()?.GetComponent<PaloIgnifugo>();
            paloEncendido = palo != null && palo.EstaEncendido();
        }

        var candidatos = new List<IInteractable>();
        InteractPriority mejorPrioridad = InteractPriority.VeryLow;
        var currentInRange = new HashSet<IInteractable>();

        for (int i = 0; i < elements; i++)
        {
            var col = interactables[i];
            if (col == null) continue;

            // Ignorar el objeto que está en la mano
            if (holder != null && holder.HasObjectInHand())
            {
                var heldObj = holder.GetHeldObject();
                if (heldObj != null && (col.gameObject == heldObj || col.transform.IsChildOf(heldObj.transform)))
                {
                    continue;
                }
            }

            var candidato = col.GetComponentInParent<IInteractable>();
            if (candidato == null) continue;

            currentInRange.Add(candidato);

            if (ignoredInteractables.Contains(candidato)) continue;

            var torch = col.GetComponentInParent<TorchInteractable>();
            var prioridadEfectiva = candidato.InteractPriority;
            if (paloEncendido && torch != null)
            {
                prioridadEfectiva = InteractPriority.VeryHigh;
            }

            if (prioridadEfectiva > mejorPrioridad)
            {
                mejorPrioridad = prioridadEfectiva;
                candidatos.Clear();
                candidatos.Add(candidato);
            }
            else if (prioridadEfectiva == mejorPrioridad)
            {
                candidatos.Add(candidato);
            }
        }

        if (candidatos.Count > 0)
        {
            ShowInteractionUI();

            if (interactAction.triggered || Input.GetKeyDown(interactKey))
            {
                var seleccionado = candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
                seleccionado.Interact(this.gameObject);

                ignoredInteractables.Add(seleccionado);
            }
        }
        else
        {
            HideInteractionUI();
        }

        // Remover de ignorados aquellos que salieron del rango
        foreach (var ignored in new List<IInteractable>(ignoredInteractables))
        {
            if (!currentInRange.Contains(ignored))
            {
                ignoredInteractables.Remove(ignored);
            }
        }
    }

    private void ShowInteractionUI()
    {
        // Mostrar la UI de interacción siempre que haya interactuables válidos cerca
        if (interactionUIImage != null && !interactionUIImage.gameObject.activeInHierarchy)
        {
            interactionUIImage.gameObject.SetActive(true);
        }
    }

    private void HideInteractionUI()
    {
        if (interactionUIImage != null && interactionUIImage.gameObject.activeInHierarchy)
        {
            interactionUIImage.gameObject.SetActive(false);
        }
    }

    // Mostrar/ocultar BuildUI (similar a InteractionUI)
    private void UpdateBuildUI()
    {
        if (buildUIImage == null || bridgeInteraction == null || objectHolder == null)
        {
            HideBuildUI();
            return;
        }

        bool hasMaterialInHand = objectHolder.HasObjectInHand() &&
                                 objectHolder.GetHeldObject() != null &&
                                 objectHolder.GetHeldObject().GetComponent<BridgeMaterialInfo>() != null;

        bool targetInRange = hasMaterialInHand && bridgeInteraction.HasTargetQuadrantInRange();

        if (targetInRange) ShowBuildUI();
        else HideBuildUI();
    }

    private void ShowBuildUI()
    {
        if (!buildUIImage.gameObject.activeInHierarchy)
            buildUIImage.gameObject.SetActive(true);
    }

    private void HideBuildUI()
    {
        if (buildUIImage.gameObject.activeInHierarchy)
            buildUIImage.gameObject.SetActive(false);
    }

    private void TryDropObject()
    {
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            objectHolder.DropObject();
            
            // Activar animación de drop
            if (playerAnimator != null)
            {
                playerAnimator.TriggerDropAnimation();
            }
        }
    }
    
    public void OnLaunched(Vector3 targetPosition)
    {
        // Cancelar dash si está activo durante un lanzamiento
        if (isDashing)
        {
            EndDash();
        }
        
        // Ya no soltamos el objeto al ser lanzados: el holder mantiene el objeto en la mano.
        // Por seguridad, si hay algo en la mano, reafirmamos su estado físico (kinematic + sin gravedad).
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            var obj = objectHolder.GetHeldObject();
            if (obj != null)
            {
                var rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
        
        // Visualizar dirección y distancia del dash
        if (isDashing && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + dashDirection * dashDistance);
        }
    }
}