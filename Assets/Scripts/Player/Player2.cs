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
        Debug.Log("🎮 TryInteract() llamado - buscando interacciones...");
        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer);

        if (elements == 0)
            return;

        for (int i = 0; i < interactables.Length; i++)
        {
            var interactable = interactables[i];
            if (interactable == null) continue;

            var interactableComponent = interactable.GetComponent<IInteractable>();

            if (interactableComponent != null)
            {
                interactableComponent.Interact(this.gameObject);
                return;
            }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
}

