using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Furnace : MonoBehaviour, IInteractable
{
    // Evento estático para notificar cuando un jugador agrega carbón exitosamente
    public static event System.Action<Furnace, GameObject> OnCoalAdded;
    
    // Evento estático para notificar cuando un jugador activa el horno (TurnOn es llamado)
    public static event System.Action<Furnace, GameObject> OnFurnaceTurnedOn;
    
    [SerializeField] private InteractPriority interactPriority = InteractPriority.High;

    [Header("Carbón")]
    [SerializeField] private int maxCoal = 1;
    private int currentCoal = 0;

    [Header("UI")]
    [SerializeField] private GameObject coalImage;
    [SerializeField] private TextMeshProUGUI coalText;
    [SerializeField] private Image heatImage;

    [Header("Heat")]
    [SerializeField] private HeatSphere heatSphere;
    [SerializeField, Tooltip("Si está activo, al apagarse la HeatSphere el horno vacía el carbón para permitir recarga.")] private bool resetCoalWhenHeatEnds = true;
    [SerializeField, Tooltip("Tiempo de bloqueo tras apagarse antes de aceptar nuevo carbón.")] private float reloadDelay = 0f;
    private float reloadTimer = 0f;
    private bool lastHeatActive = false;

    [Header("Interacción Simple")]
    [Tooltip("Si está activo, al entrar un jugador con carbón en el trigger del horno, se consume automáticamente el carbón.")]
    [SerializeField] private bool autoAcceptCoalOnTrigger = false;
    [SerializeField] private GameObject shadow;

    public InteractPriority InteractPriority => interactPriority;

    private void OnValidate()
    {
        var col = GetComponent<Collider>();
        if (col != null && !autoAcceptCoalOnTrigger && col.isTrigger)
        {
            // Aviso: si querés horno sólido, desactiva isTrigger en el collider
            // No lo cambiamos automáticamente para no sorprender en el editor.
            // Debug.LogWarning("[Furnace] El collider está en modo Trigger pero 'autoAcceptCoalOnTrigger' está desactivado. Para horno sólido, desactiva IsTrigger.", this);
        }
    }

    private void Start()
    {
        shadow.SetActive(false);
        UpdateUI();
    }

    public void Interact(GameObject interactor)
    {
        // Verificación explícita: solo interactuar si el jugador tiene carbón en la mano
        if (interactor == null) return;

        if (reloadTimer > 0f) return; // todavía en cooldown de recarga tras apagarse

        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand())
        {
            return;
        }

        GameObject held = holder.GetHeldObject();
        if (held == null)
        {
            return;
        }

        // ¿Es carbón?
        CoalItem coalComponent = held.GetComponent<CoalItem>();
        if (coalComponent == null)
        {
            coalComponent = held.GetComponentInChildren<CoalItem>();
        }

        if (coalComponent == null)
        {
            // No es carbón; no hacemos nada
            return;
        }

        // Es carbón: intentar agregar
        TryAddCoal(interactor);
    }
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }
    
    public bool TryAddCoal(GameObject interactor)
    {
        if (reloadTimer > 0f) return false;
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand())
        {
            Debug.Log("[Furnace] No holder or no object in hand");
            return false;
        }

        GameObject held = holder.GetHeldObject();
        if (held == null)
        {
            Debug.Log("[Furnace] Held object is null");
            return false;
        }

        CoalItem coalComponent = held.GetComponent<CoalItem>();
        if (coalComponent == null)
        {
            coalComponent = held.GetComponentInChildren<CoalItem>();
        }
        if (coalComponent == null)
        {
            Debug.Log($"[Furnace] Object {held.name} does not have CoalItem component");
            return false;
        }

        // If furnace is not full, add coal and turn on
        if (currentCoal < maxCoal)
        {
            currentCoal++;
            holder.UseHeldObject();
            
            bool justTurnedOn = false;
            if (currentCoal >= maxCoal)
            {
                TurnOn();
                justTurnedOn = true;
            }
            
            // Notificar que el jugador agregó carbón exitosamente
            OnCoalAdded?.Invoke(this, interactor);
            
            // Notificar si el horno fue activado (TurnOn fue llamado)
            if (justTurnedOn)
            {
                OnFurnaceTurnedOn?.Invoke(this, interactor);
            }
            
            UpdateUI();
            return true;
        }
        // If furnace is full and heat is active, allow reload and reset cooldown
        else if (heatSphere != null && heatSphere.gameObject.activeSelf)
        {
            holder.UseHeldObject();
            heatSphere.ResetCooldown();
            Debug.Log("[Furnace] Coal added while active, cooldown reset.");
            
            // Notificar que el jugador agregó carbón exitosamente
            OnCoalAdded?.Invoke(this, interactor);
            return true;
        }
        else
        {
            Debug.Log("[Furnace] Coal storage is full and heat is not active.");
            return false;
        }
    }

    private void TurnOn()
    {
        // Activar HeatSphere (mecánica independiente)
        if (heatSphere != null)
        {
            heatSphere.gameObject.SetActive(true);
            heatSphere.ResetCooldown();
            lastHeatActive = true;
        }
    }

    // Método para cuando el carbón se agote (opcional)
    public void TurnOff()
    {
        
    }

    private void Update()
    {
        if (reloadTimer > 0f)
        {
            reloadTimer -= Time.deltaTime;
        }

        if (heatSphere != null)
        {
            bool active = heatSphere.gameObject.activeSelf;
            if (lastHeatActive != active)
            {
                // Cambio de estado de la HeatSphere
                if (lastHeatActive && !active)
                {
                    // flanco de apagado
                    HandleHeatSphereJustTurnedOff();
                }
                UpdateUI();
            }
            // Actualizar UI cada frame si la HeatSphere está activa (para el fillAmount)
            else if (active)
            {
                UpdateUI();
            }
            lastHeatActive = active;
        }
    }

    private void HandleHeatSphereJustTurnedOff()
    {
        if (resetCoalWhenHeatEnds)
        {
            currentCoal = 0;
            if (reloadDelay > 0f) reloadTimer = reloadDelay;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // Actualizar texto del carbón
        if (coalText != null)
        {
            coalText.text = $"{currentCoal}/{maxCoal}";
        }

        bool heatSphereActive = heatSphere != null && heatSphere.gameObject.activeSelf;

        // Actualizar visibilidad de la imagen del carbón
        // La imagen se muestra solo cuando la HeatSphere está desactivada
        if (coalImage != null)
        {
            coalImage.gameObject.SetActive(!heatSphereActive);
        }

        // Actualizar visibilidad y fillAmount de la imagen de calor
        // La imagen se muestra solo cuando la HeatSphere está activada
        if (heatImage != null)
        {
            heatImage.gameObject.SetActive(heatSphereActive);
            
            if (heatSphereActive)
            {
                // Actualizar el fillAmount basado en el cooldown
                // GetCooldownProgress retorna 1 cuando recién inicia y 0 cuando termina
                heatImage.fillAmount = heatSphere.GetCooldownProgress();
            }
        }
    }
}