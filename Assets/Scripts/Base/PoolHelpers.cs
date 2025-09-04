using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esta es una clase de ayuda para asegurarnos de que VehiclePool esté disponible para AutoGenerator.
/// </summary>
public static class PoolHelpers
{
    /// <summary>
    /// Una función de ayuda para obtener un pool de vehículos
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
