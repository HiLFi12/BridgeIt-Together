using UnityEngine;

public class StatueInteractable : MonoBehaviour, IInteractable
{
    public PowerUpMotivacionEstatua powerUp;
    public Transform destinationPoint;
    private bool isCarried = false;

    [Header("Lifetime / Despawn")]
    [Tooltip("Tiempo en segundos que la estatua permanece en el mapa antes de desaparecer.")]
    public float lifeDuration = 60f;
    [Tooltip("Prefab de efecto que se instancia al morir (opcional).")]
    public GameObject dieEffectPrefab;
    [Tooltip("Si true el efecto se parenta a la estatua antes de destruirla.")]
    public bool attachDieEffect = false;

    [Header("Motivación")]
    [Tooltip("Duración en segundos del efecto de motivación cuando la estatua es impactada por una flecha.")]
    public float motivationDuration = 20f;

    private float lifeTimer;
    private bool isDead = false;

    // Evento opcional para suscriptores externos
    public System.Action OnDie; 

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        if (isCarried) return;
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder != null)
        {
            holder.PickUpExistingInstance(gameObject);
            isCarried = true;
            // Feedback visual/sonoro opcional aquí
        }
    }

    private void Update()
    {
        // Countdown de vida
        if (!isDead && lifeDuration > 0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeDuration)
            {
                Die();
            }
        }

        if (isCarried && Vector3.Distance(transform.position, destinationPoint.position) < 1f)
        {
            powerUp.OnStatueArrived();
            // Feedback visual/sonoro de llegada
        }
    }

    /// <summary>
    /// Despawning controlado de la estatua. Instancia efecto y destruye el GameObject.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (dieEffectPrefab != null)
        {
            GameObject fx = Instantiate(dieEffectPrefab, transform.position, transform.rotation);
            if (attachDieEffect && fx != null)
            {
                fx.transform.SetParent(transform, true);
            }
            // Autodestruir efecto si tiene ParticleSystem principal
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float ttl = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, ttl);
            }
        }

        OnDie?.Invoke();
        Destroy(gameObject);
    }

    // Detección de Arrow para activar motivación y luego morir
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
        {
            TriggerMotivationAndDie();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.GetComponent<Arrow>() != null)
        {
            TriggerMotivationAndDie();
        }
    }

    private void TriggerMotivationAndDie()
    {
        // Activar motivación usando la duración configurable desde el inspector.
        if (motivationDuration > 0f)
        {
            MotivationBuffManager.Activate(motivationDuration);
        }
        Die();
    }
} 