using UnityEngine;

/// <summary>
/// Script de prueba para verificar que el sistema de pausa de GameConditionManager funciona
/// Agrega este script a cualquier GameObject en la escena para hacer debugging
/// </summary>
public class TestPauseSystem : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    private GameConditionManager gameConditionManager;
    
    private void Start()
    {
        gameConditionManager = FindFirstObjectByType<GameConditionManager>();
        
        if (showDebugInfo)
        {
            Debug.Log($"🧪 TestPauseSystem iniciado:");
            Debug.Log($"   - GameConditionManager encontrado: {gameConditionManager != null}");
            if (gameConditionManager != null)
            {
                // Usar reflexión para verificar si pauseCanvas está asignado
                var pauseCanvasField = typeof(GameConditionManager).GetField("pauseCanvas", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (pauseCanvasField != null)
                {
                    var pauseCanvas = pauseCanvasField.GetValue(gameConditionManager);
                    Debug.Log($"   - Pause Canvas asignado: {pauseCanvas != null}");
                    if (pauseCanvas != null)
                    {
                        Debug.Log($"   - Pause Canvas name: {((Canvas)pauseCanvas).name}");
                    }
                }
            }
            Debug.Log($"   - Usa ESC para pausar/reanudar");
            Debug.Log($"   - Usa P para test manual de pausa");
            Debug.Log($"   - Usa R para test manual de reanudación");
        }
    }
    
    private void Update()
    {
        if (gameConditionManager == null) return;
        
        // Pruebas manuales con teclas
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestPauseManual();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestResumeManual();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowGameState();
        }
    }
    
    [ContextMenu("Test Pause Manual")]
    public void TestPauseManual()
    {
        if (gameConditionManager != null)
        {
            Debug.Log("🧪 Probando pausa manual...");
            gameConditionManager.PausarJuego();
        }
    }
    
    [ContextMenu("Test Resume Manual")]
    public void TestResumeManual()
    {
        if (gameConditionManager != null)
        {
            Debug.Log("🧪 Probando reanudación manual...");
            gameConditionManager.ReanudarJuego();
        }
    }
    
    [ContextMenu("Show Game State")]
    public void ShowGameState()
    {
        if (gameConditionManager != null)
        {
            bool isPaused = gameConditionManager.IsJuegoEnPausa();
            Debug.Log($"🎮 Estado del juego:");
            Debug.Log($"   - Pausado: {isPaused}");
            Debug.Log($"   - Time.timeScale: {Time.timeScale}");
            Debug.Log($"   - Terminado: {gameConditionManager.IsJuegoTerminado()}");
            Debug.Log($"   - Activo: {gameConditionManager.IsJuegoActivo()}");
        }
    }
}
