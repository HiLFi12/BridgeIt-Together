using UnityEngine;

/// <summary>
/// PlayerObjectHolder minimal: al agarrar un objeto lo hace hijo del holder
/// y lo alinea en posición/rotación según su tipo (palos, materiales, etc.).
/// Sin lógica de drop, física ni colisiones.
/// </summary>
public class PlayerObjectHolder : MonoBehaviour
{
    [Header("Ancla para sostener (opcional)")]
    [SerializeField] private Transform holdAnchor;

    [Header("Rotación por defecto (genéricos)")]
    [SerializeField] private Vector3 objectLocalRotation = Vector3.zero; // Siempre posición 0,0,0

    [Header("Overrides por tipo de objeto")]
    [SerializeField] private Vector3 paloIgnifugoRotation = new Vector3(0f, 0f, -90f);
    [SerializeField] private Vector3 material1Rotation = new Vector3(0f, 0f, -90f);

    private GameObject heldObject;
    private Rigidbody heldRigidbody;

    private Transform Anchor => holdAnchor != null ? holdAnchor : transform;

    /// <summary>
    /// Agarra una instancia existente en escena, la vuelve hija del holder y la centra/rota.
    /// </summary>
    public void PickUp(GameObject objectInstance)
    {
        if (objectInstance == null)
        {
            Debug.LogError("PlayerObjectHolder.PickUp: instancia nula");
            return;
        }

        // Si ya había un objeto, simplemente lo desparentamos
        if (heldObject != null && heldObject != objectInstance)
        {
            heldObject.transform.SetParent(null, true);
        }

        heldObject = objectInstance;
        heldObject.transform.SetParent(Anchor, true);

        // Asegurar que siga al holder: hacer kinematic mientras está agarrado
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

    // Compat: API antigua
    public void PickUpExistingInstance(GameObject objectInstance) => PickUp(objectInstance);
    public bool HasObjectInHand() => heldObject != null;
    public GameObject GetHeldObject() => heldObject;
    public void DropObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null, true);
        // Restaurar física
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
        // No dejar kinematic perdido
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

    // Siempre centrado en el anchor
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

    public GameObject GetHeldObjectLegacy() => heldObject; // alias por si algún script antiguo usa un nombre alterno
}