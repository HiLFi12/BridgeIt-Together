using System.Collections;
using UnityEngine;

public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Configuración General")]
    public float duration = 10f; // Duración del efecto
    public float timeToLive = 15f; // Tiempo que permanece en el escenario
    protected bool isActive = false;
    protected bool isAvailable = true;
    protected Coroutine lifeCoroutine;

    [Header("Audio - PowerUp (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando el power-up aparece (spawn). -1 desactiva.")]
    [SerializeField] private int spawnSfxIndex = -1;
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando se activa el efecto. -1 desactiva.")]
    [SerializeField] private int activateSfxIndex = -1;

    public delegate void PowerUpActivated(PowerUpBase powerUp);
    public static event PowerUpActivated OnPowerUpActivated;

    protected virtual void Start()
    {
        // Iniciar temporizador de vida
        lifeCoroutine = StartCoroutine(LifeTimer());

        // SFX de spawn
        PlaySfx(spawnSfxIndex);
    }

    protected virtual IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(timeToLive);
        if (!isActive)
        {
            Despawn();
        }
    }

    public virtual void TryActivate(GameObject activator)
    {
        Debug.Log("Intentando activar PowerUp...");
        if (!isAvailable)
        {
            Debug.Log("PowerUp no disponible para activación.");
            return;
        }
        isActive = true;
        isAvailable = false;
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        
        // SFX de activación
        PlaySfx(activateSfxIndex);
        
        OnPowerUpActivated?.Invoke(this);
        StartCoroutine(EffectCoroutine(activator));
    }

    protected abstract IEnumerator EffectCoroutine(GameObject activator);

    protected virtual void Despawn()
    {
        // Feedback visual/sonoro de desaparición
        // Removido Destroy(gameObject) para evitar destrucción automática
        gameObject.SetActive(false);
    }

    private void PlaySfx(int sfxIndex)
    {
        if (sfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(sfxIndex);
        }
    }
} 