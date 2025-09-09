using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    public float interactionRadius;

    private Collider[] interactables = new Collider[5];
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private KeyCode interactKey = KeyCode.P;
    [SerializeField] private KeyCode dropKey = KeyCode.O;
    [SerializeField] private KeyCode buildKey = KeyCode.L;

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
        // Incluir colliders con IsTrigger (como antorchas del tótem)
        int elements = Physics.OverlapSphereNonAlloc(
            interactionPoint.position,
            interactionRadius,
            interactables,
            interactionLayer,
            QueryTriggerInteraction.Collide);

        if (elements == 0)
        {
            return;
        }

        // Selección por prioridad y distancia
        IInteractable mejor = null;
        Collider mejorCol = null;
        InteractPriority mejorPri = InteractPriority.VeryLow;
        float mejorDist = float.MaxValue;

    // Favorecer TorchInteractable si sostenemos PaloIgnifugo encendido
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
            var comp = col.GetComponentInParent<IInteractable>();
            if (comp == null) continue;
            float dist = Vector3.Distance(interactionPoint.position, col.transform.position);
            var torch = col.GetComponentInParent<TorchInteractable>();
            var priEff = comp.InteractPriority;
            if (paloEncendido && torch != null) priEff = InteractPriority.VeryHigh;
            if (priEff > mejorPri || (priEff == mejorPri && dist < mejorDist))
            {
                mejor = comp; mejorCol = col; mejorPri = priEff; mejorDist = dist;
            }
        }

    if (mejor != null) mejor.Interact(this.gameObject);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
}

