using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Corteza resistente, objeto único de la era prehistórica.
/// Produce resina (material tipo2) cuando se interactúa con ella usando un palo ignífugo encendido.
/// </summary>
public class CortezaResistente : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; // Referencia al SO con los prefabs
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 3.0f; // Tiempo antes de poder generar otra resina
    [SerializeField] private Transform puntoSpawn; // Punto donde aparecerá la resina
    [SerializeField] private GameObject shadow;
    
    [Header("Audio - Resina (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al spawnear la resina. -1 desactiva.")]
    [SerializeField] private int resinSpawnSfxIndex = -1;
    
    private bool enRecarga = false;
    
    // Event fired when resin is successfully extracted from this bark
    public static event Action<GameObject> OnResinExtracted;

    // Propiedad requerida por la interfaz IInteractable
    public InteractPriority InteractPriority => interactPriority;
    
    private void Start()
    {
        shadow.SetActive(false);
        
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
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
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
            
            // Generar resina (material tipo2) directamente en la mano del jugador
            GenerarResina(playerObjectHolder);
            
            // Iniciar recarga
            StartCoroutine(Recargar());
            
            Debug.Log("Has extraído resina de la corteza.");

            // Notify listeners that resin was extracted by this interactor
            OnResinExtracted?.Invoke(interactor);
        }
        else
        {
            Debug.Log("Necesitas un palo ignífugo encendido para extraer resina de la corteza.");
        }
    }
    
    // Genera el material tipo2 (resina)
    private void GenerarResina(PlayerObjectHolder holder)
    {
        if (materialPrefabsSO == null || holder == null) return;
        if (holder.HasObjectInHand()) return; // por seguridad, debería estar libre tras consumir el palo
        
        // Obtener el prefab de la resina (material tipo2) para la era prehistórica
        GameObject resinaPrefab = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Prehistoric);
        
        if (resinaPrefab != null)
        {
            // Instanciar la resina cerca/de la mano del jugador (PickUp la re-posicionará en el ancla)
            Vector3 spawnPos = holder.transform.position;
            GameObject resina = Instantiate(resinaPrefab, spawnPos, Quaternion.identity);

            // Entregar al holder del jugador
            holder.PickUpExistingInstance(resina);
            
            // Reproducir SFX de spawn de resina (AudioManager)
            PlayResinSpawnSfx();
            
            // Opcional: Añadir efectos visuales o sonidos
            ProducirEfectos();
        }
        else
        {
            Debug.LogError("No se encontró prefab para material tipo2 (resina) de la era prehistórica.");
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

    private void PlayResinSpawnSfx()
    {
        if (resinSpawnSfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(resinSpawnSfxIndex);
        }
    }
}