using System.Collections;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class Ballista : MonoBehaviour, IInteractable
{
    [Header("Ballista Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject smokeEffectPrefab;
    
    [Header("Launch Settings")]
    [SerializeField, Min(0.1f)] private float launchForce = 15f;
    [SerializeField] private Vector3 launchDirection = Vector3.forward;
    
    [Header("Reload Settings")]
    [SerializeField, Min(0.1f)] private float arrowMoveSpeed = 2f;
    [SerializeField, Min(0.1f)] private float reloadDelay = 1f;
    
    [Header("Interaction")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    
    private bool isReady;
    private GameObject currentArrow;
    private GameObject currentSmokeEffect;
    private Coroutine reloadCoroutine;
    
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
        StartReload();
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

        // Limpiar referencia
        currentArrow = null;

        // Destruir efecto de humo
        if (currentSmokeEffect)
        {
            Destroy(currentSmokeEffect);
            currentSmokeEffect = null;
        }

        // Iniciar recarga después del delay
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(DelayedReload());
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