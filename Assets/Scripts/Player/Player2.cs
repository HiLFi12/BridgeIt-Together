using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player2 : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    public float interactionRadius;

    private Collider[] interactables = new Collider[5];
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private KeyCode interactKey = KeyCode.P;
    [SerializeField] private KeyCode dropKey = KeyCode.O;
    [SerializeField] private KeyCode buildKey = KeyCode.L;

    [Header("Build UI")]
    [SerializeField] private Image buildUIImage;

    private PlayerObjectHolder objectHolder;
    private PlayerBridgeInteraction bridgeInteraction;
    private PlayerAnimator playerAnimator;

    void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        bridgeInteraction = GetComponent<PlayerBridgeInteraction>();
        playerAnimator = GetComponent<PlayerAnimator>();
        // Inicializar BuildUI oculto
        if (buildUIImage != null) buildUIImage.gameObject.SetActive(false);
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

        // Mostrar/ocultar BuildUI según condiciones
        UpdateBuildUI();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }

    public void OnLaunched(Vector3 targetPosition)
    {
        // No forzamos soltar el objeto al ser lanzados; mantenemos el objeto en mano.
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            var obj = objectHolder.GetHeldObject();
            if (obj != null)
            {
                var rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }
    }
}

