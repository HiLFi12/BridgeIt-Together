using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericObject1 : MonoBehaviour, IInteractable
{
    [Header("Configuración de objeto")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO;
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 1.0f;
    

    [SerializeField] private GameObject modeloVisual;
    
    private bool enRecarga = false;
    

    public InteractPriority InteractPriority => interactPriority;
    
    private void Start()
    {

        if (materialPrefabsSO == null)
        {
            Debug.LogError("No se ha asignado el MaterialPrefabSO en " + gameObject.name);
        }
        
        ConfigurarModeloVisual();
    }
    
    private void ConfigurarModeloVisual()
    {
        //Si quieren cambiar el modelo in-game, se hace acá
    }
    
    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Objeto en recarga, espera un momento.");
            return;
        }
        
        if (materialPrefabsSO == null)
        {
            Debug.LogError("No hay MaterialPrefabSO asignado. No se puede generar material.");
            return;
        }
        
        GameObject materialPrefab = materialPrefabsSO.GetMaterialPrefab(1, era);
        
        if (materialPrefab == null)
        {
            Debug.LogError($"No se encontró prefab para material tipo 1 de la era {era}.");
            return;
        }
        
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder != null)
        {
            if (playerObjectHolder.HasObjectInHand())
            {
                Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
            }
            
            // Generar una nueva instancia del PrefabMaterial1 en la escena
            GameObject nuevoMaterial = Instantiate(materialPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            
            // Configurar el material generado
            ConfigurarMaterialGenerado(nuevoMaterial);
            
            // Hacer que el jugador recoja la instancia recién creada
            playerObjectHolder.PickUpExistingInstance(nuevoMaterial);
            
            StartCoroutine(Recargar());
            
            ProducirEfectos();
        }
        else
        {
            Debug.Log("El interactor no tiene el componente PlayerObjectHolder.");
        }
    }
    
    private void ConfigurarMaterialGenerado(GameObject material)
    {
        // Configurar el material generado como material de tipo 1 de la era correspondiente
        BridgeMaterialInfo materialInfo = material.GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = material.AddComponent<BridgeMaterialInfo>();
        }
        
        materialInfo.layerIndex = 0; // Tipo 1 corresponde a capa 0
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Wood; // Tipo 1 es madera
        
        material.tag = "BridgeLayer0";
        
        Debug.Log($"Material generado configurado: Era {era}, Capa 0, Tipo Wood");
    }
    
    private IEnumerator Recargar()
    {
        enRecarga = true;
        
        yield return new WaitForSeconds(tiempoRecarga);
        
        enRecarga = false;
    }
    private void ProducirEfectos()
    {
        //Aca pueden meter efectos
    }
} 