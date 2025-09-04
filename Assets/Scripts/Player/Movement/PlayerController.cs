using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(WalkableSurfaceDetector))]

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private WalkableSurfaceDetector surfaceDetector;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 10.0f;
    
    [Header("Reposicionamiento")]
    [SerializeField] private float repositionDuration = 0.2f;
    [SerializeField] private bool useDirectTeleport = true;
    
    private Vector2 movementInput = Vector2.zero;
    private bool isRepositioning = false;
    private float fallTime = 0f;
    private float maxFallTime = 1.0f; // Tiempo máximo permitido de caída antes de forzar reposicionamiento
    
    // Propiedades públicas para el sistema de animaciones
    public Vector2 MovementInput => movementInput;
    public bool IsMoving => movementInput.magnitude > 0.1f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (surfaceDetector != null)
        {
            surfaceDetector.OnUnsafePositionDetected -= HandleUnsafePosition;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // Si estamos en proceso de reposicionamiento, no procesar movimiento
        if (isRepositioning)
        {
            return;
        }
        
        groundedPlayer = controller.isGrounded;
        
        // Restablecer la velocidad vertical cuando estamos en el suelo
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            fallTime = 0f; // Restablecer el tiempo de caída
        }
        
        // Verificar si estamos cayendo y no hay terreno debajo
        if (!groundedPlayer && playerVelocity.y < 0)
        {
            // Si estamos cayendo, incrementar el contador de tiempo
            fallTime += Time.deltaTime;
            
            // Si hemos estado cayendo demasiado tiempo, forzar reposicionamiento
        }

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        
        // Verificar si el movimiento nos llevaría fuera de una superficie caminable
        if (move != Vector3.zero && surfaceDetector != null)
        {
            Vector3 potentialPosition = transform.position + move.normalized * playerSpeed * Time.deltaTime;
            
            // Verificar si la posición potencial tiene superficie caminable
            if (!surfaceDetector.IsSafePosition(potentialPosition))
            {
                // Si no hay superficie, reducir la magnitud del movimiento para evitar caer
                move = Vector3.Lerp(Vector3.zero, move, 0.3f);
            }
        }
        
        // Aplicar movimiento horizontal
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Rotar el personaje en la dirección del movimiento
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplicar gravedad
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
    /// <summary>
    /// Maneja el evento de detección de posición insegura
    /// </summary>
    /// <param name="safePosition">Última posición segura registrada</param>
    private void HandleUnsafePosition(Vector3 safePosition)
    {
        // Si ya estamos reposicionando, ignorar
        if (isRepositioning) return;
        
        // Verificar si tenemos PlayerLaunchController para usar parábola de rescate
        PlayerLaunchController launchController = GetComponent<PlayerLaunchController>();
        
        if (launchController != null && !launchController.EstaSiendoLanzado())
        {
            Debug.Log($"🚁 Iniciando lanzamiento de rescate parabólico hacia posición segura: {safePosition}");
            
            // Usar parábola de rescate más sutil y cercana
            float alturaRescate = 1.5f; // Altura más baja para rescate sutil
            float tiempoRescate = 0.8f;  // Tiempo más rápido para rescate cercano
            
            // Ajustar la posición de destino para que sea más cercana al jugador
            Vector3 posicionRescateCercana = CalcularPosicionRescateCercana(transform.position, safePosition);
            
            // Crear curva de vuelo suave para el rescate
            AnimationCurve curvaRescate = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            // Lanzar usando parábola hacia la posición de rescate cercana
            launchController.LanzarJugador(posicionRescateCercana, alturaRescate, tiempoRescate, curvaRescate);
            
            // Marcar que estamos en proceso de rescate para evitar otras intervenciones
            isRepositioning = true;
            if (surfaceDetector != null)
            {
                surfaceDetector.SetRepositioning(true);
            }
            
            // Programar la limpieza del estado de reposicionamiento después del rescate
            StartCoroutine(LimpiarEstadoRescate(tiempoRescate + 0.15f)); // Ajustado para el nuevo tiempo más rápido
        }
        else
        {
            // Fallback: usar el sistema original de reposicionamiento directo
            Debug.Log("🚁 PlayerLaunchController no disponible, usando reposicionamiento directo");
            StartCoroutine(RepositionPlayer(safePosition));
        }
    }
    
    /// <summary>
    /// Limpia el estado de reposicionamiento después del rescate parabólico
    /// </summary>
    private System.Collections.IEnumerator LimpiarEstadoRescate(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        // Reactivar el sistema normal
        isRepositioning = false;
        if (surfaceDetector != null)
        {
            surfaceDetector.SetRepositioning(false);
        }
        
        Debug.Log("🚁 Rescate parabólico completado, sistemas reactivados");
    }
    
    /// <summary>
    /// Corrutina para reposicionar suavemente al jugador a una posición segura
    /// </summary>
    private System.Collections.IEnumerator RepositionPlayer(Vector3 safePosition)
    {
        // Marcar que estamos reposicionando
        isRepositioning = true;
        if (surfaceDetector != null)
        {
            surfaceDetector.SetRepositioning(true);
        }
        
        // Desactivar el controlador temporalmente
        controller.enabled = false;
        
        // Restablecer velocidades
        playerVelocity = Vector3.zero;
        fallTime = 0f;
        
        // Guardar posición inicial
        Vector3 startPosition = transform.position;
        
        // Si usamos teletransporte directo, mover inmediatamente
        if (useDirectTeleport)
        {
            transform.position = safePosition;
            yield return new WaitForSeconds(0.1f); // Pequeña pausa para estabilizar
        }
        else
        {
            // Mover suavemente hacia la posición segura
            float elapsedTime = 0;
            
            while (elapsedTime < repositionDuration)
            {
                transform.position = Vector3.Lerp(startPosition, safePosition, elapsedTime / repositionDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Asegurar que llegamos exactamente a la posición objetivo
            transform.position = safePosition;
        }
        
        // Reactivar el controlador
        controller.enabled = true;
        
        // Marcar que terminamos de reposicionar
        isRepositioning = false;
        if (surfaceDetector != null)
        {
            surfaceDetector.SetRepositioning(false);
        }
        
        Debug.Log("Jugador reposicionado a posición segura.");
    }
    
    /// <summary>
    /// Calcula una posición de rescate más cercana al jugador en lugar de usar la posición segura lejana
    /// </summary>
    private Vector3 CalcularPosicionRescateCercana(Vector3 posicionActual, Vector3 posicionSeguraOriginal)
    {
        // Calcular la dirección hacia la posición segura
        Vector3 direccionSegura = (posicionSeguraOriginal - posicionActual).normalized;
        
        // Limitar la distancia de rescate a un máximo de 3 unidades
        float distanciaMaximaRescate = 3.0f;
        float distanciaOriginal = Vector3.Distance(posicionActual, posicionSeguraOriginal);
        
        Vector3 posicionRescate;
        
        if (distanciaOriginal <= distanciaMaximaRescate)
        {
            // Si la posición segura ya está cerca, usarla directamente
            posicionRescate = posicionSeguraOriginal;
        }
        else
        {
            // Si está lejos, mover solo la distancia máxima en esa dirección
            posicionRescate = posicionActual + (direccionSegura * distanciaMaximaRescate);
            
            // Verificar si esta posición cercana es segura
            if (surfaceDetector != null && !surfaceDetector.IsSafePosition(posicionRescate))
            {
                // Si no es segura, usar la posición segura original pero con altura ajustada
                posicionRescate = new Vector3(posicionSeguraOriginal.x, posicionActual.y, posicionSeguraOriginal.z);
            }
        }
        
        Debug.Log($"🎯 Rescate: de {posicionActual} hacia {posicionRescate} (distancia: {Vector3.Distance(posicionActual, posicionRescate):F2})");
        return posicionRescate;
    }
}
