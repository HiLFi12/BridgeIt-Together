using UnityEngine;

/// <summary>
/// Maneja el Root Motion durante las animaciones de fin de juego para evitar problemas de física
/// Este script debe ser agregado al mismo GameObject que tiene el PlayerAnimator
/// </summary>
[RequireComponent(typeof(Animator))]
public class EndGameRootMotionHandler : MonoBehaviour
{
    [Header("Configuración de Root Motion")]
    [SerializeField] private bool enableRootMotionDuringEndGame = true;
    [SerializeField] private bool maintainGroundContact = true;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Capa del suelo
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGroundRay = true;
    
    // Referencias
    private Animator animator;
    private PlayerAnimator playerAnimator;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;
    
    // Estado
    private bool isInEndGameAnimation = false;
    private Vector3 lastValidGroundPosition;
    
    #region Unity Events
    
    private void Start()
    {
        // Obtener componentes
        animator = GetComponent<Animator>();
        playerAnimator = GetComponent<PlayerAnimator>();
        characterController = GetComponent<CharacterController>();
        playerRigidbody = GetComponent<Rigidbody>();
        
        // Verificar componentes requeridos
        if (animator == null)
        {
            Debug.LogError("[EndGameRootMotionHandler] No se encontró Animator en el GameObject");
            enabled = false;
            return;
        }
        
        if (playerAnimator == null)
        {
            Debug.LogWarning("[EndGameRootMotionHandler] No se encontró PlayerAnimator. Algunas funciones pueden no estar disponibles.");
        }
        
        // Guardar posición inicial válida
        lastValidGroundPosition = transform.position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[EndGameRootMotionHandler] Inicializado en {gameObject.name}");
        }
    }
    
    private void Update()
    {
        // Verificar si estamos en animación de fin de juego
        UpdateEndGameState();
        
        // Manejar contacto con el suelo si es necesario
        if (isInEndGameAnimation && maintainGroundContact)
        {
            HandleGroundContact();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showGroundRay && Application.isPlaying)
        {
            // Dibujar ray de detección de suelo
            Gizmos.color = Color.red;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawRay(rayStart, Vector3.down * groundCheckDistance);
            
            // Dibujar última posición válida
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastValidGroundPosition, 0.2f);
        }
    }
    
    #endregion
    
    #region Root Motion Handling
    
    /// <summary>
    /// Unity callback para manejar Root Motion
    /// </summary>
    private void OnAnimatorMove()
    {
        if (!enableRootMotionDuringEndGame || !isInEndGameAnimation)
        {
            return;
        }
        
        // Aplicar Root Motion solo si estamos en animación de fin de juego
        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;
        
        // Aplicar movimiento usando el método más apropiado según los componentes disponibles
        ApplyRootMotion(deltaPosition, deltaRotation);
    }
    
    /// <summary>
    /// Aplica el Root Motion usando el método más apropiado
    /// </summary>
    private void ApplyRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        if (characterController != null && characterController.enabled)
        {
            // Usar CharacterController para mover
            characterController.Move(deltaPosition);
            transform.rotation = deltaRotation * transform.rotation;
            
            if (showDebugInfo)
            {
                Debug.Log($"[EndGameRootMotion] Movimiento aplicado via CharacterController: {deltaPosition}");
            }
        }
        else if (playerRigidbody != null)
        {
            // Usar Rigidbody para mover
            playerRigidbody.MovePosition(transform.position + deltaPosition);
            playerRigidbody.MoveRotation(deltaRotation * transform.rotation);
            
            if (showDebugInfo)
            {
                Debug.Log($"[EndGameRootMotion] Movimiento aplicado via Rigidbody: {deltaPosition}");
            }
        }
        else
        {
            // Fallback: mover directamente el Transform
            transform.position += deltaPosition;
            transform.rotation = deltaRotation * transform.rotation;
            
            if (showDebugInfo)
            {
                Debug.Log($"[EndGameRootMotion] Movimiento aplicado via Transform: {deltaPosition}");
            }
        }
    }
    
    #endregion
    
    #region State Management
    
    /// <summary>
    /// Actualiza el estado de animación de fin de juego
    /// </summary>
    private void UpdateEndGameState()
    {
        if (playerAnimator != null)
        {
            bool wasInEndGame = isInEndGameAnimation;
            isInEndGameAnimation = playerAnimator.IsGameEnded();
            
            // Log cuando cambia el estado
            if (wasInEndGame != isInEndGameAnimation && showDebugInfo)
            {
                Debug.Log($"[EndGameRootMotion] Estado cambiado - En animación de fin de juego: {isInEndGameAnimation}");
            }
        }
    }
    
    /// <summary>
    /// Maneja el contacto con el suelo durante las animaciones
    /// </summary>
    private void HandleGroundContact()
    {
        // Hacer raycast hacia abajo para detectar el suelo
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            // Hay suelo debajo, verificar si estamos demasiado hundidos
            float distanceToGround = hit.distance - 0.1f; // Restar el offset del ray
            
            if (distanceToGround < -0.1f) // Si estamos hundidos más de 10cm
            {
                // Ajustar posición para estar sobre el suelo
                Vector3 correctedPosition = hit.point;
                transform.position = correctedPosition;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[EndGameRootMotion] Posición corregida - Estaba hundido {-distanceToGround:F2}m");
                }
            }
            
            // Actualizar última posición válida
            lastValidGroundPosition = hit.point;
        }
        else
        {
            // No hay suelo debajo, verificar si estamos muy lejos de la última posición válida
            float distanceFromLastValid = Vector3.Distance(transform.position, lastValidGroundPosition);
            
            if (distanceFromLastValid > 2f) // Si estamos más de 2 metros de la última posición válida
            {
                // Restaurar a la última posición válida
                transform.position = lastValidGroundPosition;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[EndGameRootMotion] Restaurado a última posición válida - Distancia: {distanceFromLastValid:F2}m");
                }
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Fuerza la activación del manejo de Root Motion
    /// </summary>
    public void EnableEndGameRootMotion()
    {
        isInEndGameAnimation = true;
        
        if (showDebugInfo)
        {
            Debug.Log("[EndGameRootMotion] Root Motion habilitado manualmente");
        }
    }
    
    /// <summary>
    /// Fuerza la desactivación del manejo de Root Motion
    /// </summary>
    public void DisableEndGameRootMotion()
    {
        isInEndGameAnimation = false;
        
        if (showDebugInfo)
        {
            Debug.Log("[EndGameRootMotion] Root Motion deshabilitado manualmente");
        }
    }
    
    /// <summary>
    /// Establece una nueva posición válida de suelo
    /// </summary>
    public void SetValidGroundPosition(Vector3 position)
    {
        lastValidGroundPosition = position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[EndGameRootMotion] Nueva posición válida establecida: {position}");
        }
    }
    
    #endregion
}
