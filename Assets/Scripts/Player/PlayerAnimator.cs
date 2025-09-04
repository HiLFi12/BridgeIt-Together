using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animation Components")]
    private Animator animator;
    
    [Header("Animation Parameters")]
    private readonly string SPEED_PARAMETER = "Speed";
    private readonly string IS_HOLDING_PARAMETER = "IsHolding";
    private readonly string IS_BUILDING_PARAMETER = "IsBuilding";
    private readonly string TRIGGER_BUILD_PARAMETER = "TriggerBuild";
    private readonly string TRIGGER_DROP_PARAMETER = "TriggerDrop";
    
    // Nuevos parámetros para fin de juego
    private readonly string TRIGGER_VICTORY_PARAMETER = "TriggerVictory";
    private readonly string TRIGGER_DEFEAT_PARAMETER = "TriggerDefeat";
    private readonly string IS_GAME_ENDED_PARAMETER = "IsGameEnded";
    
    // Parámetros de vuelo
    private readonly string IS_FLYING_PARAMETER = "IsFlying";
    private readonly string TRIGGER_FLYING_PARAMETER = "TriggerFlying";
    
    // Referencias a otros componentes
    private PlayerController playerController;
    private PlayerObjectHolder objectHolder;
    private Player player;
    
    // Variables para controlar estados
    private bool isBuilding = false;
    private float buildingTimer = 0f;
    private float buildingDuration = 1.5f; // Duración de la animación de construcción
    
    private bool isDropping = false;
    private float droppingTimer = 0f;
    private float droppingDuration = 1.0f; // Duración de la animación de drop
    
    // Variables para controlar estados de fin de juego
    private bool gameEnded = false;
    private bool inVictoryState = false;
    private bool inDefeatState = false;
    
    // Referencias adicionales para control de física
    private Rigidbody playerRigidbody;
    private CharacterController characterController;
    
    // Variables para guardar configuración original
    private bool originalApplyRootMotion;
    private bool originalRigidbodyKinematic;
    private bool originalControllerEnabled;
    
    // Debug
    private bool mostrarDebugInfo = true;
    
    // Estado de vuelo y chequeo de suelo
    private bool isFlying = false;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private LayerMask groundLayers = ~0; // por defecto, todo
    // Chequeo robusto y suavizado (histeresis)
    [SerializeField] private float groundCheckRadius = 0.18f;   // esfera pequeña para evitar "agujeros"
    [SerializeField] private float airborneMinTime = 0.12f;     // tiempo mínimo en aire para considerar “volando”
    [SerializeField] private float groundedMinTime = 0.06f;     // tiempo mínimo en suelo para salir de “volando”
    private float airborneTimer = 0f;
    private float groundedTimer = 0f;
    
    // Debug de chequeo de suelo y saneo de configuración
    [SerializeField] private bool debugGroundCheck = false;

    private void OnValidate()
    {
        if (groundCheckDistance < 0.3f) groundCheckDistance = 0.6f;
        if (groundCheckRadius < 0.08f) groundCheckRadius = 0.15f;
        // Si la máscara quedó en Nothing, usar todas las capas
        if (groundLayers.value == 0) groundLayers = Physics.AllLayers;
    }
    
    private void Start()
    {
        // Obtener componentes
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        objectHolder = GetComponent<PlayerObjectHolder>();
        player = GetComponent<Player>();
        playerRigidbody = GetComponent<Rigidbody>();
    characterController = GetComponent<CharacterController>();
        
        // Fallback del mask por si alguna instancia quedó en “Nothing”
        if (groundLayers.value == 0) groundLayers = Physics.AllLayers;
        
        // Guardar configuración original
        if (animator != null)
        {
            originalApplyRootMotion = animator.applyRootMotion;
        }
        
        if (playerRigidbody != null)
        {
            originalRigidbodyKinematic = playerRigidbody.isKinematic;
        }
        
        if (characterController != null)
        {
            originalControllerEnabled = characterController.enabled;
        }
        
        // Verificar que tenemos el Animator
        if (animator == null)
        {
            Debug.LogError($"No se encontró el componente Animator en {gameObject.name}. Asegúrate de agregarlo.");
            enabled = false;
            return;
        }
        
        // Verificar que el Animator tiene un Controller asignado
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"No hay un Animator Controller asignado en {gameObject.name}. Asigna el P1_AnimatorController.");
            enabled = false;
            return;
        }
        
        // Inicializar parámetros del animator
        InitializeAnimatorParameters();
        
        Debug.Log($"PlayerAnimator inicializado correctamente en {gameObject.name}");
    }
    
    private void Update()
    {
        UpdateAnimationStates();
        UpdateBuildingState();
        UpdateDroppingState();
    }
    
    /// <summary>
    /// Inicializa los parámetros del animator si no existen
    /// </summary>
    private void InitializeAnimatorParameters()
    {
        // Verificar y agregar parámetros si no existen
        if (!HasParameter(SPEED_PARAMETER))
            Debug.LogWarning($"Parámetro '{SPEED_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(IS_HOLDING_PARAMETER))
            Debug.LogWarning($"Parámetro '{IS_HOLDING_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(IS_BUILDING_PARAMETER))
            Debug.LogWarning($"Parámetro '{IS_BUILDING_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(TRIGGER_BUILD_PARAMETER))
            Debug.LogWarning($"Parámetro '{TRIGGER_BUILD_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(TRIGGER_DROP_PARAMETER))
            Debug.LogWarning($"Parámetro '{TRIGGER_DROP_PARAMETER}' no encontrado en el Animator Controller");
        
        // Parámetros de vuelo
        if (!HasParameter(IS_FLYING_PARAMETER))
            Debug.LogWarning($"Parámetro '{IS_FLYING_PARAMETER}' no encontrado en el Animator Controller");
        if (!HasParameter(TRIGGER_FLYING_PARAMETER))
            Debug.LogWarning($"Parámetro '{TRIGGER_FLYING_PARAMETER}' no encontrado en el Animator Controller");
            
        // Verificar nuevos parámetros de fin de juego
        if (!HasParameter(TRIGGER_VICTORY_PARAMETER))
            Debug.LogWarning($"Parámetro '{TRIGGER_VICTORY_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(TRIGGER_DEFEAT_PARAMETER))
            Debug.LogWarning($"Parámetro '{TRIGGER_DEFEAT_PARAMETER}' no encontrado en el Animator Controller");
            
        if (!HasParameter(IS_GAME_ENDED_PARAMETER))
            Debug.LogWarning($"Parámetro '{IS_GAME_ENDED_PARAMETER}' no encontrado en el Animator Controller");
    }
    
    /// <summary>
    /// Actualiza los estados de animación basados en el estado actual del jugador
    /// </summary>
    private void UpdateAnimationStates()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        // Si el juego ha terminado, no actualizar animaciones normales
        if (gameEnded)
        {
            // Mantener la velocidad en 0 para que no se reproduzcan animaciones de movimiento
            if (HasParameter(SPEED_PARAMETER))
            {
                animator.SetFloat(SPEED_PARAMETER, 0f);
            }
            // Forzar no volando en fin de juego
            if (HasParameter(IS_FLYING_PARAMETER))
            {
                animator.SetBool(IS_FLYING_PARAMETER, false);
            }
            return;
        }
        
        // Suavizado de estado de vuelo con histéresis
        bool groundDetected = IsGroundedSimple();
        if (groundDetected)
        {
            groundedTimer += Time.deltaTime;
            airborneTimer = 0f;
        }
        else
        {
            airborneTimer += Time.deltaTime;
            groundedTimer = 0f;
        }

        bool nextIsFlying = isFlying;
        // Entrar a vuelo solo si llevamos suficiente tiempo sin suelo
        if (!groundDetected && !isFlying && airborneTimer >= airborneMinTime)
            nextIsFlying = true;
        // Salir de vuelo solo si llevamos suficiente tiempo en suelo
        if (groundDetected && isFlying && groundedTimer >= groundedMinTime)
            nextIsFlying = false;

        if (nextIsFlying != isFlying)
        {
            if (nextIsFlying && HasParameter(TRIGGER_FLYING_PARAMETER))
            {
                animator.SetTrigger(TRIGGER_FLYING_PARAMETER);
                if (mostrarDebugInfo) Debug.Log($"✈️ {gameObject.name} entró en estado Flying");
            }
            if (HasParameter(IS_FLYING_PARAMETER))
            {
                animator.SetBool(IS_FLYING_PARAMETER, nextIsFlying);
            }
            isFlying = nextIsFlying;
        }
        else
        {
            // Mantener sincronizado el bool
            if (HasParameter(IS_FLYING_PARAMETER))
                animator.SetBool(IS_FLYING_PARAMETER, isFlying);
        }
        
        // Calcular velocidad de movimiento
        float currentSpeed = CalculateMovementSpeed();
        
        // Solo actualizar si el parámetro existe
        if (HasParameter(SPEED_PARAMETER))
        {
            animator.SetFloat(SPEED_PARAMETER, currentSpeed);
        }
        
        // Estado de sosteniendo objeto
        bool holdingObject = objectHolder != null && objectHolder.HasObjectInHand();
        if (HasParameter(IS_HOLDING_PARAMETER))
        {
            animator.SetBool(IS_HOLDING_PARAMETER, holdingObject);
        }
        
        // Estado de construcción (solo si no está en movimiento)
        if (HasParameter(IS_BUILDING_PARAMETER))
        {
            animator.SetBool(IS_BUILDING_PARAMETER, isBuilding && currentSpeed < 0.1f);
        }
        
        // Log para debugging (remover después de verificar que funciona)
        if (holdingObject && currentSpeed > 0.1f)
        {
            Debug.Log($"Player {gameObject.name}: Running while holding - Speed: {currentSpeed:F2}, Holding: {holdingObject}");
        }
    }
    
    /// <summary>
    /// Calcula la velocidad de movimiento actual del jugador
    /// </summary>
    private float CalculateMovementSpeed()
    {
        if (playerController == null)
        {
            // Fallback: usar Input directo si no hay PlayerController disponible
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            return new Vector2(horizontal, vertical).magnitude;
        }
        
        // Usar los datos del PlayerController
        return playerController.MovementInput.magnitude;
    }
    
    /// <summary>
    /// Actualiza el estado de construcción
    /// </summary>
    private void UpdateBuildingState()
    {
        if (isBuilding)
        {
            buildingTimer += Time.deltaTime;
            if (buildingTimer >= buildingDuration)
            {
                isBuilding = false;
                buildingTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Actualiza el estado de drop
    /// </summary>
    private void UpdateDroppingState()
    {
        if (isDropping)
        {
            droppingTimer += Time.deltaTime;
            if (droppingTimer >= droppingDuration)
            {
                isDropping = false;
                droppingTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Activa la animación de construcción
    /// </summary>
    public void TriggerBuildAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            Debug.LogWarning("No se puede activar animación de construcción: Animator no configurado");
            return;
        }
        
        if (HasParameter(TRIGGER_BUILD_PARAMETER))
        {
            animator.SetTrigger(TRIGGER_BUILD_PARAMETER);
            isBuilding = true;
            buildingTimer = 0f;
            Debug.Log("Animación de construcción activada");
        }
        else
        {
            Debug.LogWarning($"Parámetro {TRIGGER_BUILD_PARAMETER} no encontrado en el Animator Controller");
        }
    }
    
    /// <summary>
    /// Activa la animación de drop/soltar objeto
    /// </summary>
    public void TriggerDropAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            Debug.LogWarning("No se puede activar animación de drop: Animator no configurado");
            return;
        }
        
        if (HasParameter(TRIGGER_DROP_PARAMETER))
        {
            animator.SetTrigger(TRIGGER_DROP_PARAMETER);
            isDropping = true;
            droppingTimer = 0f;
            Debug.Log("Animación de drop activada");
        }
        else
        {
            Debug.LogWarning($"Parámetro {TRIGGER_DROP_PARAMETER} no encontrado en el Animator Controller");
        }
    }
    
    /// <summary>
    /// Activa la animación de victoria y bloquea otras animaciones
    /// </summary>
    public void TriggerVictoryAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            Debug.LogWarning("No se puede activar animación de victoria: Animator no configurado");
            return;
        }
        
        // Marcar el fin del juego
        gameEnded = true;
        inVictoryState = true;
        inDefeatState = false;
        
        // Configurar física para animaciones de fin de juego
        ConfigurarFisicaParaFinDeJuego();
        
        // Ajustar posición en el suelo
        AjustarPosicionEnSuelo();
        
        // Activar el estado de fin de juego
        if (HasParameter(IS_GAME_ENDED_PARAMETER))
        {
            animator.SetBool(IS_GAME_ENDED_PARAMETER, true);
        }
        
        // Disparar el trigger de victoria
        if (HasParameter(TRIGGER_VICTORY_PARAMETER))
        {
            animator.SetTrigger(TRIGGER_VICTORY_PARAMETER);
            Debug.Log($"🎉 Animación de victoria activada en {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Parámetro {TRIGGER_VICTORY_PARAMETER} no encontrado en el Animator Controller");
        }
        
        // Asegurar que la velocidad esté en 0
        if (HasParameter(SPEED_PARAMETER))
        {
            animator.SetFloat(SPEED_PARAMETER, 0f);
        }
        
        // Configurar física para fin de juego
        ConfigurarFisicaParaFinDeJuego();
    }
    
    /// <summary>
    /// Activa la animación de derrota y bloquea otras animaciones
    /// </summary>
    public void TriggerDefeatAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            Debug.LogWarning("No se puede activar animación de derrota: Animator no configurado");
            return;
        }
        
        // Marcar el fin del juego
        gameEnded = true;
        inVictoryState = false;
        inDefeatState = true;
        
        // Configurar física para animaciones de fin de juego
        ConfigurarFisicaParaFinDeJuego();
        
        // Ajustar posición en el suelo
        AjustarPosicionEnSuelo();
        
        // Activar el estado de fin de juego
        if (HasParameter(IS_GAME_ENDED_PARAMETER))
        {
            animator.SetBool(IS_GAME_ENDED_PARAMETER, true);
        }
        
        // Disparar el trigger de derrota
        if (HasParameter(TRIGGER_DEFEAT_PARAMETER))
        {
            animator.SetTrigger(TRIGGER_DEFEAT_PARAMETER);
            Debug.Log($"💀 Animación de derrota activada en {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Parámetro {TRIGGER_DEFEAT_PARAMETER} no encontrado en el Animator Controller");
        }
        
        // Asegurar que la velocidad esté en 0
        if (HasParameter(SPEED_PARAMETER))
        {
            animator.SetFloat(SPEED_PARAMETER, 0f);
        }
        
        // Configurar física para fin de juego
        ConfigurarFisicaParaFinDeJuego();
    }
    
    /// <summary>
    /// Reinicia el estado de animación cuando se reinicia el juego
    /// </summary>
    public void ResetToGameplayState()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            Debug.LogWarning("No se puede reiniciar estado de animación: Animator no configurado");
            return;
        }
        
        // Resetear estados de fin de juego
        gameEnded = false;
        inVictoryState = false;
        inDefeatState = false;
        
        // Desactivar el estado de fin de juego
        if (HasParameter(IS_GAME_ENDED_PARAMETER))
        {
            animator.SetBool(IS_GAME_ENDED_PARAMETER, false);
        }
        
        // Resetear velocidad
        if (HasParameter(SPEED_PARAMETER))
        {
            animator.SetFloat(SPEED_PARAMETER, 0f);
        }
        
        // Restaurar configuración original de física
        RestaurarFisicaOriginal();
        
        Debug.Log($"🔄 Estado de animación reiniciado en {gameObject.name}");
    }
    
    /// <summary>
    /// Verifica si un parámetro existe en el Animator Controller
    /// </summary>
    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Método público para forzar un estado específico (útil para testing)
    /// </summary>
    public void SetAnimationState(string parameterName, bool value)
    {
        if (HasParameter(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
    }
    
    /// <summary>
    /// Método público para forzar un valor float específico
    /// </summary>
    public void SetAnimationFloat(string parameterName, float value)
    {
        if (HasParameter(parameterName))
        {
            animator.SetFloat(parameterName, value);
        }
    }
    
    /// <summary>
    /// Obtiene si el juego ha terminado según el estado de animación
    /// </summary>
    public bool IsGameEnded()
    {
        return gameEnded;
    }
    
    /// <summary>
    /// Obtiene si está en estado de victoria
    /// </summary>
    public bool IsInVictoryState()
    {
        return inVictoryState;
    }
    
    /// <summary>
    /// Obtiene si está en estado de derrota
    /// </summary>
    public bool IsInDefeatState()
    {
        return inDefeatState;
    }
    
    /// <summary>
    /// Fuerza un estado específico (para testing o casos especiales)
    /// </summary>
    public void ForceEndGameState(bool victory)
    {
        if (victory)
        {
            TriggerVictoryAnimation();
        }
        else
        {
            TriggerDefeatAnimation();
        }
    }
    
    /// <summary>
    /// Configura la física para animaciones de fin de juego
    /// </summary>
    private void ConfigurarFisicaParaFinDeJuego()
    {
        // Habilitar Root Motion para que las animaciones muevan el personaje
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }
        
        // Si hay Rigidbody, hacerlo kinematic para que el Animator controle el movimiento
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
        
        // Mantener CharacterController activo pero sin interferir
        if (characterController != null)
        {
            // El CharacterController puede quedarse activo para detectar colisiones
            // pero no controlará el movimiento
        }
        
        Debug.Log($"⚙️ Física configurada para fin de juego en {gameObject.name}");
    }
    
    /// <summary>
    /// Restaura la configuración original de física
    /// </summary>
    private void RestaurarFisicaOriginal()
    {
        // Restaurar Root Motion
        if (animator != null)
        {
            animator.applyRootMotion = originalApplyRootMotion;
        }
        
        // Restaurar Rigidbody
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = originalRigidbodyKinematic;
        }
        
        // Restaurar CharacterController
        if (characterController != null)
        {
            characterController.enabled = originalControllerEnabled;
        }
        
        Debug.Log($"⚙️ Física restaurada a configuración original en {gameObject.name}");
    }
    
    /// <summary>
    /// Ajusta la posición del personaje para evitar que se hunda en el suelo
    /// </summary>
    private void AjustarPosicionEnSuelo()
    {
        // Usar raycast para detectar el suelo
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f))
        {
            // Ajustar posición para que esté sobre el suelo
            Vector3 nuevaPosicion = hit.point;
            transform.position = nuevaPosicion;
            
            Debug.Log($"🎯 Posición ajustada al suelo en {gameObject.name}: {nuevaPosicion}");
        }
    }
    
    /// <summary>
    /// Chequeo robusto de suelo: CharacterController o SphereCast (sin dependencias externas)
    /// </summary>
    private bool IsGroundedSimple()
    {
        // 1) CharacterController si existe
        if (characterController != null && characterController.enabled && characterController.isGrounded)
            return true;

        // 2) SphereCast de respaldo (mejor que Raycast para bordes/escalones)
        // Origen cerca de los pies del CharacterController si existe
        Vector3 origin = transform.position + Vector3.up * 0.2f; // leve offset por defecto
        if (characterController != null)
        {
            float feetOffset = characterController.center.y - (characterController.height * 0.5f)
                               + characterController.radius + 0.05f;
            origin = transform.position + Vector3.up * Mathf.Max(0.05f, feetOffset);
        }

        int mask = groundLayers.value == 0 ? Physics.AllLayers : groundLayers.value;

        bool hit = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out var hitInfo,
            groundCheckDistance,
            mask,
            QueryTriggerInteraction.Ignore
        );

        if (debugGroundCheck)
        {
            Debug.DrawLine(origin, origin + Vector3.down * groundCheckDistance, hit ? Color.green : Color.red, 0f, false);
            if (hit) Debug.DrawRay(hitInfo.point, hitInfo.normal * 0.3f, Color.cyan, 0f, false);
        }

        return hit;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugGroundCheck) return;
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        if (characterController != null)
        {
            float feetOffset = characterController.center.y - (characterController.height * 0.5f)
                               + characterController.radius + 0.05f;
            origin = transform.position + Vector3.up * Mathf.Max(0.05f, feetOffset);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
#endif
}
