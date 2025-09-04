using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para conectar AutoGenerator con AutoGeneratorPool
/// </summary>
public class VehiclePoolConnector
{
    /// <summary>
    /// Obtiene o crea un componente AutoGeneratorPool en el GameObject especificado
    /// </summary>
    public static AutoGeneratorPool GetPoolComponent(GameObject gameObject)
    {
        AutoGeneratorPool pool = gameObject.GetComponent<AutoGeneratorPool>();
        if (pool == null)
        {
            pool = gameObject.AddComponent<AutoGeneratorPool>();
        }
        return pool;
    }
}
