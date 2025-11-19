using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpCalorHumano : PowerUpBase, IInteractable
{
    // Evento estático para notificar cuando el PowerUp es activado (TurnOn llamado)
    public static event System.Action<PowerUpCalorHumano> OnCalorHumanoActivated;
    
    [Header("Requerimientos de Activación (Carbón)")]
    [Tooltip("Cantidad de carbones necesarios para activar el power up.")]
    public int carbonesNecesarios = 3;

    [Header("Visual")]
    [SerializeField] private GameObject shadow;

    [Header("UI")]
    [SerializeField] private Image[] coalImages;
    [SerializeField] private TextMeshProUGUI[] coalTexts;

    [Header("Debug")]
    public bool debugLogs = false;

    // Estado interno
    private int carbonesActuales = 0;

    [Header("Construcción Automática")]
    [Tooltip("Lista de grids de puente sobre los que aplicar construcción automática al finalizar Calor Humano.")]
    [SerializeField] private BridgeConstructionGrid[] bridgeGrids;
    [Tooltip("Si está activo y la lista está vacía, autodescubre todos los BridgeConstructionGrid en la escena al iniciar.")]
    [SerializeField] private bool autoFindBridgeGrids = true;
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie) aplicado a cada grid.")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 2;

    [Header("VFX de Activación")]
    [Tooltip("Lista de efectos VFX a instanciar al activarse el power-up.")]
    [SerializeField] private GameObject[] activationVfxPrefabs;
    [Tooltip("Puntos de spawn para cada VFX; índice i corresponde a activationVfxPrefabs[i]. Si falta, usa la posición de este objeto.")]
    [SerializeField] private Transform[] activationVfxSpawnPoints;
    [Tooltip("Si está activo, el VFX se parenta al spawnpoint correspondiente (si existe). Útil para efectos adheridos a la escena.")]
    [SerializeField] private bool parentVfxToSpawn = false;
    [Tooltip("Si > 0, destruye automáticamente cada VFX tras estos segundos.")]
    [SerializeField] private float vfxAutoDestroyAfter = -1f;

    [Header("Interacción (Carbón)")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    public InteractPriority InteractPriority => interactPriority;

    #region Interacción estilo Furnace
    public void Interact(GameObject interactor)
    {
        if (isActive) return; // Ya activo, ignorar
        TryAddCoal(interactor);
    }

    public bool TryAddCoal(GameObject interactor)
    {
        if (carbonesActuales >= carbonesNecesarios || isActive) return false;
        if (interactor == null) return false;

        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand()) return false;

        GameObject held = holder.GetHeldObject();
        if (held == null) return false;

        // Buscar CoalItem en el objeto o hijos
        CoalItem coal = held.GetComponent<CoalItem>();
        if (coal == null) coal = held.GetComponentInChildren<CoalItem>();
        if (coal == null) return false; // No es carbón

        int before = carbonesActuales;
        carbonesActuales++;
        holder.UseHeldObject(); // consumir
        if (debugLogs) Debug.Log($"[CalorHumano] Carbón aceptado ({before} -> {carbonesActuales})", this);

        UpdateUI();

        if (carbonesActuales >= carbonesNecesarios)
        {
            if (debugLogs) Debug.Log("[CalorHumano] Requisitos completos. (TryActivate)", this);
            // Activamos el PowerUp usando el flujo estándar de PowerUpBase
            TryActivate(interactor);
        }
        return true;
    }
    #endregion

    // Método legacy opcional (retiene compatibilidad si alguien lo llama explícitamente)
    public void InsertarCarbon()
    {
        if (carbonesActuales >= carbonesNecesarios || isActive) return;
        carbonesActuales++;
        if (debugLogs) Debug.Log($"[CalorHumano] (Legacy) Carbón insertado {carbonesActuales}/{carbonesNecesarios}", this);
        
        UpdateUI();
        
        if (carbonesActuales >= carbonesNecesarios)
        {
            if (debugLogs) Debug.Log("[CalorHumano] (Legacy) Requisitos completos. (TryActivate)", this);
            TryActivate(null);
        }
    }
    
    private new void Start()
    {
        shadow.SetActive(false);
        UpdateUI();

        if ((bridgeGrids == null || bridgeGrids.Length == 0) && autoFindBridgeGrids)
        {
#if UNITY_2023_1_OR_NEWER
            bridgeGrids = FindObjectsByType<BridgeConstructionGrid>(FindObjectsSortMode.None);
#else
            bridgeGrids = FindObjectsOfType<BridgeConstructionGrid>();
#endif
        }
    }

    private void Update() { /* activación ahora sucede en TryAddCoal / InsertarCarbon */ }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Solo permitimos el efecto si se cumplió el requisito de carbones
        if (carbonesActuales < carbonesNecesarios)
        {
            if (debugLogs) Debug.Log("[CalorHumano] EffectCoroutine cancelado: no se cumplen carbones.", this);
            yield break;
        }
        
        // Notificar que el PowerUp fue activado (para tutoriales)
        OnCalorHumanoActivated?.Invoke(this);

        // Ocultar / actualizar UI al activarse
        UpdateUI();

        // Instanciar efectos de activación en sus spawnpoints
        SpawnActivationVfx();

        // Construir automáticamente, igual que RitualGranFuego
        ConstructBridgeAutomatically();

        // Calor Humano: efecto instantáneo, no hace falta esperar duration
        Despawn();
        yield break;
    }

    /// <summary>
    /// Instancia los VFX de activación mapeando por índice a sus spawnpoints.
    /// </summary>
    private void SpawnActivationVfx()
    {
        if (activationVfxPrefabs == null || activationVfxPrefabs.Length == 0) return;

        int count = activationVfxPrefabs.Length;
        for (int i = 0; i < count; i++)
        {
            var prefab = activationVfxPrefabs[i];
            if (prefab == null) continue;

            Transform sp = (activationVfxSpawnPoints != null && i < activationVfxSpawnPoints.Length)
                ? activationVfxSpawnPoints[i]
                : null;

            Vector3 pos = sp != null ? sp.position : transform.position;
            Quaternion rot = sp != null ? sp.rotation : transform.rotation;

            GameObject vfx = Instantiate(prefab, pos, rot);
            if (parentVfxToSpawn && sp != null)
            {
                vfx.transform.SetParent(sp, true);
            }

            if (vfxAutoDestroyAfter > 0f)
            {
                Destroy(vfx, vfxAutoDestroyAfter);
            }

            if (debugLogs)
            {
                Debug.Log($"[CalorHumano] VFX instanciado: {prefab.name} en {(sp != null ? sp.name : name)}", this);
            }
        }
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa indicada en buildUpToLayer,
    /// reutilizando la misma lógica que PowerUpRitualGranFuego.
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
                        grid.TryBuildLayer(x, z, layerIndex, null);
                    }
                }
            }
        }
    }

    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    private void UpdateUI()
    {
        // Actualizar textos del carbón
        if (coalTexts != null)
        {
            foreach (var coalText in coalTexts)
            {
                if (coalText != null)
                {
                    coalText.text = $"{carbonesActuales}/{carbonesNecesarios}";
                }
            }
        }
    }
}