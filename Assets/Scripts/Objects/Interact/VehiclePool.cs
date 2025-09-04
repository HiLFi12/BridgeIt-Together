using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Sistema de pooling de vehículos para optimizar el rendimiento
/// Esta clase maneja exclusivamente la lógica del pool de objetos
/// </summary>
public class VehiclePool : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [SerializeField] private GameObject vehiclePrefab; // Prefab del vehículo principal a utilizar
    [SerializeField] private int poolSize = 10; // Tamaño inicial del pool de autos
    [SerializeField] private bool poolExpandible = true; // Si el pool puede crecer automáticamente
    
    // Lista de vehículos en el pool
    private List<GameObject> vehiclePoolList = new List<GameObject>();
    
    // Diccionario para múltiples tipos de prefabs y sus pools
    private Dictionary<GameObject, List<GameObject>> multiPrefabPools = new Dictionary<GameObject, List<GameObject>>();
    
    // Referencia al grid del puente para compartir con los vehículos
    private BridgeConstructionGrid bridgeGrid;

    /// <summary>
    /// Inicializa el pool con la referencia al bridge grid
    /// </summary>
    /// <param name="grid">Referencia al BridgeConstructionGrid</param>
    public void Initialize(BridgeConstructionGrid grid)
    {
        bridgeGrid = grid;
        InitializePool();
    }

    /// <summary>
    /// Inicializa el pool con parámetros específicos
    /// </summary>
    /// <param name="prefab">Prefab del vehículo</param>
    /// <param name="size">Tamaño del pool</param>
    /// <param name="expandable">Si el pool puede expandirse</param>
    /// <param name="grid">Referencia al BridgeConstructionGrid</param>
    public void Initialize(GameObject prefab, int size, bool expandable, BridgeConstructionGrid grid)
    {
        vehiclePrefab = prefab;
        poolSize = size;
        poolExpandible = expandable;
        bridgeGrid = grid;
        InitializePool();
    }

    /// <summary>
    /// Crea los objetos iniciales del pool
    /// </summary>
    private void InitializePool()
    {
        if (vehiclePrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab de vehículo al pool");
            return;
        }

        // Crear objetos iniciales en el pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewPoolObject();
        }
    }

    /// <summary>
    /// Crea un nuevo objeto y lo agrega al pool principal
    /// </summary>
    private GameObject CreateNewPoolObject()
    {
        return CreateNewPoolObject(vehiclePrefab, vehiclePoolList);
    }
    
    /// <summary>
    /// Crea un nuevo objeto de un prefab específico y lo agrega a un pool específico
    /// </summary>
    /// <param name="prefab">Prefab a usar para crear el objeto</param>
    /// <param name="poolList">Lista del pool donde agregar el objeto</param>
    private GameObject CreateNewPoolObject(GameObject prefab, List<GameObject> poolList)
    {
        GameObject newVehicle = Instantiate(prefab);
        newVehicle.SetActive(false);
        newVehicle.transform.SetParent(transform);
        poolList.Add(newVehicle);
        
        return newVehicle;
    }

    /// <summary>
    /// Obtiene un vehículo disponible del pool principal
    /// </summary>
    /// <returns>GameObject del vehículo o null si no hay disponibles</returns>
    public GameObject GetVehicleFromPool()
    {
        return GetVehicleFromPool(vehiclePrefab);
    }
    
    /// <summary>
    /// Obtiene un vehículo disponible del pool para un prefab específico
    /// </summary>
    /// <param name="prefab">Prefab específico del vehículo a obtener</param>
    /// <returns>GameObject del vehículo o null si no hay disponibles</returns>
    public GameObject GetVehicleFromPool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab es null, usando prefab principal");
            prefab = vehiclePrefab;
        }
        
        // Si es el prefab principal, usar el pool principal
        if (prefab == vehiclePrefab)
        {
            // Buscar un objeto inactivo en el pool principal (depurando referencias destruidas)
            for (int i = vehiclePoolList.Count - 1; i >= 0; i--)
            {
                var vehicle = vehiclePoolList[i];
                if (vehicle == null)
                {
                    vehiclePoolList.RemoveAt(i);
                    continue;
                }
                if (!vehicle.activeInHierarchy)
                {
                    return vehicle;
                }
            }

            // Si no hay objetos disponibles y el pool es expandible, crear uno nuevo
            if (poolExpandible)
            {
                Debug.Log("Pool principal expandido: creando nuevo vehículo");
                return CreateNewPoolObject();
            }
        }
        else
        {
            // Usar pool específico para este prefab
            if (!multiPrefabPools.ContainsKey(prefab))
            {
                multiPrefabPools[prefab] = new List<GameObject>();
            }
            
            List<GameObject> specificPool = multiPrefabPools[prefab];
            
            // Buscar un objeto inactivo en el pool específico (depurando referencias destruidas)
            for (int i = specificPool.Count - 1; i >= 0; i--)
            {
                var vehicle = specificPool[i];
                if (vehicle == null)
                {
                    specificPool.RemoveAt(i);
                    continue;
                }
                if (!vehicle.activeInHierarchy)
                {
                    return vehicle;
                }
            }

            // Si no hay objetos disponibles y el pool es expandible, crear uno nuevo
            if (poolExpandible)
            {
                Debug.Log($"Pool específico expandido: creando nuevo vehículo de tipo {prefab.name}");
                return CreateNewPoolObject(prefab, specificPool);
            }
        }

        Debug.LogWarning("No hay vehículos disponibles en el pool y la expansión está deshabilitada");
        return null;
    }

    /// <summary>
    /// Retorna un vehículo al pool (lo desactiva y resetea)
    /// </summary>
    /// <param name="vehicle">El vehículo a retornar</param>
    public void ReturnVehicleToPool(GameObject vehicle)
    {
        if (vehicle == null)
        {
            Debug.LogWarning("Intentando retornar un vehículo nulo al pool");
            return;
        }

        // Verificar que el vehículo pertenece a algún pool
        bool pertenece = vehiclePoolList.Contains(vehicle);
        if (!pertenece)
        {
            // Verificar en pools específicos
            foreach (var kvp in multiPrefabPools)
            {
                if (kvp.Value.Contains(vehicle))
                {
                    pertenece = true;
                    break;
                }
            }
        }
        
        if (!pertenece)
        {
            Debug.LogWarning("El vehículo no pertenece a ningún pool");
            return;
        }
        
        // SOLUCIÓN ADICIONAL: Limpiar el vehículo de todos los triggers de condición antes de resetear
        LimpiarVehiculoDeTriggersDeCondicion(vehicle);
        
        // Resetear el vehículo
        vehicle.transform.position = Vector3.zero;
        vehicle.transform.rotation = Quaternion.identity;
        vehicle.SetActive(false);
        
        // Resetear completamente el componente de movimiento si existe
        AutoMovement autoMovement = vehicle.GetComponent<AutoMovement>();
        if (autoMovement != null)
        {
            autoMovement.ResetearAuto();
            autoMovement.enabled = false;
        }

        // Resetear AutoController si existe (nueva arquitectura)
        AutoController autoController = vehicle.GetComponent<AutoController>();
        //if (autoController != null)
        //{
        //    autoController.ResetearAuto();
        //}
    }
    
    /// <summary>
    /// Limpia un vehículo de todas las listas de vehículos contados en GameConditionTriggers
    /// SOLUCIÓN: Esto previene que los vehículos reutilizados del pool sean ignorados en conteos futuros
    /// </summary>
    /// <param name="vehicle">El vehículo a limpiar</param>
    private void LimpiarVehiculoDeTriggersDeCondicion(GameObject vehicle)
    {
        // Encontrar todos los GameConditionTrigger en la escena
        GameConditionTrigger[] conditionTriggers = FindObjectsByType<GameConditionTrigger>(FindObjectsSortMode.None);
        
        foreach (GameConditionTrigger trigger in conditionTriggers)
        {
            if (trigger != null)
            {
                // Forzar la limpieza del vehículo específico
                trigger.RemoverVehiculoContado(vehicle);
            }
        }
    }

    /// <summary>
    /// Desactiva todos los vehículos activos y los retorna al pool
    /// </summary>
    public void ClearActiveVehicles()
    {
        // Limpiar pool principal
        foreach (GameObject vehicle in vehiclePoolList)
        {
            if (vehicle.activeInHierarchy)
            {
                ReturnVehicleToPool(vehicle);
            }
        }
        
        // Limpiar pools específicos
        foreach (var kvp in multiPrefabPools)
        {
            foreach (GameObject vehicle in kvp.Value)
            {
                if (vehicle.activeInHierarchy)
                {
                    ReturnVehicleToPool(vehicle);
                }
            }
        }
    }

    /// <summary>
    /// Verifica si un vehículo pertenece a este pool
    /// </summary>
    /// <param name="vehicle">El GameObject del vehículo a verificar</param>
    /// <returns>True si el vehículo pertenece a este pool</returns>
    public bool IsVehicleFromPool(GameObject vehicle)
    {
        // Verificar pool principal
        if (vehiclePoolList.Contains(vehicle))
            return true;
        
        // Verificar pools específicos
        foreach (var kvp in multiPrefabPools)
        {
            if (kvp.Value.Contains(vehicle))
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Obtiene el número total de vehículos en todos los pools
    /// </summary>
    /// <returns>Cantidad total de vehículos</returns>
    public int GetTotalVehicleCount()
    {
        int total = vehiclePoolList.Count;
        
        foreach (var kvp in multiPrefabPools)
        {
            total += kvp.Value.Count;
        }
        
        return total;
    }

    /// <summary>
    /// Obtiene el número de vehículos activos en todos los pools
    /// </summary>
    /// <returns>Cantidad de vehículos activos</returns>
    public int GetActiveVehicleCount()
    {
        int activeCount = 0;
        
        // Contar activos en pool principal
        foreach (GameObject vehicle in vehiclePoolList)
        {
            if (vehicle.activeInHierarchy)
                activeCount++;
        }
        
        // Contar activos en pools específicos
        foreach (var kvp in multiPrefabPools)
        {
            foreach (GameObject vehicle in kvp.Value)
            {
                if (vehicle.activeInHierarchy)
                    activeCount++;
            }
        }
        
        return activeCount;
    }

    /// <summary>
    /// Obtiene el número de vehículos disponibles en el pool
    /// </summary>
    /// <returns>Cantidad de vehículos disponibles</returns>
    public int GetAvailableVehicleCount()
    {
        return GetTotalVehicleCount() - GetActiveVehicleCount();
    }

    /// <summary>
    /// Configura el prefab del vehículo
    /// </summary>
    /// <param name="prefab">Nuevo prefab a utilizar</param>
    public void SetVehiclePrefab(GameObject prefab)
    {
        vehiclePrefab = prefab;
    }

    /// <summary>
    /// Obtiene la referencia al BridgeConstructionGrid
    /// </summary>
    /// <returns>Referencia al BridgeConstructionGrid</returns>
    public BridgeConstructionGrid GetBridgeGrid()
    {        return bridgeGrid;
    }

    /// <summary>
    /// Configura la colisión del vehículo con el puente (método de compatibilidad)
    /// </summary>
    /// <param name="vehicle">El vehículo a configurar</param>
    public void ConfigureBridgeCollision(GameObject vehicle)
    {
        if (vehicle == null || bridgeGrid == null) return;
        
        VehicleBridgeCollision vehicleCollision = vehicle.GetComponent<VehicleBridgeCollision>();
        if (vehicleCollision != null)
        {
            vehicleCollision.bridgeGrid = bridgeGrid;
        }
    }

    private void OnDestroy()
    {
        // Limpiar el pool principal al destruir el objeto
        if (vehiclePoolList != null)
        {
            foreach (GameObject vehicle in vehiclePoolList)
            {
                if (vehicle != null)
                {
                    DestroyImmediate(vehicle);
                }
            }
            vehiclePoolList.Clear();
        }
        
        // Limpiar pools específicos
        if (multiPrefabPools != null)
        {
            foreach (var kvp in multiPrefabPools)
            {
                foreach (GameObject vehicle in kvp.Value)
                {
                    if (vehicle != null)
                    {
                        DestroyImmediate(vehicle);
                    }
                }
            }
            multiPrefabPools.Clear();
        }
    }
}
