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
    
    [System.Serializable]
    public class EraButton
    {
        public string eraName; // Nombre de la era (ej: "Prehistoric", "Medieval")
        public Button button; // Referencia al botón de la era
        public Image completionStar; // Imagen de estrella de la era
        public List<int> levelButtonIndices; // Índices de los LevelButton en la lista levelButtons
    }
    
    [Header("Referencias de Botones")]
    [SerializeField] private List<LevelButton> levelButtons = new List<LevelButton>();
    
    [Header("Referencias de Eras")]
    [SerializeField] private List<EraButton> eraButtons = new List<EraButton>();
    
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
        
        // Actualizar estrellas de eras
        UpdateEraStars(manager);
    }
    
    /// <summary>
    /// Actualiza las estrellas de las eras.
    /// Una era se marca como completada si TODOS sus niveles están completados.
    /// </summary>
    private void UpdateEraStars(LevelProgressManager manager)
    {
        if (manager == null)
        {
            return;
        }
        
        if (eraButtons.Count == 0)
        {
            return; // No hay eras configuradas
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🔄 Actualizando {eraButtons.Count} estrellas de eras...");
        }
        
        int eraUpdated = 0;
        int eraCompleted = 0;
        int eraSkipped = 0;
        
        foreach (var eraButton in eraButtons)
        {
            // Verificar referencias
            if (eraButton.button == null || eraButton.completionStar == null)
            {
                eraSkipped++;
                continue;
            }
            
            // Verificar si la era tiene niveles
            if (eraButton.levelButtonIndices == null || eraButton.levelButtonIndices.Count == 0)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"⚠️ Era '{eraButton.eraName}' no tiene niveles configurados");
                }
                eraSkipped++;
                continue;
            }
            
            // Verificar si TODOS los niveles de la era están completados
            bool allLevelsCompleted = true;
            int completedLevels = 0;
            int totalLevels = 0;
            
            foreach (int levelIndex in eraButton.levelButtonIndices)
            {
                // Verificar que el índice sea válido
                if (levelIndex < 0 || levelIndex >= levelButtons.Count)
                {
                    if (showDebugLogs)
                    {
                        Debug.LogWarning($"⚠️ Índice {levelIndex} fuera de rango en era '{eraButton.eraName}'");
                    }
                    continue;
                }
                
                // Obtener el LevelButton desde la lista usando el índice
                LevelButton levelButton = levelButtons[levelIndex];
                
                if (string.IsNullOrEmpty(levelButton.levelSceneName))
                {
                    continue; // Saltar niveles sin nombre configurado
                }
                
                totalLevels++;
                
                // Preguntar al manager si el nivel está completado
                bool isLevelCompleted = manager.IsLevelCompleted(levelButton.levelSceneName);
                
                if (isLevelCompleted)
                {
                    completedLevels++;
                }
                else
                {
                    allLevelsCompleted = false;
                }
            }
            
            // Activar/desactivar la estrella de la era
            eraButton.completionStar.gameObject.SetActive(allLevelsCompleted && totalLevels > 0);
            
            eraUpdated++;
            if (allLevelsCompleted && totalLevels > 0)
            {
                eraCompleted++;
                
                if (showDebugLogs)
                {
                    Debug.Log($"⭐ Era '{eraButton.eraName}' - COMPLETADA ({completedLevels}/{totalLevels} niveles)");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"📊 Era '{eraButton.eraName}' - {completedLevels}/{totalLevels} niveles completados");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"✅ Eras: {eraUpdated} actualizadas, {eraCompleted} completadas, {eraSkipped} saltadas");
        }
    }
    
    /// <summary>
    /// Obtiene la lista de botones.
    /// </summary>
    public List<LevelButton> GetLevelButtons()
    {
        return levelButtons;
    }
    
    /// <summary>
    /// Obtiene la lista de botones de eras.
    /// </summary>
    public List<EraButton> GetEraButtons()
    {
        return eraButtons;
    }
}


