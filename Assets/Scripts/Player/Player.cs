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
    [SerializeField] private KeyCode dropKey = KeyCode.Q; // Tecla dedicada para soltar objeto
    
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
    
    [Header("Audio - Dash (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al iniciar el dash. -1 desactiva.")]
    [SerializeField] private int dashSfxIndex = -1;
    
    [Header("Interaction UI")]
    [SerializeField] private Image interactionKeyUI;
    [SerializeField] private Image interactionPadUI;

    [Header("Build UI")]
    [SerializeField] private Image buildKeyUI;
    [SerializeField] private Image buildPadUI;

    [Header("Dash UI")]
    [SerializeField] private Image dashKeyUI;
    [SerializeField] private Image dashPadUI;

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject respawnEffectPrefab;
    [SerializeField] private float manualRespawnCooldown = 3f;
    [SerializeField] private float shakeIntensityMax = 0.5f;
    [SerializeField] private float shakeFrequency = 20f;
    [SerializeField] private KeyCode manualRespawnKey = KeyCode.F;

    private bool usePadUI = false;
    private Image CurrentInteractionUI => usePadUI ? interactionPadUI : interactionKeyUI;
    private Image CurrentBuildUI => usePadUI ? buildPadUI : buildKeyUI;
    private Image CurrentDashUI => usePadUI ? dashPadUI : dashKeyUI;

    // Exponer referencias de UI para PlayerUIManager
    public Image InteractionKeyUI => interactionKeyUI;
    public Image InteractionPadUI => interactionPadUI;
    public Image BuildKeyUI => buildKeyUI;
    public Image BuildPadUI => buildPadUI;

    private GameConditionManager gameConditionManager;
    private PlayerObjectHolder objectHolder;
    private PlayerBridgeInteraction bridgeInteraction;
    private PlayerAnimator playerAnimator;
    private CharacterController characterController;
    private PlayerController playerController;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction dropAction;
    
    private InputAction dashAction;
    
    private InputAction pauseAction;
    private InputAction buildAction;

    // Dash state
    private bool isDashing = false;
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private float dashTimer = 0f;
    
    // Manual Respawn state
    private float manualRespawnTimer = 0f;
    private bool isHoldingRespawn = false;
    private Vector3 originalPosition;
    private bool isShaking = false;
    
    public PlayerInput PlayerInput => playerInput;
    public PlayerController PlayerController => playerController;

    public delegate void PlayerInteractedHandler();
    public event PlayerInteractedHandler OnPlayerInteracted;

    private HashSet<IInteractable> ignoredInteractables = new HashSet<IInteractable>();
    private HashSet<IInteractable> activeShadows = new HashSet<IInteractable>(); // Rastrea qué sombras están activas

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
            dropAction = playerInput.actions.FindAction("Drop");
            dashAction = playerInput.actions.FindAction("Dash");
            pauseAction = playerInput.actions.FindAction("Pause");
            buildAction = playerInput.actions.FindAction("Build");
        }
        
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

        // Mostrar/ocultar UI del dash según estado del cooldown
        if (!canDash && CurrentDashUI != null)
        {
            // Mostrar UI y actualizar progreso durante cooldown
            if (!CurrentDashUI.gameObject.activeInHierarchy)
                CurrentDashUI.gameObject.SetActive(true);

            float fill = 0f;
            if (dashCooldown > 0f)
            {
                fill = 1f - Mathf.Clamp01(dashCooldownTimer / dashCooldown);
            }
            CurrentDashUI.fillAmount = fill;
        }
        else
        {
            // Ocultar ambas UIs cuando el dash está listo
            if (dashKeyUI != null && dashKeyUI.gameObject.activeInHierarchy)
            {
                dashKeyUI.gameObject.SetActive(false);
                dashKeyUI.fillAmount = 0f;
            }
            if (dashPadUI != null && dashPadUI.gameObject.activeInHierarchy)
            {
                dashPadUI.gameObject.SetActive(false);
                dashPadUI.fillAmount = 0f;
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

        // Sistema de respawn manual
        UpdateManualRespawn();

        TryInteract();

        if (pauseAction.triggered)
        {
            gameConditionManager.PauseGame();
        }

        // Nota: Build ahora se maneja dentro de TryInteract() como fallback del botón Interact

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
        
        if (dashAction != null && dashAction.triggered)
        {
            Debug.Log($"Dash triggered por {gameObject.name} time={Time.time}");
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
        
        // SFX de dash
        PlayDashSfx();
        
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
        if(characterController == null){
            return;
        }
        
        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer, QueryTriggerInteraction.Collide);

        if (elements == 0) 
        {
            HideInteractionUI();
            ignoredInteractables.Clear();
            
            // Desactivar todas las sombras activas
            foreach (var shadowActive in new List<IInteractable>(activeShadows))
            {
                ActivarSombra(shadowActive, false);
            }

            // Si no hay interactuables cerca y se pulsa Interact, intentar construir
            if (bridgeInteraction != null && objectHolder != null &&
                (interactAction.triggered || Input.GetKeyDown(interactKey)))
            {
                bool hasObject = objectHolder.HasObjectInHand();
                bool targetInRange = bridgeInteraction.HasTargetQuadrantInRange();
                if (hasObject && targetInRange)
                {
                    bridgeInteraction.TryInteractWithQuadrant();
                    if (playerAnimator != null)
                    {
                        playerAnimator.TriggerBuildAnimation();
                    }
                }
            }
            
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

            // Ignorar el objeto que está en la mano de este jugador
            if (holder != null && holder.HasObjectInHand())
            {
                var heldObj = holder.GetHeldObject();
                if (heldObj != null && (col.gameObject == heldObj || col.transform.IsChildOf(heldObj.transform)))
                {
                    continue;
                }
            }

            if (IsObjectHeldByAnyPlayer(col.gameObject))
            {
                continue;
            }

            var candidato = col.GetComponentInParent<IInteractable>();
            if (candidato == null) continue;

            currentInRange.Add(candidato);

            // Ballista y Catapult nunca deben ser ignorados
            bool isNeverIgnored = candidato is Ballista || candidato is Catapult;
            if (!isNeverIgnored && ignoredInteractables.Contains(candidato)) continue;

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
            
            // Activar sombra de los interactables candidatos
            foreach (var candidato in candidatos)
            {
                ActivarSombra(candidato, true);
            }
            
            if (interactAction.triggered || Input.GetKeyDown(interactKey))
            {
                var seleccionado = candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
                // Si ya tengo un objeto en mano y el seleccionado es un pickup/generador, NO hacer nada
                if (holder != null && holder.HasObjectInHand() && IsPickupLike(seleccionado))
                {
                    // Bloquea la acción para evitar spam de generación y dropeos implícitos
                    Debug.Log("[Player] Interacción bloqueada: ya sostienes un objeto y el objetivo es un pickup/generador.");
                }
                else
                {
                    seleccionado.Interact(this.gameObject);

                    // Ballista y Catapult nunca deben ser ignorados
                    if (!(seleccionado is Ballista) && !(seleccionado is Catapult))
                    {
                        ignoredInteractables.Add(seleccionado);
                    }
                    OnPlayerInteracted?.Invoke();
                }
            }
        }
        else
        {
            HideInteractionUI();
            
            // Sin interactuables válidos pero con algo cerca (colliders que no implementan IInteractable):
            // permitir que el botón de Interact intente construir igualmente.
            if (bridgeInteraction != null && objectHolder != null &&
                (interactAction.triggered || Input.GetKeyDown(interactKey)))
            {
                bool hasObject = objectHolder.HasObjectInHand();
                bool targetInRange = bridgeInteraction.HasTargetQuadrantInRange();
                if (hasObject && targetInRange)
                {
                    bridgeInteraction.TryInteractWithQuadrant();
                    if (playerAnimator != null)
                    {
                        playerAnimator.TriggerBuildAnimation();
                    }
                }
            }
        }

        // Remover de ignorados aquellos que salieron del rango
        foreach (var ignored in new List<IInteractable>(ignoredInteractables))
        {
            if (!currentInRange.Contains(ignored))
            {
                ignoredInteractables.Remove(ignored);
            }
        }
        
        // Desactivar sombras de interactables que ya no están en la lista de candidatos
        foreach (var shadowActive in new List<IInteractable>(activeShadows))
        {
            if (!candidatos.Contains(shadowActive))
            {
                ActivarSombra(shadowActive, false);
            }
        }

        // Input dedicado para dropear (separado de Interact)
        if ((dropAction != null && dropAction.triggered) || Input.GetKeyDown(dropKey))
        {
            if (objectHolder != null && objectHolder.HasObjectInHand())
            {
                TryDropObject();
            }
        }
    }

    /// <summary>
    /// Busca y activa/desactiva automáticamente el GameObject 'shadow' en un interactable
    /// </summary>
    private void ActivarSombra(IInteractable interactable, bool activar)
    {
        if (interactable == null) return;
        
        var comp = interactable as Component;
        if (comp == null) return;
        
        // Buscar el campo 'shadow' en el componente
        var tipo = comp.GetType();
        var campoShadow = tipo.GetField("shadow", System.Reflection.BindingFlags.Instance | 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Public);
        
        if (campoShadow != null && campoShadow.FieldType == typeof(GameObject))
        {
            var shadowObj = campoShadow.GetValue(comp) as GameObject;
            if (shadowObj != null)
            {
                shadowObj.SetActive(activar);
                
                // Rastrea el estado de las sombras activas
                if (activar)
                {
                    activeShadows.Add(interactable);
                }
                else
                {
                    activeShadows.Remove(interactable);
                }
            }
        }
    }

    // Determina si la interacción corresponde a un pickup/generador de materiales (bloqueable si ya hay objeto en mano)
    private bool IsPickupLike(IInteractable interactable)
    {
        if (interactable == null) return false;
        var comp = interactable as Component;
        if (comp == null) return false;
        // Detectar explícitamente material pickups conocidos
        if (comp.GetComponentInParent<BridgeMaterialPickup>() != null) return true;
        // Heurística por nombre de tipo para otros generadores/spawners
        var behaviours = comp.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            var tname = behaviours[i].GetType().Name;
            if (string.IsNullOrEmpty(tname)) continue;
            string lname = tname.ToLowerInvariant();
            if (lname.Contains("pickup") || lname.Contains("generator") || lname.Contains("spawner") || lname.Contains("spawn"))
                return true;
        }
        return false;
    }

    private void ShowInteractionUI()
    {
        if (CurrentInteractionUI != null && !CurrentInteractionUI.gameObject.activeInHierarchy)
        {
            CurrentInteractionUI.gameObject.SetActive(true);
        }
    }

    private void HideInteractionUI()
    {
        if (CurrentInteractionUI != null && CurrentInteractionUI.gameObject.activeInHierarchy)
        {
            CurrentInteractionUI.gameObject.SetActive(false);
        }
    }

    // Mostrar/ocultar BuildUI (similar a InteractionUI)
    private void UpdateBuildUI()
    {
        if (CurrentBuildUI == null || bridgeInteraction == null || objectHolder == null)
        {
            HideBuildUI();
            return;
        }

        bool hasMaterialInHand = objectHolder.HasObjectInHand() &&
                                 objectHolder.GetHeldObject() != null &&
                                 objectHolder.GetHeldObject().GetComponent<BridgeMaterialInfo>() != null;

        // Solo mostrar UI si hay posibilidad real de construir o reparar
        bool canBuildOrRepair = hasMaterialInHand && bridgeInteraction.CanBuildOrRepairInRange();

        if (canBuildOrRepair) ShowBuildUI();
        else HideBuildUI();
    }

    private void ShowBuildUI()
    {
        if (CurrentBuildUI != null && !CurrentBuildUI.gameObject.activeInHierarchy)
            CurrentBuildUI.gameObject.SetActive(true);
    }

    private void HideBuildUI()
    {
        if (CurrentBuildUI != null && CurrentBuildUI.gameObject.activeInHierarchy)
            CurrentBuildUI.gameObject.SetActive(false);
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
        
        // Resetear el timer de respawn manual al ser lanzado
        manualRespawnTimer = 0f;
        isHoldingRespawn = false;
        if (isShaking)
        {
            // Reactivar CharacterController si estaba desactivado
            if (characterController != null && !characterController.enabled)
            {
                characterController.enabled = true;
            }
            isShaking = false;
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

    public void SetUIType(bool usePad)
    {
        usePadUI = usePad;
        // Ocultar todas y mostrar la correcta
        if (interactionKeyUI != null) interactionKeyUI.gameObject.SetActive(!usePad);
        if (interactionPadUI != null) interactionPadUI.gameObject.SetActive(usePad);
        if (buildKeyUI != null) buildKeyUI.gameObject.SetActive(!usePad);
        if (buildPadUI != null) buildPadUI.gameObject.SetActive(usePad);

        // Dash UI
        if (dashKeyUI != null) dashKeyUI.gameObject.SetActive(!usePad);
        if (dashPadUI != null) dashPadUI.gameObject.SetActive(usePad);
    }

    private void PlayDashSfx()
    {
        if (dashSfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(dashSfxIndex);
        }
    }

    // Respawn básico para compatibilidad con RespawnTrigger
    public void Respawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("[Player] No se puede hacer respawn: respawnPoint no está asignado.");
            return;
        }

        // Guardar posición actual para el efecto de desaparición
        Vector3 currentPosition = transform.position;

        // Instanciar efecto en la posición donde el player desaparece
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, currentPosition, Quaternion.identity);
        }

        // Crear un nuevo Vector3 con la posición del respawn point
        Vector3 posicionDestino = new Vector3(respawnPoint.position.x, respawnPoint.position.y, respawnPoint.position.z);

        // Verificar si tiene CharacterController (requiere tratamiento especial)
        if (characterController != null)
        {
            // Desactivar temporalmente para poder mover
            characterController.enabled = false;
            transform.position = posicionDestino;
            characterController.enabled = true;
            Debug.Log($"[Player] Respawneado (con CharacterController) de {currentPosition} a {transform.position}");
        }
        else
        {
            // Teletransportar al respawn point usando new Vector3
            transform.position = posicionDestino;
            Debug.Log($"[Player] Respawneado de {currentPosition} a {transform.position}");
        }

        // Si tiene Rigidbody, resetear velocidad
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        // Instanciar efecto en el respawn point
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, respawnPoint.position, Quaternion.identity);
        }
        
        // Resetear el timer de respawn manual
        manualRespawnTimer = 0f;
        isHoldingRespawn = false;
        isShaking = false;
    }

    /// <summary>
    /// Actualiza el sistema de respawn manual que se activa manteniendo presionado el botón Build.
    /// </summary>
    private void UpdateManualRespawn()
    {
        // Verificar si el botón está siendo presionado (InputAction o KeyCode)
        bool isPressed = false;
        
        if (buildAction != null)
        {
            isPressed = buildAction.ReadValue<float>() > 0.1f;
        }
        
        // Fallback a KeyCode si no hay InputAction o no está presionado
        if (!isPressed)
        {
            isPressed = Input.GetKey(manualRespawnKey);
        }

        if (isPressed)
        {
            if (!isHoldingRespawn)
            {
                // Comenzar a mantener presionado
                isHoldingRespawn = true;
                manualRespawnTimer = manualRespawnCooldown;
                isShaking = false;
                Debug.Log("[Player] Iniciando cooldown de respawn manual");
            }

            // Guardar la posición base antes de aplicar el temblor (solo la primera vez)
            if (!isShaking)
            {
                originalPosition = transform.position;
                isShaking = true;
                
                // Desactivar CharacterController al inicio del shake
                if (characterController != null)
                {
                    characterController.enabled = false;
                }
            }

            // Reducir el timer
            manualRespawnTimer -= Time.deltaTime;

            // Aplicar temblor según el progreso
            float progress = 1f - Mathf.Clamp01(manualRespawnTimer / manualRespawnCooldown);
            ApplyShake(progress);

            // Cuando el timer llega a 0, ejecutar respawn
            if (manualRespawnTimer <= 0f)
            {
                // Reactivar CharacterController antes del respawn
                if (characterController != null && !characterController.enabled)
                {
                    characterController.enabled = true;
                }
                
                Debug.Log($"[Player] {gameObject.name} - Cooldown de respawn manual completado, ejecutando respawn");
                Respawn();
                // El método Respawn() ya resetea las variables
            }
        }
        else
        {
            if (isHoldingRespawn)
            {
                Debug.Log($"[Player] {gameObject.name} - Respawn manual cancelado, reseteando cooldown");
                manualRespawnTimer = 0f;
                isHoldingRespawn = false;
                
                // Detener el temblor y restaurar posición
                if (isShaking)
                {
                    // Reactivar CharacterController si estaba desactivado
                    if (characterController != null && !characterController.enabled)
                    {
                        characterController.enabled = true;
                    }
                    
                    transform.position = originalPosition;
                    isShaking = false;
                }
            }
        }
    }

    /// <summary>
    /// Aplica un efecto de temblor al jugador basado en el progreso del respawn manual.
    /// El temblor se aplica como un offset desde la posición original sin mover al jugador.
    /// </summary>
    /// <param name="progress">Progreso de 0 a 1, donde 1 es máximo temblor</param>
    private void ApplyShake(float progress)
    {
        // Calcular intensidad del temblor basado en el progreso
        float intensity = Mathf.Lerp(0f, shakeIntensityMax, progress);

        // Generar offset aleatorio usando ruido de Perlin para un movimiento más suave
        float time = Time.time * shakeFrequency;
        float offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * intensity;
        float offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * intensity;
        float offsetZ = (Mathf.PerlinNoise(time, time + 100f) - 0.5f) * 2f * intensity;

        // Aplicar el temblor como offset desde la posición original
        Vector3 shakeOffset = new Vector3(offsetX, offsetY, offsetZ);
        
        // Aplicar directamente el offset a la posición
        // UpdateManualRespawn ya desactivó el CharacterController si es necesario
        transform.position = originalPosition + shakeOffset;
    }

    /// <summary>
    /// Verifica si un objeto está siendo sostenido por cualquier jugador en la escena.
    /// </summary>
    private bool IsObjectHeldByAnyPlayer(GameObject obj)
    {
        if (obj == null) return false;

        // Buscar todos los PlayerObjectHolder en la escena
        PlayerObjectHolder[] allHolders = FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
        
        foreach (var holder in allHolders)
        {
            if (holder != null && holder.HasObjectInHand())
            {
                var heldObj = holder.GetHeldObject();
                if (heldObj != null && (obj == heldObj || obj.transform.IsChildOf(heldObj.transform)))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}
