using UnityEngine;

/// <summary>
/// Script helper para configurar automáticamente los canvas de fin de juego en GameConditionManager
/// </summary>
public class GameConditionCanvasSetup : MonoBehaviour
{
    [Header("Referencias de Canvas Prefabs")]
    [SerializeField] private GameObject victoryCanvasPrefab;
    [SerializeField] private GameObject defeatCanvasPrefab;
    
    [Header("Configuración de Desactivación")]
    [SerializeField] private bool desactivarPlayerController = true;
    [SerializeField] private bool desactivarAutoGenerator = true;
    [SerializeField] private bool activarAnimacionesFinDeJuego = true;
    
    [Header("Auto-Configuración")]
    [SerializeField] private bool configurarAutomaticamenteAlIniciar = true;
    [SerializeField] private bool buscarPrefabsAutomaticamente = true;
    
    private GameConditionManager gameConditionManager;
    
    #region Unity Events
    
    private void Start()
    {
        if (configurarAutomaticamenteAlIniciar)
        {
            ConfigurarGameConditionManager();
        }
    }
    
    #endregion
    
    #region Configuración Automática
    
    /// <summary>
    /// Configura automáticamente el GameConditionManager con los canvas prefabs
    /// </summary>
    [ContextMenu("Configurar GameConditionManager")]
    public void ConfigurarGameConditionManager()
    {
        // Buscar GameConditionManager
        if (gameConditionManager == null)
        {
            gameConditionManager = GameConditionManager.Instance;
            if (gameConditionManager == null)
            {
                gameConditionManager = FindFirstObjectByType<GameConditionManager>();
            }
        }
        
        if (gameConditionManager == null)
        {
            Debug.LogError("[GameConditionCanvasSetup] No se encontró GameConditionManager en la escena");
            return;
        }
        
        // Buscar prefabs automáticamente si están vacíos
        if (buscarPrefabsAutomaticamente)
        {
            BuscarPrefabsAutomaticamente();
        }
        
        // Configurar canvas prefabs
        gameConditionManager.ConfigurarCanvasPrefabs(victoryCanvasPrefab, defeatCanvasPrefab);
        
        // Configurar opciones de sistemas de fin de juego
        gameConditionManager.ConfigurarSistemasFinDeJuego(desactivarPlayerController, desactivarAutoGenerator, activarAnimacionesFinDeJuego);
        
        Debug.Log("[GameConditionCanvasSetup] ✅ GameConditionManager configurado correctamente");
    }
    
    /// <summary>
    /// Busca automáticamente los prefabs de Victory y Defeat en la carpeta Resources
    /// </summary>
    [ContextMenu("Buscar Prefabs Automáticamente")]
    public void BuscarPrefabsAutomaticamente()
    {
        if (victoryCanvasPrefab == null)
        {
            // Intentar cargar desde Resources
            GameObject victoryPrefab = Resources.Load<GameObject>("Victory");
            if (victoryPrefab == null)
            {
                // Buscar en Assets/Prefabs/GameConditions/
                victoryPrefab = Resources.Load<GameObject>("GameConditions/Victory");
            }
            
            if (victoryPrefab != null)
            {
                victoryCanvasPrefab = victoryPrefab;
                Debug.Log($"✅ Victory Canvas Prefab encontrado automáticamente: {victoryPrefab.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo encontrar automáticamente el Victory Canvas Prefab. Asígnalo manualmente.");
            }
        }
        
        if (defeatCanvasPrefab == null)
        {
            // Intentar cargar desde Resources
            GameObject defeatPrefab = Resources.Load<GameObject>("Defeat");
            if (defeatPrefab == null)
            {
                // Buscar en Assets/Prefabs/GameConditions/
                defeatPrefab = Resources.Load<GameObject>("GameConditions/Defeat");
            }
            
            if (defeatPrefab != null)
            {
                defeatCanvasPrefab = defeatPrefab;
                Debug.Log($"✅ Defeat Canvas Prefab encontrado automáticamente: {defeatPrefab.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo encontrar automáticamente el Defeat Canvas Prefab. Asígnalo manualmente.");
            }
        }
    }
    
    #endregion
    
    #region Métodos Públicos
    
    /// <summary>
    /// Configura manualmente los prefabs de canvas
    /// </summary>
    public void ConfigurarCanvasPrefabs(GameObject victory, GameObject defeat)
    {
        victoryCanvasPrefab = victory;
        defeatCanvasPrefab = defeat;
        
        Debug.Log($"Canvas Prefabs configurados manualmente - Victory: {(victory?.name ?? "null")}, Defeat: {(defeat?.name ?? "null")}");
    }
    
    /// <summary>
    /// Configura las opciones de desactivación
    /// </summary>
    public void ConfigurarOpcionesDesactivacion(bool desactivarPlayer, bool desactivarAutoGen)
    {
        desactivarPlayerController = desactivarPlayer;
        desactivarAutoGenerator = desactivarAutoGen;
        
        Debug.Log($"Opciones de desactivación configuradas - Player: {desactivarPlayer}, AutoGenerator: {desactivarAutoGen}");
    }
    
    /// <summary>
    /// Configura todas las opciones de sistemas de fin de juego
    /// </summary>
    public void ConfigurarSistemasFinDeJuego(bool desactivarPlayer, bool desactivarAutoGen, bool activarAnimaciones)
    {
        desactivarPlayerController = desactivarPlayer;
        desactivarAutoGenerator = desactivarAutoGen;
        activarAnimacionesFinDeJuego = activarAnimaciones;
        
        Debug.Log($"Sistemas de fin de juego configurados - Player: {desactivarPlayer}, AutoGenerator: {desactivarAutoGen}, Animaciones: {activarAnimaciones}");
    }
    
    #endregion
}
