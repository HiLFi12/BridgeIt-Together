using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para detectar superficies caminables y prevenir caídas
/// </summary>
public class WalkableSurfaceDetector : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [SerializeField] private float groundDetectionRadius = 0.5f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask walkableSurfaceLayer;
    [SerializeField] private float safeRepositionDistance = 0.3f;
    [SerializeField] private bool showDebugSphere = true;
    
    [Header("Detección de Vacío")]
    [SerializeField] private float raycastDistance = 2.0f;
    [SerializeField] private LayerMask anyGroundLayer = -1; // Por defecto, todas las capas
    [SerializeField] private bool useRaycastFallback = true;
    [SerializeField] private float fallVelocityThreshold = -3.0f; // Velocidad de caída que activa la reposición
    [SerializeField] private float fallCheckInterval = 0.1f; // Intervalo para verificar caídas
    
    // Almacenamiento de resultados para evitar asignaciones de memoria
    private Collider[] groundDetectionResults = new Collider[1];
    private Vector3 lastSafePosition;
    private RaycastHit[] raycastResults = new RaycastHit[1];
    
    // Variables para detección de caídas
    private Vector3 lastPosition;
    private float verticalVelocity;
    private float timeSinceLastCheck;
    private bool isRepositioning = false;
    private CharacterController characterController;
    
    // Evento que se dispara cuando se detecta una posición insegura
    public delegate void UnsafePositionDetected(Vector3 safePosition);
    public event UnsafePositionDetected OnUnsafePositionDetected;
    
    private void Start()
    {
        // Verificar si tenemos un punto de verificación de suelo
        if (groundCheckPoint == null)
        {
            GameObject checkPoint = new GameObject("GroundCheckPoint");
            checkPoint.transform.SetParent(transform);
            checkPoint.transform.localPosition = new Vector3(0, -0.9f, 0); // Ligeramente por debajo del jugador
            groundCheckPoint = checkPoint.transform;
            Debug.Log("Se ha creado un punto de verificación de suelo automáticamente.");
        }
        
        // Inicializar la última posición segura y variables de caída
        lastSafePosition = transform.position;
        lastPosition = transform.position;
        
        // Obtener el CharacterController si existe
        characterController = GetComponent<CharacterController>();
    }
    
    private void Update()
    {
        // Si estamos en proceso de reposicionamiento, no hacer nada
        if (isRepositioning)
            return;
        
        // Verificar si estamos sobre una superficie caminable
        bool isOverWalkableSurface = CheckForWalkableSurface();
        
        // Si estamos sobre una superficie caminable, actualizar la última posición segura
        if (isOverWalkableSurface)
        {
            lastSafePosition = transform.position;
        }
        
        // Calcular velocidad vertical aproximada
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= fallCheckInterval)
        {
            verticalVelocity = (transform.position.y - lastPosition.y) / timeSinceLastCheck;
            lastPosition = transform.position;
            timeSinceLastCheck = 0;
        }
        
        // Verificar si estamos cayendo y no hay terreno debajo
        bool isFalling = false;
        
        // Método 1: Usando velocidad vertical calculada
        if (verticalVelocity < fallVelocityThreshold)
        {
            isFalling = true;
        }
        
        // Método 2: Usando CharacterController.isGrounded (si está disponible)
        if (characterController != null && !characterController.isGrounded)
        {
            // Si no estamos en el suelo y no hay superficie debajo, probablemente estamos cayendo
            if (!isOverWalkableSurface)
            {
                isFalling = true;
            }
        }
        
        // Si estamos cayendo al vacío, notificar para reposicionar
        if (isFalling && !isOverWalkableSurface)
        {
            Debug.Log("¡Caída detectada! Reposicionando jugador...");
            OnUnsafePositionDetected?.Invoke(lastSafePosition);
        }
    }
    
    /// <summary>
    /// Verifica si hay una superficie caminable en la posición actual
    /// </summary>
    /// <returns>True si hay superficie caminable, false en caso contrario</returns>
    public bool CheckForWalkableSurface()
    {
        // Método 1: Verificar superficie con capa específica usando OverlapSphereNonAlloc
        int hitCount = Physics.OverlapSphereNonAlloc(groundCheckPoint.position, groundDetectionRadius, groundDetectionResults, walkableSurfaceLayer);
        
        // Si encontramos superficie caminable, retornar true inmediatamente
        if (hitCount > 0)
        {
            return true;
        }
        
        // Método 2 (fallback): Verificar si hay CUALQUIER terreno usando Raycast
        if (useRaycastFallback)
        {
            // Lanzar un rayo hacia abajo para detectar cualquier superficie
            int rayHits = Physics.RaycastNonAlloc(
                groundCheckPoint.position + Vector3.up * 0.1f, // Ligeramente elevado para evitar colisiones iniciales
                Vector3.down,
                raycastResults,
                raycastDistance,
                anyGroundLayer
            );
            
            // Si el rayo golpea algo, hay terreno debajo
            return rayHits > 0;
        }
        
        // No se encontró superficie caminable ni terreno genérico
        return false;
    }
    
    /// <summary>
    /// Verifica si una posición potencial tiene superficie caminable debajo
    /// </summary>
    /// <param name="potentialPosition">Posición a verificar</param>
    /// <returns>True si la posición es segura, false en caso contrario</returns>
    public bool IsSafePosition(Vector3 potentialPosition)
    {
        Vector3 checkPosition = new Vector3(potentialPosition.x, groundCheckPoint.position.y, potentialPosition.z);
        
        // Método 1: Verificar superficie con capa específica
        int hitCount = Physics.OverlapSphereNonAlloc(checkPosition, groundDetectionRadius, groundDetectionResults, walkableSurfaceLayer);
        if (hitCount > 0)
        {
            return true;
        }
        
        // Método 2 (fallback): Verificar si hay CUALQUIER terreno
        if (useRaycastFallback)
        {
            // Calcular posición de inicio del rayo (ligeramente elevada)
            Vector3 rayStart = new Vector3(potentialPosition.x, transform.position.y + 0.1f, potentialPosition.z);
            
            // Lanzar un rayo hacia abajo para detectar cualquier superficie
            int rayHits = Physics.RaycastNonAlloc(
                rayStart,
                Vector3.down,
                raycastResults,
                raycastDistance,
                anyGroundLayer
            );
            
            // Si el rayo golpea algo, hay terreno debajo
            return rayHits > 0;
        }
        
        return false;
    }
    
    /// <summary>
    /// Calcula una dirección segura hacia la que moverse para evitar caídas
    /// </summary>
    /// <param name="currentPosition">Posición actual</param>
    /// <returns>Vector de dirección hacia una posición segura</returns>
    public Vector3 GetSafeRepositionDirection(Vector3 currentPosition)
    {
        Vector3 repositionDirection = (lastSafePosition - currentPosition).normalized;
        
        // Si la dirección es cero, usar un vector hacia atrás como respaldo
        if (repositionDirection == Vector3.zero)
        {
            repositionDirection = -transform.forward;
        }
        
        return repositionDirection;
    }
    
    /// <summary>
    /// Calcula una posición segura a la que moverse para evitar caídas
    /// </summary>
    /// <returns>Posición segura calculada</returns>
    public Vector3 GetSafePosition()
    {
        return lastSafePosition;
    }
    
    /// <summary>
    /// Obtiene la última posición segura registrada
    /// </summary>
    public Vector3 LastSafePosition => lastSafePosition;
    
    /// <summary>
    /// Marca que el jugador está siendo reposicionado
    /// </summary>
    public void SetRepositioning(bool value)
    {
        isRepositioning = value;
    }
    
    /// <summary>
    /// Actualiza manualmente la posición segura (útil después de lanzamientos o teletransportes)
    /// </summary>
    /// <param name="nuevaPosicion">La nueva posición segura</param>
    public void ActualizarPosicionSegura(Vector3 nuevaPosicion)
    {
        lastSafePosition = nuevaPosicion;
        Debug.Log($"Posición segura actualizada a: {nuevaPosicion}");
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugSphere || groundCheckPoint == null) return;
        
        // Dibujar esfera de detección de suelo
        if (Application.isPlaying)
        {
            Gizmos.color = CheckForWalkableSurface() ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundDetectionRadius);
        
        // Dibujar rayo de detección de terreno genérico
        if (useRaycastFallback)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(groundCheckPoint.position + Vector3.up * 0.1f, groundCheckPoint.position + Vector3.down * raycastDistance);
        }
        
        // Dibujar última posición segura
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lastSafePosition, 0.2f);
        }
    }
}