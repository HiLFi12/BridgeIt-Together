using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script de debug para verificar y forzar el guardado de progreso de niveles.
/// Agregar este componente temporalmente a un GameObject en la escena para hacer pruebas.
/// </summary>
public class LevelProgressDebugger : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private string testLevelName = "Level1";
    
    [Header("Auto Test en Victoria")]
    [SerializeField] private bool autoTestOnVictory = true;
    
    private void Start()
    {
        Debug.Log("=== LEVEL PROGRESS DEBUGGER INICIADO ===");
        
        // Verificar que exista el LevelProgressManager
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogWarning("⚠️ NO SE ENCONTRÓ LevelProgressManager.Instance");
            
            LevelProgressManager manager = FindFirstObjectByType<LevelProgressManager>();
            if (manager == null)
            {
                Debug.LogError("❌ NO HAY LevelProgressManager EN LA ESCENA");
            }
            else
            {
                Debug.Log("✅ LevelProgressManager encontrado pero no es Instance");
            }
        }
        else
        {
            Debug.Log("✅ LevelProgressManager.Instance existe correctamente");
        }
        
        // Verificar GameConditionManager
        if (GameConditionManager.Instance == null)
        {
            Debug.LogWarning("⚠️ NO SE ENCONTRÓ GameConditionManager.Instance");
        }
        else
        {
            Debug.Log("✅ GameConditionManager.Instance existe");
            
            // Suscribirse al evento de victoria para hacer debug
            if (autoTestOnVictory)
            {
                GameConditionManager.Instance.OnVictoria.AddListener(OnVictoriaDetectada);
                Debug.Log("📝 Suscrito al evento OnVictoria para debug");
            }
        }
        
        // Mostrar nombre de la escena actual
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Escena actual: '{currentScene}'");
    }
    
    private void OnVictoriaDetectada()
    {
        Debug.Log("=== 🎉 VICTORIA DETECTADA - INICIANDO DEBUG ===");
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Nivel completado: '{currentScene}'");
        
        // Esperar un frame para que se guarde
        StartCoroutine(VerificarGuardadoDespuesDeVictoria(currentScene));
    }
    
    private System.Collections.IEnumerator VerificarGuardadoDespuesDeVictoria(string levelName)
    {
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("=== VERIFICANDO GUARDADO ===");
        
        if (LevelProgressManager.Instance != null)
        {
            bool isCompleted = LevelProgressManager.Instance.IsLevelCompleted(levelName);
            Debug.Log($"🔍 Nivel '{levelName}' completado en PlayerPrefs: {isCompleted}");
            
            if (isCompleted)
            {
                Debug.Log("✅ ¡EL PROGRESO SE GUARDÓ CORRECTAMENTE!");
            }
            else
            {
                Debug.LogError("❌ EL PROGRESO NO SE GUARDÓ - Verificar condiciones");
                
                // Verificar vidas
                var lifeStarsUI = FindFirstObjectByType<LifeStarsUI>();
                if (lifeStarsUI != null)
                {
                    int lives = lifeStarsUI.GetCurrentLives();
                    Debug.Log($"⭐ Vidas restantes: {lives}");
                    
                    if (lives <= 0)
                    {
                        Debug.LogWarning("⚠️ El jugador no tiene vidas, por eso no se guardó");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("❌ LevelProgressManager.Instance es NULL después de victoria");
        }
        
        Debug.Log("=== FIN VERIFICACIÓN ===");
    }
    
    #region Botones de Debug (Context Menu)
    
    [ContextMenu("1. Mostrar Estado Actual")]
    public void MostrarEstadoActual()
    {
        Debug.Log("=== ESTADO ACTUAL DEL SISTEMA ===");
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Escena actual: '{currentScene}'");
        
        // Verificar LevelProgressManager
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ LevelProgressManager.Instance NO EXISTE");
        }
        else
        {
            Debug.Log("✅ LevelProgressManager.Instance EXISTE");
            bool isCompleted = LevelProgressManager.Instance.IsLevelCompleted(currentScene);
            Debug.Log($"🔍 Nivel '{currentScene}' completado: {isCompleted}");
        }
        
        // Verificar GameConditionManager
        if (GameConditionManager.Instance == null)
        {
            Debug.LogError("❌ GameConditionManager.Instance NO EXISTE");
        }
        else
        {
            Debug.Log("✅ GameConditionManager.Instance EXISTE");
        }
        
        // Verificar LifeStarsUI
        var lifeStarsUI = FindFirstObjectByType<LifeStarsUI>();
        if (lifeStarsUI != null)
        {
            int lives = lifeStarsUI.GetCurrentLives();
            Debug.Log($"⭐ Vidas actuales: {lives}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró LifeStarsUI");
        }
        
        Debug.Log("=== FIN ESTADO ===");
    }
    
    [ContextMenu("2. Forzar Guardar Nivel Actual")]
    public void ForzarGuardarNivelActual()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ No se puede guardar: LevelProgressManager.Instance no existe");
            return;
        }
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🔧 Forzando guardado del nivel '{currentScene}'...");
        
        LevelProgressManager.Instance.MarkLevelAsCompleted(currentScene);
        
        // Verificar que se guardó
        bool isCompleted = LevelProgressManager.Instance.IsLevelCompleted(currentScene);
        if (isCompleted)
        {
            Debug.Log($"✅ Nivel '{currentScene}' marcado como completado EXITOSAMENTE");
        }
        else
        {
            Debug.LogError($"❌ ERROR: No se pudo marcar el nivel como completado");
        }
    }
    
    [ContextMenu("3. Verificar PlayerPrefs Directamente")]
    public void VerificarPlayerPrefs()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string key = $"Level_{currentScene}_Completed";
        
        Debug.Log($"=== VERIFICACIÓN DIRECTA DE PLAYERPREFS ===");
        Debug.Log($"🔑 Key: '{key}'");
        
        int value = PlayerPrefs.GetInt(key, -1);
        
        if (value == -1)
        {
            Debug.Log("❌ La key NO EXISTE en PlayerPrefs");
        }
        else if (value == 0)
        {
            Debug.Log("⚠️ La key existe pero el valor es 0 (no completado)");
        }
        else if (value == 1)
        {
            Debug.Log("✅ La key existe y el valor es 1 (COMPLETADO)");
        }
        else
        {
            Debug.Log($"⚠️ La key existe pero tiene un valor inesperado: {value}");
        }
        
        Debug.Log("=== FIN VERIFICACIÓN PLAYERPREFS ===");
    }
    
    [ContextMenu("4. Guardar Nivel de Prueba")]
    public void GuardarNivelDePrueba()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ No se puede guardar: LevelProgressManager.Instance no existe");
            return;
        }
        
        Debug.Log($"🔧 Guardando nivel de prueba '{testLevelName}'...");
        LevelProgressManager.Instance.MarkLevelAsCompleted(testLevelName);
        
        bool isCompleted = LevelProgressManager.Instance.IsLevelCompleted(testLevelName);
        if (isCompleted)
        {
            Debug.Log($"✅ Nivel de prueba '{testLevelName}' guardado EXITOSAMENTE");
        }
    }
    
    [ContextMenu("5. Limpiar Progreso Nivel Actual")]
    public void LimpiarProgresoNivelActual()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ No se puede limpiar: LevelProgressManager.Instance no existe");
            return;
        }
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🗑️ Limpiando progreso del nivel '{currentScene}'...");
        
        LevelProgressManager.Instance.ClearLevelProgress(currentScene);
        Debug.Log($"✅ Progreso limpiado");
    }
    
    [ContextMenu("6. Mostrar TODOS los Niveles Completados")]
    public void MostrarTodosLosNivelesCompletados()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ LevelProgressManager.Instance no existe");
            return;
        }
        
        LevelProgressManager.Instance.ShowLevelProgress();
    }
    
    [ContextMenu("7. Test Completo de Guardado")]
    public void TestCompletoDeGuardado()
    {
        Debug.Log("=== TEST COMPLETO DE GUARDADO ===");
        
        string testLevel = "TestLevel123";
        
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ FALLO: LevelProgressManager.Instance no existe");
            return;
        }
        
        Debug.Log($"1. Verificando que '{testLevel}' NO está completado...");
        bool antes = LevelProgressManager.Instance.IsLevelCompleted(testLevel);
        Debug.Log($"   Estado inicial: {antes}");
        
        Debug.Log($"2. Marcando '{testLevel}' como completado...");
        LevelProgressManager.Instance.MarkLevelAsCompleted(testLevel);
        
        Debug.Log($"3. Verificando que '{testLevel}' SÍ está completado...");
        bool despues = LevelProgressManager.Instance.IsLevelCompleted(testLevel);
        Debug.Log($"   Estado después: {despues}");
        
        if (despues)
        {
            Debug.Log("✅ TEST EXITOSO: El sistema de guardado funciona correctamente");
        }
        else
        {
            Debug.LogError("❌ TEST FALLIDO: El nivel no se marcó como completado");
        }
        
        Debug.Log($"4. Limpiando nivel de prueba...");
        LevelProgressManager.Instance.ClearLevelProgress(testLevel);
        
        Debug.Log("=== FIN TEST ===");
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Desuscribirse del evento
        if (GameConditionManager.Instance != null)
        {
            GameConditionManager.Instance.OnVictoria.RemoveListener(OnVictoriaDetectada);
        }
    }
}

