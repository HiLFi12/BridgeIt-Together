using UnityEngine;

/// <summary>
/// Configurador masivo para múltiples VehicleReturnTriggers
/// Permite configurar fácilmente el comportamiento de destrucción en todos los triggers
/// </summary>
public class VehicleTriggerConfigurator : MonoBehaviour
{
    [Header("Configuración Global")]
    [SerializeField] private bool destroyNonVehiclesEnabled = true;
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Tags Protegidos Adicionales")]
    [SerializeField] private string[] globalProtectedTags = new string[0];
    
    [Header("Referencias")]
    [SerializeField] private VehicleReturnTrigger[] triggers;
    [SerializeField] private bool autoFindTriggers = true;
    
    private void Start()
    {
        if (autoFindTriggers || triggers == null || triggers.Length == 0)
        {
            FindAllTriggers();
        }
        
        ApplyConfigurationToAllTriggers();
    }
    
    /// <summary>
    /// Busca automáticamente todos los VehicleReturnTrigger en la escena
    /// </summary>
    [ContextMenu("Buscar Todos los Triggers")]
    public void FindAllTriggers()
    {
        triggers = FindObjectsOfType<VehicleReturnTrigger>();
        Debug.Log($"🔍 Encontrados {triggers.Length} VehicleReturnTriggers en la escena");
    }
    
    /// <summary>
    /// Aplica la configuración actual a todos los triggers
    /// </summary>
    [ContextMenu("Aplicar Configuración a Todos")]
    public void ApplyConfigurationToAllTriggers()
    {
        if (triggers == null || triggers.Length == 0)
        {
            Debug.LogWarning("No hay triggers configurados. Usa 'Buscar Todos los Triggers' primero.");
            return;
        }
        
        int configured = 0;
        foreach (VehicleReturnTrigger trigger in triggers)
        {
            if (trigger != null)
            {
                trigger.SetDestroyNonVehicles(destroyNonVehiclesEnabled);
                trigger.SetDebugMessages(showDebugMessages);
                
                if (globalProtectedTags != null && globalProtectedTags.Length > 0)
                {
                    trigger.AddProtectedTags(globalProtectedTags);
                }
                
                configured++;
            }
        }
        
        Debug.Log($"✅ Configuración aplicada a {configured} triggers");
    }
    
    /// <summary>
    /// Activa la destrucción de no-vehículos en todos los triggers
    /// </summary>
    [ContextMenu("Activar Destrucción en Todos")]
    public void EnableDestructionOnAll()
    {
        destroyNonVehiclesEnabled = true;
        ApplyDestructionSetting(true);
    }
    
    /// <summary>
    /// Desactiva la destrucción de no-vehículos en todos los triggers
    /// </summary>
    [ContextMenu("Desactivar Destrucción en Todos")]
    public void DisableDestructionOnAll()
    {
        destroyNonVehiclesEnabled = false;
        ApplyDestructionSetting(false);
    }
    
    /// <summary>
    /// Aplica configuración de destrucción a todos los triggers
    /// </summary>
    private void ApplyDestructionSetting(bool enabled)
    {
        if (triggers == null) return;
        
        foreach (VehicleReturnTrigger trigger in triggers)
        {
            if (trigger != null)
            {
                trigger.SetDestroyNonVehicles(enabled);
            }
        }
        
        Debug.Log($"🔧 Destrucción de no-vehículos {(enabled ? "activada" : "desactivada")} en todos los triggers");
    }
    
    /// <summary>
    /// Activa o desactiva mensajes de debug en todos los triggers
    /// </summary>
    /// <param name="enabled">True para activar, False para desactivar</param>
    public void SetDebugMessagesForAll(bool enabled)
    {
        showDebugMessages = enabled;
        
        if (triggers == null) return;
        
        foreach (VehicleReturnTrigger trigger in triggers)
        {
            if (trigger != null)
            {
                trigger.SetDebugMessages(enabled);
            }
        }
        
        Debug.Log($"📝 Mensajes de debug {(enabled ? "activados" : "desactivados")} en todos los triggers");
    }
    
    /// <summary>
    /// Añade un tag protegido globalmente a todos los triggers
    /// </summary>
    /// <param name="newTag">El tag a proteger</param>
    public void AddGlobalProtectedTag(string newTag)
    {
        if (string.IsNullOrEmpty(newTag)) return;
        
        // Añadir al array global
        System.Collections.Generic.List<string> tagList = new System.Collections.Generic.List<string>();
        if (globalProtectedTags != null)
        {
            tagList.AddRange(globalProtectedTags);
        }
        
        if (!tagList.Contains(newTag))
        {
            tagList.Add(newTag);
            globalProtectedTags = tagList.ToArray();
            
            // Aplicar a todos los triggers
            if (triggers != null)
            {
                foreach (VehicleReturnTrigger trigger in triggers)
                {
                    if (trigger != null)
                    {
                        trigger.AddProtectedTags(new string[] { newTag });
                    }
                }
            }
            
            Debug.Log($"🛡️ Tag protegido '{newTag}' añadido globalmente");
        }
    }
    
    /// <summary>
    /// Obtiene estadísticas de los triggers configurados
    /// </summary>
    [ContextMenu("Mostrar Estadísticas")]
    public void ShowStatistics()
    {
        if (triggers == null || triggers.Length == 0)
        {
            Debug.Log("📊 No hay triggers configurados");
            return;
        }
        
        int active = 0;
        int withDestruction = 0;
        int withDebug = 0;
        
        foreach (VehicleReturnTrigger trigger in triggers)
        {
            if (trigger != null)
            {
                if (trigger.IsActive()) active++;
                // No podemos verificar las configuraciones privadas desde aquí
            }
        }
        
        Debug.Log($"📊 ESTADÍSTICAS DE TRIGGERS:");
        Debug.Log($"   • Total de triggers: {triggers.Length}");
        Debug.Log($"   • Triggers activos: {active}");
        Debug.Log($"   • Destrucción global: {(destroyNonVehiclesEnabled ? "Activada" : "Desactivada")}");
        Debug.Log($"   • Debug global: {(showDebugMessages ? "Activado" : "Desactivado")}");
        Debug.Log($"   • Tags protegidos globales: {(globalProtectedTags?.Length ?? 0)}");
    }
    
    /// <summary>
    /// Método para testing - simula objetos no-vehículos
    /// </summary>
    [ContextMenu("Test: Crear Objeto de Prueba")]
    public void CreateTestObject()
    {
        GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testObj.name = "TestObject_NonVehicle";
        testObj.tag = "Untagged";
        
        // Posicionar cerca de un trigger si es posible
        if (triggers != null && triggers.Length > 0 && triggers[0] != null)
        {
            Vector3 triggerPos = triggers[0].transform.position;
            testObj.transform.position = triggerPos + Vector3.up * 2f;
            
            // Añadir rigidbody para que caiga
            Rigidbody rb = testObj.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }
        
        Debug.Log($"🧪 Objeto de prueba creado: {testObj.name}");
    }
}
