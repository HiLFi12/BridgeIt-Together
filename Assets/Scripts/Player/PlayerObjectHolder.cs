using UnityEngine;

public class PlayerObjectHolder : MonoBehaviour
{
    // Eventos para notificar cambios del objeto en mano (útiles para UI y previews)
    public event System.Action<GameObject> OnPickedUp;
    public event System.Action<GameObject> OnDropped;
    public event System.Action<GameObject> OnUsed;
    [Header("Ancla para sostener (opcional)")]
    [SerializeField] private Transform holdAnchor;

    [Header("Transform por defecto (genéricos)")]
    [SerializeField] private Vector3 objectLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 objectLocalRotation = Vector3.zero;

    [System.Serializable]
    private class HolderOverride
    {
        [Tooltip("Prefab o referencia para detectar el tipo de objeto")]
        public GameObject referencePrefab;
        [Tooltip("Nombre contiene (fallback si referencePrefab es nulo)")]
        public string nameContains;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;
    }

    [Header("Overrides por tipo de objeto")]
    [SerializeField] private HolderOverride[] overrides;
    
    [Header("UI Manager")]
    [SerializeField] private PlayerUIManager playerUIManager;

    [Header("Audio - Pick Up (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al recoger un objeto. -1 desactiva.")]
    [SerializeField] private int pickUpSfxIndex = -1;

    [Header("Audio - Drop (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al soltar un objeto. -1 desactiva.")]
    [SerializeField] private int dropSfxIndex = -1;

    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    private int lastUIIndex = -1;

    private Transform Anchor => holdAnchor != null ? holdAnchor : transform;

    private void Update()
    {
        if (heldObject == null) return;
        // Actualizar la UI del objeto en mano en cada frame
        UpdateHeldObjectUI();
    }
    
    public void PickUp(GameObject objectInstance)
    {
        if (objectInstance == null)
        {
            Debug.LogError("PlayerObjectHolder.PickUp: instancia nula");
            return;
        }

        if (heldObject != null && heldObject != objectInstance)
        {
            DeactivateUIForObject(heldObject);
            
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = false;
                heldRigidbody.useGravity = true;
                heldRigidbody = null;
            }
            heldObject.transform.SetParent(null, true);
        }

        heldObject = objectInstance;
        // Mantener posición/rotación/escala en mundo al parentear para no deformar el objeto
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
        ActivateUIForObject(heldObject);
        // Set lastUIIndex
        IUIActivatable uiActivatable = heldObject.GetComponent<IUIActivatable>();
        if (uiActivatable != null)
        {
            lastUIIndex = uiActivatable.UIIndex;
        }

        // Reproducir SFX de pick-up (vía AudioManager, como Campfire)
        PlayPickUpSfx();

        // Notificar
        try { OnPickedUp?.Invoke(heldObject); } catch { }
    }

    public void PickUpExistingInstance(GameObject objectInstance) => PickUp(objectInstance);
    public bool HasObjectInHand() => heldObject != null;
    public GameObject GetHeldObject() => heldObject;
    
    public void DropObject()
    {
        if (heldObject == null) return;
        
        DeactivateUIForObject(heldObject);
        lastUIIndex = -1;
        var dropped = heldObject;
        heldObject.transform.SetParent(null, true);
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody = null;
        }
        // Reproducir SFX de drop (vía AudioManager)
        PlayDropSfx();
        heldObject = null;
        // Notificar
        try { OnDropped?.Invoke(dropped); } catch { }
    }
    
    public void UseHeldObject()
    {
        if (heldObject == null) return;
        
        DeactivateUIForObject(heldObject);
        var used = heldObject;
        
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody = null;
        }
        Destroy(heldObject);
        heldObject = null;
        // Notificar
        try { OnUsed?.Invoke(used); } catch { }
    }
    
    private void ActivateUIForObject(GameObject obj)
    {
        if (playerUIManager == null || obj == null) return;
        
        IUIActivatable uiActivatable = obj.GetComponent<IUIActivatable>();
        if (uiActivatable != null && uiActivatable.UIIndex >= 0)
        {
            playerUIManager.TurnOnUI(uiActivatable.UIIndex);
        }
    }
    
    private void DeactivateUIForObject(GameObject obj)
    {
        if (playerUIManager == null || obj == null) return;
        
        IUIActivatable uiActivatable = obj.GetComponent<IUIActivatable>();
        if (uiActivatable != null && uiActivatable.UIIndex >= 0)
        {
            playerUIManager.TurnOffUI(uiActivatable.UIIndex);
        }
    }

    private void ApplyPickupPositioning(GameObject instance)
    {
        if (instance == null) return;
        Vector3 targetPos = objectLocalPosition;
        Vector3 targetRot = objectLocalRotation;

        // Buscar override específico por referencia o por nombre
        if (overrides != null)
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var ov = overrides[i];
                if (ov == null) continue;

                bool match = false;

                if (ov.referencePrefab != null)
                {
                    // Comparar por nombre de prefab (instancias clonadas comparten nombre base)
                    if (instance.name.StartsWith(ov.referencePrefab.name))
                        match = true;
                }
                else if (!string.IsNullOrEmpty(ov.nameContains))
                {
                    if (instance.name.Contains(ov.nameContains))
                        match = true;
                }

                if (match)
                {
                    targetPos = ov.localPosition;
                    targetRot = ov.localRotation;
                    break;
                }
            }
        }

        instance.transform.localPosition = targetPos;
        instance.transform.localRotation = Quaternion.Euler(targetRot);
    }

    private void UpdateHeldObjectUI()
    {
        if (playerUIManager == null || heldObject == null) return;
        
        IUIActivatable uiActivatable = heldObject.GetComponent<IUIActivatable>();
        if (uiActivatable != null && uiActivatable.UIIndex >= 0)
        {
            int currentIndex = uiActivatable.UIIndex;
            if (currentIndex != lastUIIndex)
            {
                if (lastUIIndex != -1)
                    playerUIManager.TurnOffUI(lastUIIndex);
                playerUIManager.TurnOnUI(currentIndex);
                lastUIIndex = currentIndex;
            }
            playerUIManager.RefreshHeldObjectUI(currentIndex);
        }
    }

    public GameObject GetHeldObjectLegacy() => heldObject;

    private void PlayPickUpSfx()
    {
        if (pickUpSfxIndex < 0) return; // desactivado
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(pickUpSfxIndex);
        }
    }

    private void PlayDropSfx()
    {
        if (dropSfxIndex < 0) return; // desactivado
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(dropSfxIndex);
        }
    }
}