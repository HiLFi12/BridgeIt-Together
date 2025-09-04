using UnityEngine;

/// <summary>
/// Clase que se encarga de gestionar la seguridad de los jugadores, asegurándose de que no caigan del mapa
/// </summary>
[DefaultExecutionOrder(-100)] // Se ejecuta antes que otros scripts para asegurar que los detectores estén configurados
public class PlayerSafetyManager : MonoBehaviour
{
    [Header("Configuración de Capas")]
    [SerializeField] private LayerMask walkableSurfaceLayer;
    
    [Header("Configuración de Detección de Superficie")]
    [SerializeField] private float groundDetectionRadius = 0.5f;
    [SerializeField] private float repositionDistance = 0.3f;
    [SerializeField] private bool showDebugSpheres = true;
    
    [Header("Configuración de Detección de Vacío")]
    [SerializeField] private bool useRaycastFallback = true;
    [SerializeField] private float raycastDistance = 2.0f;
    [SerializeField] private LayerMask anyGroundLayer = -1; // Por defecto, todas las capas
    
    private void Awake()
    {
        // Buscar todos los jugadores en la escena
        Player[] players = FindObjectsOfType<Player>();
        Player2[] players2 = FindObjectsOfType<Player2>();
        
        // Configurar WalkableSurfaceDetector para cada jugador
        foreach (Player player in players)
        {
            ConfigureWalkableSurfaceDetector(player.gameObject);
        }
        
        foreach (Player2 player in players2)
        {
            ConfigureWalkableSurfaceDetector(player.gameObject);
        }
        
        Debug.Log($"PlayerSafetyManager: Configurados {players.Length + players2.Length} jugadores con protección anti-caídas.");
    }
    
    /// <summary>
    /// Configura el detector de superficies caminables para un jugador
    /// </summary>
    /// <param name="playerObject">GameObject del jugador</param>
    private void ConfigureWalkableSurfaceDetector(GameObject playerObject)
    {
        // Verificar si ya tiene el componente
        WalkableSurfaceDetector detector = playerObject.GetComponent<WalkableSurfaceDetector>();
        
        // Si no lo tiene, añadirlo
        if (detector == null)
        {
            detector = playerObject.AddComponent<WalkableSurfaceDetector>();
            Debug.Log($"Añadido WalkableSurfaceDetector a {playerObject.name}");
        }
        
        // Configurar el detector mediante reflexión para establecer los valores serializados
        var detectorType = detector.GetType();
        
        // Configuración de detección de superficie
        SetFieldValue(detector, detectorType, "walkableSurfaceLayer", walkableSurfaceLayer);
        SetFieldValue(detector, detectorType, "groundDetectionRadius", groundDetectionRadius);
        SetFieldValue(detector, detectorType, "safeRepositionDistance", repositionDistance);
        SetFieldValue(detector, detectorType, "showDebugSphere", showDebugSpheres);
        
        // Configuración de detección de vacío
        SetFieldValue(detector, detectorType, "useRaycastFallback", useRaycastFallback);
        SetFieldValue(detector, detectorType, "raycastDistance", raycastDistance);
        SetFieldValue(detector, detectorType, "anyGroundLayer", anyGroundLayer);
        
        // Asegurarse de que el jugador tenga un CharacterController
        if (playerObject.GetComponent<CharacterController>() == null)
        {
            Debug.LogWarning($"El jugador {playerObject.name} no tiene un CharacterController. La detección de superficies puede no funcionar correctamente.");
        }
        
        // Asegurarse de que el jugador tenga un PlayerLaunchController para lanzamientos de rescate parabólicos
        PlayerLaunchController launchController = playerObject.GetComponent<PlayerLaunchController>();
        if (launchController == null)
        {
            launchController = playerObject.AddComponent<PlayerLaunchController>();
            Debug.Log($"Añadido PlayerLaunchController a {playerObject.name} para lanzamientos de rescate parabólicos");
        }
    }
    
    /// <summary>
    /// Establece un valor en un campo de un objeto mediante reflexión
    /// </summary>
    private void SetFieldValue(object target, System.Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"No se pudo encontrar el campo '{fieldName}' en {type.Name}");
        }
    }
} 