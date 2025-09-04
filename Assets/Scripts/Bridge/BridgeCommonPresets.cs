using UnityEngine;

/// <summary>
/// Componente que incluye presets comunes predefinidos para construcción inicial del puente
/// </summary>
public class BridgeCommonPresets : MonoBehaviour
{
    [Header("Constructor de Destino")]
    public BridgeInitialConstructor targetConstructor;
    
    private void Start()
    {
        if (targetConstructor == null)
        {
            targetConstructor = FindObjectOfType<BridgeInitialConstructor>();
        }
    }
    
    /// <summary>
    /// Preset: Puente completamente vacío
    /// Ideal para niveles tutorial donde los jugadores construyen desde cero
    /// </summary>
    [ContextMenu("Aplicar: Puente Vacío")]
    public void ApplyEmptyBridge()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 0,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Puente Vacío (0 capas)");
        LogPresetDescription("Los jugadores deben construir el puente completamente desde cero. Ideal para tutoriales.");
    }
    
    /// <summary>
    /// Preset: Solo fundación construida
    /// Los jugadores necesitan añadir soporte, estructura y superficie
    /// </summary>
    [ContextMenu("Aplicar: Solo Base")]
    public void ApplyFoundationOnly()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 1,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Solo Base (1 capa)");
        LogPresetDescription("La fundación está lista. Los jugadores deben añadir soporte, estructura y superficie.");
    }
    
    /// <summary>
    /// Preset: Base y soporte construidos
    /// Los jugadores necesitan añadir estructura y superficie
    /// </summary>
    [ContextMenu("Aplicar: Base y Soporte")]
    public void ApplyBaseAndSupport()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 2,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Base y Soporte (2 capas)");
        LogPresetDescription("La base y soporte están listos. Los jugadores deben añadir estructura y superficie.");
    }
    
    /// <summary>
    /// Preset: Casi completo
    /// Solo falta la superficie final
    /// </summary>
    [ContextMenu("Aplicar: Casi Completo")]
    public void ApplyNearlyComplete()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 3,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Casi Completo (3 capas)");
        LogPresetDescription("El puente está casi terminado. Solo falta añadir la superficie final.");
    }
    
    /// <summary>
    /// Preset: Puente completo
    /// Ideal para niveles de mantenimiento o defensa
    /// </summary>
    [ContextMenu("Aplicar: Puente Completo")]
    public void ApplyCompleteBridge()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 4,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Puente Completo (4 capas)");
        LogPresetDescription("El puente está completamente construido y funcional.");
    }
    
    /// <summary>
    /// Preset: Puente completo pero dañado
    /// Ideal para niveles de reparación
    /// </summary>
    [ContextMenu("Aplicar: Puente Dañado")]
    public void ApplyDamagedBridge()
    {
        if (!ValidateConstructor()) return;
        
        SetConstructorValues(
            layers: 4,
            allQuadrants: true,
            lastState: BridgeQuadrantSO.LastLayerState.Damaged,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Puente Dañado (4 capas, estado dañado)");
        LogPresetDescription("El puente está completo pero dañado. Los jugadores deben repararlo.");
    }
    
    /// <summary>
    /// Preset: Construcción alternada
    /// Algunos cuadrantes construidos, otros no
    /// </summary>
    [ContextMenu("Aplicar: Construcción Alternada")]
    public void ApplyAlternatingConstruction()
    {
        if (!ValidateConstructor()) return;
        
        // Configurar patrón alternado
        targetConstructor.ConfigureSpecificQuadrants();
        CreateAlternatingPattern();
        
        SetConstructorValues(
            layers: 2,
            allQuadrants: false,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Construcción Alternada (patrón de tablero)");
        LogPresetDescription("Patrón alternado de cuadrantes construidos. Crea un desafío de navegación interesante.");
    }
    
    /// <summary>
    /// Preset: Solo bordes construidos
    /// El centro del puente está vacío
    /// </summary>
    [ContextMenu("Aplicar: Solo Bordes")]
    public void ApplyEdgesOnly()
    {
        if (!ValidateConstructor()) return;
        
        // Configurar solo los bordes
        targetConstructor.ConfigureSpecificQuadrants();
        CreateEdgePattern();
        
        SetConstructorValues(
            layers: 3,
            allQuadrants: false,
            lastState: BridgeQuadrantSO.LastLayerState.Complete,
            debugMessages: true
        );
        
        Debug.Log("✓ Preset aplicado: Solo Bordes (bordes del puente construidos)");
        LogPresetDescription("Solo los bordes del puente están construidos. El centro debe ser completado.");
    }
    
    /// <summary>
    /// Preset: Construcción progresiva
    /// El puente está más construido en un extremo que en el otro
    /// </summary>
    [ContextMenu("Aplicar: Construcción Progresiva")]
    public void ApplyProgressiveConstruction()
    {
        if (!ValidateConstructor()) return;
        
        // Aplicar construcción progresiva
        ApplyProgressivePattern();
        
        Debug.Log("✓ Preset aplicado: Construcción Progresiva");
        LogPresetDescription("El puente muestra progreso de construcción desde un extremo. Simula construcción en curso.");
    }
    
    // Métodos auxiliares
    
    private bool ValidateConstructor()
    {
        if (targetConstructor == null)
        {
            Debug.LogError("BridgeCommonPresets: No hay BridgeInitialConstructor asignado");
            return false;
        }
        return true;
    }
    
    private void SetConstructorValues(int layers, bool allQuadrants, BridgeQuadrantSO.LastLayerState lastState, bool debugMessages)
    {
        // Usar reflexión para establecer valores privados
        System.Type constructorType = targetConstructor.GetType();
        
        SetField("initialConstructedLayers", layers);
        SetField("constructAllQuadrants", allQuadrants);
        SetField("lastLayerState", lastState);
        SetField("showDebugMessages", debugMessages);
        SetField("applyOnStart", true);
    }
    
    private void SetField(string fieldName, object value)
    {
        System.Type constructorType = targetConstructor.GetType();
        System.Reflection.FieldInfo field = constructorType.GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(targetConstructor, value);
        }
    }
    
    private void CreateAlternatingPattern()
    {
        // Obtener referencia al array de cuadrantes específicos
        System.Type constructorType = targetConstructor.GetType();
        System.Reflection.FieldInfo field = constructorType.GetField("specificQuadrants", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            bool[] quadrants = field.GetValue(targetConstructor) as bool[];
            if (quadrants != null)
            {
                // Crear patrón de tablero
                BridgeConstructionGrid grid = GetBridgeGrid();
                if (grid != null)
                {
                    for (int x = 0; x < grid.gridWidth; x++)
                    {
                        for (int z = 0; z < grid.gridLength; z++)
                        {
                            int index = x * grid.gridLength + z;
                            if (index < quadrants.Length)
                            {
                                quadrants[index] = (x + z) % 2 == 0;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void CreateEdgePattern()
    {
        System.Type constructorType = targetConstructor.GetType();
        System.Reflection.FieldInfo field = constructorType.GetField("specificQuadrants", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            bool[] quadrants = field.GetValue(targetConstructor) as bool[];
            if (quadrants != null)
            {
                BridgeConstructionGrid grid = GetBridgeGrid();
                if (grid != null)
                {
                    // Limpiar todo primero
                    for (int i = 0; i < quadrants.Length; i++)
                        quadrants[i] = false;
                    
                    // Construir solo los bordes
                    for (int x = 0; x < grid.gridWidth; x++)
                    {
                        for (int z = 0; z < grid.gridLength; z++)
                        {
                            int index = x * grid.gridLength + z;
                            if (index < quadrants.Length)
                            {
                                // Es un borde si está en x=0, x=max, z=0, o z=max
                                bool isEdge = (x == 0 || x == grid.gridWidth - 1 || z == 0 || z == grid.gridLength - 1);
                                quadrants[index] = isEdge;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void ApplyProgressivePattern()
    {
        BridgeConstructionGrid grid = GetBridgeGrid();
        if (grid == null) return;
        
        // Aplicar diferentes niveles de construcción progresivamente
        for (int x = 0; x < grid.gridWidth; x++)
        {
            // Calcular cuántas capas construir basado en la posición x
            float progress = (float)x / (grid.gridWidth - 1);
            int layersForThisRow = Mathf.RoundToInt(progress * 4);
            
            // Aplicar construcción para esta fila
            SetConstructorValues(
                layers: layersForThisRow,
                allQuadrants: true,
                lastState: BridgeQuadrantSO.LastLayerState.Complete,
                debugMessages: true
            );
            
            // Configurar cuadrantes específicos para esta fila
            targetConstructor.ConfigureSpecificQuadrants();
            CreateRowPattern(x);
            
            // Aplicar construcción
            targetConstructor.ConstructInitialBridge();
        }
    }
    
    private void CreateRowPattern(int targetRow)
    {
        System.Type constructorType = targetConstructor.GetType();
        System.Reflection.FieldInfo field = constructorType.GetField("specificQuadrants", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            bool[] quadrants = field.GetValue(targetConstructor) as bool[];
            if (quadrants != null)
            {
                BridgeConstructionGrid grid = GetBridgeGrid();
                if (grid != null)
                {
                    // Limpiar todo
                    for (int i = 0; i < quadrants.Length; i++)
                        quadrants[i] = false;
                    
                    // Seleccionar solo la fila objetivo
                    for (int z = 0; z < grid.gridLength; z++)
                    {
                        int index = targetRow * grid.gridLength + z;
                        if (index < quadrants.Length)
                        {
                            quadrants[index] = true;
                        }
                    }
                }
            }
        }
    }
    
    private BridgeConstructionGrid GetBridgeGrid()
    {
        System.Type constructorType = targetConstructor.GetType();
        System.Reflection.FieldInfo field = constructorType.GetField("bridgeGrid", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return field?.GetValue(targetConstructor) as BridgeConstructionGrid;
    }
    
    private void LogPresetDescription(string description)
    {
        Debug.Log($"Descripción: {description}");
    }
    
    /// <summary>
    /// Lista todos los presets disponibles
    /// </summary>
    [ContextMenu("Listar Todos los Presets")]
    public void ListAllPresets()
    {
        Debug.Log("=== PRESETS COMUNES DISPONIBLES ===");
        Debug.Log("1. Puente Vacío - Construcción desde cero");
        Debug.Log("2. Solo Base - Solo fundación construida");
        Debug.Log("3. Base y Soporte - Primeras dos capas");
        Debug.Log("4. Casi Completo - Solo falta superficie");
        Debug.Log("5. Puente Completo - Totalmente funcional");
        Debug.Log("6. Puente Dañado - Completo pero necesita reparación");
        Debug.Log("7. Construcción Alternada - Patrón de tablero");
        Debug.Log("8. Solo Bordes - Centro del puente vacío");
        Debug.Log("9. Construcción Progresiva - Diferentes niveles por sección");
    }
}
