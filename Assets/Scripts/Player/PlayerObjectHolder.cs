using UnityEngine;

public class PlayerObjectHolder : MonoBehaviour
{
    [Header("Ancla para sostener (opcional)")]
    [SerializeField] private Transform holdAnchor;

    [Header("Rotación por defecto (genéricos)")]
    [SerializeField] private Vector3 objectLocalRotation = Vector3.zero;

    [Header("Overrides por tipo de objeto")]
    [SerializeField] private Vector3 paloIgnifugoRotation = new Vector3(0f, 0f, -90f);
    [SerializeField] private Vector3 material1Rotation = new Vector3(0f, 0f, -90f);

    private GameObject heldObject;
    private Rigidbody heldRigidbody;

    private Transform Anchor => holdAnchor != null ? holdAnchor : transform;

    public void PickUp(GameObject objectInstance)
    {
        if (objectInstance == null)
        {
            Debug.LogError("PlayerObjectHolder.PickUp: instancia nula");
            return;
        }

        if (heldObject != null && heldObject != objectInstance)
        {
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = false;
                heldRigidbody.useGravity = true;
                heldRigidbody = null;
            }
            heldObject.transform.SetParent(null, true);
        }

        heldObject = objectInstance;
        heldObject.transform.SetParent(Anchor, true);

        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
        }

        ApplyPickupPositioning(heldObject);
    }

    public void PickUpExistingInstance(GameObject objectInstance) => PickUp(objectInstance);
    public bool HasObjectInHand() => heldObject != null;
    public GameObject GetHeldObject() => heldObject;
    
    public void DropObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null, true);
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody = null;
        }
        heldObject = null;
    }
    
    public void UseHeldObject()
    {
        if (heldObject == null) return;
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody = null;
        }
        Destroy(heldObject);
        heldObject = null;
    }

    private void ApplyPickupPositioning(GameObject instance)
    {
        if (instance == null) return;

        bool esPaloIgnifugo = instance.GetComponent<PaloIgnifugo>() != null || instance.name.Contains("PaloIgnifugo");
        bool esPrefabMaterial1 = instance.GetComponent<MaterialTipo1>() != null || instance.name.Contains("PrefabMaterial1");

        Vector3 targetRotation = objectLocalRotation;

        if (esPaloIgnifugo)
        {
            targetRotation = paloIgnifugoRotation;
        }
        else if (esPrefabMaterial1)
        {
            targetRotation = material1Rotation;
        }

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(targetRotation);
    }

    public GameObject GetHeldObjectLegacy() => heldObject;
}
