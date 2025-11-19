using System.Collections;
using UnityEngine;

/// <summary>
/// Power Up de la era prehistórica: "Ritual del Gran Fuego"
/// Los jugadores deben usar PaloIgnifugo encendidos para encender ambas antorchas del tótem
/// casi simultáneamente para activar el efecto que construye automáticamente los cuadrantes
/// del puente hasta la capa 3.
/// </summary>
public class PowerUpRitualGranFuego : PowerUpBase
{
    [Header("Referencias del Tótem")]
    [SerializeField] private GameObject leftTorchCollider; // Collider de la antorcha izquierda
    [SerializeField] private GameObject rightTorchCollider; // Collider de la antorcha derecha
    [Tooltip("Grillas de construcción que serán afectadas por el ritual. Si está vacío, se autodescubre en Start.")]
    [SerializeField] private BridgeConstructionGrid[] bridgeGrids; // Multi-grid support
    [Tooltip("Mantener referencia legacy para otros sistemas; se asigna al primer elemento de bridgeGrids si existe.")]
    public BridgeConstructionGrid bridgeGrid; // Legacy single reference (primera grilla)
    
    [Header("Configuración del Ritual")]
    [SerializeField] private float torchActiveTime = 1f; // Tiempo que permanece encendida cada antorcha
    [SerializeField] private GameObject torchFireEffectPrefab; // Efecto visual del fuego en las antorchas
    
    // Estado de las antorchas
    private bool leftTorchLit = false;
    private bool rightTorchLit = false;
    private float leftTorchTimer = 0f;
    private float rightTorchTimer = 0f;
    
    // Efectos visuales de las antorchas
    private GameObject leftTorchFireEffect;
    private GameObject rightTorchFireEffect;
    [SerializeField] private Transform leftFireSpawnPoint; // Nuevo: punto de spawn del fuego izquierda
    [SerializeField]private Transform rightFireSpawnPoint; // Nuevo: punto de spawn del fuego derecha

    [Header("Lifetime / Despawn")]
    [SerializeField, Tooltip("Efecto visual al morir (opcional)")]
    private GameObject dieEffectPrefab;
    [SerializeField, Tooltip("Si true, el efecto se parenta al tótem antes de ocultarlo")]
    private bool attachDieEffect = false;
    private bool isDead = false;
    public System.Action OnDie;

    [Header("Lifetime Options")]
    [SerializeField, Tooltip("Si está activado, el power-up no expirará por tiempo (TTL) y permanecerá en el mapa hasta ser consumido.")]
    private bool infiniteLifetime = false;

