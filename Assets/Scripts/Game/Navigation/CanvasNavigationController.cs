using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de navegación por Canvas para una experiencia de menú unificada.
/// Maneja la activación/desactivación de diferentes Canvas en lugar de cambiar escenas.
/// Solo el botón Prototype Level carga una escena diferente.
/// </summary>
public class CanvasNavigationController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Canvas levelSelectorCanvas;
    [SerializeField] private Canvas creditsCanvas;
    [SerializeField] private Canvas prehistoricLevelsCanvas;
    
    [Header("Scene Names")]
    [SerializeField] private string prototypeLevelSceneName = "PrototypeLevel";
    
    [Header("Audio Settings")]
    [SerializeField] private int menuBGMIndex = 0;
    
    // Singleton instance para fácil acceso
    public static CanvasNavigationController Instance { get; private set; }
    
    // Estado actual del menú
    public enum MenuState
    {
        MainMenu,
        LevelSelector,
        Credits,
        PrehistoricLevels
    }
    
    private MenuState currentState = MenuState.MainMenu;
    private AudioManager audioManager;
    
    private void Awake()
    {
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Obtener referencia al AudioManager
        audioManager = FindFirstObjectByType<AudioManager>();
    }
    
    private void Start()
    {
        // Asegurar que solo el menú principal esté activo al inicio
        ShowMainMenu();
        
        // Reproducir música de menú
        PlayMenuMusic();
    }
    
    #region Public Navigation Methods
    
    /// <summary>
    /// Muestra el menú principal
    /// </summary>
    public void ShowMainMenu()
    {
        SetCanvasActive(MenuState.MainMenu);
        currentState = MenuState.MainMenu;
        Debug.Log("Navegando a: Menú Principal");
    }
    
    /// <summary>
    /// Muestra el selector de niveles
    /// </summary>
    public void ShowLevelSelector()
    {
        SetCanvasActive(MenuState.LevelSelector);
        currentState = MenuState.LevelSelector;
        Debug.Log("Navegando a: Selector de Niveles");
    }
    
    /// <summary>
    /// Muestra los créditos
    /// </summary>
    public void ShowCredits()
    {
        SetCanvasActive(MenuState.Credits);
        currentState = MenuState.Credits;
        Debug.Log("Navegando a: Créditos");
    }
    
    /// <summary>
    /// Muestra los niveles prehistóricos
    /// </summary>
    public void ShowPrehistoricLevels()
    {
        SetCanvasActive(MenuState.PrehistoricLevels);
        currentState = MenuState.PrehistoricLevels;
        Debug.Log("Navegando a: Niveles Prehistóricos");
    }
    
    /// <summary>
    /// Navega al nivel prototipo (carga escena)
    /// </summary>
    public void GoToPrototypeLevel()
    {
        if (string.IsNullOrEmpty(prototypeLevelSceneName))
        {
            Debug.LogError("Nombre de escena del nivel prototipo no configurado");
            return;
        }
        
        Debug.Log($"Cargando escena: {prototypeLevelSceneName}");
        SceneManager.LoadScene(prototypeLevelSceneName);
    }
    
    /// <summary>
    /// Navega hacia atrás según el contexto actual
    /// </summary>
    public void GoBack()
    {
        switch (currentState)
        {
            case MenuState.LevelSelector:
                ShowMainMenu();
                break;
            case MenuState.Credits:
                ShowMainMenu();
                break;
            case MenuState.PrehistoricLevels:
                ShowLevelSelector();
                break;
            case MenuState.MainMenu:
                // Opcional: Salir del juego o mostrar confirmación
                Debug.Log("Ya estás en el menú principal");
                break;
        }
    }
    
    #endregion
    
    #region Canvas Management
    
    /// <summary>
    /// Activa el canvas correspondiente y desactiva los demás
    /// </summary>
    /// <param name="targetState">Estado del menú a mostrar</param>
    private void SetCanvasActive(MenuState targetState)
    {
        // Desactivar todos los canvas
        if (mainMenuCanvas != null) mainMenuCanvas.gameObject.SetActive(false);
        if (levelSelectorCanvas != null) levelSelectorCanvas.gameObject.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.gameObject.SetActive(false);
        
        // Activar el canvas correspondiente
        switch (targetState)
        {
            case MenuState.MainMenu:
                if (mainMenuCanvas != null)
                    mainMenuCanvas.gameObject.SetActive(true);
                else
                    Debug.LogWarning("Main Menu Canvas no está asignado");
                break;
                
            case MenuState.LevelSelector:
                if (levelSelectorCanvas != null)
                    levelSelectorCanvas.gameObject.SetActive(true);
                else
                    Debug.LogWarning("Level Selector Canvas no está asignado");
                break;
                
            case MenuState.Credits:
                if (creditsCanvas != null)
                    creditsCanvas.gameObject.SetActive(true);
                else
                    Debug.LogWarning("Credits Canvas no está asignado");
                break;
        }
    }
    
    #endregion
    
    #region Audio Management
    
    /// <summary>
    /// Reproduce la música de menú
    /// DESHABILITADO: Evitar conflictos con SceneNavigatorCanvas
    /// </summary>
    private void PlayMenuMusic()
    {
        // Verificar si SceneNavigatorCanvas está manejando la música
        var sceneNavCanvas = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (sceneNavCanvas != null)
        {
            return;
        }
        
        // Solo ejecutar si no hay SceneNavigatorCanvas
        if (audioManager != null)
        {
            audioManager.PlayBGM(menuBGMIndex);
        }
        else
        {
            Debug.LogWarning("AudioManager no encontrado");
        }
    }
    
    /// <summary>
    /// Cambia la música de menú
    /// </summary>
    /// <param name="newBGMIndex">Nuevo índice de música</param>
    public void SetMenuBGM(int newBGMIndex)
    {
        menuBGMIndex = newBGMIndex;
        PlayMenuMusic();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Obtiene el estado actual del menú
    /// </summary>
    /// <returns>Estado actual</returns>
    public MenuState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Verifica si estamos en un estado específico
    /// </summary>
    /// <param name="state">Estado a verificar</param>
    /// <returns>True si estamos en ese estado</returns>
    public bool IsInState(MenuState state)
    {
        return currentState == state;
    }
    
    /// <summary>
    /// Asigna referencias de Canvas automáticamente si no están asignadas
    /// </summary>
    [ContextMenu("Auto-Assign Canvas References")]
    public void AutoAssignCanvasReferences()
    {
        Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in allCanvas)
        {
            string canvasName = canvas.name.ToLower();
            
            if (canvasName.Contains("menu") && canvasName.Contains("main") && mainMenuCanvas == null)
            {
                mainMenuCanvas = canvas;
                Debug.Log($"Auto-asignado Main Menu Canvas: {canvas.name}");
            }
            else if (canvasName.Contains("level") && canvasName.Contains("selector") && levelSelectorCanvas == null)
            {
                levelSelectorCanvas = canvas;
                Debug.Log($"Auto-asignado Level Selector Canvas: {canvas.name}");
            }
            else if (canvasName.Contains("credits") && creditsCanvas == null)
            {
                creditsCanvas = canvas;
                Debug.Log($"Auto-asignado Credits Canvas: {canvas.name}");
            }
        }
    }
    
    #endregion
    
    #region Static Methods for Easy Access
    
    /// <summary>
    /// Navegación estática al menú principal
    /// </summary>
    public static void NavigateToMainMenu()
    {
        if (Instance != null)
            Instance.ShowMainMenu();
        else
            Debug.LogError("CanvasNavigationController instance no encontrada");
    }
    
    /// <summary>
    /// Navegación estática al selector de niveles
    /// </summary>
    public static void NavigateToLevelSelector()
    {
        if (Instance != null)
            Instance.ShowLevelSelector();
        else
            Debug.LogError("CanvasNavigationController instance no encontrada");
    }
    
    /// <summary>
    /// Navegación estática a créditos
    /// </summary>
    public static void NavigateToCredits()
    {
        if (Instance != null)
            Instance.ShowCredits();
        else
            Debug.LogError("CanvasNavigationController instance no encontrada");
    }
    
    /// <summary>
    /// Navegación estática al nivel prototipo
    /// </summary>
    public static void NavigateToPrototypeLevel()
    {
        if (Instance != null)
            Instance.GoToPrototypeLevel();
        else
            Debug.LogError("CanvasNavigationController instance no encontrada");
    }
    
    /// <summary>
    /// Navegación estática hacia atrás
    /// </summary>
    public static void NavigateBack()
    {
        if (Instance != null)
            Instance.GoBack();
        else
            Debug.LogError("CanvasNavigationController instance no encontrada");
    }
    
    #endregion
}
