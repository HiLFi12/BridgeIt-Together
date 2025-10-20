using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterySystem : MonoBehaviour, IInteractable
{
    [Header("Configuración de batería")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private int maxCargas = 1;
    [SerializeField] private float cooldownTime = 5f; // Tiempo en segundos hasta que se descarga
    
    [Header("Estado actual")]
    [SerializeField] private int cargasActuales = 0;
    [SerializeField] private bool isCharged = false;
    
    private float tiempoRestanteCooldown = 0f;

    public InteractPriority InteractPriority => interactPriority;
    
    public bool IsCharged => isCharged;
    public int CargasActuales => cargasActuales;
    public float TiempoRestanteCooldown => tiempoRestanteCooldown;

    private void Update()
    {
        // Si está cargada, decrementar el cooldown
        if (isCharged && tiempoRestanteCooldown > 0f)
        {
            tiempoRestanteCooldown -= Time.deltaTime;
            
            // Cuando el cooldown llega a 0, descargar la batería
            if (tiempoRestanteCooldown <= 0f)
            {
                TurnOffBattery();
            }
        }
    }

    public void Interact(GameObject interactor)
    {
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        
        if (playerObjectHolder == null || !playerObjectHolder.HasObjectInHand())
        {
            Debug.Log("No tienes ningún material en la mano.");
            return;
        }
        
        GameObject heldObject = playerObjectHolder.GetHeldObject();
        
        // Verificar si el objeto es material tipo superficie (nuevo tipo 3)
        if (!EsMaterialTipoSuperficie(heldObject))
        {
            Debug.Log("Este material no se puede usar para cargar la batería. Necesitas material tipo 3 (superficie).");
            return;
        }
        
        // Si ya está cargada (isCharged = true), permitir recargar y resetear el cooldown
        if (isCharged)
        {
            // Resetear el cooldown
            tiempoRestanteCooldown = cooldownTime;
            
            // Consumir el objeto de la mano del jugador
            playerObjectHolder.UseHeldObject();
            
            Debug.Log("Batería recargada. Cooldown reiniciado.");
            return;
        }
        
        // Si no está cargada, verificar si ya está al máximo de cargas (antes de activar isCharged)
        if (cargasActuales >= maxCargas)
        {
            Debug.Log("La batería ya está completamente cargada.");
            return;
        }
        
        // Agregar carga
        AgregarCarga();
        
        // Consumir el objeto de la mano del jugador
        playerObjectHolder.UseHeldObject();
        
        Debug.Log($"Batería cargada. Cargas actuales: {cargasActuales}/{maxCargas}");
    }

    private bool EsMaterialTipoSuperficie(GameObject objeto)
    {
        if (objeto == null) return false;
        
        // Verificar si tiene el componente MaterialTipo4 (legacy nombre de script)
        if (objeto.GetComponent<MaterialTipo4>() != null)
            return true;
        
        // Verificar mediante BridgeMaterialInfo
        BridgeMaterialInfo materialInfo = objeto.GetComponent<BridgeMaterialInfo>();
        if (materialInfo != null && materialInfo.layerIndex == 2) // layerIndex 2 = tipo superficie
            return true;
        
        return false;
    }

    private void AgregarCarga()
    {
        cargasActuales++;
        
        // Si alcanza el máximo de cargas, activar isCharged y el cooldown
        if (cargasActuales >= maxCargas)
        {
            isCharged = true;
            tiempoRestanteCooldown = cooldownTime;
            Debug.Log("¡Batería completamente cargada! Iniciando cooldown.");
        }
    }

    private void TurnOffBattery()
    {
        isCharged = false;
        cargasActuales = 0;
        tiempoRestanteCooldown = 0f;
        Debug.Log("La batería se ha descargado.");
    }

    // Método público para descargar manualmente (útil para otras mecánicas)
    public void ForzarDescarga()
    {
        TurnOffBattery();
    }

    // Método público para verificar y consumir carga (útil para máquinas que requieren batería)
    public bool TryConsumeCharge()
    {
        if (isCharged)
        {
            TurnOffBattery();
            return true;
        }
        return false;
    }
}