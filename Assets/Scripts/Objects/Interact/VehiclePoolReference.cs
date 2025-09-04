using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esta clase actúa como un intermediario entre AutoGenerator y VehiclePool
/// </summary>
public class VehiclePoolReference : MonoBehaviour
{
    private VehiclePool poolInstance;
    
    /// <summary>
    /// Obtiene o crea una instancia de VehiclePool en el GameObject
    /// </summary>
    public VehiclePool GetOrCreatePool()
    {
        if (poolInstance == null)
        {
            poolInstance = GetComponent<VehiclePool>();
            if (poolInstance == null)
            {
                poolInstance = gameObject.AddComponent<VehiclePool>();
            }
        }
        return poolInstance;
    }
    
    /// <summary>
    /// Inicializa el pool de vehículos
    /// </summary>
    public void InitializePool(GameObject prefab, int size, bool expandable, BridgeConstructionGrid grid)
    {
        VehiclePool pool = GetOrCreatePool();
        pool.Initialize(prefab, size, expandable, grid);
    }
    
    /// <summary>
    /// Obtiene un vehículo del pool
    /// </summary>
    public GameObject GetVehicleFromPool()
    {
        VehiclePool pool = GetOrCreatePool();
        return pool.GetVehicleFromPool();
    }
    
    /// <summary>
    /// Devuelve un vehículo al pool
    /// </summary>
    public void ReturnVehicleToPool(GameObject vehicle)
    {
        if (poolInstance != null)
        {
            poolInstance.ReturnVehicleToPool(vehicle);
        }
    }
    
    /// <summary>
    /// Limpia todos los vehículos activos
    /// </summary>
    public void ClearActiveVehicles()
    {
        if (poolInstance != null)
        {
            poolInstance.ClearActiveVehicles();
        }
    }
    
    /// <summary>
    /// Configura un vehículo para interactuar con el puente
    /// </summary>
    public void ConfigureBridgeCollision(GameObject vehicle)
    {
        if (poolInstance != null)
        {
            poolInstance.ConfigureBridgeCollision(vehicle);
        }
    }
    
    /// <summary>
    /// Verifica si un vehículo pertenece a este pool
    /// </summary>
    public bool IsVehicleFromPool(GameObject vehicle)
    {
        if (poolInstance == null) return false;
        return poolInstance.IsVehicleFromPool(vehicle);
    }
}
