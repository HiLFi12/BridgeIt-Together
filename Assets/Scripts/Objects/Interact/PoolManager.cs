using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para conectar AutoGenerator con VehiclePool
/// </summary>
public static class PoolManager
{
    /// <summary>
    /// Obtiene o crea un componente VehiclePool en el GameObject especificado
    /// </summary>
    public static VehiclePool GetOrCreateVehiclePool(GameObject gameObject)
    {
        VehiclePool pool = gameObject.GetComponent<VehiclePool>();
        if (pool == null)
        {
            pool = gameObject.AddComponent<VehiclePool>();
        }
        return pool;
    }
}
