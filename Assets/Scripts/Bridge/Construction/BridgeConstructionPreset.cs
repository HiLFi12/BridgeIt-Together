using UnityEngine;

/// <summary>
/// ScriptableObject para guardar configuraciones preestablecidas de construcción inicial del puente
/// </summary>
[CreateAssetMenu(fileName = "BridgePreset", menuName = "Bridge/Initial Construction Preset")]
public class BridgeConstructionPreset : ScriptableObject
{
    [Header("Información del Preset")]
    public string presetName = "Nuevo Preset";
    [TextArea(3, 5)]
    public string description = "Descripción del preset de construcción inicial";
    
    [Header("Configuración de Construcción")]
    [Range(0, 4)]
    public int initialConstructedLayers = 0;
    
    [Header("Configuración de Cuadrantes")]
    public bool constructAllQuadrants = true;
    public bool[] specificQuadrants;
    
    [Header("Estado de la Última Capa")]
    public BridgeQuadrantSO.LastLayerState lastLayerState = BridgeQuadrantSO.LastLayerState.Complete;
    
    [Header("Configuración de Aplicación")]
    public bool applyOnStart = true;
    public bool showDebugMessages = false;
    
    /// <summary>
    /// Aplica este preset a un BridgeInitialConstructor
    /// </summary>
    public void ApplyTo(BridgeInitialConstructor constructor)
    {
        if (constructor == null) return;
        
        // Usar reflexión para establecer los valores privados
        System.Type constructorType = constructor.GetType();
        
        // Establecer valores usando reflexión
        SetFieldValue(constructor, "initialConstructedLayers", initialConstructedLayers);
        SetFieldValue(constructor, "constructAllQuadrants", constructAllQuadrants);
        SetFieldValue(constructor, "specificQuadrants", specificQuadrants);
        SetFieldValue(constructor, "lastLayerState", lastLayerState);
        SetFieldValue(constructor, "applyOnStart", applyOnStart);
        SetFieldValue(constructor, "showDebugMessages", showDebugMessages);
        
        Debug.Log($"Preset '{presetName}' aplicado exitosamente");
    }
    
    /// <summary>
    /// Crea un preset desde un BridgeInitialConstructor existente
    /// </summary>
    public void CreateFromConstructor(BridgeInitialConstructor constructor)
    {
        if (constructor == null) return;
        
        // Usar reflexión para obtener los valores privados
        initialConstructedLayers = (int)GetFieldValue(constructor, "initialConstructedLayers");
        constructAllQuadrants = (bool)GetFieldValue(constructor, "constructAllQuadrants");
        specificQuadrants = (bool[])GetFieldValue(constructor, "specificQuadrants");
        lastLayerState = (BridgeQuadrantSO.LastLayerState)GetFieldValue(constructor, "lastLayerState");
        applyOnStart = (bool)GetFieldValue(constructor, "applyOnStart");
        showDebugMessages = (bool)GetFieldValue(constructor, "showDebugMessages");
        
        Debug.Log($"Preset '{presetName}' creado desde constructor existente");
    }
    
    private void SetFieldValue(object target, string fieldName, object value)
    {
        System.Type type = target.GetType();
        System.Reflection.FieldInfo field = type.GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"No se pudo encontrar el campo '{fieldName}' en {type.Name}");
        }
    }
    
    private object GetFieldValue(object target, string fieldName)
    {
        System.Type type = target.GetType();
        System.Reflection.FieldInfo field = type.GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return field.GetValue(target);
        }
        else
        {
            Debug.LogWarning($"No se pudo encontrar el campo '{fieldName}' en {type.Name}");
            return null;
        }
    }
}

/// <summary>
/// Componente que gestiona múltiples presets de construcción inicial
/// </summary>
public class BridgePresetManager : MonoBehaviour
{
    [Header("Presets Disponibles")]
    public BridgeConstructionPreset[] availablePresets;
    
    [Header("Preset Actual")]
    public BridgeConstructionPreset currentPreset;
    
    [Header("Constructor a Configurar")]
    public BridgeInitialConstructor targetConstructor;
    
    /// <summary>
    /// Aplica el preset seleccionado
    /// </summary>
    [ContextMenu("Aplicar Preset Actual")]
    public void ApplyCurrentPreset()
    {
        if (currentPreset == null)
        {
            Debug.LogWarning("No hay preset seleccionado");
            return;
        }
        
        if (targetConstructor == null)
        {
            targetConstructor = FindObjectOfType<BridgeInitialConstructor>();
            if (targetConstructor == null)
            {
                Debug.LogError("No se encontró BridgeInitialConstructor en la escena");
                return;
            }
        }
        
        currentPreset.ApplyTo(targetConstructor);
    }
    
    /// <summary>
    /// Aplica un preset específico por índice
    /// </summary>
    public void ApplyPreset(int index)
    {
        if (availablePresets == null || index < 0 || index >= availablePresets.Length)
        {
            Debug.LogError($"Índice de preset inválido: {index}");
            return;
        }
        
        currentPreset = availablePresets[index];
        ApplyCurrentPreset();
    }
    
    /// <summary>
    /// Aplica un preset específico por nombre
    /// </summary>
    public void ApplyPreset(string presetName)
    {
        if (availablePresets == null) return;
        
        foreach (var preset in availablePresets)
        {
            if (preset != null && preset.presetName == presetName)
            {
                currentPreset = preset;
                ApplyCurrentPreset();
                return;
            }
        }
        
        Debug.LogWarning($"No se encontró preset con nombre '{presetName}'");
    }
    
    /// <summary>
    /// Lista todos los presets disponibles
    /// </summary>
    [ContextMenu("Listar Presets Disponibles")]
    public void ListAvailablePresets()
    {
        if (availablePresets == null || availablePresets.Length == 0)
        {
            Debug.Log("No hay presets disponibles");
            return;
        }
        
        Debug.Log("=== PRESETS DISPONIBLES ===");
        for (int i = 0; i < availablePresets.Length; i++)
        {
            var preset = availablePresets[i];
            if (preset != null)
            {
                Debug.Log($"{i}: {preset.presetName} - {preset.initialConstructedLayers} capas");
            }
        }
    }
    
    /// <summary>
    /// Crea presets comunes predefinidos
    /// </summary>
    [ContextMenu("Crear Presets Comunes")]
    public void CreateCommonPresets()
    {
        Debug.Log("Para crear presets comunes, usa el menú: Assets -> Create -> Bridge -> Initial Construction Preset");
        Debug.Log("Presets recomendados:");
        Debug.Log("- 'Puente Vacío' (0 capas)");
        Debug.Log("- 'Solo Base' (1 capa)");
        Debug.Log("- 'Base y Soporte' (2 capas)");
        Debug.Log("- 'Casi Completo' (3 capas)");
        Debug.Log("- 'Completo' (4 capas)");
        Debug.Log("- 'Completo Dañado' (4 capas, estado dañado)");
    }
}
