using System;
using System.Collections;
using UnityEngine;

public class StatueInteractable : PowerUpBase, IInteractable, IUIActivatable
{
    // Evento para notificar cuando un player agarra la estatua
    public static event System.Action<StatueInteractable, GameObject> OnStatuePickedUp;
    
    private bool isCarried = false;

    [Header("Lifetime / Despawn")]
    [Tooltip("Tiempo en segundos que la estatua permanece en el mapa antes de desaparecer si no se activó.")]
    public float lifeDuration = 60f;
    [Tooltip("Prefab de efecto que se instancia al morir (opcional).")]
    public GameObject dieEffectPrefab;
    [Tooltip("Si true el efecto se parenta a la estatua antes de destruirla.")]
    public bool attachDieEffect = false;

    [Header("Construcción Automática")]
    [Tooltip("Lista de grids sobre los que aplicar construcción automática al activar la estatua.")]
    [SerializeField] private BridgeConstructionGrid[] bridgeGrids;
    [Tooltip("Si está activo y la lista está vacía, autodescubre todos los BridgeConstructionGrid en la escena al iniciar.")]
    [SerializeField] private bool autoFindBridgeGrids = true;
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie) aplicado a cada grid. Igual que RitualGranFuego.")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 2;

    [Header("VFX de Activación")]
    [Tooltip("Lista de efectos VFX a instanciar al activarse la estatua.")]
    [SerializeField] private GameObject[] activationVfxPrefabs;
    [Tooltip("Puntos de spawn para cada VFX; índice i corresponde a activationVfxPrefabs[i]. Si falta, usa la posición de la estatua.")]
    [SerializeField] private Transform[] activationVfxSpawnPoints;
    [Tooltip("Si está activo, el VFX se parenta al spawnpoint (o a la estatua si no hay spawnpoint).")]
    [SerializeField] private bool parentVfxToSpawn = false;
    [Tooltip("Si > 0, destruye automáticamente cada VFX tras estos segundos.")]
    [SerializeField] private float vfxAutoDestroyAfter = -1f;
    
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 3;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject shadow;
    
    public int UIIndex => uiIndex;

    private float lifeTimer;

    public InteractPriority InteractPriority => InteractPriority.High;

    protected override void Start()
    {
        base.Start();
        shadow.SetActive(false);

        if ((bridgeGrids == null || bridgeGrids.Length == 0) && autoFindBridgeGrids)
        {
#if UNITY_2023_1_OR_NEWER
            bridgeGrids = FindObjectsByType<BridgeConstructionGrid>(FindObjectsSortMode.None);
#else
            bridgeGrids = FindObjectsOfType<BridgeConstructionGrid>();
#endif
        }
    }

    public void Interact(GameObject interactor)
    {
        if (isCarried) return;
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder != null)
        {
            holder.PickUpExistingInstance(gameObject);
            isCarried = true;
            
            // Notificar que la estatua fue agarrada
            OnStatuePickedUp?.Invoke(this, interactor);
            
            // Suscribirse al evento OnDropped para detectar cuando se suelta
            holder.OnDropped += OnStatueDropped;
            
            // Feedback visual/sonoro opcional aquí
        }
    }
    
    private void OnStatueDropped(GameObject droppedObject)
    {
        // Verificar que el objeto soltado sea esta estatua
        if (droppedObject == gameObject)
        {
            isCarried = false;
            
            // Desuscribirse del evento para evitar memory leaks
            PlayerObjectHolder[] holders = FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
            foreach (var holder in holders)
            {
                holder.OnDropped -= OnStatueDropped;
            }
        }
    }
    
    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }

    private void Update()
    {
        // Contador de vida específico de la estatua (independiente del TTL de PowerUpBase)
        // Countdown de vida
        if (!isActive && lifeDuration > 0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeDuration)
            {
                // Si expira por tiempo sin ser activada, solo se destruye sin efecto
                Despawn();
            }
        }

        if (isCarried)
        {
            canvas.SetActive(false);
        }
        else
        {
            canvas.SetActive(true);
        }
    }

    // Detección de Arrow para activar la construcción y luego morir
    private void OnCollisionEnter(Collision collision)
    {
        if (isActive) return;
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
        {
            TryActivate(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive) return;
        if (other.GetComponent<Arrow>() != null)
        {
            TryActivate(other.gameObject);
        }
    }

    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa indicada en buildUpToLayer,
    /// usando el mismo patrón que PowerUpRitualGranFuego y PowerUpCalorHumano.
    /// </summary>
    private void ConstructBridgeAutomatically()
    {
        if (bridgeGrids == null || bridgeGrids.Length == 0) return;

        foreach (var grid in bridgeGrids)
        {
            if (grid == null) continue;

            int maxGridLayer = (grid.layerHeights != null)
                ? Mathf.Max(0, grid.layerHeights.Length - 1)
                : 2;
            int targetMax = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);

            for (int x = 0; x < grid.gridWidth; x++)
            {
                for (int z = 0; z < grid.gridLength; z++)
                {
                    for (int layerIndex = 0; layerIndex <= targetMax; layerIndex++)
                    {
                        // Usar TryBuildLayer del grid (igual que los otros power-ups)
                        // Esto actualiza automáticamente el SO, el currentLayer, visuales y sonidos
                        grid.TryBuildLayer(x, z, layerIndex, null);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Efecto del power-up: construir el puente y luego destruir la estatua.
    /// </summary>
    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Construcción automática
        ConstructBridgeAutomatically();

        // Instanciar efectos de activación
        SpawnActivationVfx();

        // Despawn inmediato tras el efecto
        Despawn();
        yield break;
    }

    /// <summary>
    /// Instancia los VFX de activación mapeando por índice a sus spawnpoints.
    /// </summary>
    private void SpawnActivationVfx()
    {
        if (activationVfxPrefabs == null || activationVfxPrefabs.Length == 0) return;

        for (int i = 0; i < activationVfxPrefabs.Length; i++)
        {
            var prefab = activationVfxPrefabs[i];
            if (prefab == null) continue;

            Transform sp = (activationVfxSpawnPoints != null && i < activationVfxSpawnPoints.Length)
                ? activationVfxSpawnPoints[i]
                : null;

            Vector3 pos = sp != null ? sp.position : transform.position;
            Quaternion rot = sp != null ? sp.rotation : transform.rotation;

            GameObject vfx = Instantiate(prefab, pos, rot);
            if (parentVfxToSpawn)
            {
                vfx.transform.SetParent(sp != null ? sp : transform, true);
            }

            if (vfxAutoDestroyAfter > 0f)
            {
                Destroy(vfx, vfxAutoDestroyAfter);
            }
        }
    }
}