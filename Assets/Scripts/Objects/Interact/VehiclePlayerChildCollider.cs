using UnityEngine;

/// <summary>
/// Script para objetos hijos de un vehículo que necesitan reportar colisiones
/// al script VehiclePlayerCollision del padre para detección de jugadores
/// </summary>
public class VehiclePlayerChildCollider : MonoBehaviour
{
    [Header("Configuración")]
    public bool debugMode = true;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (debugMode) Debug.Log($"Colisión detectada en objeto hijo {gameObject.name} con: {collision.gameObject.name}");
        
        // Reportar la colisión al script del vehículo padre
        VehiclePlayerCollision.HandleCollisionFromChild(gameObject, collision);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (debugMode) Debug.Log($"Trigger detectado en objeto hijo {gameObject.name} con: {other.gameObject.name}");
        
        // Reportar el trigger al script del vehículo padre
        VehiclePlayerCollision.HandleTriggerFromChild(gameObject, other);
    }
}
