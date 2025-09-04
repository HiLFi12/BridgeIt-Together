using UnityEngine;

/// <summary>
/// Ejemplo de cómo usar el sistema de triggers de retorno de vehículos
/// Este script demuestra diferentes formas de configurar y controlar los triggers
/// </summary>
public class ExampleTriggerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AutoGenerator autoGenerator; // Referencia al AutoGenerator
    [SerializeField] private GameObject[] triggerObjects; // Objetos que serán triggers
    
    [Header("Configuración Dinámica")]
    [SerializeField] private bool activarTriggersAlIniciar = true;
    [SerializeField] private bool crearTriggersAutomaticamente = true;
    
    private Collider[] triggers;
    
    void Start()
    {
        // Si no se asignó el AutoGenerator, buscar en la escena
        if (autoGenerator == null)
        {
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
        }
        
        if (autoGenerator == null)
        {
            Debug.LogError("No se encontró AutoGenerator en la escena");
            return;
        }
        
        // Configurar triggers dinámicamente si está habilitado
        if (crearTriggersAutomaticamente)
        {
            SetupDynamicTriggers();
        }
    }
    
    /// <summary>
    /// Configura triggers dinámicamente desde los objetos asignados
    /// </summary>
    void SetupDynamicTriggers()
    {
        if (triggerObjects == null || triggerObjects.Length == 0) return;
        
        triggers = new Collider[triggerObjects.Length];
        
        for (int i = 0; i < triggerObjects.Length; i++)
        {
            GameObject obj = triggerObjects[i];
            if (obj == null) continue;
            
            // Obtener o agregar un collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
            {
                // Agregar un BoxCollider por defecto
                col = obj.AddComponent<BoxCollider>();
                Debug.Log($"Agregado BoxCollider a {obj.name}");
            }
            
            // Asegurar que es un trigger
            col.isTrigger = true;
            triggers[i] = col;
            
            // Agregar el trigger al AutoGenerator
            autoGenerator.AddReturnTrigger(col, activarTriggersAlIniciar);
        }
        
        Debug.Log($"Configurados {triggers.Length} triggers dinámicamente");
    }
    
    // ===== MÉTODOS PÚBLICOS PARA CONTROL DESDE INSPECTOR =====
    
    /// <summary>
    /// Activa todos los triggers
    /// </summary>
    [ContextMenu("Activar Todos los Triggers")]
    public void ActivateAllTriggers()
    {
        if (autoGenerator != null)
        {
            autoGenerator.SetTriggersActive(true);
            Debug.Log("Todos los triggers activados");
        }
    }
    
    /// <summary>
    /// Desactiva todos los triggers
    /// </summary>
    [ContextMenu("Desactivar Todos los Triggers")]
    public void DeactivateAllTriggers()
    {
        if (autoGenerator != null)
        {
            autoGenerator.SetTriggersActive(false);
            Debug.Log("Todos los triggers desactivados");
        }
    }
    
    /// <summary>
    /// Alterna el estado de todos los triggers
    /// </summary>
    [ContextMenu("Alternar Estado de Triggers")]
    public void ToggleAllTriggers()
    {
        if (autoGenerator == null) return;
        
        bool anyActive = autoGenerator.GetActiveTriggerCount() > 0;
        autoGenerator.SetTriggersActive(!anyActive);
        
        Debug.Log($"Triggers {(!anyActive ? "activados" : "desactivados")}");
    }
    
    /// <summary>
    /// Muestra información sobre los triggers activos
    /// </summary>
    [ContextMenu("Mostrar Info de Triggers")]
    public void ShowTriggerInfo()
    {
        if (autoGenerator == null) return;
        
        int activeCount = autoGenerator.GetActiveTriggerCount();
        Debug.Log($"Triggers activos: {activeCount}");
    }
    
    // ===== MÉTODOS PARA EVENTOS ESPECÍFICOS DEL JUEGO =====
    
    /// <summary>
    /// Activa triggers solo cuando el puente esté completo
    /// (ejemplo de uso condicional)
    /// </summary>
    public void ActivateTriggersOnBridgeComplete()
    {
        // Aquí podrías verificar el estado del puente
        // Por ejemplo: if (bridgeGrid.IsBridgeComplete())
        
        if (autoGenerator != null)
        {
            autoGenerator.SetTriggersActive(true);
            Debug.Log("Triggers activados - puente completado");
        }
    }
    
    /// <summary>
    /// Desactiva triggers temporalmente (por ejemplo, durante reparaciones)
    /// </summary>
    public void TemporarilyDisableTriggers(float duration)
    {
        if (autoGenerator != null)
        {
            autoGenerator.SetTriggersActive(false);
            Invoke(nameof(ReenableTriggers), duration);
            Debug.Log($"Triggers desactivados temporalmente por {duration} segundos");
        }
    }
    
    private void ReenableTriggers()
    {
        if (autoGenerator != null)
        {
            autoGenerator.SetTriggersActive(true);
            Debug.Log("Triggers reactivados");
        }
    }
    
    /// <summary>
    /// Agrega un nuevo trigger en tiempo de ejecución
    /// </summary>
    public void AddNewTrigger(GameObject obj)
    {
        if (obj == null || autoGenerator == null) return;
        
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            col = obj.AddComponent<BoxCollider>();
        }
        
        col.isTrigger = true;
        autoGenerator.AddReturnTrigger(col, true);
        
        Debug.Log($"Nuevo trigger agregado: {obj.name}");
    }
    
    /// <summary>
    /// Remueve un trigger específico
    /// </summary>
    public void RemoveTrigger(GameObject obj)
    {
        if (obj == null || autoGenerator == null) return;
        
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            autoGenerator.RemoveReturnTrigger(col);
            Debug.Log($"Trigger removido: {obj.name}");
        }
    }
    
    // ===== DEBUGGING Y VISUALIZACIÓN =====
    
    void OnDrawGizmos()
    {
        // Dibujar conexiones entre este controlador y los objetos trigger
        if (triggerObjects != null)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject obj in triggerObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar información más detallada cuando está seleccionado
        if (triggerObjects != null)
        {
            Gizmos.color = Color.cyan;
            foreach (GameObject obj in triggerObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireSphere(obj.transform.position, 0.5f);
                }
            }
        }
    }
}
