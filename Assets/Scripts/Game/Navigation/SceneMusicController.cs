using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script para manejar música específica por escena usando un AudioManager global.
/// Se puede usar cuando tienes un AudioManager que persiste entre escenas.
/// </summary>
public class SceneMusicController : MonoBehaviour
{
    [Header("Scene Music Configuration")]
    [SerializeField] private SceneMusicConfig[] sceneMusicConfigs;
    
    [System.Serializable]
    public class SceneMusicConfig
    {
        public string sceneName;
        public int bgmIndex;
        public bool playOnSceneLoad = true;
        public bool useFadeIn = false;
        public float fadeInDuration = 2f;
    }
    
    private AudioManager audioManager;
    
    private void Start()
    {
        // Buscar AudioManager en la escena o en DontDestroyOnLoad
        audioManager = FindFirstObjectByType<AudioManager>();
        
        if (audioManager == null)
        {
            Debug.LogWarning("SceneMusicController: No se encontró AudioManager");
            return;
        }
        
        // Suscribirse a eventos de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Reproducir música para la escena actual
        PlayMusicForCurrentScene();
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }
    
    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentSceneName);
    }
    
    private void PlayMusicForScene(string sceneName)
    {
        if (audioManager == null) return;
        
        // Verificar si SceneNavigatorCanvas está manejando la música
        var sceneNavCanvas = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (sceneNavCanvas != null && sceneName.Equals("Menu", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("🎵 SceneNavigatorCanvas detectado para escena Menu, delegando control de música");
            return;
        }
        
        // Buscar configuración para esta escena
        SceneMusicConfig config = GetConfigForScene(sceneName);
        
        if (config != null && config.playOnSceneLoad)
        {
            if (config.useFadeIn)
            {
                StartCoroutine(FadeInMusic(config.bgmIndex, config.fadeInDuration));
            }
            else
            {
                audioManager.PlayBGM(config.bgmIndex);
                Debug.Log($"🎵 Reproduciendo música para {sceneName} (índice: {config.bgmIndex})");
            }
        }
    }
    
    private SceneMusicConfig GetConfigForScene(string sceneName)
    {
        foreach (var config in sceneMusicConfigs)
        {
            if (config.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }
        return null;
    }
    
    private System.Collections.IEnumerator FadeInMusic(int bgmIndex, float duration)
    {
        if (audioManager == null || audioManager.music == null) yield break;
        
        float originalVolume = audioManager.music.volume;
        audioManager.music.volume = 0f;
        
        audioManager.PlayBGM(bgmIndex);
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioManager.music.volume = Mathf.Lerp(0f, originalVolume, timer / duration);
            yield return null;
        }
        
        audioManager.music.volume = originalVolume;
    }
    
    /// <summary>
    /// Cambia la música manualmente
    /// </summary>
    /// <param name="bgmIndex">Índice de la música a reproducir</param>
    public void PlayMusic(int bgmIndex)
    {
        if (audioManager != null)
        {
            audioManager.PlayBGM(bgmIndex);
        }
    }
}
