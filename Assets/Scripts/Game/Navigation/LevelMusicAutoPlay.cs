using UnityEngine;

/// <summary>
/// Script simple para reproducir música automáticamente en niveles.
/// Coloca este script en el mismo GameObject que el AudioManager del nivel.
/// </summary>
[RequireComponent(typeof(AudioManager))]
public class LevelMusicAutoPlay : MonoBehaviour
{
    [Header("Level Music Settings")]
    [SerializeField] private int levelBGMIndex = 0;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float delayBeforePlay = 0.5f;
    
    [Header("Fade Settings")]
    [SerializeField] private bool useFadeIn = false;
    [SerializeField] private float fadeInDuration = 2f;
    
    private AudioManager audioManager;
    
    private void Start()
    {
        audioManager = GetComponent<AudioManager>();
        
        if (audioManager == null)
        {
            Debug.LogError("LevelMusicAutoPlay: No se encontró AudioManager en este GameObject");
            return;
        }
        
        if (playOnStart)
        {
            if (delayBeforePlay > 0)
            {
                Invoke(nameof(PlayLevelMusic), delayBeforePlay);
            }
            else
            {
                PlayLevelMusic();
            }
        }
    }
    
    /// <summary>
    /// Reproduce la música del nivel
    /// </summary>
    public void PlayLevelMusic()
    {
        if (audioManager == null) return;
        
        // Verificar si estamos en la escena Menu y hay SceneNavigatorCanvas
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene.Equals("Menu", System.StringComparison.OrdinalIgnoreCase))
        {
            var sceneNavCanvas = FindFirstObjectByType<SceneNavigatorCanvas>();
            if (sceneNavCanvas != null)
            {
                return;
            }
        }
        
        if (useFadeIn)
        {
            StartCoroutine(FadeInMusic());
        }
        else
        {
            audioManager.PlayBGM(levelBGMIndex);
        }
    }
    
    /// <summary>
    /// Cambia la música del nivel
    /// </summary>
    /// <param name="newBGMIndex">Nuevo índice de música</param>
    public void ChangeLevelMusic(int newBGMIndex)
    {
        levelBGMIndex = newBGMIndex;
        PlayLevelMusic();
    }
    
    /// <summary>
    /// Detiene la música del nivel
    /// </summary>
    public void StopLevelMusic()
    {
        if (audioManager != null && audioManager.music != null)
        {
            audioManager.music.Stop();
        }
    }
    
    /// <summary>
    /// Fade in de la música
    /// </summary>
    private System.Collections.IEnumerator FadeInMusic()
    {
        if (audioManager == null || audioManager.music == null) yield break;
        
        // Configurar volumen inicial a 0
        float originalVolume = audioManager.music.volume;
        audioManager.music.volume = 0f;
        
        // Reproducir música
        audioManager.PlayBGM(levelBGMIndex);
        
        // Fade in
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeInDuration;
            audioManager.music.volume = Mathf.Lerp(0f, originalVolume, normalizedTime);
            yield return null;
        }
        
        // Asegurar que el volumen final sea el correcto
        audioManager.music.volume = originalVolume;
    }
}
