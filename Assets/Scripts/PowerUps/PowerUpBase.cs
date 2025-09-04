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

    public delegate void PowerUpActivated(PowerUpBase powerUp);
    public static event PowerUpActivated OnPowerUpActivated;

    protected virtual void Start()
    {
        // Iniciar temporizador de vida
        lifeCoroutine = StartCoroutine(LifeTimer());
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
} 