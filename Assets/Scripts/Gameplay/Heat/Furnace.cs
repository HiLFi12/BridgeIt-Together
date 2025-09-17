using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class Furnace : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractPriority interactPriority = InteractPriority.High;

    [Header("Carbón")]
    [SerializeField] private int maxCoal = 1;
    private int currentCoal = 0;

    [Header("Heat")]
    [SerializeField] private HeatSphere heatSphere;

    [Header("Cooking")]
    [SerializeField] private float cookTime = 2f;
    [SerializeField] private bool debugLogs = false;
    private bool cooking = false;
    private bool canCook = false;
    [Header("Interacción Simple")]
    [Tooltip("Si está activo, al entrar un jugador con carbón en el trigger del horno, se consume automáticamente el carbón.")]
    [SerializeField] private bool autoAcceptCoalOnTrigger = false;

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

    public void Interact(GameObject interactor)
    {
        // Si no hay carbón suficiente, tratar de agregar carbón
        if (currentCoal < maxCoal)
        {
            TryAddCoal(interactor);
            return;
        }

        // Si puede cocinar y no está cocinando, cocinar
        if (canCook && !cooking)
        {
            StartCoroutine(CookRoutine(interactor));
        }
        else if (debugLogs)
        {
            Debug.Log("[Furnace] Cannot cook - furnace is not ready or busy.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoAcceptCoalOnTrigger) return;
        var holder = other.GetComponentInParent<PlayerObjectHolder>();
        if (holder != null && currentCoal < maxCoal)
        {
            TryAddCoal(holder.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!autoAcceptCoalOnTrigger) return;
        var holder = other.GetComponentInParent<PlayerObjectHolder>();
        if (holder != null && currentCoal < maxCoal)
        {
            TryAddCoal(holder.gameObject);
        }
    }

    public bool TryAddCoal(GameObject interactor)
    {
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

        Debug.Log($"[Furnace] Checking object: {held.name}");

        // Aceptar carbón incluso si el componente está en un hijo
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

        Debug.Log($"[Furnace] Valid coal found: {held.name}");

        if (currentCoal < maxCoal)
        {
            Debug.Log($"[Furnace] Before: currentCoal = {currentCoal}, held object = {held.name}");
        
            currentCoal++;
            holder.UseHeldObject();
        
            Debug.Log($"[Furnace] After: currentCoal = {currentCoal}, holder has object = {holder.HasObjectInHand()}");

            if (currentCoal >= maxCoal)
            {
                TurnOn();
            }

            return true;
        }
        else
        {
            Debug.Log("[Furnace] Coal storage is full.");
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
        }

        // Activar capacidad de cocinar del horno
        canCook = true;
    }

    private IEnumerator CookRoutine(GameObject user)
    {
        cooking = true;
        float t = 0f;

        if (debugLogs) Debug.Log("[Furnace] Starting cooking process...", this);

        while (t < cookTime)
        {
            // Verificar si el horno todavía puede cocinar
            if (!canCook)
            {
                if (debugLogs) Debug.Log("[Furnace] Cooking aborted - furnace can no longer cook.", this);
                cooking = false;
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (debugLogs) Debug.Log("[Furnace] Cooking completed!", this);
        cooking = false;

        // Aquí agregas la lógica de mezclar materiales
        // ProcessCookedItems(user);
    }

    // Método para cuando el carbón se agote (opcional)
    public void TurnOff()
    {
        canCook = false;
        if (debugLogs) Debug.Log("[Furnace] Furnace turned off - cooking disabled.", this);
    }
}