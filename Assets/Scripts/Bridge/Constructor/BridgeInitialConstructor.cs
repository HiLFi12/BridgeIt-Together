using UnityEngine;

/// <summary>
/// Script para configurar el estado inicial del puente construido hasta diferentes capas.
/// Permite establecer en el inspector hasta qué capa debe aparecer construido el puente al inicio del nivel.
/// </summary>
public class BridgeInitialConstructor : MonoBehaviour
{
    [Header("Configuración Inicial del Puente")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    
    [Header("Estado Inicial de Construcción")]
    [Range(0, 3), Tooltip("0 = Sin construir, 1 = Solo capa base, 2 = Base + soporte, 3 = Puente completo (base + soporte + superficie)")]
    [SerializeField] private int initialConstructedLayers = 0;
    
    [Header("Configuración Específica")]
    [SerializeField] private bool constructAllQuadrants = true;
    [SerializeField] private bool[] specificQuadrants;
    
    [Header("Estado de la Última Capa")]
    [SerializeField] private BridgeQuadrantSO.LastLayerState lastLayerState = BridgeQuadrantSO.LastLayerState.Complete;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private bool applyOnStart = true;
    
    private void Start()
    {
        if (applyOnStart)
        {
            // Asegurar que el BridgeConstructionGrid esté inicializado antes de construir.
            // Usamos una corrutina para esperar al menos un frame y permitir que Awake/Start del Grid corran.
            StartCoroutine(DeferredConstruct());
        }
    }

    private System.Collections.IEnumerator DeferredConstruct()
    {
        // Esperar a que exista bridgeGrid
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }

        // Esperar 1 frame para garantizar ejecución de Start en el Grid
        yield return null;

        // Intentar asegurar inicialización del grid (en caso de que Start aún no haya creado la grilla)
        EnsureGridInitialized();

        // Si aún no está, esperar algunos frames como fallback
        int safetyFrames = 5;
        while (!IsGridBackerAllocated() && safetyFrames-- > 0)
        {
            yield return null;
            EnsureGridInitialized();
        }

        ConstructInitialBridge();
    }
    
