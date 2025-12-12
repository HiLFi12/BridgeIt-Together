using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona el progreso de niveles completados usando PlayerPrefs.
/// Se mueve entre escenas (DontDestroyOnLoad).
/// Trabaja con las referencias del MenuLevelUpdater.
/// </summary>
public class LevelProgressManager : MonoBehaviour
{
    // Singleton para fácil acceso
    public static LevelProgressManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Awake()
    {
        // Configurar singleton (no destruir entre escenas)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (showDebugLogs)
            {
                Debug.Log("✅ LevelProgressManager inicializado (DontDestroyOnLoad)");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Marca un nivel como completado usando PlayerPrefs
    /// </summary>
    /// <param name="levelSceneName">Nombre de la escena del nivel (ej: "Level1", "Level0_M")</param>
    public void MarkLevelAsCompleted(string levelSceneName)
    {
        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogWarning("LevelProgressManager: Nombre de nivel vacío, no se puede marcar como completado");
            return;
        }
        
        string key = GetLevelCompletionKey(levelSceneName);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        
        if (showDebugLogs)
        {
            Debug.Log($"✅ Nivel '{levelSceneName}' marcado como completado (Key: {key})");
        }
    }
    
    /// <summary>
    /// Verifica si un nivel está completado
    /// </summary>
    /// <param name="levelSceneName">Nombre de la escena del nivel</param>
    /// <returns>True si el nivel está completado, false si no</returns>
    public bool IsLevelCompleted(string levelSceneName)
    {
        if (string.IsNullOrEmpty(levelSceneName))
        {
            return false;
        }
        
        string key = GetLevelCompletionKey(levelSceneName);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    
    /// <summary>
    /// Método público que llama MenuLevelUpdater en su Start.
    /// Busca al updater y le actualiza la información.
    /// </summary>
    public void RegisterAndUpdateLevelUpdater()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 RegisterAndUpdateLevelUpdater llamado - Buscando MenuLevelUpdater...");
        }
        
        // Buscar el MenuLevelUpdater en la escena
        MenuLevelUpdater updater = FindFirstObjectByType<MenuLevelUpdater>();
        
        if (updater != null)
        {
            if (showDebugLogs)
            {
                Debug.Log("✅ MenuLevelUpdater encontrado - Actualizando estrellas...");
            }
            
            // Llamar al método del updater pasándole esta instancia
            updater.UpdateStars(this);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ No se encontró MenuLevelUpdater en la escena");
            }
        }
    }
    
    /// <summary>
    /// Marca el nivel actual como completado
    /// </summary>
    public void MarkCurrentLevelAsCompleted()
    {
        string currentLevelName = SceneManager.GetActiveScene().name;
        MarkLevelAsCompleted(currentLevelName);
    }
    

    
    /// <summary>
    /// Limpia el progreso de un nivel específico (útil para debug/testing)
    /// </summary>
    /// <param name="levelSceneName">Nombre de la escena del nivel</param>
    public void ClearLevelProgress(string levelSceneName)
    {
        if (string.IsNullOrEmpty(levelSceneName))
        {
            return;
        }
        
        string key = GetLevelCompletionKey(levelSceneName);
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        
        if (showDebugLogs)
        {
            Debug.Log($"🗑️ Progreso del nivel '{levelSceneName}' eliminado");
        }
    }
    
    /// <summary>
    /// Limpia todo el progreso de niveles (útil para debug/testing)
    /// </summary>
    [ContextMenu("Limpiar Todo el Progreso")]
    public void ClearAllLevelProgress()
    {
        if (showDebugLogs)
        {
            Debug.Log("🗑️ Limpiando todo el progreso de niveles...");
        }
        
        // Limpiar niveles comunes
        string[] commonLevels = new string[]
        {
            "Level0", "Level1", "Level2", "Level3", "Level4", "Level5", 
            "Level6", "Level7", "Level8", "Level9", "Level01", "Level02",
            "Level0_M", "Level1_M", "Level2_M", "Level3_M", "Level4_M", "Level5_M",
            "Level0_I", "Level1_I", "Level2_I", "Level3_I", "Level4_I", "Level5_I",
            "Level0_C", "Level1_C", "Level2_C", "Level3_C", "Level4_C", "Level5_C",
            "Level0_F", "Level1_F", "Level2_F", "Level3_F", "Level4_F", "Level5_F"
        };
        
        foreach (string levelName in commonLevels)
        {
            string key = GetLevelCompletionKey(levelName);
            PlayerPrefs.DeleteKey(key);
        }
        
        PlayerPrefs.Save();
        
        if (showDebugLogs)
        {
            Debug.Log("✅ Todo el progreso de niveles ha sido eliminado");
        }
    }
    
    /// <summary>
    /// Obtiene la key de PlayerPrefs para un nivel específico
    /// </summary>
    /// <param name="levelSceneName">Nombre de la escena del nivel</param>
    /// <returns>Key única para PlayerPrefs</returns>
    private string GetLevelCompletionKey(string levelSceneName)
    {
        return $"Level_{levelSceneName}_Completed";
    }
    
    #region Debug Methods
    
    /// <summary>
    /// Muestra información de debug sobre el progreso de niveles
    /// </summary>
    [ContextMenu("Mostrar Progreso de Niveles")]
    public void ShowLevelProgress()
    {
        Debug.Log("=== PROGRESO DE NIVELES ===");
        
        string[] commonLevels = new string[]
        {
            "Level0", "Level1", "Level2", "Level3", "Level4", "Level5",
            "Level0_M", "Level1_M", "Level2_M", "Level3_M", "Level4_M", "Level5_M"
        };
        
        int completados = 0;
        
        foreach (string levelName in commonLevels)
        {
            bool completed = IsLevelCompleted(levelName);
            string status = completed ? "✅ COMPLETADO" : "❌ No completado";
            Debug.Log($"{levelName}: {status}");
            
            if (completed) completados++;
        }
        
        Debug.Log($"=== TOTAL: {completados} niveles completados ===");
    }
    
    #endregion
}

