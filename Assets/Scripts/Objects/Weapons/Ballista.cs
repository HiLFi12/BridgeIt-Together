using System.Collections;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;
using UnityEngine.UI;

public class Ballista : MonoBehaviour, IInteractable
{
    [Header("Ballista Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject smokeEffectPrefab;
    [SerializeField] private GameObject shadow;
    
    [Header("Launch Settings")]
    [SerializeField, Min(0.1f)] private float launchForce = 15f;
    [SerializeField] private Vector3 launchDirection = Vector3.forward;
    
    [Header("Reload Settings")]
    [SerializeField, Min(0.1f)] private float arrowMoveSpeed = 2f;
    [SerializeField, Min(0.1f)] private float reloadDelay = 1f;

    // Reload UI (barra de carga) -> ahora es una Image con Image.Type = Filled
    [Header("UI")]
    [SerializeField] private Image reloadImage;
    
    [Header("Interaction")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;

    [Header("Audio - Disparo (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al disparar la flecha. -1 desactiva.")]
    [SerializeField] private int shootSfxIndex = -1;
    
    private bool isReady;
    private GameObject currentArrow;
    private GameObject currentSmokeEffect;
    private Coroutine reloadCoroutine;
    
    // UI coroutine ref
    private Coroutine reloadUIUpdateCoroutine;
    
    public InteractPriority InteractPriority => interactPriority;

    private void Awake()
    {
        if (!shootPoint) shootPoint = transform;
        if (!spawnPoint) spawnPoint = transform;
    }

    private void OnValidate()
    {
        if (!shootPoint) shootPoint = transform;
        if (!spawnPoint) spawnPoint = transform;
        launchForce = Mathf.Max(0.1f, launchForce);
        arrowMoveSpeed = Mathf.Max(0.1f, arrowMoveSpeed);
        reloadDelay = Mathf.Max(0.1f, reloadDelay);
    }

    private void Start()
    {
        shadow.SetActive(false);
        StartReload();
    }
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    public void Interact(GameObject interactor)
    {
        if (!isReady || !currentArrow) return;
        
        FireArrow();
    }

    private void FireArrow()
    {
        if (!currentArrow) return;

        isReady = false;

        // Preparar la flecha para el disparo
        var arrowTransform = currentArrow.transform;
        arrowTransform.SetParent(null, true);
        arrowTransform.position = shootPoint.position;
        arrowTransform.rotation = shootPoint.rotation;

        // Añadir fuerza de lanzamiento
        var arrowRb = currentArrow.GetComponent<Rigidbody>();
        if (!arrowRb)
            arrowRb = currentArrow.AddComponent<Rigidbody>();

        // Reactivar física
        arrowRb.isKinematic = false;
        arrowRb.useGravity = true;

        Vector3 forceDirection = shootPoint.TransformDirection(launchDirection.normalized);
        arrowRb.AddForce(forceDirection * launchForce, ForceMode.Impulse);

    // SFX de disparo
    PlayShootSfx();

        // Limpiar referencia
        currentArrow = null;

        // Destruir efecto de humo
        if (currentSmokeEffect)
        {
            Destroy(currentSmokeEffect);
            currentSmokeEffect = null;
        }

        // Calcular duración de movimiento de la flecha (estimada)
        float moveDistance = Vector3.Distance(spawnPoint.position, shootPoint.position);
        float moveDuration = moveDistance / Mathf.Max(0.0001f, arrowMoveSpeed);

        // Iniciar la barra de recarga (delay + movimiento)
        StartReloadUI(reloadDelay + moveDuration);

        // Iniciar recarga después del delay
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(DelayedReload());
    }

    private void PlayShootSfx()
    {
        if (shootSfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(shootSfxIndex);
        }
    }

    private IEnumerator DelayedReload()
    {
        yield return new WaitForSeconds(reloadDelay);
        StartReload();
    }

    private void StartReload()
    {
        if (!arrowPrefab || !spawnPoint || !shootPoint) return;
        
        // Crear nueva flecha en el spawn point
        currentArrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
        currentArrow.transform.SetParent(transform, true);
        
        // Desactivar física de la flecha durante la carga
        var arrowRb = currentArrow.GetComponent<Rigidbody>();
        if (arrowRb)
        {
            arrowRb.isKinematic = true;
            arrowRb.useGravity = false;
        }
        
        // Crear efecto de humo
        if (smokeEffectPrefab)
        {
            currentSmokeEffect = Instantiate(smokeEffectPrefab, spawnPoint.position, spawnPoint.rotation);
            currentSmokeEffect.transform.SetParent(transform, true);
        }
        
        // Calcular duración del movimiento y, si no hay ya una barra en marcha, iniciar la UI
        float moveDistance = Vector3.Distance(spawnPoint.position, shootPoint.position);
        float moveDuration = moveDistance / Mathf.Max(0.0001f, arrowMoveSpeed);

        if (reloadUIUpdateCoroutine == null)
            StartReloadUI(moveDuration);
        
        // Iniciar movimiento de la flecha
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(MoveArrowToShootPoint());
    }

    private IEnumerator MoveArrowToShootPoint()
    {
        if (!currentArrow || !shootPoint) yield break;
        
        Vector3 startPos = spawnPoint.position;
        Vector3 targetPos = shootPoint.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / arrowMoveSpeed;
        
        float t = 0f;
        while (t < duration && currentArrow)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentArrow.transform.position = currentPos;
            
            yield return null;
        }
        
        // Asegurar posición final
        if (currentArrow)
        {
            currentArrow.transform.position = shootPoint.position;
            currentArrow.transform.rotation = shootPoint.rotation;
            isReady = true;
        }

        // Asegurar que la barra llega al máximo y se oculta
        StopAndHideReloadUI();
    }

    // Inicia la UI de recarga con una duración total (segundos). Reemplaza cualquier UI en curso.
    private void StartReloadUI(float totalDuration)
    {
        if (reloadImage == null || totalDuration <= 0f) return;

        if (reloadUIUpdateCoroutine != null)
        {
            StopCoroutine(reloadUIUpdateCoroutine);
            reloadUIUpdateCoroutine = null;
        }

        reloadUIUpdateCoroutine = StartCoroutine(UpdateReloadBar(totalDuration));
    }

    private IEnumerator UpdateReloadBar(float totalDuration)
    {
        if (reloadImage == null) yield break;

        reloadImage.gameObject.SetActive(true);
        reloadImage.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            reloadImage.fillAmount = Mathf.Clamp01(elapsed / totalDuration);
            yield return null;
        }

        reloadImage.fillAmount = 1f;

        reloadUIUpdateCoroutine = null;
    }

    private void StopAndHideReloadUI()
    {
        if (reloadUIUpdateCoroutine != null)
        {
            StopCoroutine(reloadUIUpdateCoroutine);
            reloadUIUpdateCoroutine = null;
        }

        if (reloadImage)
        {
            reloadImage.fillAmount = 1f;
        }
    }

    public bool IsReady() => isReady;
    public bool HasArrow() => currentArrow != null;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (shootPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            
            Vector3 direction = shootPoint.TransformDirection(launchDirection.normalized);
            Gizmos.DrawRay(shootPoint.position, direction * 2f);
        }
        
        if (spawnPoint)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.15f);
        }
        
        if (shootPoint && spawnPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, shootPoint.position);
        }
    }
#endif
}