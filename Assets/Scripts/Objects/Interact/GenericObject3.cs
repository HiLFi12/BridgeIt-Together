using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objeto genérico tipo 3 que permite al jugador obtener un material tipo 4
/// al mantener presionado el botón de interacción durante 1 segundo.
/// En la era prehistórica, este material es un adoquín.
/// </summary>
public class GenericObject3 : MonoBehaviour, IInteractable, IHoldInteractable
{    
    [Header("Configuración de objeto")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; // Referencia al SO con los prefabs
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    
    [Header("Configuración de generación")]
    [SerializeField] private float tiempoPresionado = 1.0f; // Tiempo que hay que mantener presionado
    [SerializeField] private float tiempoRecarga = 2.0f; // Tiempo antes de poder generar otro material
    
    // Variables para controlar el estado
    private bool enRecarga = false;
    private bool enProceso = false;
    private float tiempoActual = 0f;
    private GameObject jugadorInteractuando = null;
    
    // Propiedad requerida por la interfaz IInteractable
    public InteractPriority InteractPriority => interactPriority;    
    
    private void Start()
    {
        // Verificar que tengamos las referencias necesarias
        if (materialPrefabsSO == null)
        {
            Debug.LogError("No se ha asignado el MaterialPrefabSO en " + gameObject.name);
        }
    }
    
    private void Update()
    {
        // Si estamos en proceso de generar el material
        if (enProceso && jugadorInteractuando != null)
        {
            tiempoActual += Time.deltaTime;
            
            // Si se completa el tiempo requerido
            if (tiempoActual >= tiempoPresionado)
            {
                EntregarMaterial();
                ResetearProceso();
                StartCoroutine(Recargar());
            }
            
            // Aquí podrías añadir efectos visuales o sonidos que indiquen el progreso
            // Por ejemplo, una barra de progreso o un efecto de partículas que aumenta
        }
    }
    
    // Método llamado cuando un jugador interactúa con este objeto
    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Objeto en recarga, espera un momento.");
            return;
        }
        
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder != null)
        {
            // Iniciamos el proceso de mantener presionado
            if (!enProceso)
            {
                IniciarProceso(interactor);
                Debug.Log("Mantén presionado para obtener el material tipo 4.");
            }
        }
        else
        {
            Debug.Log("El interactor no tiene el componente PlayerObjectHolder.");
        }
    }
    
    // Método llamado cuando el jugador suelta el botón de interacción
    public void DetenerInteraccion()
    {
        if (enProceso)
        {
            ResetearProceso();
            Debug.Log("Proceso cancelado. Debes mantener presionado el botón.");
        }
    }
    
    private void IniciarProceso(GameObject interactor)
    {
        enProceso = true;
        tiempoActual = 0f;
        jugadorInteractuando = interactor;
        
        // Aquí podrías iniciar efectos visuales o sonidos
    }
    
    private void ResetearProceso()
    {
        enProceso = false;
        tiempoActual = 0f;
        jugadorInteractuando = null;
        
        // Detener efectos visuales o sonidos
    }    
    
    private void EntregarMaterial()
    {
        if (materialPrefabsSO == null || jugadorInteractuando == null)
        {
            Debug.LogError("Falta MaterialPrefabSO o jugador para entregar material.");
            return;
        }
        
        // Obtener el prefab del material tipo 4 para la era actual
        GameObject materialPrefab = materialPrefabsSO.GetMaterialPrefab(4, era);
        
        if (materialPrefab == null)
        {
            Debug.LogError($"No se encontró prefab para material tipo 4 de la era {era}.");
            return;
        }

        PlayerObjectHolder playerObjectHolder = jugadorInteractuando.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder != null)
        {
            if (playerObjectHolder.HasObjectInHand())
            {
                Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
            }
            
            // Generar una nueva instancia del PrefabMaterial4 en la escena
            GameObject nuevoMaterial = Instantiate(materialPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            
            // Configurar el material generado
            ConfigurarMaterialGenerado(nuevoMaterial);
            
            // Hacer que el jugador recoja la instancia recién creada
            playerObjectHolder.PickUpExistingInstance(nuevoMaterial);
            Debug.Log("Material tipo 4 entregado con éxito.");
            
            // Opcional: Reproducir sonido o efecto visual
            ProducirEfectos();
        }
    }
    
    private void ConfigurarMaterialGenerado(GameObject material)
    {
        // Configurar el material generado como material de tipo 4 de la era correspondiente
        BridgeMaterialInfo materialInfo = material.GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = material.AddComponent<BridgeMaterialInfo>();
        }
        
        materialInfo.layerIndex = 3; // Tipo 4 corresponde a capa 3 (superficie)
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Stone; // Tipo 4 es piedra/adoquín
        
        material.tag = "BridgeLayer3";
        
        Debug.Log($"Material tipo 4 generado configurado: Era {era}, Capa 3, Tipo Stone");
    }
    
    private IEnumerator Recargar()
    {
        enRecarga = true;
        
        // TODO: Aquí puedes añadir una animación o cambio visual que indique la recarga
        
        yield return new WaitForSeconds(tiempoRecarga);
        
        enRecarga = false;
    }
    
    private void ProducirEfectos()
    {
        // TODO: Reproducir efectos visuales o sonidos
    }
}