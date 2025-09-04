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
        Debug.Log("🎮 TryInteract() llamado - buscando interacciones...");
        Debug.Log($"🔍 Punto de interacción: {interactionPoint.position}");
        Debug.Log($"🔍 Radio de interacción: {interactionRadius}");
        Debug.Log($"🔍 Layer de interacción: {interactionLayer.value} (bits: {Convert.ToString(interactionLayer.value, 2)})");
        
        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer);
        
        Debug.Log($"🔍 Objetos detectados: {elements}");
        
        if (elements == 0)
        {
            Debug.Log("❌ No se detectaron objetos interactuables");
            return;
        }

        for (int i = 0; i < elements; i++) // Cambié de interactables.Length a elements para solo iterar sobre los detectados
        {
            var interactable = interactables[i];
            if (interactable == null) continue;

            Debug.Log($"🔍 Objeto detectado {i}: {interactable.name} - Layer: {interactable.gameObject.layer} ({LayerMask.LayerToName(interactable.gameObject.layer)})");
            Debug.Log($"🔍 Posición del objeto: {interactable.transform.position}");
            Debug.Log($"🔍 Distancia al jugador: {Vector3.Distance(interactionPoint.position, interactable.transform.position)}");

            var interactableComponent = interactable.GetComponent<IInteractable>();
            Debug.Log($"🔍 ¿Tiene IInteractable? {interactableComponent != null}");
            
            if (interactableComponent != null)
            {
                Debug.Log($"✅ Interactuando con: {interactable.name}");
                interactableComponent.Interact(this.gameObject);
                return;
            }
            else
            {
                Debug.Log($"❌ {interactable.name} no tiene componente IInteractable");
            }
        }
        
        Debug.Log("❌ Ningún objeto detectado tenía componente IInteractable");
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