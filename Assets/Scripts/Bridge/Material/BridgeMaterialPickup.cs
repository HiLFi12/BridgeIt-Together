using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeMaterialPickup : MonoBehaviour, IInteractable
{
    public InteractPriority InteractPriority => InteractPriority.High;

    [Header("Configuración de Material")]
    public GameObject materialPrefab;
    public int layerIndex = 0;
    public BridgeQuadrantSO.EraType era;

    [Header("Configuración de Interacción")]
    [SerializeField] private float respawnTime = 5f;
    private bool isRespawning = false;

    // Referencia al modelo visual del objeto
    private GameObject visualModel;
    private Collider objectCollider;

    private void Start()
    {
        objectCollider = GetComponent<Collider>();
        
        // Guardar referencia al primer hijo como modelo visual
        if (transform.childCount > 0)
        {
            visualModel = transform.GetChild(0).gameObject;
        }
        
        // CRÍTICO: Si tenemos un materialPrefab, configurarlo con el layerIndex correcto
        if (materialPrefab != null)
        {
            ConfigureMaterialPrefab();
        }
    }
    
    // Método para configurar correctamente el prefab del material
    private void ConfigureMaterialPrefab()
    {
        // Asignar un tag específico según el layerIndex para poder identificarlo
        switch (layerIndex)
        {
            case 0:
                materialPrefab.tag = "BridgeLayer0"; // Base
                break;
            case 1:
                materialPrefab.tag = "BridgeLayer1"; // Soporte
                break;
            case 2:
                materialPrefab.tag = "BridgeLayer2"; // Estructura
                break;
            case 3:
                materialPrefab.tag = "BridgeLayer3"; // Superficie
                break;
            default:
                materialPrefab.tag = "BridgeLayer0"; // Por defecto, asignamos Base
                break;
        }
        
        // Guardar el layerIndex en el prefab
        BridgeMaterialInfo materialInfo = materialPrefab.GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = materialPrefab.AddComponent<BridgeMaterialInfo>();
        }
        
        materialInfo.layerIndex = layerIndex;
        materialInfo.era = era;
        
        Debug.Log($"Material configurado: {materialPrefab.name}, LayerIndex: {layerIndex}, Era: {era}, Tag: {materialPrefab.tag}");
    }

    public void Interact(GameObject interactor)
    {
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();

        if (playerObjectHolder != null)
        {
            if (playerObjectHolder.HasObjectInHand())
            {
                Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
            }
            
            // Configurar este objeto como material de puente
            ConfigureAsBridgeMaterial();
            
            // Usar PickUpExistingInstance para recoger este objeto directamente
            playerObjectHolder.PickUpExistingInstance(gameObject);
        }
        else
        {
            Debug.Log("El interactor no tiene el componente PlayerObjectHolder.");
        }
    }
    
    // Método para configurar este objeto como material de puente
    private void ConfigureAsBridgeMaterial()
    {
        // Asignar un tag específico según el layerIndex
        switch (layerIndex)
        {
            case 0:
                gameObject.tag = "BridgeLayer0"; // Base
                break;
            case 1:
                gameObject.tag = "BridgeLayer1"; // Soporte
                break;
            case 2:
                gameObject.tag = "BridgeLayer2"; // Estructura
                break;
            case 3:
                gameObject.tag = "BridgeLayer3"; // Superficie
                break;
            default:
                gameObject.tag = "BridgeLayer0"; // Por defecto, asignamos Base
                break;
        }
        
        // Guardar el layerIndex en este objeto
        BridgeMaterialInfo materialInfo = GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = gameObject.AddComponent<BridgeMaterialInfo>();
        }
        
        materialInfo.layerIndex = layerIndex;
        materialInfo.era = era;
        
        Debug.Log($"Material configurado: {gameObject.name}, LayerIndex: {layerIndex}, Era: {era}, Tag: {gameObject.tag}");
    }
    
    private void DisablePickup()
    {
        // Desactivar el modelo visual y el collider
        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        
        isRespawning = true;
    }
    
    private void EnablePickup()
    {
        // Reactivar el modelo visual y el collider
        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
        
        isRespawning = false;
    }
    
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);
        EnablePickup();
    }

    // Para ayudar a visualizar en el editor
    private void OnDrawGizmos()
    {
        // Si tiene un material prefab asignado, indicar de qué tipo es
        if (materialPrefab != null)
        {
            Color gizmoColor = Color.white;
            
            // Colores diferentes según el índice de capa
            switch (layerIndex)
            {
                case 0: gizmoColor = Color.blue; break;   // Base
                case 1: gizmoColor = Color.green; break;  // Soporte
                case 2: gizmoColor = Color.yellow; break; // Estructura
                case 3: gizmoColor = Color.red; break;    // Superficie
                default: gizmoColor = Color.white; break;
            }
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Mostrar a qué era pertenece
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, 
                                        $"Era: {era} - Capa: {layerIndex}");
            }
#endif
        }
    }
} 