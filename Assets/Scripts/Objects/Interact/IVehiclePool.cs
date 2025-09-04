using UnityEngine;

/// <summary>
/// Interfaz para el sistema de pool de vehículos
/// </summary>
public interface IVehiclePool 
{
    void Initialize(GameObject prefab, int size, bool expandable, BridgeConstructionGrid grid);
    GameObject GetVehicleFromPool();
    void ReturnVehicleToPool(GameObject vehicle);
    bool IsVehicleFromPool(GameObject vehicle);
    void ClearActiveVehicles();
    void ConfigureBridgeCollision(GameObject vehicle);
}
