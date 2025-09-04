using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Componente que se agrega a los triggers para detectar vehículos y comunicarse con el GameConditionManager
/// </summary>
[RequireComponent(typeof(Collider))]
public class GameConditionTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private TipoTrigger tipoTrigger = TipoTrigger.Victoria;
    [SerializeField] private string tagVehiculoRequerido = "Vehicle";
    [SerializeField] private bool mostrarDebugInfo = true;
    [SerializeField] private bool activarGizmosVisuales = true;
    
    [Header("Estado")]
    [SerializeField] private bool triggerActivo = true;
    
    // Referencias
    private GameConditionManager gameManager;
    private Collider triggerCollider;
    
    // Control de vehículos para evitar conteo múltiple
    private System.Collections.Generic.HashSet<GameObject> vehiculosYaContados = new System.Collections.Generic.HashSet<GameObject>();
    
    public enum TipoTrigger
    {
        Victoria, // Para vehículos que pasan el puente
        Derrota   // Para vehículos que caen
    }
    
    #region Unity Events
    
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"GameConditionTrigger en {gameObject.name} requiere un Collider!");
            return;
        }
        
        // Asegurar que sea un trigger
        triggerCollider.isTrigger = true;
    }
    
    private void Start()
    {
        // Buscar el GameConditionManager
        if (gameManager == null)
        {
            gameManager = GameConditionManager.Instance;
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameConditionManager>();
            }
        }
        
        if (gameManager == null)
        {
            Debug.LogError($"No se encontró GameConditionManager para el trigger {gameObject.name}");
        }
        
        if (mostrarDebugInfo)
        {
            string tipoStr = tipoTrigger == TipoTrigger.Victoria ? "Victoria" : "Derrota";
            Debug.Log($"GameConditionTrigger configurado en {gameObject.name} - Tipo: {tipoStr}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!triggerActivo || gameManager == null) return;
        
        // Verificar si el juego está activo
        if (!gameManager.IsJuegoActivo()) return;
        
        // Verificar si el objeto que entra es un vehículo
        GameObject vehiculo = other.gameObject;
        
        // Buscar el vehículo en el objeto o en sus padres
        GameObject vehiculoFinal = BuscarVehiculo(vehiculo);
        
        if (vehiculoFinal != null && !vehiculosYaContados.Contains(vehiculoFinal))
        {
            // Marcar como ya contado para evitar conteo múltiple
            vehiculosYaContados.Add(vehiculoFinal);
            
            // Procesar según el tipo de trigger
            ProcesarVehiculo(vehiculoFinal);
            
            if (mostrarDebugInfo)
            {
                string tipoStr = tipoTrigger == TipoTrigger.Victoria ? "pasó el puente" : "cayó";
                Debug.Log($"🚗 Vehículo {vehiculoFinal.name} {tipoStr} - Trigger: {gameObject.name}");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Limpiar el registro cuando el vehículo sale del trigger
        GameObject vehiculo = BuscarVehiculo(other.gameObject);
        if (vehiculo != null)
        {
            vehiculosYaContados.Remove(vehiculo);
        }
    }
    
    #endregion
    
    #region Detección de Vehículos
    
    /// <summary>
    /// Busca un vehículo válido en el objeto o sus padres
    /// </summary>
    private GameObject BuscarVehiculo(GameObject obj)
    {
        // Verificar el propio objeto
        if (EsVehiculo(obj))
        {
            return obj;
        }
        
        // Buscar en los padres
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (EsVehiculo(parent.gameObject))
            {
                return parent.gameObject;
            }
            parent = parent.transform.parent;
        }
        
        // Buscar en los hijos (por si el collider está en un hijo)
        AutoMovement movement = obj.GetComponentInChildren<AutoMovement>();
        if (movement != null && EsVehiculo(movement.gameObject))
        {
            return movement.gameObject;
        }
        
        VehicleBridgeCollision bridgeCollision = obj.GetComponentInChildren<VehicleBridgeCollision>();
        if (bridgeCollision != null && EsVehiculo(bridgeCollision.gameObject))
        {
            return bridgeCollision.gameObject;
        }

        // Nuevo: soportar vehículos basados en AutoController (SRP/SOLID)
        AutoController autoControllerChild = obj.GetComponentInChildren<AutoController>();
        if (autoControllerChild != null && EsVehiculo(autoControllerChild.gameObject))
        {
            return autoControllerChild.gameObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// Verifica si un GameObject es un vehículo válido
    /// </summary>
    private bool EsVehiculo(GameObject obj)
    {
        // Verificar tag
        if (!obj.CompareTag(tagVehiculoRequerido))
        {
            return false;
        }
        
    // Verificar que tenga componentes de vehículo
    bool tieneAutoMovement = obj.GetComponent<AutoMovement>() != null;
    bool tieneVehicleBridgeCollision = obj.GetComponent<VehicleBridgeCollision>() != null;
    bool tieneAutoController = obj.GetComponent<AutoController>() != null;
        
    return tieneAutoMovement || tieneVehicleBridgeCollision || tieneAutoController;
    }
    
    #endregion
    
    #region Procesamiento de Vehículos
    
    /// <summary>
    /// Procesa un vehículo según el tipo de trigger
    /// </summary>
    private void ProcesarVehiculo(GameObject vehiculo)
    {
        switch (tipoTrigger)
        {
            case TipoTrigger.Victoria:
                gameManager.OnVehiculoPasaPuente(vehiculo);
                break;
                
            case TipoTrigger.Derrota:
                gameManager.OnVehiculoCae(vehiculo);
                break;
        }
    }
    
    #endregion
    
    #region Configuración Pública
    
    /// <summary>
    /// Configura este trigger como trigger de victoria
    /// </summary>
    public void ConfigurarComoTriggerVictoria(GameConditionManager manager, string tagVehiculo)
    {
        gameManager = manager;
        tipoTrigger = TipoTrigger.Victoria;
        tagVehiculoRequerido = tagVehiculo;
        triggerActivo = true;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Trigger {gameObject.name} configurado como trigger de VICTORIA");
        }
    }
    
    /// <summary>
    /// Configura este trigger como trigger de derrota
    /// </summary>
    public void ConfigurarComoTriggerDerrota(GameConditionManager manager, string tagVehiculo)
    {
        gameManager = manager;
        tipoTrigger = TipoTrigger.Derrota;
        tagVehiculoRequerido = tagVehiculo;
        triggerActivo = true;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Trigger {gameObject.name} configurado como trigger de DERROTA");
        }
    }
    
    /// <summary>
    /// Activa o desactiva este trigger
    /// </summary>
    public void SetTriggerActivo(bool activo)
    {
        triggerActivo = activo;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Trigger {gameObject.name} {(activo ? "activado" : "desactivado")}");
        }
    }
    
    /// <summary>
    /// Obtiene si el trigger está activo
    /// </summary>
    public bool IsTriggerActivo() => triggerActivo;
    
    /// <summary>
    /// Limpia la lista de vehículos ya contados
    /// </summary>
    public void LimpiarVehiculosContados()
    {
        vehiculosYaContados.Clear();
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Lista de vehículos contados limpiada en {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Remueve un vehículo específico de la lista de contados
    /// SOLUCIÓN: Esto permite reutilizar vehículos del pool sin problemas de conteo
    /// </summary>
    /// <param name="vehiculo">El vehículo a remover de la lista</param>
    public void RemoverVehiculoContado(GameObject vehiculo)
    {
        if (vehiculo != null && vehiculosYaContados.Contains(vehiculo))
        {
            vehiculosYaContados.Remove(vehiculo);
            
            if (mostrarDebugInfo)
            {
                Debug.Log($"🧹 Vehículo {vehiculo.name} removido de lista de contados en trigger {gameObject.name}");
            }
        }
    }
    
    #endregion
    
    #region Visualización y Debug
    
    private void OnDrawGizmos()
    {
        if (!activarGizmosVisuales) return;
        
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        // Color según el tipo de trigger
        switch (tipoTrigger)
        {
            case TipoTrigger.Victoria:
                Gizmos.color = triggerActivo ? Color.green : Color.gray;
                break;
            case TipoTrigger.Derrota:
                Gizmos.color = triggerActivo ? Color.red : Color.gray;
                break;
        }
        
        // Hacer el color semi-transparente
        Color gizmoColor = Gizmos.color;
        gizmoColor.a = 0.3f;
        Gizmos.color = gizmoColor;
        
        // Dibujar según el tipo de collider
        if (col is BoxCollider box)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position + box.center, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(Vector3.zero, box.size);
            
            // Wireframe para mejor visibilidad
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(Vector3.zero, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position + sphere.center, transform.rotation, transform.lossyScale);
            Gizmos.DrawSphere(Vector3.zero, sphere.radius);
            
            // Wireframe para mejor visibilidad
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireSphere(Vector3.zero, sphere.radius);
        }
        
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!activarGizmosVisuales) return;
        
        // Mostrar información adicional cuando está seleccionado
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Dibujar una línea hacia arriba para indicar el tipo
        Vector3 lineStart = transform.position;
        Vector3 lineEnd = lineStart + Vector3.up * 2f;
        
        switch (tipoTrigger)
        {
            case TipoTrigger.Victoria:
                Gizmos.color = Color.green;
                break;
            case TipoTrigger.Derrota:
                Gizmos.color = Color.red;
                break;
        }
        
        Gizmos.DrawLine(lineStart, lineEnd);
        Gizmos.DrawSphere(lineEnd, 0.2f);
    }
    
    #endregion
    
    #region Context Menu para Testing
    
    [ContextMenu("Simular Trigger")]
    public void SimularTrigger()
    {
        if (gameManager != null)
        {
            GameObject vehiculoFake = new GameObject("VehiculoTest");
            vehiculoFake.tag = tagVehiculoRequerido;
            vehiculoFake.AddComponent<AutoMovement>();
            
            ProcesarVehiculo(vehiculoFake);
            
            DestroyImmediate(vehiculoFake);
        }
    }
    
    [ContextMenu("Toggle Activo")]
    public void ToggleActivo()
    {
        SetTriggerActivo(!triggerActivo);
    }
    
    [ContextMenu("Limpiar Vehículos Contados")]
    public void ContextLimpiarVehiculos()
    {
        LimpiarVehiculosContados();
    }
    
    #endregion
}
