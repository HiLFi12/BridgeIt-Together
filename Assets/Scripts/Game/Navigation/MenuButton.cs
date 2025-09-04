using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para botones de menú que facilita la navegación entre escenas.
/// Simplemente agrega este script a un botón y selecciona el tipo de navegación.
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuButton : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private NavigationType navigationType = NavigationType.MainMenu;
    [SerializeField] private SceneReference customScene = null;

    private Button button;

    /// <summary>
    /// Tipos de navegación disponibles
    /// </summary>
    public enum NavigationType
    {
        MainMenu,
        LevelSelector,
        Credits,
        PrototypeLevel,
        PrehistoricLevels,
    MedievalLevels,
        Back,
        Custom,
        // Nuevos tipos específicos para fin de juego
        NextLevel,
        RestartLevel,
        MenuFromGame,
        LevelSelectFromGame,
        // Nuevo tipo para menú de pausa
        ResumeGame,
        ExitGame
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Conectar el evento del botón
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }        else
        {
            Debug.LogError($"MenuButton en {gameObject.name} requiere un componente Button");
        }
    }

    /// <summary>
    /// Maneja el click del botón
    /// </summary>
    private void OnButtonClick()
    {
        // Primero intentar usar SceneNavigatorCanvas si está disponible
        var canvasNavigator = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (canvasNavigator != null)
        {
            switch (navigationType)
            {
                case NavigationType.MainMenu:
                    SceneNavigatorCanvas.NavigateToMainMenu();
                    break;
                    
                case NavigationType.LevelSelector:
                    SceneNavigatorCanvas.NavigateToLevelSelector();
                    break;
                    
                case NavigationType.Credits:
                    SceneNavigatorCanvas.NavigateToCredits();
                    break;
                    
                case NavigationType.PrototypeLevel:
                    SceneNavigatorCanvas.NavigateToPrototypeLevel();
                    break;
                    
                case NavigationType.PrehistoricLevels:
                    SceneNavigatorCanvas.NavigateToPrehistoricLevels();
                    break;
                case NavigationType.MedievalLevels:
                    SceneNavigatorCanvas.NavigateToMedievalLevels();
                    break;
                    
                case NavigationType.Back:
                    SceneNavigatorCanvas.NavigateBack();
                    break;
                    
                case NavigationType.Custom:
                    if (customScene != null && customScene.IsValid())
                    {
                        customScene.Load();
                    }
                    else
                    {
                        Debug.LogError("Custom scene no está asignada o no es válida cuando se selecciona navegación Custom");
                    }
                    break;
                    
                case NavigationType.NextLevel:
                    HandleNextLevel();
                    break;
                    
                case NavigationType.RestartLevel:
                    HandleRestartLevel();
                    break;
                    
                case NavigationType.MenuFromGame:
                    HandleMenuFromGame();
                    break;
                    
                case NavigationType.LevelSelectFromGame:
                    HandleLevelSelectFromGame();
                    break;
                    
                case NavigationType.ResumeGame:
                    HandleResumeGame();
                    break;
                case NavigationType.ExitGame:
                    HandleExitGame();
                    break;
            }
            return;
        }

        // Si no hay SceneNavigatorCanvas, usar SceneNavigator tradicional
        switch (navigationType)
        {
            case NavigationType.MainMenu:
                SceneNavigator.NavigateToMainMenu();
                break;
                
            case NavigationType.LevelSelector:
                SceneNavigator.NavigateToLevelSelector();
                break;
                
            case NavigationType.Credits:
                SceneNavigator.NavigateToCredits();
                break;
                
            case NavigationType.PrototypeLevel:
                SceneNavigator.NavigateToPrototypeLevel();
                break;
                
            case NavigationType.PrehistoricLevels:
                // Para SceneNavigator tradicional, no hay implementación específica
                // porque PrehistoricLevels es solo para navegación por Canvas
                Debug.LogWarning("PrehistoricLevels solo está disponible con SceneNavigatorCanvas");
                break;
                case NavigationType.MedievalLevels:
                    // Para SceneNavigator tradicional, no hay implementación específica
                    Debug.LogWarning("MedievalLevels solo está disponible con SceneNavigatorCanvas");
                    break;
                
            case NavigationType.Back:
                SceneNavigator.NavigateBack();
                break;
                case NavigationType.Custom:
                    if (customScene != null && customScene.IsValid())
                    {
                        customScene.Load();
                    }
                    else
                    {
                        Debug.LogError("Custom escena no está asignada o no es válida cuando se selecciona navegación Custom");
                    }
                    break;
                
            case NavigationType.NextLevel:
                HandleNextLevel();
                break;
                
            case NavigationType.RestartLevel:
                HandleRestartLevel();
                break;
                
            case NavigationType.MenuFromGame:
                HandleMenuFromGame();
                break;
                
            case NavigationType.LevelSelectFromGame:
                HandleLevelSelectFromGame();
                break;
                
            case NavigationType.ResumeGame:
                HandleResumeGame();
                break;
            case NavigationType.ExitGame:
                HandleExitGame();
                break;

            default:
                Debug.LogWarning($"Tipo de navegación no implementado: {navigationType}");
                break;
        }
    }

    /// <summary>
    /// Permite cambiar el tipo de navegación por código
    /// </summary>
    /// <param name="newNavigationType">Nuevo tipo de navegación</param>
    public void SetNavigationType(NavigationType newNavigationType)
    {
        navigationType = newNavigationType;
    }

    /// <summary>
    /// Permite establecer una escena customizada por código
    /// </summary>
    /// <param name="sceneName">Nombre de la escena custom</param>
    public void SetCustomSceneName(string sceneName)
    {
        if (customScene == null) customScene = new SceneReference();
        customScene.SetFromSceneName(sceneName);
        navigationType = NavigationType.Custom;
    }
    private void OnDestroy()
    {
        // Limpiar listener del botón
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    #region Editor Helper
    
    private void OnValidate()
    {
        // Validación en el editor: si se usa Custom, asegurar que se haya asignado una escena
        if (navigationType == NavigationType.Custom && (customScene == null || !customScene.IsValid()))
        {
            Debug.LogWarning($"MenuButton en {gameObject.name}: Se requiere una Scene asignada cuando se usa navegación Custom");
        }
    }
    
    #endregion

    #region End Game Navigation Methods
    
    /// <summary>
    /// Maneja la navegación al siguiente nivel
    /// </summary>
    private void HandleNextLevel()
    {
        // Buscar GameConditionManager para obtener información del nivel actual
        var gameManager = FindFirstObjectByType<GameConditionManager>();
        if (gameManager != null)
        {
            // El GameConditionManager debe manejar la lógica de siguiente nivel
            gameManager.NavigateToNextLevel();
        }
        else
        {
            Debug.LogWarning("No se encontró GameConditionManager para manejar el siguiente nivel");
            // Fallback: recargar la escena actual
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    /// <summary>
    /// Maneja el reinicio del nivel actual
    /// </summary>
    private void HandleRestartLevel()
    {
        // Reiniciar la escena actual
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Maneja la navegación al menú principal desde el juego
    /// </summary>
    private void HandleMenuFromGame()
    {
        // Usar el nuevo método que configura MainMenu como estado inicial
        SceneNavigatorCanvas.NavigateToMainMenuFromGame();
    }
    
    /// <summary>
    /// Maneja la navegación al selector de niveles desde el juego
    /// </summary>
    private void HandleLevelSelectFromGame()
    {
        // Usar el nuevo método que configura LevelSelector como estado inicial
        SceneNavigatorCanvas.NavigateToLevelSelectorFromGame();
    }
    
    /// <summary>
    /// Maneja la reanudación del juego desde el menú de pausa
    /// </summary>
    private void HandleResumeGame()
    {
        // Buscar GameConditionManager y reanudar el juego
        var gameManager = FindFirstObjectByType<GameConditionManager>();
        if (gameManager != null)
        {
            gameManager.ReanudarJuego();
        }
    }

    private void HandleExitGame()
    {
        Application.Quit();
    }
    
    #endregion
}
