using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PowerUp "Calor Humano": requiere solo cargar carbón (similar a Furnace).
/// Al llenarse: busca HeatSphere en jugadores, los enciende y refresca su cooldown periódicamente durante effectDuration.
/// No modifica HeatSphere; usa ResetCooldown() y SetActive(true) para mantenerlos encendidos.
/// </summary>
public class PowerUpCalorHumano : PowerUpBase, IInteractable, ITurnable
{
    [Header("Requerimientos de Activación (Carbón)")]
    [Tooltip("Cantidad de carbones necesarios para activar el power up.")]
    public int carbonesNecesarios = 3;

    [Header("Efecto de Calor a Jugadores")]
    [Tooltip("Duración total del efecto manteniendo vivos los HeatSphere de los jugadores.")]
    public float effectDuration = 20f;
    [Tooltip("Intervalo con el que se refresca el cooldown de cada HeatSphere mientras dura el efecto.")]
    public float heatRefreshInterval = 0.75f;
    [Tooltip("Si está activo, solo se encenderán HeatSphere que estén bajo un Player. (Recomendado ON)")]
    [SerializeField] private bool soloHeatDeJugadores = true;
    [Tooltip("Opcional: si no se encuentra componente Player se puede usar un tag para validar el HeatSphere.")]
    [SerializeField] private string playerRootTag = "Player";

    [Header("Debug")]
    public bool debugLogs = false;

    [Header("Protección ITurnable")]
    [Tooltip("Ignora TurnOff externos (llamados por otros sistemas) hasta que termine la duración interna.")]
    [SerializeField] private bool protectFromExternalTurnOff = true;

    // Indicador de que el apagado fue iniciado por la propia rutina interna
    private bool internalTurnOffRequest = false;
    // Indicador de que el encendido fue solicitado internamente (al cumplir carbones)
    private bool internalTurnOnRequest = false;

    // Estado interno
    private int carbonesActuales = 0;
    private readonly List<HeatSphere> _playerHeatSpheres = new List<HeatSphere>();

    [Header("Interacción (Carbón)")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    public InteractPriority InteractPriority => interactPriority;

    // ITurnable
    public bool isTurned => isActive; // Reutilizamos isActive de PowerUpBase como estado encendido

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

        if (carbonesActuales >= carbonesNecesarios)
        {
            if (debugLogs) Debug.Log("[CalorHumano] Requisitos completos. (TurnOn interno)", this);
            internalTurnOnRequest = true;
            TurnOn();
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
        if (carbonesActuales >= carbonesNecesarios)
        {
            internalTurnOnRequest = true;
            TurnOn();
        }
    }

    private void Update() { /* activación ahora sucede en TryAddCoal / InsertarCarbon */ }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // No usado directamente; la lógica se ejecuta desde TurnOn/TurnOff ahora.
        yield break;
    }

    public void TurnOn()
    {
        if (isActive) return;
        // Si NO es un encendido interno (carbones completos) y aún no se cumple requisito, ignorar
        if (!internalTurnOnRequest && carbonesActuales < carbonesNecesarios)
        {
            if (debugLogs) Debug.Log("[CalorHumano] TurnOn externo ignorado (no cumple requisitos).", this);
            return;
        }

        // Consumir la bandera de encendido interno
        internalTurnOnRequest = false;

        isActive = true;
        isAvailable = false;
        internalTurnOffRequest = false;
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        if (debugLogs) Debug.Log("[CalorHumano] TurnOn() -> iniciando efecto válido.", this);
        StartCoroutine(RunHeatEffect());
    }

    public void TurnOff()
    {
        if (protectFromExternalTurnOff && !internalTurnOffRequest)
        {
            if (debugLogs) Debug.Log("[CalorHumano] TurnOff externo ignorado (protegido).", this);
            return; // Se ignora apagado de sistemas externos
        }
        if (!isActive) return;
        if (debugLogs) Debug.Log("[CalorHumano] TurnOff() -> finalizando efecto.", this);
        isActive = false;
        // No destruimos de inmediato: dejamos que los HeatSphere expiren solos
        Despawn();
    }

    private IEnumerator RunHeatEffect()
    {
        GatherPlayerHeatSpheres();
        float elapsed = 0f;
        while (elapsed < effectDuration && isActive)
        {
            RefreshHeatSpheres();
            yield return new WaitForSeconds(heatRefreshInterval);
            elapsed += heatRefreshInterval;
        }
        if (debugLogs) Debug.Log("[CalorHumano] Duración completada o desactivado. TurnOff interno.", this);
        internalTurnOffRequest = true;
        TurnOff();
    }

    private void GatherPlayerHeatSpheres()
    {
        _playerHeatSpheres.Clear();

        var allHeat = FindObjectsOfType<HeatSphere>(true);
        foreach (var hs in allHeat)
        {
            if (hs == null) continue;
            if (soloHeatDeJugadores)
            {
                // Criterio 1: Tiene un componente Player en su jerarquía padre
                bool esDeJugador = hs.GetComponentInParent<Player>() != null;

                // Criterio 2 (fallback opcional): El root (o algún padre) tiene un tag específico
                if (!esDeJugador && !string.IsNullOrEmpty(playerRootTag))
                {
                    Transform t = hs.transform;
                    while (t != null && !esDeJugador)
                    {
                        if (t.CompareTag(playerRootTag))
                        {
                            esDeJugador = true;
                            break;
                        }
                        t = t.parent;
                    }
                }

                if (!esDeJugador)
                {
                    // Saltar HeatSphere que no son de jugador (e.g., hornos u otros power-ups)
                    continue;
                }
            }

            _playerHeatSpheres.Add(hs);
        }

        if (debugLogs) Debug.Log($"[CalorHumano] HeatSphere jugadores detectados: {_playerHeatSpheres.Count}", this);
    }

    private void RefreshHeatSpheres()
    {
        for (int i = _playerHeatSpheres.Count - 1; i >= 0; i--)
        {
            var hs = _playerHeatSpheres[i];
            if (hs == null)
            {
                _playerHeatSpheres.RemoveAt(i);
                continue;
            }
            if (!hs.gameObject.activeSelf)
                hs.gameObject.SetActive(true);
            hs.ResetCooldown(); // renueva el tiempo interno
        }
    }
}