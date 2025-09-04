using UnityEngine;
using System;

using BridgeItTogether.Gameplay.Abstractions;

public abstract class MaterialBaseInteractable : MonoBehaviour, IGrababble, IHitable
{
    [Header("Configuración General")]
    [SerializeField] protected BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    [SerializeField] protected bool puedeConstruirse = true; 
    [SerializeField] protected InteractPriority prioridad = InteractPriority.Medium;

    protected BridgeMaterialInfo materialInfo;

    private Vector3 objectPosition;
    private Quaternion objectRotation;
    public InteractPriority InteractPriority => prioridad;

    protected virtual int LayerIndex => 0; 

    protected virtual void Awake()
    {
        EnsureBridgeMaterialInfo();
        PostEnsure();
    }

    protected virtual void PostEnsure() { }

    private void EnsureBridgeMaterialInfo()
    {
        materialInfo = GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = gameObject.AddComponent<BridgeMaterialInfo>();
        }
        materialInfo.layerIndex = LayerIndex;
        materialInfo.era = era;
        UpdateTagByLayer(LayerIndex);
    }

    private void UpdateTagByLayer(int index)
    {
        switch (index)
        {
            case 0: gameObject.tag = "BridgeLayer0"; break;
            case 1: gameObject.tag = "BridgeLayer1"; break;
            case 2: gameObject.tag = "BridgeLayer2"; break;
            case 3: gameObject.tag = "BridgeLayer3"; break;
            default: gameObject.tag = "BridgeLayer0"; break;
        }
    }

    public event Action<GameObject> OnInteracted; // Suscripción de mecánicas externas

    public void Interact(GameObject interactor)
    {
        if (!puedeConstruirse)
        {
            Debug.Log($"[MaterialBaseInteractable] Material {name} no puede construirse aún (Era: {era}).");
            return;
        }
        OnInteract(interactor);
        OnInteracted?.Invoke(interactor); // Notificar a scripts anexos
    }

    public void GrabPosition(Vector3 position)
    {
        objectPosition = transform.position;
    }

    public void GrabRotation(Quaternion rotation)
    {
        objectRotation = transform.localRotation;
    }

    protected virtual void OnInteract(GameObject interactor) { }

    public void SetPuedeConstruirse(bool valor) => puedeConstruirse = valor;

    public void OnLaunched(Vector3 targetPosition)
    {
    }

    public bool PuedeConstruirse => puedeConstruirse;
}