    [Header("Construcción Automática")]
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie).")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 2; // por defecto construye hasta capa 3 (índice 2)

    [Header("VFX de Activación")]
    [Tooltip("Lista de efectos VFX a instanciar al activarse el ritual.")]
    [SerializeField] private GameObject[] activationVfxPrefabs;
    [Tooltip("Puntos de spawn para cada VFX; índice i corresponde a activationVfxPrefabs[i]. Si falta, usa la posición del tótem.")]
    [SerializeField] private Transform[] activationVfxSpawnPoints;
    [Tooltip("Si está activo, el VFX se parenta al spawnpoint (o al tótem si no hay spawnpoint).")]
    [SerializeField] private bool parentVfxToSpawn = false;
    [Tooltip("Si > 0, destruye automáticamente cada VFX tras estos segundos.")]
    [SerializeField] private float vfxAutoDestroyAfter = -1f;

    // Permitir que otros sistemas (Grid) respeten el tope configurado
    public int MaxLayerToBuild => buildUpToLayer;

    protected override void Start()
    {
        base.Start();
        // Autodescubrimiento de grillas si no están asignadas
        if (bridgeGrids == null || bridgeGrids.Length == 0)
        {
            bridgeGrids = FindObjectsOfType<BridgeConstructionGrid>();
        }
        bridgeGrid = (bridgeGrids != null && bridgeGrids.Length > 0) ? bridgeGrids[0] : bridgeGrid;

        // Configurar los componentes TorchInteractables en los colliders
        SetupTorchInteractables();
        // Ya no se asignan los puntos de spawn por código, se asignan solo por inspector
    }

    private void SetupTorchInteractables()
    {
        // Configurar antorcha izquierda
    if (leftTorchCollider != null)
        {
            TorchInteractable leftInteractable = leftTorchCollider.GetComponent<TorchInteractable>();
            if (leftInteractable == null)
            {
                leftInteractable = leftTorchCollider.AddComponent<TorchInteractable>();
            }
            leftInteractable.SetupTorch(TorchInteractable.TorchSide.Left, this);
        }

        // Configurar antorcha derecha
    if (rightTorchCollider != null)
        {
            TorchInteractable rightInteractable = rightTorchCollider.GetComponent<TorchInteractable>();
            if (rightInteractable == null)
            {
                rightInteractable = rightTorchCollider.AddComponent<TorchInteractable>();
            }
            rightInteractable.SetupTorch(TorchInteractable.TorchSide.Right, this);
        }
    }

    private void Update()
    {
        if (!isAvailable) return;

        // Manejar temporizador de antorcha izquierda
        if (leftTorchLit)
        {
            leftTorchTimer += Time.deltaTime;
            if (leftTorchTimer >= torchActiveTime)
            {
                ExtinguishLeftTorch();
            }
        }

        // Manejar temporizador de antorcha derecha
        if (rightTorchLit)
        {
            rightTorchTimer += Time.deltaTime;
            if (rightTorchTimer >= torchActiveTime)
            {
                ExtinguishRightTorch();
            }
        }

        // Verificar si ambas antorchas están encendidas simultáneamente
        if (leftTorchLit && rightTorchLit)
        {
            TryActivate(null); // Activación cooperativa sin activador específico
        }
    }

    // Usar el TTL heredado del PowerUpBase para disparar la muerte si no se activó a tiempo
    protected override IEnumerator LifeTimer()
    {
        // Si está en modo infinito, no expira por tiempo: solo se destruye al consumirse
        if (infiniteLifetime)
        {
            yield break;
        }

        yield return new WaitForSeconds(timeToLive);
        if (!isActive)
        {
            Die();
        }
    }

    /// <summary>
    /// Enciende la antorcha izquierda del tótem
    /// </summary>
    public void LightLeftTorch()
    {
        if (!isAvailable) return;

        leftTorchLit = true;
        leftTorchTimer = 0f;
        // Activar efecto visual
        if (torchFireEffectPrefab != null && leftTorchCollider != null)
        {
            if (leftTorchFireEffect != null)
            {
                Destroy(leftTorchFireEffect);
            }
            // Instanciar en el punto de spawn si existe, si no en el centro del collider
            Transform spawnPoint = leftFireSpawnPoint != null ? leftFireSpawnPoint : leftTorchCollider.transform;
            leftTorchFireEffect = Instantiate(torchFireEffectPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
    }

    /// <summary>
    /// Enciende la antorcha derecha del tótem
    /// </summary>
    public void LightRightTorch()
    {
        if (!isAvailable) return;

        rightTorchLit = true;
        rightTorchTimer = 0f;
        // Activar efecto visual
        if (torchFireEffectPrefab != null && rightTorchCollider != null)
        {
            if (rightTorchFireEffect != null)
            {
                Destroy(rightTorchFireEffect);
            }
            // Instanciar en el punto de spawn si existe, si no en el centro del collider
            Transform spawnPoint = rightFireSpawnPoint != null ? rightFireSpawnPoint : rightTorchCollider.transform;
            rightTorchFireEffect = Instantiate(torchFireEffectPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
    }

    /// <summary>
    /// Apaga la antorcha izquierda
    /// </summary>
    private void ExtinguishLeftTorch()
    {
        leftTorchLit = false;
        leftTorchTimer = 0f;
        
        if (leftTorchFireEffect != null)
        {
            Destroy(leftTorchFireEffect);
            leftTorchFireEffect = null;
        }
    }

    /// <summary>
    /// Apaga la antorcha derecha
    /// </summary>
    private void ExtinguishRightTorch()
    {
        rightTorchLit = false;
        rightTorchTimer = 0f;
        
        if (rightTorchFireEffect != null)
        {
            Destroy(rightTorchFireEffect);
            rightTorchFireEffect = null;
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Apagar las antorchas ya que el ritual se completó
        ExtinguishLeftTorch();
        ExtinguishRightTorch();

        if (bridgeGrids != null && bridgeGrids.Length > 0)
        {
            // Construir automáticamente todos los cuadrantes de cada grilla hasta la capa indicada
            foreach (var grid in bridgeGrids)
            {
                if (grid == null) continue;
                ConstructBridgeAutomatically(grid);
            }
        }
        else
        {
            Debug.LogError("PowerUpRitualGranFuego: No se encontraron BridgeConstructionGrid para construir.");
        }

        // Instanciar efectos de activación
        SpawnActivationVfx();

        // Ritual: efecto instantáneo, no necesitamos mantenerlo activo por duration
        Despawn();
        yield break;
    }

    /// <summary>
    /// Lógica de muerte por tiempo: dispara efecto opcional, evento y oculta el objeto.
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
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float ttl = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, ttl);
            }
        }

        OnDie?.Invoke();
        base.Despawn();
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa indicada en buildUpToLayer.
    /// </summary>
    private void ConstructBridgeAutomatically(BridgeConstructionGrid targetGrid)
    {
        if (targetGrid == null) return;
        // Topes seguros según la grilla (fallback a 2 = tres capas)
        int maxGridLayer = (targetGrid.layerHeights != null)
            ? Mathf.Max(0, targetGrid.layerHeights.Length - 1)
            : 2;

        int targetMax = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);

        for (int x = 0; x < targetGrid.gridWidth; x++)
        {
            for (int z = 0; z < targetGrid.gridLength; z++)
            {
                for (int layerIndex = 0; layerIndex <= targetMax; layerIndex++)
                {
                    targetGrid.TryBuildLayer(x, z, layerIndex, null);
                }
            }
        }
    }

    protected override void Despawn()
    {
        // Limpiar efectos visuales antes de destruir
        if (leftTorchFireEffect != null)
        {
            Destroy(leftTorchFireEffect);
        }
        if (rightTorchFireEffect != null)
        {
            Destroy(rightTorchFireEffect);
        }

        base.Despawn();
    }

    // Métodos para validación en el inspector
    private void OnValidate()
    {
        if (leftTorchCollider == null || rightTorchCollider == null)
        {
            Debug.LogWarning("PowerUpRitualGranFuego: Asigna los colliders de las antorchas en el inspector.");
        }
        if (bridgeGrids == null || bridgeGrids.Length == 0)
        {
            bridgeGrids = FindObjectsOfType<BridgeConstructionGrid>();
        }
        bridgeGrid = (bridgeGrids != null && bridgeGrids.Length > 0) ? bridgeGrids[0] : bridgeGrid;
        // Asegurar el rango del tope según la primera grilla disponible
        BridgeConstructionGrid refGrid = bridgeGrid;
        int maxGridLayer = (refGrid != null && refGrid.layerHeights != null)
            ? Mathf.Max(0, refGrid.layerHeights.Length - 1)
            : 2;
        buildUpToLayer = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);
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
