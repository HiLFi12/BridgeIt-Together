using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Corteza resistente, objeto único de la era prehistórica.
/// Produce resina (material tipo 2) cuando se interactúa con ella usando un palo ignífugo encendido.
/// </summary>
public class CortezaResistente : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; // Referencia al SO con los prefabs
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 3.0f; // Tiempo antes de poder generar otra resina
    [SerializeField] private Transform puntoSpawn; // Punto donde aparecerá la resina
    
    private bool enRecarga = false;
    
    // Propiedad requerida por la interfaz IInteractable
    public InteractPriority InteractPriority => interactPriority;
    
    private void Start()
    {
        // Verificar que tengamos las referencias necesarias
        if (materialPrefabsSO == null)
        {
            Debug.LogError("No se ha asignado el MaterialPrefabSO en " + gameObject.name);
        }
        
        // Si no se asignó un punto de spawn, usar la posición del objeto
        if (puntoSpawn == null)
        {
            puntoSpawn = transform;
        }
    }
    
    // Método llamado cuando un jugador interactúa con este objeto
    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Corteza en recarga, espera un momento.");
            return;
        }
        
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder == null || !playerObjectHolder.HasObjectInHand())
        {
            Debug.Log("Necesitas un palo ignífugo encendido para extraer resina de la corteza.");
            return;
        }
        
        // Verificar si el jugador tiene un palo ignífugo encendido
        GameObject objetoSostenido = playerObjectHolder.GetHeldObject();
        PaloIgnifugo paloIgnifugo = objetoSostenido?.GetComponent<PaloIgnifugo>();
        
        if (paloIgnifugo != null && paloIgnifugo.EstaEncendido())
        {
            // Consumir el palo ignífugo
            playerObjectHolder.UseHeldObject();
            
            // Generar resina (material tipo 2)
            GenerarResina();
            
            // Iniciar recarga
            StartCoroutine(Recargar());
            
            Debug.Log("Has extraído resina de la corteza.");
        }
        else
        {
            Debug.Log("Necesitas un palo ignífugo encendido para extraer resina de la corteza.");
        }
    }
    
    // Genera el material tipo 2 (resina)
    private void GenerarResina()
    {
        if (materialPrefabsSO == null) return;
        
        // Obtener el prefab de la resina (material tipo 2) para la era prehistórica
        GameObject resinaPrefab = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Prehistoric);
        
        if (resinaPrefab != null)
        {
            // Calcular posición para la resina
            Vector3 posicionResina = puntoSpawn.position + Vector3.down * 0.5f;
            
            // Instanciar la resina
            GameObject resina = Instantiate(resinaPrefab, posicionResina, Quaternion.identity);
            
            // Opcional: Añadir efectos visuales o sonidos
            ProducirEfectos();
        }
        else
        {
            Debug.LogError("No se encontró prefab para material tipo 2 (resina) de la era prehistórica.");
        }
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
        // Por ejemplo, partículas de resina cayendo, sonido de goteo, etc.
    }
} 