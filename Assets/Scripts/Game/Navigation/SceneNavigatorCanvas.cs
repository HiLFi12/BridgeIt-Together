using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Administrador de navegación modificado para usar Canvas en lugar de escenas.
/// Solo el nivel prototipo navega realmente a una escena diferente.
/// Soporta paneles dentro de Canvas para mejor organización de UI.
/// </summary>
[DefaultExecutionOrder(-100)] // Ejecutar antes que otros scripts de navegación
public class SceneNavigatorCanvas : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string prototypeLevelSceneName = "PrototypeLevel";
    
    [Header("Canvas References")]
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Canvas levelSelectorCanvas;
    [SerializeField] private Canvas creditsCanvas;
    [SerializeField] private Canvas prehistoricLevelsCanvas;
    [SerializeField] private Canvas medievalLevelsCanvas;
    
    [Header("Panel References (dentro de LevelSelector)")]
    [SerializeField] private GameObject prehistoricLevelsPanel;
    [SerializeField] private GameObject medievalLevelsPanel;
    
    [Header("Audio Settings")]
    [SerializeField] private int menuBGMIndex = 0;

    // Singleton instance para fácil acceso
    public static SceneNavigatorCanvas Instance { get; private set; }
    
    // Sistema de estado inicial configurado desde otras escenas
    private static MenuState? requestedInitialState = null;
    
    // Estado actual del menú
    public enum MenuState
    {
        MainMenu,
        LevelSelector,
        Credits,
    PrehistoricLevels,
    MedievalLevels
    }
    
    private MenuState currentState = MenuState.MainMenu;
    private AudioManager audioManager;
    private bool musicSetup = false;
    private bool hasStarted = false; // Prevenir múltiples ejecuciones de Start()

    private void Awake()
    {
        // Verificar si estamos en la escena Menu
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isMenuScene = currentSceneName.Equals("Menu", System.StringComparison.OrdinalIgnoreCase);
        
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
            if (!isMenuScene)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            if (isMenuScene)
            {
                if (Instance.gameObject != gameObject)
                {
                    Destroy(Instance.gameObject);
                }
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // Buscar AudioManager
        audioManager = FindAnyObjectByType<AudioManager>();
        if (audioManager == null)
        {
            // AudioManager not found in scene
        }
    }

    private void Start()
    {
        if (hasStarted)
        {
            return;
        }
        hasStarted = true;
        
        // Auto-asignar referencias de canvas si no están configuradas
        AutoAssignCanvasReferences();
        
        // Configurar música una sola vez
        SetupMenuMusic();
        
        // Configurar el estado inicial
        MenuState initialState = requestedInitialState ?? MenuState.MainMenu;
        if (requestedInitialState.HasValue)
        {
            requestedInitialState = null;
        }
        
        // Activar canvas inicial
        SetCanvasActive(initialState);
    }

    /// <summary>
    /// Busca y asigna automáticamente las referencias de Canvas si están vacías
    /// </summary>
    public void AutoAssignCanvasReferences()
    {
        Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Canvas canvas in allCanvas)
        {
            string canvasName = canvas.name.ToLower();
            
            // Asignar Canvas por nombre exacto
            if (mainMenuCanvas == null && canvasName == "menu")
            {
                mainMenuCanvas = canvas;
            }
            else if (levelSelectorCanvas == null && canvasName == "levelselector")
            {
                levelSelectorCanvas = canvas;
            }
            else if (creditsCanvas == null && canvasName == "credits")
            {
                creditsCanvas = canvas;
            }
            else if (prehistoricLevelsCanvas == null && canvasName == "prehistoriclevels")
            {
                prehistoricLevelsCanvas = canvas;
            }
            else if (medievalLevelsCanvas == null && canvasName == "medievallevels")
            {
                medievalLevelsCanvas = canvas;
            }
        }
        
        // Buscar el panel PrehistoricLevels dentro del LevelSelector
        if (levelSelectorCanvas != null && prehistoricLevelsPanel == null)
        {
            Transform[] allTransforms = levelSelectorCanvas.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform t in allTransforms)
            {
                string transformName = t.name.ToLower();
                
                if (transformName.Contains("prehistoric") && t.gameObject != levelSelectorCanvas.gameObject)
                {
                    // Verificar si tiene botones hijos o contiene "levels"
                    Transform[] children = t.GetComponentsInChildren<Transform>();
                    bool hasLevelButtons = false;
                    
                    foreach (Transform child in children)
                    {
                        if (child.name.ToLower().Contains("level") || child.name.ToLower().Contains("button"))
                        {
                            hasLevelButtons = true;
                            break;
                        }
                    }
                    
                    if (hasLevelButtons || transformName.Contains("levels"))
                    {
                        prehistoricLevelsPanel = t.gameObject;
                        break;
                    }
                }
            }
            // Buscar el panel MedievalLevels dentro del LevelSelector
            if (levelSelectorCanvas != null && medievalLevelsPanel == null)
            {
                // Reusar 'allTransforms' declarado arriba
                foreach (Transform t in allTransforms)
                {
                    string transformName = t.name.ToLower();

                    if (transformName.Contains("medieval") && t.gameObject != levelSelectorCanvas.gameObject)
                    {
                        // Verificar si tiene botones hijos o contiene "levels"
                        Transform[] children = t.GetComponentsInChildren<Transform>();
                        bool hasLevelButtons = false;

                        foreach (Transform child in children)
                        {
                            if (child.name.ToLower().Contains("level") || child.name.ToLower().Contains("button"))
                            {
                                hasLevelButtons = true;
                                break;
                            }
                        }

                        if (hasLevelButtons || transformName.Contains("levels"))
                        {
                            medievalLevelsPanel = t.gameObject;
                            break;
                        }
                    }
                }

                // Fallback: buscar cualquier elemento con "medieval"
                if (medievalLevelsPanel == null)
                {
                    foreach (Transform t in allTransforms)
                    {
                        if (t.name.ToLower().Contains("medieval") && t.gameObject != levelSelectorCanvas.gameObject)
                        {
                            medievalLevelsPanel = t.gameObject;
                            break;
                        }
                    }
                }
            }
            
            // Fallback: buscar cualquier elemento con "prehistoric"
            if (prehistoricLevelsPanel == null)
            {
                foreach (Transform t in allTransforms)
                {
                    if (t.name.ToLower().Contains("prehistoric") && t.gameObject != levelSelectorCanvas.gameObject)
                    {
                        prehistoricLevelsPanel = t.gameObject;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fuerza una actualización de las referencias de canvas
    /// </summary>
    private void ForceRefreshCanvasReferences()
    {
        // Limpiar referencias existentes
        mainMenuCanvas = null;
        levelSelectorCanvas = null;
        creditsCanvas = null;
        prehistoricLevelsCanvas = null;
    prehistoricLevelsPanel = null;
    medievalLevelsCanvas = null;
    medievalLevelsPanel = null;
        
        // Reasignar automáticamente
        AutoAssignCanvasReferences();
    }

    /// <summary>
    /// Configura la música del menú (solo una vez por carga de escena)
    /// Incluye sistema de prevención de conflictos con otros scripts de música
    /// </summary>
    private void SetupMenuMusic()
    {
        if (musicSetup) return;
        
        if (audioManager != null)
        {
            // Verificar si ya está sonando la música correcta
            if (audioManager.music != null && 
                audioManager.music.isPlaying && 
                audioManager.music.clip == audioManager.bgTracks[menuBGMIndex])
            {
                musicSetup = true;
                return;
            }
            
            audioManager.PlayBGM(menuBGMIndex);
            musicSetup = true;
        }
    }

    /// <summary>
    /// Activa el canvas correspondiente al estado especificado y desactiva los demás
    /// </summary>
    /// <param name="newState">El nuevo estado de menú</param>
    private void SetCanvasActive(MenuState newState)
    {
        // Desactivar todos los canvas primero
        if (mainMenuCanvas != null) 
        {
            mainMenuCanvas.gameObject.SetActive(false);
        }
        if (levelSelectorCanvas != null) 
        {
            levelSelectorCanvas.gameObject.SetActive(false);
        }
        if (creditsCanvas != null) 
        {
            creditsCanvas.gameObject.SetActive(false);
        }
        if (prehistoricLevelsCanvas != null) 
        {
            prehistoricLevelsCanvas.gameObject.SetActive(false);
        }
        
        // Activar el canvas correspondiente
        switch (newState)
        {
            case MenuState.MainMenu:
                if (mainMenuCanvas != null)
                {
                    mainMenuCanvas.gameObject.SetActive(true);
                }
                break;
                
            case MenuState.LevelSelector:
                if (levelSelectorCanvas != null)
                {
                    levelSelectorCanvas.gameObject.SetActive(true);
                }
                break;
                
            case MenuState.Credits:
                if (creditsCanvas != null)
                {
                    creditsCanvas.gameObject.SetActive(true);
                }
                break;
                
            case MenuState.PrehistoricLevels:
                if (prehistoricLevelsCanvas != null)
                {
                    prehistoricLevelsCanvas.gameObject.SetActive(true);
                }
                else
                {
                    // Buscar el Canvas PrehistoricLevels automáticamente
                    Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    
                    foreach (Canvas canvas in allCanvas)
                    {
                        if (canvas.name.ToLower() == "prehistoriclevels")
                        {
                            prehistoricLevelsCanvas = canvas;
                            prehistoricLevelsCanvas.gameObject.SetActive(true);
                            break;
                        }
                    }
                    
                    if (prehistoricLevelsCanvas == null)
                    {
                        if (levelSelectorCanvas != null)
                        {
                            levelSelectorCanvas.gameObject.SetActive(true);
                        }
                    }
                }
                break;
            case MenuState.MedievalLevels:
                if (medievalLevelsCanvas != null)
                {
                    medievalLevelsCanvas.gameObject.SetActive(true);
                }
                else
                {
                    // Buscar automáticamente
                    Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (Canvas canvas in allCanvas)
                    {
                        if (canvas.name.ToLower() == "medievallevels")
                        {
                            medievalLevelsCanvas = canvas;
                            medievalLevelsCanvas.gameObject.SetActive(true);
                            break;
                        }
                    }

                    if (medievalLevelsCanvas == null)
                    {
                        if (levelSelectorCanvas != null)
                        {
                            levelSelectorCanvas.gameObject.SetActive(true);
                        }
                    }
                }
                break;
        }
        
        currentState = newState;
    }

    /// <summary>
    /// Navega al menú principal
    /// </summary>
    public void GoToMainMenu()
    {
        SceneNavigationEvents.InvokeBeforeMenuChange();
        SetCanvasActive(MenuState.MainMenu);
        currentState = MenuState.MainMenu;
        SceneNavigationEvents.InvokeMenuSpecificEvent("mainmenu");
        SceneNavigationEvents.InvokeAfterMenuChange();
    }

    /// <summary>
    /// Navega al selector de niveles
    /// </summary>
    public void GoToLevelSelector()
    {
        SceneNavigationEvents.InvokeBeforeMenuChange();
        SetCanvasActive(MenuState.LevelSelector);
        currentState = MenuState.LevelSelector;
        SceneNavigationEvents.InvokeMenuSpecificEvent("levelselector");
        SceneNavigationEvents.InvokeAfterMenuChange();
    }

    /// <summary>
    /// Navega a la escena de créditos
    /// </summary>
    public void GoToCredits()
    {
        SceneNavigationEvents.InvokeBeforeMenuChange();
        SetCanvasActive(MenuState.Credits);
        currentState = MenuState.Credits;
        SceneNavigationEvents.InvokeMenuSpecificEvent("credits");
        SceneNavigationEvents.InvokeAfterMenuChange();
    }

    /// <summary>
    /// Navega al menú de niveles prehistóricos
    /// </summary>
    public void GoToPrehistoricLevels()
    {
        SceneNavigationEvents.InvokeBeforeMenuChange();
        
        // Verificar si tenemos la referencia de PrehistoricLevels Canvas
        if (prehistoricLevelsCanvas == null)
        {
            AutoAssignCanvasReferences();
        }
        
        SetCanvasActive(MenuState.PrehistoricLevels);
        currentState = MenuState.PrehistoricLevels;
        
        SceneNavigationEvents.InvokeMenuSpecificEvent("prehistoriclevels");
        SceneNavigationEvents.InvokeAfterMenuChange();
    }

    /// <summary>
    /// Navega al menú de niveles medievales
    /// </summary>
    public void GoToMedievalLevels()
    {
        SceneNavigationEvents.InvokeBeforeMenuChange();

        // Verificar si tenemos la referencia de MedievalLevels Canvas
        if (medievalLevelsCanvas == null)
        {
            AutoAssignCanvasReferences();
        }

        SetCanvasActive(MenuState.MedievalLevels);
        currentState = MenuState.MedievalLevels;

        SceneNavigationEvents.InvokeMenuSpecificEvent("medievallevels");
        SceneNavigationEvents.InvokeAfterMenuChange();
    }

    /// <summary>
    /// Navega al nivel prototipo (carga escena)
    /// </summary>
    public void GoToPrototypeLevel()
    {
        if (string.IsNullOrEmpty(prototypeLevelSceneName))
        {
            return;
        }
        
        StartCoroutine(LoadSceneAsync(prototypeLevelSceneName));
    }

    /// <summary>
    /// Carga una escena de forma asíncrona
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar</param>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Configura el estado inicial para cuando se carga la escena de menú
    /// </summary>
    /// <param name="initialState">Estado inicial a configurar</param>
    public static void SetInitialMenuState(MenuState initialState)
    {
        requestedInitialState = initialState;
    }

    /// <summary>
    /// Métodos estáticos para navegación desde otras escenas
    /// </summary>
    public static void NavigateToMainMenu()
    {
        // Si ya estamos en la escena Menu, usar navegación directa
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu" && Instance != null)
        {
            Instance.GoToMainMenu();
            return;
        }
        
        SetInitialMenuState(MenuState.MainMenu);
        SceneManager.LoadScene("Menu");
    }

    public static void NavigateToLevelSelector()
    {
        // Si ya estamos en la escena Menu, usar navegación directa
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu" && Instance != null)
        {
            Instance.GoToLevelSelector();
            return;
        }
        
        SetInitialMenuState(MenuState.LevelSelector);
        SceneManager.LoadScene("Menu");
    }

    public static void NavigateToCredits()
    {
        // Si ya estamos en la escena Menu, usar navegación directa
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu" && Instance != null)
        {
            Instance.GoToCredits();
            return;
        }
        
        SetInitialMenuState(MenuState.Credits);
        SceneManager.LoadScene("Menu");
    }

    public static void NavigateToPrehistoricLevels()
    {
        // Si ya estamos en la escena Menu, usar navegación directa
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu" && Instance != null)
        {
            Instance.GoToPrehistoricLevels();
            return;
        }
        
        SetInitialMenuState(MenuState.PrehistoricLevels);
        SceneManager.LoadScene("Menu");
    }

    public static void NavigateToMedievalLevels()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu" && Instance != null)
        {
            Instance.GoToMedievalLevels();
            return;
        }

        SetInitialMenuState(MenuState.MedievalLevels);
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Navega al nivel prototipo desde un método estático
    /// </summary>
    public static void NavigateToPrototypeLevel()
    {
        if (Instance != null)
        {
            Instance.GoToPrototypeLevel();
        }
    }

    /// <summary>
    /// Navega hacia atrás (por defecto al menú principal)
    /// </summary>
    public static void NavigateBack()
    {
        NavigateToMainMenu();
    }

    /// <summary>
    /// Navega al menú principal desde el juego
    /// </summary>
    public static void NavigateToMainMenuFromGame()
    {
        NavigateToMainMenu();
    }

    /// <summary>
    /// Navega al selector de niveles desde el juego
    /// </summary>
    public static void NavigateToLevelSelectorFromGame()
    {
        NavigateToLevelSelector();
    }

    /// <summary>
    /// Utilidades para obtener información del estado actual
    /// </summary>
    public MenuState GetCurrentState()
    {
        return currentState;
    }

    public bool IsInMainMenu()
    {
        return currentState == MenuState.MainMenu;
    }

    public bool IsInLevelSelector()
    {
        return currentState == MenuState.LevelSelector;
    }

    public bool IsInCredits()
    {
        return currentState == MenuState.Credits;
    }

    public bool IsInPrehistoricLevels()
    {
        return currentState == MenuState.PrehistoricLevels;
    }

    public bool IsInMedievalLevels()
    {
        return currentState == MenuState.MedievalLevels;
    }

    // Métodos compatibles con versiones anteriores
    public void GoToMenu() => GoToMainMenu();
    public void GoToLevelSelect() => GoToLevelSelector();
}