    /// <summary>
    /// Método principal para construir el puente hasta las capas especificadas
    /// </summary>
    [ContextMenu("Construir Puente Inicial")]
    public void ConstructInitialBridge()
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
            if (bridgeGrid == null)
            {
                Debug.LogError("BridgeInitialConstructor: No se encontró BridgeConstructionGrid en la escena");
                return;
            }
        }

        // Asegurar que el grid interno esté construido antes de intentar construir capas
        EnsureGridInitialized();
        if (!IsGridBackerAllocated())
        {
            Debug.LogError("BridgeInitialConstructor: El BridgeConstructionGrid no está inicializado aún (constructionGrid es null).");
            return;
        }
        
        if (initialConstructedLayers <= 0)
        {
            if (showDebugMessages)
                Debug.Log("BridgeInitialConstructor: No se construye nada (initialConstructedLayers = 0)");
            return;
        }
        
        if (showDebugMessages)
            Debug.Log($"BridgeInitialConstructor: Iniciando construcción hasta la capa {initialConstructedLayers}");
        
        
        int quadrantsConstructed = 0;
        int layersConstructed = 0;
        
        // Recorrer toda la grilla del puente
        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                // Verificar si debemos construir este cuadrante específico
                if (!ShouldConstructQuadrant(x, z))
                    continue;
                
                // Verificar si es un cuadrante válido
                if (!IsValidQuadrant(x, z))
                    continue;
                
                if (showDebugMessages)
                    Debug.Log($"Construyendo cuadrante [{x},{z}] hasta la capa {initialConstructedLayers}");
                
                bool quadrantSuccess = true;

                // Obtener SO para validar cuántas capas soporta este cuadrante (evita out-of-range en presets raros)
                var so = bridgeGrid.GetQuadrantSO(x, z);
                if (so == null)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"  ✗ QuadrantSO nulo en [{x},{z}]. Saltando.");
                    continue;
                }
                int capasDisponibles = (so.requiredLayers != null) ? so.requiredLayers.Length : 0;
                if (capasDisponibles <= 0)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"  ✗ QuadrantSO en [{x},{z}] no tiene capas definidas (requiredLayers.Length=0). Saltando.");
                    continue;
                }
                int layersToBuild = Mathf.Clamp(initialConstructedLayers, 0, capasDisponibles);
                if (layersToBuild <= 0)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"  ✗ layersToBuild calculado en 0 para [{x},{z}] (initial={initialConstructedLayers}, disponibles={capasDisponibles}).");
                    continue;
                }
                
                // Construir las capas una por una hasta el límite especificado
                for (int layer = 0; layer < layersToBuild; layer++)
                {
                    bool layerSuccess = bridgeGrid.TryBuildLayerBySystem(x, z, layer);
                    
                    if (layerSuccess)
                    {
                        layersConstructed++;
                        if (showDebugMessages)
                            Debug.Log($"  ✓ Capa {layer} construida en cuadrante [{x},{z}]");
                    }
                    else
                    {
                        if (showDebugMessages)
                            Debug.LogWarning($"  ✗ No se pudo construir capa {layer} en cuadrante [{x},{z}]");
                        quadrantSuccess = false;
                        break;
                    }
                }
                
                // Si es la última capa y se especificó un estado particular, aplicarlo
                if (quadrantSuccess && layersToBuild == 3 && lastLayerState != BridgeQuadrantSO.LastLayerState.Complete)
                {
                    SetLastLayerState(x, z, lastLayerState);
                }
                
                if (quadrantSuccess)
                {
                    quadrantsConstructed++;
                }
            }
        }
        
        // No se crea ni necesita objeto temporal para construcción por sistema
        
        if (showDebugMessages)
        {
            Debug.Log($"BridgeInitialConstructor: Construcción completada. " +
                     $"Cuadrantes construidos: {quadrantsConstructed}, " +
                     $"Capas totales construidas: {layersConstructed}");
        }
    }

    // Usa reflexión para comprobar si el campo privado 'constructionGrid' del BridgeConstructionGrid está asignado
    private bool IsGridBackerAllocated()
    {
        if (bridgeGrid == null) return false;
        var gridType = bridgeGrid.GetType();
        var field = gridType.GetField("constructionGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null) return false;
        var value = field.GetValue(bridgeGrid);
        return value != null;
    }

    // Intenta invocar el método privado InitializeGrid() del BridgeConstructionGrid si aún no está inicializado
    private void EnsureGridInitialized()
    {
        if (bridgeGrid == null) return;
        if (IsGridBackerAllocated()) return; // ya está

        var gridType = bridgeGrid.GetType();
        var init = gridType.GetMethod("InitializeGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (init != null)
        {
            init.Invoke(bridgeGrid, null);
        }
    }
    
    /// <summary>
    /// Verifica si debemos construir un cuadrante específico
    /// </summary>
    private bool ShouldConstructQuadrant(int x, int z)
    {
        if (constructAllQuadrants)
            return true;
        
        if (specificQuadrants == null || specificQuadrants.Length == 0)
            return true;
        
        // Calcular índice lineal del cuadrante
        int index = x * bridgeGrid.gridLength + z;
        
        if (index >= 0 && index < specificQuadrants.Length)
            return specificQuadrants[index];
        
        return false;
    }
    
    /// <summary>
    /// Verifica si un cuadrante es válido usando reflexión para acceder al método privado
    /// </summary>
    private bool IsValidQuadrant(int x, int z)
    {
        // Verificación básica de límites
        if (x < 0 || x >= bridgeGrid.gridWidth || z < 0 || z >= bridgeGrid.gridLength)
            return false;
        
        // Usar reflexión para acceder al método IsValidQuadrant del BridgeConstructionGrid
        System.Type gridType = bridgeGrid.GetType();
        System.Reflection.MethodInfo isValidMethod = gridType.GetMethod("IsValidQuadrant", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (isValidMethod != null)
        {
            return (bool)isValidMethod.Invoke(bridgeGrid, new object[] { x, z });
        }
        
        // Si no podemos acceder al método, asumir que es válido
        return true;
    }
    
    /// <summary>
    /// Establece el estado de la última capa de un cuadrante específico
    /// </summary>
    private void SetLastLayerState(int x, int z, BridgeQuadrantSO.LastLayerState state)
    {
        // Usar reflexión para acceder al grid interno
        System.Type gridType = bridgeGrid.GetType();
        System.Reflection.FieldInfo gridField = gridType.GetField("constructionGrid", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (gridField != null)
        {
            object[,] grid = gridField.GetValue(bridgeGrid) as object[,];
            if (grid != null && x < grid.GetLength(0) && z < grid.GetLength(1))
            {
                object quadrantInfo = grid[x, z];
                if (quadrantInfo != null)
                {
                    System.Type quadrantInfoType = quadrantInfo.GetType();
                    System.Reflection.FieldInfo quadrantSOField = quadrantInfoType.GetField("quadrantSO");
                    
                    if (quadrantSOField != null)
                    {
                        BridgeQuadrantSO quadrantSO = quadrantSOField.GetValue(quadrantInfo) as BridgeQuadrantSO;
                        if (quadrantSO != null)
                        {
                            quadrantSO.lastLayerState = state;
                            if (showDebugMessages)
                                Debug.Log($"Estado de última capa establecido a {state} en cuadrante [{x},{z}]");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Método para reinicializar el puente (destruir todas las capas)
    /// </summary>
    [ContextMenu("Reinicializar Puente")]
    public void ResetBridge()
    {
        if (bridgeGrid == null)
        {
            Debug.LogError("BridgeInitialConstructor: No hay BridgeConstructionGrid asignado");
            return;
        }
        
        if (showDebugMessages)
            Debug.Log("BridgeInitialConstructor: Reinicializando puente...");
        
        // Usar reflexión para acceder al método DebugRellenarTodoPuente y luego resetear
        System.Type gridType = bridgeGrid.GetType();
        System.Reflection.FieldInfo gridField = gridType.GetField("constructionGrid", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (gridField != null)
        {
            object[,] grid = gridField.GetValue(bridgeGrid) as object[,];
            if (grid != null)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    for (int z = 0; z < grid.GetLength(1); z++)
                    {
                        object quadrantInfo = grid[x, z];
                        if (quadrantInfo != null)
                        {
                            // Resetear el ScriptableObject del cuadrante
                            System.Type quadrantInfoType = quadrantInfo.GetType();
                            System.Reflection.FieldInfo quadrantSOField = quadrantInfoType.GetField("quadrantSO");
                            
                            if (quadrantSOField != null)
                            {
                                BridgeQuadrantSO quadrantSO = quadrantSOField.GetValue(quadrantInfo) as BridgeQuadrantSO;
                                if (quadrantSO != null)
                                {
                                    // Reinicializar el ScriptableObject
                                    quadrantSO.Initialize();
                                }
                            }
                        }
                    }
                }
            }
        }
        
        if (showDebugMessages)
            Debug.Log("BridgeInitialConstructor: Puente reinicializado");
    }
    
    /// <summary>
    /// Método para configurar cuadrantes específicos desde el inspector
    /// </summary>
    [ContextMenu("Configurar Cuadrantes Específicos")]
    public void ConfigureSpecificQuadrants()
    {
        if (bridgeGrid == null)
        {
            Debug.LogError("BridgeInitialConstructor: No hay BridgeConstructionGrid asignado");
            return;
        }
        
        int totalQuadrants = bridgeGrid.gridWidth * bridgeGrid.gridLength;
        specificQuadrants = new bool[totalQuadrants];
        
        // Por defecto, todos los cuadrantes están seleccionados
        for (int i = 0; i < totalQuadrants; i++)
        {
            specificQuadrants[i] = true;
        }
        
        Debug.Log($"BridgeInitialConstructor: Configuración de cuadrantes específicos creada ({totalQuadrants} cuadrantes)");
    }
    
    /// <summary>
    /// Método para validar la configuración
    /// </summary>
    [ContextMenu("Validar Configuración")]
    public void ValidateConfiguration()
    {
        Debug.Log("=== VALIDACIÓN DE CONFIGURACIÓN ===");
        
        if (bridgeGrid == null)
        {
            Debug.LogError("✗ BridgeConstructionGrid no asignado");
        }
        else
        {
            Debug.Log("✓ BridgeConstructionGrid asignado");
            Debug.Log($"  - Tamaño de grilla: {bridgeGrid.gridWidth}x{bridgeGrid.gridLength}");
        }
        
        Debug.Log($"✓ Capas iniciales a construir: {initialConstructedLayers}");
        Debug.Log($"✓ Construir todos los cuadrantes: {constructAllQuadrants}");
        
        if (!constructAllQuadrants)
        {
            if (specificQuadrants != null)
            {
                int selectedCount = 0;
                for (int i = 0; i < specificQuadrants.Length; i++)
                {
                    if (specificQuadrants[i]) selectedCount++;
                }
                Debug.Log($"✓ Cuadrantes específicos configurados: {selectedCount}/{specificQuadrants.Length}");
            }
            else
            {
                Debug.LogWarning("⚠ Cuadrantes específicos no configurados");
            }
        }
        
        Debug.Log($"✓ Estado de última capa: {lastLayerState}");
        Debug.Log($"✓ Aplicar al inicio: {applyOnStart}");
    }
}
