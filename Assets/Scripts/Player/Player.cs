using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IHitable
{
    [SerializeField] private Transform interactionPoint;
    public float interactionRadius;

    private Collider[] interactables = new Collider[5];
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    [SerializeField] private KeyCode buildKey = KeyCode.F;

    private PlayerObjectHolder objectHolder;
    private PlayerBridgeInteraction bridgeInteraction;
    private PlayerAnimator playerAnimator;

    void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        bridgeInteraction = GetComponent<PlayerBridgeInteraction>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

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
    }

    private void TryInteract()
    {
        // Importante: incluir colliders con IsTrigger (antorchas) en la búsqueda
        int elements = Physics.OverlapSphereNonAlloc(
            interactionPoint.position,
            interactionRadius,
            interactables,
            interactionLayer,
            QueryTriggerInteraction.Collide);
    if (elements == 0) return;

        // Elegir el mejor candidato por prioridad y distancia
        IInteractable mejorInteractuable = null;
        Collider mejorCollider = null;
        InteractPriority mejorPrioridad = InteractPriority.VeryLow;
        float distanciaMasCercana = float.MaxValue;

        // ¿Sostiene un PaloIgnífugo encendido? Si sí, favorecemos TorchInteractable
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
            mejorInteractuable.Interact(this.gameObject);
            return;
        }
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
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
}