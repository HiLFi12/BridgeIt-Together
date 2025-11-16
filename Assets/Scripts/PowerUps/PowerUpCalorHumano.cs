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
    [Tooltip("Grid de puente sobre el que aplicar construcción automática al finalizar Calor Humano.")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie). Igual que RitualGranFuego.")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 2;

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

        // Construir automáticamente, igual que RitualGranFuego
        ConstructBridgeAutomatically();

        // Calor Humano: efecto instantáneo, no hace falta esperar duration
        Despawn();
        yield break;
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa indicada en buildUpToLayer,
    /// reutilizando la misma lógica que PowerUpRitualGranFuego.
    /// </summary>
    private void ConstructBridgeAutomatically()
    {
        if (bridgeGrid == null) return;

        int maxGridLayer = (bridgeGrid.layerHeights != null)
            ? Mathf.Max(0, bridgeGrid.layerHeights.Length - 1)
            : 2;

        int targetMax = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);

        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                for (int layerIndex = 0; layerIndex <= targetMax; layerIndex++)
                {
                    bridgeGrid.TryBuildLayer(x, z, layerIndex, null);
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