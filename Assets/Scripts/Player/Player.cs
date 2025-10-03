using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IHitable
{
    [SerializeField] private Transform interactionPoint;
    public float interactionRadius;

    private Collider[] interactables = new Collider[5];
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    [SerializeField] private KeyCode buildKey = KeyCode.F;
    
    [Header("Interaction UI")]
    [SerializeField] private Image interactionUIImage;

    [Header("Build UI")]
    [SerializeField] private Image buildUIImage;

    private PlayerObjectHolder objectHolder;
    private PlayerBridgeInteraction bridgeInteraction;
    private PlayerAnimator playerAnimator;

    [Header("Debug Bridge Hotkey")]
    [SerializeField] private bool enableFillBridgeHotkey = false;
    [SerializeField] private KeyCode fillBridgeKey = KeyCode.G;
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // puede asignarse manualmente, o se buscará

    void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        bridgeInteraction = GetComponent<PlayerBridgeInteraction>();
        playerAnimator = GetComponent<PlayerAnimator>();
        interactionUIImage.gameObject.SetActive(false);
        // Inicializar BuildUI oculto
        if (buildUIImage != null) buildUIImage.gameObject.SetActive(false);

        // Intentar auto-asignar grid si no se arrastró en inspector
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }
    }

    void Update()
    {
        TryInteract();

        if (Input.GetKeyDown(dropKey))
        {
            TryDropObject();
        }

        if (Input.GetKeyDown(buildKey) && bridgeInteraction != null)
        {
            bridgeInteraction.TryInteractWithQuadrant();
            
            // Activar animación de construcción
            if (playerAnimator != null)
            {
                playerAnimator.TriggerBuildAnimation();
            }
        }

        // Mostrar/ocultar BuildUI según condiciones
        UpdateBuildUI();

        // Hotkey debug para rellenar todo el puente
        if (enableFillBridgeHotkey && Input.GetKeyDown(fillBridgeKey))
        {
            if (bridgeGrid != null)
            {
                // Llamar a método público que llena todo (asumiendo DebugRellenarTodoPuente es público)
                bridgeGrid.DebugRellenarTodoPuente();
            }
            else
            {
                Debug.LogWarning("[Player] Hotkey de rellenar puente presionado pero no se encontró BridgeConstructionGrid en la escena.");
            }
        }
    }

    private void TryInteract()
    {
        int elements = Physics.OverlapSphereNonAlloc(
            interactionPoint.position,
            interactionRadius,
            interactables,
            interactionLayer,
            QueryTriggerInteraction.Collide);

        if (elements == 0) 
        {
            HideInteractionUI();
            return;
        }

        IInteractable mejorInteractuable = null;
        Collider mejorCollider = null;
        InteractPriority mejorPrioridad = InteractPriority.VeryLow;
        float distanciaMasCercana = float.MaxValue;

        bool paloEncendido = false;
        var holder = GetComponent<PlayerObjectHolder>();
        if (holder != null && holder.HasObjectInHand())
        {
            var palo = holder.GetHeldObject()?.GetComponent<PaloIgnifugo>();
            paloEncendido = palo != null && palo.EstaEncendido();
        }

        for (int i = 0; i < elements; i++)
        {
            var col = interactables[i];
            if (col == null) continue;

            var candidato = col.GetComponentInParent<IInteractable>();
            if (candidato == null) continue;

            float distancia = Vector3.Distance(interactionPoint.position, col.transform.position);
            var torch = col.GetComponentInParent<TorchInteractable>();
            var prioridadEfectiva = candidato.InteractPriority;
            if (paloEncendido && torch != null)
            {
                prioridadEfectiva = InteractPriority.VeryHigh;
            }
            if (prioridadEfectiva > mejorPrioridad ||
                (prioridadEfectiva == mejorPrioridad && distancia < distanciaMasCercana))
            {
                mejorInteractuable = candidato;
                mejorCollider = col;
                mejorPrioridad = prioridadEfectiva;
                distanciaMasCercana = distancia;
            }
        }

        if (mejorInteractuable != null)
        {
            ShowInteractionUI();

            if (Input.GetKeyDown(interactKey))
            {
                mejorInteractuable.Interact(this.gameObject);
            }
        }
        else
        {
            HideInteractionUI();
        }
    }

    private void ShowInteractionUI()
    {
        // Si las manos están ocupadas, no mostrar el InteractionUI
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            HideInteractionUI();
            return;
        }

        if (interactionUIImage != null && !interactionUIImage.gameObject.activeInHierarchy)
        {
            interactionUIImage.gameObject.SetActive(true);
        }
    }

    private void HideInteractionUI()
    {
        if (interactionUIImage != null && interactionUIImage.gameObject.activeInHierarchy)
        {
            interactionUIImage.gameObject.SetActive(false);
        }
    }

    // Mostrar/ocultar BuildUI (similar a InteractionUI)
    private void UpdateBuildUI()
    {
        if (buildUIImage == null || bridgeInteraction == null || objectHolder == null)
        {
            HideBuildUI();
            return;
        }

        bool hasMaterialInHand = objectHolder.HasObjectInHand() &&
                                 objectHolder.GetHeldObject() != null &&
                                 objectHolder.GetHeldObject().GetComponent<BridgeMaterialInfo>() != null;

        bool targetInRange = hasMaterialInHand && bridgeInteraction.HasTargetQuadrantInRange();

        if (targetInRange) ShowBuildUI();
        else HideBuildUI();
    }

    private void ShowBuildUI()
    {
        if (!buildUIImage.gameObject.activeInHierarchy)
            buildUIImage.gameObject.SetActive(true);
    }

    private void HideBuildUI()
    {
        if (buildUIImage.gameObject.activeInHierarchy)
            buildUIImage.gameObject.SetActive(false);
    }

    private void TryDropObject()
    {
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            objectHolder.DropObject();
            
            // Activar animación de drop
            if (playerAnimator != null)
            {
                playerAnimator.TriggerDropAnimation();
            }
        }
    }
    
    public void OnLaunched(Vector3 targetPosition)
    {
        var holder = GetComponent<PlayerObjectHolder>();
        if (holder != null && holder.HasObjectInHand())
        {
            var obj = holder.GetHeldObject();
            if (obj != null)
            {
                // Dejar de ser hijo del jugador y mantener posición/rotación en mundo
                obj.transform.SetParent(null, true);

                // Reactivar física del objeto para que pueda ser impactado inmediatamente
                var rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            // Limpiar estado interno del holder sin usar DropObject
            var t = typeof(PlayerObjectHolder);
            var fObj = t.GetField("heldObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fObj != null) fObj.SetValue(holder, null);
            var fRb = t.GetField("heldRigidbody", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fRb != null) fRb.SetValue(holder, null);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
}