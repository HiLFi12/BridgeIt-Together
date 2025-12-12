using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Script que se coloca en cada Canvas de niveles (PrehistoricLevels, MedievalLevels, etc).
/// Contiene las referencias a los botones de nivel y sus estrellas.
/// Se registra con LevelProgressManager en Start.
/// </summary>
public class MenuLevelUpdater : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public string levelSceneName; // Nombre de la escena (ej: "Level1", "Level0_M")
        public Button button; // Referencia al botón
        public Image completionStar; // Imagen de estrella
    }
    
    [Header("Referencias de Botones")]
    [SerializeField] private List<LevelButton> levelButtons = new List<LevelButton>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔄 MenuLevelUpdater en '{gameObject.name}' iniciado - Registrándose con LevelProgressManager...");
        }
        
        // Buscar el LevelProgressManager y registrarse
        LevelProgressManager manager = FindFirstObjectByType<LevelProgressManager>();
        
        if (manager != null)
        {
            // Llamar al método público del manager para que nos busque y actualice
            manager.RegisterAndUpdateLevelUpdater();
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró LevelProgressManager en la escena");
        }
    }
    
    /// <summary>
    /// Actualiza las estrellas según el progreso.
    /// Es llamado por LevelProgressManager.
    /// </summary>
    public void UpdateStars(LevelProgressManager manager)
    {
        if (manager == null)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ LevelProgressManager es null");
            }
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🔄 Actualizando {levelButtons.Count} estrellas...");
        }
        
        int updated = 0;
        int completed = 0;
        int skipped = 0;
        
        foreach (var levelButton in levelButtons)
        {
            // Verificar referencias
            if (levelButton.button == null || levelButton.completionStar == null)
            {
                skipped++;
                continue;
            }
            
            // Preguntar al manager si el nivel está completado
            bool isCompleted = manager.IsLevelCompleted(levelButton.levelSceneName);
            
            // Activar/desactivar la estrella
            levelButton.completionStar.gameObject.SetActive(isCompleted);
            
            updated++;
            if (isCompleted)
            {
                completed++;
                
                if (showDebugLogs)
                {
                    Debug.Log($"⭐ Nivel '{levelButton.levelSceneName}' - COMPLETADO");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"✅ {updated} estrellas actualizadas, {completed} niveles completados, {skipped} saltados");
        }
    }
    
    /// <summary>
    /// Obtiene la lista de botones.
    /// </summary>
    public List<LevelButton> GetLevelButtons()
    {
        return levelButtons;
    }
}


