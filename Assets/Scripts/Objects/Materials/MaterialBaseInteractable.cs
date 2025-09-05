using UnityEngine;
using System;

using BridgeItTogether.Gameplay.Abstractions;

public abstract class MaterialBaseInteractable : MonoBehaviour, IGrababble, IHitable
{
    [Header("Configuración General")]
    [SerializeField] protected BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    [SerializeField] protected bool puedeConstruirse = true; 
    [SerializeField] protected InteractPriority prioridad = InteractPriority.Medium;

    [Header("Estado de Disponibilidad (Ready Gate)")]
    [Tooltip("Si está activo, la construcción sólo será posible cuando isReady sea true.")]
    [SerializeField] protected bool useReadyState = false;
    [Tooltip("Estado actual de disponibilidad. Se ignora si useReadyState es false.")]
    [SerializeField] protected bool isReady = true;

    protected BridgeMaterialInfo materialInfo;

    private Vector3 objectPosition;
    private Quaternion objectRotation;
    public InteractPriority InteractPriority => prioridad;
    public virtual bool PuedeConstruirse => puedeConstruirse && (!useReadyState || isReady);
    public bool IsReady => isReady;

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
        // Antes: if (!puedeConstruirse)
        if (!PuedeConstruirse)
        {
            Debug.Log($"[MaterialBaseInteractable] Material {name} no puede construirse aún (Era: {era}).");
            return;
        }
        OnInteract(interactor);
        OnInteracted?.Invoke(interactor);
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

    /// <summary>
    /// Cambia el estado de readiness cuando el gating está activo.
    /// </summary>
    /// <param name="valor">Nuevo estado</param>
    protected void SetReady(bool valor)
    {
        if (!useReadyState) return; // No hace nada si no está usando gating
        isReady = valor;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}
