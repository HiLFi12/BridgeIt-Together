using UnityEngine;

/// <summary>
/// Script de diagnóstico para detectar todos los cambios de Canvas y posibles conflictos de música
/// </summary>
public class CanvasChangeDetector : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool enableDetection = true;
    [SerializeField] private bool logCanvasChanges = true;
    [SerializeField] private bool logMusicCalls = true;
    
    private AudioManager audioManager;
    private AudioClip lastMusicClip;
    private bool lastMusicPlayingState;
    
    private void Start()
    {
        if (!enableDetection) return;
        
        audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager != null && audioManager.music != null)
        {
            lastMusicClip = audioManager.music.clip;
            lastMusicPlayingState = audioManager.music.isPlaying;
            Debug.Log("🔍 CanvasChangeDetector iniciado - monitoreando cambios de música");
        }
        
        // Monitorear todos los Canvas en la escena
        if (logCanvasChanges)
        {
            Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"🔍 Canvas detectados al inicio ({allCanvas.Length}):");
            foreach (Canvas canvas in allCanvas)
            {
                Debug.Log($"   📄 {canvas.name} (activo: {canvas.gameObject.activeSelf})");
            }
        }
    }
    
    private void Update()
    {
        if (!enableDetection) return;
        
        CheckMusicChanges();
    }
    
    private void CheckMusicChanges()
    {
        if (audioManager == null || audioManager.music == null) return;
        
        bool currentPlayingState = audioManager.music.isPlaying;
        AudioClip currentClip = audioManager.music.clip;
        
        // Detectar cambios en el estado de reproducción
        if (currentPlayingState != lastMusicPlayingState)
        {
            if (logMusicCalls)
            {
                Debug.Log($"🎵 CAMBIO DE ESTADO MÚSICA: {(currentPlayingState ? "INICIADA" : "DETENIDA")} - Clip: {(currentClip != null ? currentClip.name : "NULL")}");
                LogCurrentCanvasState();
            }
            lastMusicPlayingState = currentPlayingState;
        }
        
        // Detectar cambios en el clip de música
        if (currentClip != lastMusicClip)
        {
            if (logMusicCalls)
            {
                Debug.Log($"🎵 CAMBIO DE CLIP MÚSICA: De '{(lastMusicClip != null ? lastMusicClip.name : "NULL")}' a '{(currentClip != null ? currentClip.name : "NULL")}'");
                LogCurrentCanvasState();
                LogStackTrace();
            }
            lastMusicClip = currentClip;
        }
    }
    
    private void LogCurrentCanvasState()
    {
        Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"📊 Estado actual de Canvas:");
        foreach (Canvas canvas in allCanvas)
        {
            if (canvas.gameObject.activeSelf)
            {
                Debug.Log($"   ✅ {canvas.name} (ACTIVO)");
            }
        }
    }
    
    private void LogStackTrace()
    {
        Debug.Log($"📍 Stack Trace del cambio de música:");
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        for (int i = 0; i < Mathf.Min(10, stackTrace.FrameCount); i++)
        {
            var frame = stackTrace.GetFrame(i);
            Debug.Log($"   {i}: {frame.GetMethod().DeclaringType?.Name}.{frame.GetMethod().Name}");
        }
    }
    
    [ContextMenu("Log Estado Actual")]
    public void LogCurrentState()
    {
        Debug.Log("🔍 === ESTADO ACTUAL DEL SISTEMA ===");
        LogCurrentCanvasState();
        
        if (audioManager != null && audioManager.music != null)
        {
            Debug.Log($"🎵 Música: {(audioManager.music.isPlaying ? "REPRODUCIENDO" : "PAUSADA")}");
            Debug.Log($"🎵 Clip: {(audioManager.music.clip != null ? audioManager.music.clip.name : "NULL")}");
        }
        
        var sceneNav = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (sceneNav != null)
        {
            Debug.Log($"🎯 SceneNavigatorCanvas: Estado actual = {sceneNav.GetCurrentState()}");
        }
        
        Debug.Log("🔍 === FIN ESTADO ACTUAL ===");
    }
}
