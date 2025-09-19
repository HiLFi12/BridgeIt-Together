using UnityEngine;

/// <summary>
/// Script para objetos hijos de un vehículo que necesitan reportar colisiones
/// al script VehicleBridgeCollision del padre Y al VehiclePlayerCollision
/// </summary>
public class VehicleChildCollider : MonoBehaviour
{
    [Header("Configuración")]
    public bool debugMode = true;
    [Header("Comunicación")]
    public bool reportarABridgeCollision = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (debugMode) Debug.Log($"Trigger detectado en objeto hijo {gameObject.name} con: {other.gameObject.name}");
        
        // Reportar al sistema de triggers con el puente
        if (reportarABridgeCollision)
        {
            VehicleBridgeCollision.HandleTriggerFromChild(gameObject, other);
        }
    }
}
