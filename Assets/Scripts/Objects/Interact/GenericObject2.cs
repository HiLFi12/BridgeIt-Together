using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericObject2 : MonoBehaviour, IInteractable, IHoldInteractable
{
    [Header("Configuración de objeto")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; 
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    
    [Header("Configuración de mezcladora")]
    [SerializeField] private float tiempoCoccion = 1.5f; 
    [SerializeField] private Transform slotVisualTipo1;
    [SerializeField] private Transform slotVisualTipo2; 
    
    private bool slotTipo1Ocupado = false;
    private bool slotTipo2Ocupado = false;
    private bool enProcesoCoccion = false;

    [Header("Interacción manual (Medieval)")]
    [SerializeField] private float tiempoMantenerE = 1.5f; // segundos que hay que mantener presionado
    private bool esperandoHold = false;
    private float tiempoHoldActual = 0f;
    private GameObject jugadorInteractuando = null;


    public InteractPriority InteractPriority => interactPriority;
    
    // Método para obtener la era del objeto
    public BridgeQuadrantSO.EraType GetEra()
    {
        return era;
    }
    
    private void Start()
    {

        if (materialPrefabsSO == null)
        {
            Debug.LogError("No se ha asignado el MaterialPrefabSO en " + gameObject.name);
        }
        

        ActualizarVisualesSlots();
    }

    private void Update()
    {
        // Progreso de interacción mantenida solo aplica a la era medieval
        if (era == BridgeQuadrantSO.EraType.Medieval && esperandoHold && jugadorInteractuando != null)
        {
            // Solo avanzar si ambos materiales siguen cargados y no hay cocción en curso
            if (slotTipo1Ocupado && slotTipo2Ocupado && !enProcesoCoccion)
            {
                tiempoHoldActual += Time.deltaTime;
                if (tiempoHoldActual >= tiempoMantenerE)
                {
                    // Iniciar el proceso de creación y resetear el hold
                    StartCoroutine(ProcesarMaterial());
                    ResetearHold();
                }
            }
            else
            {
                // Si ya no están disponibles las condiciones, cancelar el hold
                ResetearHold();
            }
        }
    }
    

    public void Interact(GameObject interactor)
    {
        if (enProcesoCoccion)
        {
            Debug.Log("Mezcladora en proceso de cocción, espera a que termine.");
            return;
        }
        
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder == null || !playerObjectHolder.HasObjectInHand())
        {
            if (slotTipo1Ocupado && slotTipo2Ocupado)
            {
                if (era == BridgeQuadrantSO.EraType.Prehistoric && !enProcesoCoccion)
                {
                    StartCoroutine(ProcesarMaterial());
                }
                else if (era == BridgeQuadrantSO.EraType.Medieval)
                {
                    // Iniciar interacción mantenida
                    if (!esperandoHold)
                    {
                        esperandoHold = true;
                        tiempoHoldActual = 0f;
                        jugadorInteractuando = interactor;
                        Debug.Log("Mantén presionado el botón para activar la cocción (era medieval).");
                    }
                }
            }
            else
            {
                Debug.Log("No tienes un material para colocar en la mezcladora o ésta no está lista para entregar.");
            }
            return;
        }
        
        GameObject heldObject = playerObjectHolder.GetHeldObject();
        int materialTipo = DeterminarTipoMaterial(heldObject);
        
        switch (materialTipo)
        {
            case 1:
                if (!slotTipo1Ocupado)
                {
                    slotTipo1Ocupado = true;
                    playerObjectHolder.UseHeldObject(); 
                    ActualizarVisualesSlots();
                    VerificarCoccionAutomatica();
                    Debug.Log("Material tipo 1 colocado en la mezcladora.");
                }
                else
                {
                    Debug.Log("Ya hay un material tipo 1 en la mezcladora.");
                }
                break;
                
            case 2:
                if (!slotTipo2Ocupado)
                {
                    slotTipo2Ocupado = true;
                    playerObjectHolder.UseHeldObject(); 
                    ActualizarVisualesSlots();
                    VerificarCoccionAutomatica();
                    Debug.Log("Material tipo 2 colocado en la mezcladora.");
                }
                else
                {
                    Debug.Log("Ya hay un material tipo 2 en la mezcladora.");
                }
                break;
                
            default:
                Debug.Log("Este material no se puede usar en la mezcladora.");
                break;
        }
    }

    // Cancelación al soltar la tecla de interacción
    public void DetenerInteraccion()
    {
        if (esperandoHold)
        {
            ResetearHold();
            Debug.Log("Interacción cancelada. Debes mantener presionado para iniciar la cocción.");
        }
    }
    

    private int DeterminarTipoMaterial(GameObject objeto)
    {
        if (objeto == null) return 0;
        

        if (objeto.GetComponent<MaterialTipo1>() != null)
            return 1;
            

        BridgeMaterialInfo materialInfo = objeto.GetComponent<BridgeMaterialInfo>();
        if (materialInfo != null)
        {

            if (materialInfo.layerIndex == 0) return 1;
            if (materialInfo.layerIndex == 1) return 2;
        }
        
        return 0; 
    }
    

    private void ActualizarVisualesSlots()
    {
        if (slotVisualTipo1 != null)
            slotVisualTipo1.gameObject.SetActive(slotTipo1Ocupado);
            
        if (slotVisualTipo2 != null)
            slotVisualTipo2.gameObject.SetActive(slotTipo2Ocupado);
    }
    

    private void VerificarCoccionAutomatica()
    {
        if (era == BridgeQuadrantSO.EraType.Prehistoric && slotTipo1Ocupado && slotTipo2Ocupado && !enProcesoCoccion)
        {
            StartCoroutine(ProcesarMaterial());
        }
    }
    
    private IEnumerator ProcesarMaterial()
    {
        enProcesoCoccion = true;
        
        Debug.Log("Comenzando proceso de cocción...");
        
        yield return new WaitForSeconds(tiempoCoccion);
        
        GameObject materialTipo3Prefab = materialPrefabsSO.GetMaterialPrefab(3, era);
        if (materialTipo3Prefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 1.0f;
            Instantiate(materialTipo3Prefab, spawnPosition, Quaternion.identity);
            Debug.Log("Material tipo 3 creado con éxito.");
        }
        else
        {
            Debug.LogError($"No se encontró prefab para material tipo 3 de la era {era}");
        }
        
        slotTipo1Ocupado = false;
        slotTipo2Ocupado = false;
        enProcesoCoccion = false;
        ActualizarVisualesSlots();
    }
    
    public void IniciarCoccionManual()
    {
        if (era == BridgeQuadrantSO.EraType.Medieval && slotTipo1Ocupado && slotTipo2Ocupado && !enProcesoCoccion)
        {
            StartCoroutine(ProcesarMaterial());
        }
    }

    private void ResetearHold()
    {
        esperandoHold = false;
        tiempoHoldActual = 0f;
        jugadorInteractuando = null;
    }
}