using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra una imagen de "completado" en el botón de nivel si el nivel ha sido completado.
/// Agregar este componente a cada botón de nivel en el selector de niveles.
/// </summary>
public class LevelCompletionMarker : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Nombre de la escena del nivel (ej: 'Level1', 'Level0_M')")]
    [SerializeField] private string levelSceneName;
    
    [Tooltip("Imagen que se mostrará cuando el nivel esté completado")]
    [SerializeField] private Image completionImage;
    
    [Header("Configuración")]
    [Tooltip("Si está activado, intenta obtener el nombre del nivel desde el MenuButton")]
    [SerializeField] private bool autoDetectLevelFromButton = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;
    
    private void Start()
    {
        // Si está activado, intentar obtener el nombre del nivel desde MenuButton
        if (autoDetectLevelFromButton)
        {
            TryAutoDetectLevelName();
        }
        
        // Validar referencias
        if (completionImage == null)
        {
            Debug.LogError($"LevelCompletionMarker en '{gameObject.name}': No se asignó la imagen de completado");
            return;
        }
        
        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogError($"LevelCompletionMarker en '{gameObject.name}': No se asignó el nombre del nivel");
            return;
        }
        
        // Verificar si el nivel está completado y actualizar la imagen
        UpdateCompletionDisplay();
    }
    
    /// <summary>
    /// Intenta detectar automáticamente el nombre del nivel desde MenuButton
    /// </summary>
    private void TryAutoDetectLevelName()
    {
        MenuButton menuButton = GetComponent<MenuButton>();
        if (menuButton != null)
        {
            // Intentar obtener el nombre del nivel desde SceneReference del MenuButton
            // Nota: Esto requeriría acceso al campo customScene que es privado
            // Por ahora, buscaremos en componentes hijos o hermanos
            
            // Opción 1: Buscar en el nombre del GameObject
            if (gameObject.name.Contains("Level"))
            {
                ExtractLevelNameFromGameObjectName();
            }
        }
    }
    
    /// <summary>
    /// Extrae el nombre del nivel desde el nombre del GameObject
    /// </summary>
    private void ExtractLevelNameFromGameObjectName()
    {
        string objectName = gameObject.name;
        
        // Ejemplos de nombres: "Level1Button", "ButtonLevel0_M", "Level2", etc.
        if (objectName.Contains("Level"))
        {
            // Intentar extraer el nombre del nivel
            // Patrón común: "Level#" o "Level#_X" donde X es la era
            
            int startIndex = objectName.IndexOf("Level", System.StringComparison.Ordinal);
            if (startIndex >= 0)
            {
                string levelPart = objectName.Substring(startIndex);
                
                // Remover "Button" o cualquier sufijo común
                levelPart = levelPart.Replace("Button", "").Replace("button", "").Trim();
                
                if (!string.IsNullOrEmpty(levelPart))
                {
                    levelSceneName = levelPart;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"LevelCompletionMarker: Auto-detectado nivel '{levelSceneName}' desde GameObject '{objectName}'");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Actualiza la visualización de la imagen de completado
    /// </summary>
    private void UpdateCompletionDisplay()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogWarning("LevelCompletionMarker: No se encontró LevelProgressManager en la escena");
            
            // Intentar encontrarlo
            LevelProgressManager manager = FindFirstObjectByType<LevelProgressManager>();
            if (manager == null)
            {
                // Si no existe, crear uno
                GameObject managerObj = new GameObject("LevelProgressManager");
                managerObj.AddComponent<LevelProgressManager>();
                
                if (showDebugLogs)
                {
                    Debug.Log("LevelCompletionMarker: LevelProgressManager creado automáticamente");
                }
            }
        }
        
        // Verificar si el nivel está completado
        bool isCompleted = LevelProgressManager.Instance.IsLevelCompleted(levelSceneName);
        
        // Mostrar u ocultar la imagen según el estado
        if (completionImage != null)
        {
            completionImage.gameObject.SetActive(isCompleted);
            
            if (showDebugLogs)
            {
                Debug.Log($"LevelCompletionMarker: Nivel '{levelSceneName}' - Completado: {isCompleted}");
            }
        }
    }
    
    /// <summary>
    /// Fuerza una actualización de la visualización (útil para refrescar después de completar un nivel)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateCompletionDisplay();
    }
    
    /// <summary>
    /// Establece el nombre del nivel manualmente (útil si se configura por código)
    /// </summary>
    public void SetLevelSceneName(string sceneName)
    {
        levelSceneName = sceneName;
        UpdateCompletionDisplay();
    }
    
    #region Editor Helper
    
    private void OnValidate()
    {
        // Validación en el editor
        if (completionImage == null)
        {
            // Intentar encontrar automáticamente una imagen hija llamada "CompletedImage" o similar
            Transform completedChild = transform.Find("CompletedImage");
            if (completedChild == null)
            {
                completedChild = transform.Find("Completed");
            }
            if (completedChild == null)
            {
                completedChild = transform.Find("CheckMark");
            }
            if (completedChild == null)
            {
                completedChild = transform.Find("Star");
            }
            
            if (completedChild != null)
            {
                completionImage = completedChild.GetComponent<Image>();
                
                if (completionImage != null && showDebugLogs)
                {
                    Debug.Log($"LevelCompletionMarker: Auto-asignada imagen '{completedChild.name}'");
                }
            }
        }
        
        // Intentar auto-detectar el nombre del nivel desde el nombre del GameObject
        if (autoDetectLevelFromButton && string.IsNullOrEmpty(levelSceneName))
        {
            ExtractLevelNameFromGameObjectName();
        }
    }
    
    #endregion
}

