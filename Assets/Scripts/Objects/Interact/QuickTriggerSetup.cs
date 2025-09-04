using UnityEngine;

/// <summary>
/// Utilidad para configurar rápidamente triggers de retorno de vehículos
/// Usa este script para crear y configurar triggers automáticamente
/// </summary>
public class QuickTriggerSetup : MonoBehaviour
{
    [Header("Configuración Rápida")]
    [SerializeField] private AutoGenerator autoGenerator;
    [SerializeField] private Transform[] posicionesTriggers; // Posiciones donde crear triggers
    [SerializeField] private Vector3 tamaņoTrigger = new Vector3(5f, 3f, 2f); // Tamaño de los box colliders
    
    [Header("Configuración Automática")]
    [SerializeField] private bool crearTriggersAlIniciar = true;
    [SerializeField] private bool crearEnBordesDelMapa = false;
    [SerializeField] private float distanciaBordes = 20f; // Distancia desde el centro para crear bordes
    
    [Header("Debugging")]
    [SerializeField] private bool mostrarGizmosEnScene = true;
    
    private GameObject[] objetosTriggerCreados;
    
    void Start()
    {
        if (crearTriggersAlIniciar)
        {
            SetupQuickTriggers();
        }
    }
    
    /// <summary>
    /// Configura triggers rápidamente basándose en las posiciones especificadas
    /// </summary>
    [ContextMenu("Configurar Triggers Rápido")]
    public void SetupQuickTriggers()
    {
        if (autoGenerator == null)
        {
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
            if (autoGenerator == null)
            {
                Debug.LogError("No se encontró AutoGenerator en la escena");
                return;
            }
        }
        
        // Limpiar triggers existentes si los hay
        CleanupExistingTriggers();
        
        // Crear triggers en posiciones especificadas
        if (posicionesTriggers != null && posicionesTriggers.Length > 0)
        {
            CreateTriggersAtPositions();
        }
        
        // Crear triggers en bordes del mapa si está habilitado
        if (crearEnBordesDelMapa)
        {
            CreateMapBorderTriggers();
        }
        
        Debug.Log($"Configuración rápida de triggers completada");
    }
    
    /// <summary>
    /// Crea triggers en las posiciones especificadas
    /// </summary>
    void CreateTriggersAtPositions()
    {
        objetosTriggerCreados = new GameObject[posicionesTriggers.Length];
        
        for (int i = 0; i < posicionesTriggers.Length; i++)
        {
            if (posicionesTriggers[i] == null) continue;
            
            // Crear objeto trigger
            GameObject triggerObj = new GameObject($"VehicleTrigger_{i+1}");
            triggerObj.transform.position = posicionesTriggers[i].position;
            triggerObj.transform.parent = transform; // Organizar bajo este objeto
            
            // Agregar y configurar BoxCollider
            BoxCollider boxCol = triggerObj.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = tamaņoTrigger;
            
            // Agregar al AutoGenerator
            autoGenerator.AddReturnTrigger(boxCol, true);
            
            objetosTriggerCreados[i] = triggerObj;
            
            Debug.Log($"Trigger creado en posición {i+1}: {triggerObj.name}");
        }
    }
    
    /// <summary>
    /// Crea triggers en los bordes del mapa automáticamente
    /// </summary>
    void CreateMapBorderTriggers()
    {
        Vector3 center = transform.position;
        
        // Posiciones de los bordes (Norte, Sur, Este, Oeste)
        Vector3[] borderPositions = {
            center + Vector3.forward * distanciaBordes,  // Norte
            center + Vector3.back * distanciaBordes,     // Sur
            center + Vector3.right * distanciaBordes,    // Este
            center + Vector3.left * distanciaBordes      // Oeste
        };
        
        string[] borderNames = { "Norte", "Sur", "Este", "Oeste" };
        
        for (int i = 0; i < borderPositions.Length; i++)
        {
            GameObject triggerObj = new GameObject($"BorderTrigger_{borderNames[i]}");
            triggerObj.transform.position = borderPositions[i];
            triggerObj.transform.parent = transform;
            
            BoxCollider boxCol = triggerObj.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(tamaņoTrigger.x * 2, tamaņoTrigger.y, tamaņoTrigger.z);
            
            autoGenerator.AddReturnTrigger(boxCol, true);
            
            Debug.Log($"Trigger de borde creado: {triggerObj.name}");
        }
    }
    
    /// <summary>
    /// Limpia triggers existentes creados por este script
    /// </summary>
    [ContextMenu("Limpiar Triggers Creados")]
    public void CleanupExistingTriggers()
    {
        // Buscar y destruir objetos trigger hijos
        VehicleReturnTrigger[] existingTriggers = GetComponentsInChildren<VehicleReturnTrigger>();
        foreach (var trigger in existingTriggers)
        {
            if (trigger.gameObject != gameObject) // No destruir este objeto
            {
                DestroyImmediate(trigger.gameObject);
            }
        }
        
        // Limpiar array
        objetosTriggerCreados = null;
        
        Debug.Log("Triggers existentes limpiados");
    }
    
    /// <summary>
    /// Crea un trigger en una posición específica
    /// </summary>
    public GameObject CreateTriggerAtPosition(Vector3 position, string nombre = "CustomTrigger")
    {
        if (autoGenerator == null) return null;
        
        GameObject triggerObj = new GameObject(nombre);
        triggerObj.transform.position = position;
        triggerObj.transform.parent = transform;
        
        BoxCollider boxCol = triggerObj.AddComponent<BoxCollider>();
        boxCol.isTrigger = true;
        boxCol.size = tamaņoTrigger;
        
        autoGenerator.AddReturnTrigger(boxCol, true);
        
        Debug.Log($"Trigger personalizado creado: {nombre}");
        return triggerObj;
    }
    
    /// <summary>
    /// Ajusta el tamaño de todos los triggers creados
    /// </summary>
    [ContextMenu("Ajustar Tamaño de Triggers")]
    public void AdjustTriggerSizes()
    {
        BoxCollider[] triggers = GetComponentsInChildren<BoxCollider>();
        foreach (var trigger in triggers)
        {
            if (trigger.isTrigger)
            {
                trigger.size = tamaņoTrigger;
            }
        }
        
        Debug.Log($"Tamaño de triggers ajustado a {tamaņoTrigger}");
    }
    
    // ===== GIZMOS PARA VISUALIZACIÓN =====
    
    void OnDrawGizmos()
    {
        if (!mostrarGizmosEnScene) return;
        
        // Mostrar posiciones donde se crearán triggers
        if (posicionesTriggers != null)
        {
            Gizmos.color = Color.green;
            foreach (var pos in posicionesTriggers)
            {
                if (pos != null)
                {
                    Gizmos.DrawWireCube(pos.position, tamaņoTrigger);
                }
            }
        }
        
        // Mostrar bordes del mapa si está habilitado
        if (crearEnBordesDelMapa)
        {
            Gizmos.color = Color.red;
            Vector3 center = transform.position;
            
            Vector3[] borderPositions = {
                center + Vector3.forward * distanciaBordes,
                center + Vector3.back * distanciaBordes,
                center + Vector3.right * distanciaBordes,
                center + Vector3.left * distanciaBordes
            };
            
            foreach (var pos in borderPositions)
            {
                Gizmos.DrawWireCube(pos, new Vector3(tamaņoTrigger.x * 2, tamaņoTrigger.y, tamaņoTrigger.z));
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar área de influencia cuando está seleccionado
        Gizmos.color = Color.yellow;
        if (crearEnBordesDelMapa)
        {
            Gizmos.DrawWireSphere(transform.position, distanciaBordes);
        }
    }
}
