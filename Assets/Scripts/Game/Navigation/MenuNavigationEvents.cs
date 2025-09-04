using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de eventos para navegación modificado para Canvas.
/// Permite a otros sistemas reaccionar cuando se navega entre diferentes canvas/menús.
/// </summary>
public class MenuNavigationEvents : MonoBehaviour
{
    [Header("Navigation Events")]
    public UnityEvent OnBeforeMenuChange;
    public UnityEvent OnAfterMenuChange;
    
    [Header("Menu Specific Events")]
    public UnityEvent OnEnterMainMenu;
    public UnityEvent OnEnterLevelSelector;
    public UnityEvent OnEnterCredits;
    public UnityEvent OnEnterPrototypeLevel;

    public static MenuNavigationEvents Instance { get; private set; }

    private void Awake()
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

    private void Start()
    {
        // Suscribirse a eventos de escena de Unity (solo para nivel prototipo)
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Se llama cuando una escena termina de cargar (solo para PrototypeLevel)
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Solo manejar el nivel prototipo ya que los menús ahora usan Canvas
        if (scene.name == "PrototypeLevel")
        {
            OnEnterPrototypeLevel?.Invoke();
            Debug.Log($"Escena cargada: {scene.name}");
        }
    }

    /// <summary>
    /// Invoca el evento antes de cambiar de menú/canvas
    /// </summary>
    public static void InvokeBeforeMenuChange()
    {
        if (Instance != null)
        {
            Instance.OnBeforeMenuChange?.Invoke();
        }
    }

    /// <summary>
    /// Invoca el evento después de cambiar de menú/canvas
    /// </summary>
    public static void InvokeAfterMenuChange()
    {
        if (Instance != null)
        {
            Instance.OnAfterMenuChange?.Invoke();
        }
    }

    /// <summary>
    /// Invoca el evento específico del menú al que se navegó
    /// </summary>
    public static void InvokeMenuSpecificEvent(string menuName)
    {
        if (Instance == null) return;

        switch (menuName.ToLower())
        {
            case "mainmenu":
            case "menu":
                Instance.OnEnterMainMenu?.Invoke();
                Debug.Log("Evento: Entrando a Menú Principal");
                break;
            case "levelselector":
            case "selector":
                Instance.OnEnterLevelSelector?.Invoke();
                Debug.Log("Evento: Entrando a Selector de Niveles");
                break;
            case "credits":
                Instance.OnEnterCredits?.Invoke();
                Debug.Log("Evento: Entrando a Créditos");
                break;
        }
    }

    /// <summary>
    /// Agrega un listener para cuando se va a cambiar de menú
    /// </summary>
    public static void AddBeforeMenuChangeListener(UnityEngine.Events.UnityAction listener)
    {
        if (Instance != null)
        {
            Instance.OnBeforeMenuChange.AddListener(listener);
        }
    }

    /// <summary>
    /// Agrega un listener para cuando se entra al menú principal
    /// </summary>
    public static void AddMainMenuListener(UnityEngine.Events.UnityAction listener)
    {
        if (Instance != null)
        {
            Instance.OnEnterMainMenu.AddListener(listener);
        }
    }

    /// <summary>
    /// Agrega un listener para cuando se entra al selector de niveles
    /// </summary>
    public static void AddLevelSelectorListener(UnityEngine.Events.UnityAction listener)
    {
        if (Instance != null)
        {
            Instance.OnEnterLevelSelector.AddListener(listener);
        }
    }

    /// <summary>
    /// Agrega un listener para cuando se entra a créditos
    /// </summary>
    public static void AddCreditsListener(UnityEngine.Events.UnityAction listener)
    {
        if (Instance != null)
        {
            Instance.OnEnterCredits.AddListener(listener);
        }
    }
}
