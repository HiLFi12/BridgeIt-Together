using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generador de palos ignífugos, objeto único de la era prehistórica.
/// Permite al jugador obtener un palo ignífugo al interactuar con él.
/// </summary>
public class PaloIgnifugoGenerator : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private GameObject paloIgnifugoPrefab; // Prefab del palo ignífugo
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 0.5f; // Tiempo antes de poder generar otro palo
    
    private bool enRecarga = false;
    
    // Propiedad requerida por la interfaz IInteractable
    public InteractPriority InteractPriority => interactPriority;
    
    // Método llamado cuando un jugador interactúa con este objeto
    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Generador en recarga, espera un momento.");
            return;
        }
        
        if (paloIgnifugoPrefab == null)
        {
            Debug.LogError("No hay prefab de palo ignífugo asignado.");
            return;
        }
        
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder != null)
        {
            if (playerObjectHolder.HasObjectInHand())
            {
                Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
            }
            
            // Generar una nueva instancia del palo ignífugo en la escena
            GameObject nuevoPoloIgnifugo = Instantiate(paloIgnifugoPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            
            // Hacer que el jugador recoja la instancia recién creada
            playerObjectHolder.PickUpExistingInstance(nuevoPoloIgnifugo);
            
            // Iniciamos la recarga
            StartCoroutine(Recargar());
            
            Debug.Log("Has obtenido un palo ignífugo.");
        }
        else
        {
            Debug.Log("El interactor no tiene el componente PlayerObjectHolder.");
        }
    }
    
    private IEnumerator Recargar()
    {
        enRecarga = true;
        
        // Aquí podrías añadir algún efecto visual o sonido de recarga
        
        yield return new WaitForSeconds(tiempoRecarga);
        
        enRecarga = false;
    }
} 