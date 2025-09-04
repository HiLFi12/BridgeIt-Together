using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectHolder : MonoBehaviour
{
    private GameObject heldObject;

    [Header("Ancla para sostener (opcional)")]
    [SerializeField] private Transform holdAnchor;

    [SerializeField] private Vector3 objectLocalPosition = new Vector3(0, 0, 0.3f);
    [SerializeField] private Vector3 objectLocalRotation = Vector3.zero;

    [Header("Configuraciones Específicas de Objetos")]
    [SerializeField] private Vector3 paloIgnifugoPosition = new Vector3(0.077f, 0.149f, 0.543f);
    [SerializeField] private Vector3 paloIgnifugoRotation = new Vector3(0f, 0f, -90f);
    [SerializeField] private Vector3 material1Position = new Vector3(-1.054f, 0.149f, 0.543f);
    [SerializeField] private Vector3 material1Rotation = new Vector3(0f, 0f, -90f);

    [Header("Drop seguro")]
    [SerializeField] private float minDropForward = 1.2f;
    [SerializeField] private float dropUpOffset = 0.1f;

    [Header("Asentamiento y fijación")]
    [SerializeField] private bool freezeAfterDrop = true;
    [SerializeField] private float settleSpeedThreshold = 0.05f;
    [SerializeField] private float settleStableTime = 0.3f;
    [SerializeField] private float settleMaxTime = 2f;

    // Ya no se desactivan colliders - solo se guarda referencia al rigidbody
    private Rigidbody heldRigidbody;
    private Collider[] playerColliders;

    private void Awake()
    {
        playerColliders = GetComponentsInChildren<Collider>(true);
    }

    private Transform Anchor => holdAnchor != null ? holdAnchor : transform;

    public void PickUpObject(GameObject objectPrefab)
    {
        if (objectPrefab == null)
        {
            Debug.LogError("Se intentó recoger un prefab nulo.");
            return;
        }

        if (!objectPrefab.scene.IsValid())
        {
            Debug.LogError("PickUpObject requiere una instancia en escena. Usa PickUpExistingInstance con el objeto de la escena.");
            return;
        }

        if (heldObject != null)
        {
            RestorePhysicsForHeld();
            heldObject.transform.SetParent(null, true);
            heldObject = null;
        }

        heldObject = objectPrefab;
        heldObject.transform.SetParent(Anchor, true);
        ApplyPickupPositioning(objectPrefab, heldObject);
        DisablePhysicsForHeld(heldObject);
    }

    public void PickUpExistingInstance(GameObject objectInstance)
    {
        if (objectInstance == null)
        {
            Debug.LogError("Se intentó recoger una instancia nula.");
            return;
        }

        if (heldObject != null)
        {
            RestorePhysicsForHeld();
            heldObject.transform.SetParent(null, true);
            heldObject = null;
        }

        heldObject = objectInstance;
        heldObject.transform.SetParent(Anchor, true);
        ApplyPickupPositioning(objectInstance, heldObject);
        DisablePhysicsForHeld(heldObject);
    }

    public bool HasObjectInHand()
    {
        return heldObject != null;
    }

    public void DropObject()
    {
        if (heldObject == null)
        {
            return;
        }

        var obj = heldObject;
        var objColliders = obj.GetComponentsInChildren<Collider>(true);
        Bounds b = CalculateBounds(objColliders, obj.transform.position);
        float approxRadius = b.extents.magnitude;
        float playerRadius = GetApproxPlayerRadius();
        float forwardDist = Mathf.Max(minDropForward, playerRadius + approxRadius + 0.2f);

        Vector3 dropPos = transform.position + transform.forward * forwardDist + Vector3.up * 2f;
        if (Physics.Raycast(dropPos, Vector3.down, out var hit, 5f, ~0, QueryTriggerInteraction.Ignore))
        {
            dropPos = hit.point + Vector3.up * (b.extents.y + dropUpOffset);
        }
        else
        {
            dropPos += Vector3.down * 1.5f;
        }

        obj.transform.position = dropPos;
        obj.transform.SetParent(null, true);

        SetIgnoreCollisions(objColliders, playerColliders, true);
        RestorePhysicsForHeld();

        int walkableLayer = LayerMask.NameToLayer("WalkableLayer");
        var handler = obj.GetComponent<DroppedObjectCollisionHandler>();
        if (handler == null) handler = obj.AddComponent<DroppedObjectCollisionHandler>();
        handler.Setup(
            objColliders,
            playerColliders,
            walkableLayer,
            freezeAfterDrop,
            settleSpeedThreshold,
            settleStableTime,
            settleMaxTime
        );

        heldObject = null;
    }

    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    public void UseHeldObject()
    {
        if (heldObject != null)
        {
            Destroy(heldObject);
            heldObject = null;
            heldRigidbody = null;
        }
    }

    private void ApplyPickupPositioning(GameObject source, GameObject instance)
    {
        if (instance == null) return;

        bool esPaloIgnifugo = (source != null && (source.GetComponent<PaloIgnifugo>() != null || source.name.Contains("PaloIgnifugo")))
                               || instance.GetComponent<PaloIgnifugo>() != null;
        bool esPrefabMaterial1 = (source != null && (source.GetComponent<MaterialTipo1>() != null || source.name.Contains("PrefabMaterial1")))
                                  || instance.GetComponent<MaterialTipo1>() != null;

        Vector3 targetPosition = objectLocalPosition;
        Vector3 targetRotation = objectLocalRotation;

        if (esPaloIgnifugo)
        {
            targetPosition = paloIgnifugoPosition;
            targetRotation = paloIgnifugoRotation;
        }
        else if (esPrefabMaterial1)
        {
            targetPosition = material1Position;
            targetRotation = material1Rotation;
        }

        instance.transform.localPosition = targetPosition;
        instance.transform.localEulerAngles = targetRotation;
    }

    private void DisablePhysicsForHeld(GameObject obj)
    {
        // Solo desactivar el rigidbody, mantener colliders activos
        heldRigidbody = obj.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
        }

        obj.SetActive(true);
    }

    private void RestorePhysicsForHeld()
    {
        // Solo restaurar el rigidbody
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody = null;
        }
    }

    private IEnumerator SettleThenFreeze(Rigidbody rb)
    {
        float stableFor = 0f;
        float elapsed = 0f;
        float v2 = settleSpeedThreshold * settleSpeedThreshold;
        while (elapsed < settleMaxTime)
        {
            bool lowLin = rb.linearVelocity.sqrMagnitude <= v2;
            bool lowAng = rb.angularVelocity.sqrMagnitude <= (v2 * 10f);
            if (lowLin && lowAng)
            {
                stableFor += Time.deltaTime;
                if (stableFor >= settleStableTime) break;
            }
            else
            {
                stableFor = 0f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private float GetApproxPlayerRadius()
    {
        float r = 0.5f;
        var cc = GetComponent<CharacterController>();
        if (cc) r = Mathf.Max(r, cc.radius);
        var cap = GetComponent<CapsuleCollider>();
        if (cap) r = Mathf.Max(r, cap.radius);
        var sphere = GetComponent<SphereCollider>();
        if (sphere) r = Mathf.Max(r, sphere.radius);
        return r;
    }

    private Bounds CalculateBounds(Collider[] cols, Vector3 fallbackCenter)
    {
        if (cols != null && cols.Length > 0)
        {
            Bounds b = new Bounds(cols[0].bounds.center, Vector3.zero);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null) b.Encapsulate(cols[i].bounds);
            }
            return b;
        }
        return new Bounds(fallbackCenter, Vector3.one * 0.5f);
    }

    private void SetIgnoreCollisions(Collider[] a, Collider[] b, bool ignore)
    {
        if (a == null || b == null) return;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] == null) continue;
            for (int j = 0; j < b.Length; j++)
            {
                if (b[j] == null) continue;
                Physics.IgnoreCollision(a[i], b[j], ignore);
            }
        }
    }
}