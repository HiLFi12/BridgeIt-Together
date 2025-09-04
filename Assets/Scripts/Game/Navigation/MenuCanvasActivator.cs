using UnityEngine;

/// <summary>
/// Script helper para activar el canvas correcto en la escena Menu
/// basándose en preferencias guardadas por otros sistemas
/// </summary>
public class MenuCanvasActivator : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject prehistoricLevelsCanvas;
    [SerializeField] private GameObject levelSelectorCanvas;
    [SerializeField] private GameObject creditsCanvas;
    
    [Header("Debug")]
    [SerializeField] private bool mostrarDebugInfo = true;
    
    private void Start()
    {
        // Verificar si SceneNavigatorCanvas está presente
        var sceneNavigatorCanvas = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (sceneNavigatorCanvas != null)
        {
            return; // No hacer nada si SceneNavigatorCanvas está manejando la navegación
        }
        
        // Solo ejecutar si no hay SceneNavigatorCanvas
        ActivarCanvasSegunPreferencia();
    }
    
    /// <summary>
    /// Activa el canvas apropiado basándose en las preferencias guardadas
    /// </summary>
    private void ActivarCanvasSegunPreferencia()
    {
        // Verificar si hay una preferencia guardada sobre qué canvas activar
        string canvasToActivate = PlayerPrefs.GetString("MenuCanvasToActivate", "");
        
        if (!string.IsNullOrEmpty(canvasToActivate))
        {
            // Limpiar la preferencia después de usarla
            PlayerPrefs.DeleteKey("MenuCanvasToActivate");
            PlayerPrefs.Save();
            
            if (mostrarDebugInfo)
            {
                // Activando canvas específico en Menu
            }
            
            switch (canvasToActivate)
            {
                case "PrehistoricLevels":
                    ActivarCanvas(prehistoricLevelsCanvas, "PrehistoricLevels");
                    break;
                    
                case "LevelSelector":
                    ActivarCanvas(levelSelectorCanvas, "LevelSelector");
                    break;
                    
                case "Credits":
                    ActivarCanvas(creditsCanvas, "Credits");
                    break;
                    
                default:
                    ActivarCanvas(mainMenuCanvas, "MainMenu (default)");
                    break;
            }
        }
        else
        {
            // No hay preferencia específica, activar menú principal por defecto
            ActivarCanvas(mainMenuCanvas, "MainMenu (no preference)");
        }
    }
    
    /// <summary>
    /// Activa un canvas específico y desactiva los demás
    /// </summary>
    /// <param name="canvasToActivate">Canvas a activar</param>
    /// <param name="canvasName">Nombre del canvas para debug</param>
    private void ActivarCanvas(GameObject canvasToActivate, string canvasName)
    {
        // Desactivar todos los canvas
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (prehistoricLevelsCanvas != null) prehistoricLevelsCanvas.SetActive(false);
        if (levelSelectorCanvas != null) levelSelectorCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
        
        // Activar el canvas objetivo
        if (canvasToActivate != null)
        {
            canvasToActivate.SetActive(true);
            
            if (mostrarDebugInfo)
            {
                Debug.Log($"✅ Canvas activado: {canvasName}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Canvas {canvasName} no está asignado, activando MainMenu como fallback");
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.SetActive(true);
            }
        }
    }
    
    #region Public Methods para uso desde otros scripts
    
    /// <summary>
    /// Establece una preferencia para activar un canvas específico en la próxima carga del menú
    /// </summary>
    /// <param name="canvasName">Nombre del canvas a activar</param>
    public static void SetCanvasPreference(string canvasName)
    {
        PlayerPrefs.SetString("MenuCanvasToActivate", canvasName);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Navega al menú con un canvas específico
    /// </summary>
    /// <param name="canvasName">Nombre del canvas a activar</param>
    public static void NavigateToMenuWithCanvas(string canvasName)
    {
        SetCanvasPreference(canvasName);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    
    #endregion
    
    #region Context Menu para Testing
    
    [ContextMenu("Test - Activar MainMenu")]
    public void TestActivarMainMenu()
    {
        ActivarCanvas(mainMenuCanvas, "MainMenu (test)");
    }
    
    [ContextMenu("Test - Activar PrehistoricLevels")]
    public void TestActivarPrehistoricLevels()
    {
        ActivarCanvas(prehistoricLevelsCanvas, "PrehistoricLevels (test)");
    }
    
    [ContextMenu("Test - Activar LevelSelector")]
    public void TestActivarLevelSelector()
    {
        ActivarCanvas(levelSelectorCanvas, "LevelSelector (test)");
    }
    
    [ContextMenu("Test - Simular Preferencia PrehistoricLevels")]
    public void TestSimularPreferenciaPrehistoric()
    {
        PlayerPrefs.SetString("MenuCanvasToActivate", "PrehistoricLevels");
        ActivarCanvasSegunPreferencia();
    }
    
    #endregion
}
