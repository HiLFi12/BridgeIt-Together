using UnityEngine;
using System.Collections;

/// <summary>
/// Horno de la era: requiere un <see cref="HeatSphere"/> (en este objeto o en un hijo) para funcionar.
/// - Implementa IInteractable para que el jugador pueda intentar cocinar / recargar.
/// - Implementa ITurnable: puede ser encendido/apagado por una fuente de calor (el propio HeatSphere u otra).
/// - Mientras esté encendido (IsTurn = true) expone canCook = true.
/// - Si el jugador interactúa y el horno no está encendido, la interacción "no produce" resultado (pero puede servir para agregar carbones vía otro script externo).
/// </summary>
[DisallowMultipleComponent]
public class Furnace : MonoBehaviour, IInteractable, ITurnable
{
    [Header("Referencias")]
    [SerializeField] private HeatSphere heatSphere; // Debe existir
    [Tooltip("Efecto visual de la llama (se escala/activa al encenderse).")]
    [SerializeField] private GameObject flameEffect;

    [Header("Interacción")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [Tooltip("Tiempo opcional de 'cocción' cuando se realiza una acción válida.")]
    [SerializeField] private float cookTime = 2f;

    [Header("Escala de Llama")]
    [Tooltip("Escala de la llama cuando el horno está completamente cargado.")]
    [SerializeField] private Vector3 fullFlameScale = Vector3.one;
    [Tooltip("Escala mínima de la llama (cuando vida está a punto de agotarse pero > 0).")]
    [SerializeField] private Vector3 minFlameScale = new Vector3(0.25f,0.25f,0.25f);

    [Header("Debug")] 
    [SerializeField] private bool debugLogs = false;

    // Estado
    private bool isTurn; // respaldo de ITurnable
    private bool cooking; // para ejemplo simple

    // Propiedad pública solicitada
    public bool CanCook => isTurn && heatSphere != null && heatSphere.EstaEncendido && !cooking;
    public bool IsTurn => isTurn;

    public InteractPriority InteractPriority => interactPriority;

    private void Reset()
    {
        if (!heatSphere) heatSphere = GetComponentInChildren<HeatSphere>();
        if (flameEffect)
        {
            fullFlameScale = flameEffect.transform.localScale;
        }
    }

    private void Awake()
    {
        if (!heatSphere) heatSphere = GetComponentInChildren<HeatSphere>();
        if (flameEffect)
        {
            fullFlameScale = flameEffect.transform.localScale;
            if (heatSphere == null || !heatSphere.EstaEncendido)
                flameEffect.SetActive(false);
        }
    }

    private void Update()
    {
        // Sincronizar encendido con HeatSphere (si cambia externamente)
        if (heatSphere != null)
        {
            if (heatSphere.EstaEncendido && !isTurn)
            {
                TurnOn(heatSphere.gameObject);
            }
            else if (!heatSphere.EstaEncendido && isTurn)
            {
                TurnOff(heatSphere.gameObject);
            }
        }

        // Ajustar escala de la llama según la vida restante (si está encendido)
        if (isTurn && heatSphere != null && flameEffect != null)
        {
            float ratio = Mathf.Approximately(heatSphere.VidaMaxima, 0f) ? 0f : Mathf.Clamp01(heatSphere.VidaActual / heatSphere.VidaMaxima);
            var targetScale = Vector3.Lerp(minFlameScale, fullFlameScale, ratio);
            flameEffect.transform.localScale = targetScale;
            if (!flameEffect.activeSelf)
                flameEffect.SetActive(true);
        }
        else if (!isTurn && flameEffect != null && flameEffect.activeSelf)
        {
            flameEffect.SetActive(false);
        }
    }

    // ITurnable
    public void TurnOn(GameObject source)
    {
        if (isTurn) return;
        isTurn = true;
        if (debugLogs) Debug.Log($"[Furnace] Encendido por {source?.name}", this);
        if (flameEffect && !flameEffect.activeSelf)
            flameEffect.SetActive(true);
    }

    public void TurnOff(GameObject source)
    {
        if (!isTurn) return;
        isTurn = false;
        if (debugLogs) Debug.Log($"[Furnace] Apagado por {source?.name}", this);
        if (flameEffect && flameEffect.activeSelf)
            flameEffect.SetActive(false);
    }

    // IInteractable
    public void Interact(GameObject interactor)
    {
        if (!heatSphere)
        {
            if (debugLogs) Debug.LogWarning("[Furnace] No hay HeatSphere asignado.", this);
            return;
        }
        
        // Si el fuego NO está encendido intentamos agregar carbón desde la mano del jugador
        if (!heatSphere.EstaEncendido)
        {
            if (TryAddCoal(interactor)) return; // si agregamos carbón salimos (nuevo intento posterior encenderá al llegar a requisito)
            if (debugLogs) Debug.Log("[Furnace] No se pudo agregar carbón (jugador no sostiene carbón).", this);
            return;
        }

        // Si está encendido pero aún no se puede cocinar (por estar cocinando) mostrar mensaje
        if (!CanCook)
        {
            if (debugLogs) Debug.Log("[Furnace] Horno ocupado o en estado inválido para cocinar.", this);
            return;
        }

        // Proceso de cocción simple
        if (!cooking)
            StartCoroutine(CookRoutine(interactor));
    }

    /// <summary>
    /// Intenta consumir un carbón del jugador y agregarlo al HeatSphere.
    /// </summary>
    private bool TryAddCoal(GameObject interactor)
    {
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand()) return false;

        GameObject held = holder.GetHeldObject();
        if (held == null) return false;

        // Detección flexible: script marcador CoalItem o nombre contiene "Coal" (por tu prefab actual)
        bool esCarbon = held.GetComponent<CoalItem>() != null || held.name.Contains("Coal") || held.name.Contains("carbon", System.StringComparison.OrdinalIgnoreCase);
        if (!esCarbon) return false;

        // Agregar al HeatSphere
        bool agregado = heatSphere.AgregarCarbon(held);
        if (agregado)
        {
            // Consumir objeto sostenido (similar a PaloIgnifugo al encender antorcha => UseHeldObject)
            holder.UseHeldObject();
            if (debugLogs) Debug.Log("[Furnace] Carbón insertado.", this);
        }
        return agregado;
    }

    private System.Collections.IEnumerator CookRoutine(GameObject user)
    {
        cooking = true;
        float t = 0f;
        if (debugLogs) Debug.Log("[Furnace] Comenzando cocción...", this);
        while (t < cookTime)
        {
            if (!isTurn || heatSphere == null || !heatSphere.EstaEncendido)
            {
                if (debugLogs) Debug.Log("[Furnace] Cocción abortada (fuego apagado).", this);
                cooking = false; yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (debugLogs) Debug.Log("[Furnace] Cocción finalizada.", this);
        cooking = false;
        // Aquí se podría instanciar un resultado / item cocinado.
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (heatSphere != null)
        {
            Gizmos.color = CanCook ? new Color(1f,0.5f,0f,0.6f) : new Color(0.4f,0.4f,0.4f,0.4f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(0.6f,1f,0.6f));
        }
    }
#endif
}
