using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Administrador de navegación entre escenas utilizando nombres de escenas.
/// Permite navegar entre escenas de forma simple y segura.
/// </summary>
public class SceneNavigator : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string levelSelectorSceneName = "LevelSelector";
    [SerializeField] private string creditsSceneName = "Credits";
    [SerializeField] private string prototypeLevelSceneName = "PrototypeLevel";

    // Singleton instance para fácil acceso
    public static SceneNavigator Instance { get; private set; }    private void Awake()
    {
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Public Navigation Methods
      /// <summary>
    /// Navega al menú principal (usando Canvas si está disponible)
    /// </summary>
    public void GoToMainMenu()
    {
        // Buscar SceneNavigatorCanvas en la escena
        var canvasController = FindFirstObjectByType<SceneNavigatorCanvas>();        if (canvasController != null)
        {
            canvasController.GoToMainMenu();
        }
        else
        {
            LoadScene(menuSceneName);
        }
    }

    /// <summary>
    /// Navega al selector de niveles (usando Canvas si está disponible)
    /// </summary>
    public void GoToLevelSelector()
    {
        // Buscar SceneNavigatorCanvas en la escena
        var canvasController = FindFirstObjectByType<SceneNavigatorCanvas>();        if (canvasController != null)
        {
            canvasController.GoToLevelSelector();
        }
        else
        {
            LoadScene(levelSelectorSceneName);
        }
    }

    /// <summary>
    /// Navega a la escena de créditos (usando Canvas si está disponible)
    /// </summary>
    public void GoToCredits()
    {
        // Buscar SceneNavigatorCanvas en la escena
        var canvasController = FindFirstObjectByType<SceneNavigatorCanvas>();        if (canvasController != null)
        {
            canvasController.GoToCredits();
        }
        else
        {
            LoadScene(creditsSceneName);
        }
    }

    /// <summary>
    /// Navega al nivel prototipo (siempre carga la escena)
    /// </summary>
    public void GoToPrototypeLevel()
    {
        LoadScene(prototypeLevelSceneName);
    }

    /// <summary>
    /// Navega hacia atrás según el contexto actual
    /// </summary>
    public void GoBack()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case var scene when scene == levelSelectorSceneName:
                GoToMainMenu();
                break;
            case var scene when scene == creditsSceneName:
                GoToMainMenu();
                break;
            case var scene when scene == prototypeLevelSceneName:
                GoToLevelSelector();
                break;
            default:
                Debug.LogWarning($"No se ha definido navegación hacia atrás para la escena: {currentScene}");
                GoToMainMenu(); // Fallback al menú principal
                break;
        }
    }

    #endregion

    #region Scene Loading    /// <summary>
    /// Carga una escena por nombre de forma segura
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar</param>
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("El nombre de la escena no puede estar vacío");
            return;
        }

        // Verificar si la escena existe en el build
        if (!IsSceneInBuild(sceneName))
        {
            Debug.LogError($"La escena '{sceneName}' no está incluida en Build Settings");
            return;
        }        // Invocar evento antes del cambio de escena
        SceneNavigationEvents.InvokeBeforeMenuChange();

        Debug.Log($"Navegando a la escena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Verifica si una escena está incluida en el build
    /// </summary>
    /// <param name="sceneName">Nombre de la escena</param>
    /// <returns>True si la escena está en el build</returns>
    private bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Obtiene el nombre de la escena actual
    /// </summary>
    /// <returns>Nombre de la escena actual</returns>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Verifica si estamos en una escena específica
    /// </summary>
    /// <param name="sceneName">Nombre de la escena</param>
    /// <returns>True si estamos en esa escena</returns>
    public bool IsInScene(string sceneName)
    {
        return GetCurrentSceneName() == sceneName;
    }

    #endregion

    #region Static Methods for Easy Access

    /// <summary>
    /// Navegación estática al menú principal
    /// </summary>
    public static void NavigateToMainMenu()
    {
        if (Instance != null)
            Instance.GoToMainMenu();
        else
            Debug.LogError("SceneNavigator instance no encontrada");
    }

    /// <summary>
    /// Navegación estática al selector de niveles
    /// </summary>
    public static void NavigateToLevelSelector()
    {
        if (Instance != null)
            Instance.GoToLevelSelector();
        else
            Debug.LogError("SceneNavigator instance no encontrada");
    }

    /// <summary>
    /// Navegación estática a créditos
    /// </summary>
    public static void NavigateToCredits()
    {
        if (Instance != null)
            Instance.GoToCredits();
        else
            Debug.LogError("SceneNavigator instance no encontrada");
    }

    /// <summary>
    /// Navegación estática al nivel prototipo
    /// </summary>
    public static void NavigateToPrototypeLevel()
    {
        if (Instance != null)
            Instance.GoToPrototypeLevel();
        else
            Debug.LogError("SceneNavigator instance no encontrada");
    }

    /// <summary>
    /// Navegación estática hacia atrás
    /// </summary>
    public static void NavigateBack()
    {
        if (Instance != null)
            Instance.GoBack();
        else
            Debug.LogError("SceneNavigator instance no encontrada");
    }

    #endregion
}
