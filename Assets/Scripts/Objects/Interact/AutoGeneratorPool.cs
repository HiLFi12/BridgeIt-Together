using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de pooling de vehículos para AutoGenerator
/// Versión independiente, separada de la lógica de generación
/// </summary>
public class AutoGeneratorPool : MonoBehaviour
{
    [SerializeField] private GameObject vehiclePrefab; // Prefab del vehículo a utilizar
    [SerializeField] private int initialPoolSize = 10; // Tamaño inicial del pool
    [SerializeField] private bool expandablePool = true; // Si el pool puede crecer automáticamente

    // Lista de vehículos en el pool
    private List<GameObject> vehiclePool = new List<GameObject>();
    
    // Referencia al grid del puente para compartir con los vehículos
    private BridgeConstructionGrid bridgeGrid;

    /// <summary>
    /// Inicializa el pool con el prefab y el tamaño especificados
    /// </summary>
    public void Initialize(GameObject prefab, int size, bool expandable, BridgeConstructionGrid grid)
    {
        vehiclePrefab = prefab;
        initialPoolSize = size;
        expandablePool = expandable;
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
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPoolObject();
        }
    }

    /// <summary>
    /// Crea un nuevo objeto en el pool
    /// </summary>
    private GameObject CreateNewPoolObject()
    {
        GameObject vehicle = Instantiate(vehiclePrefab, Vector3.zero, Quaternion.identity);
        vehicle.SetActive(false);
        vehicle.transform.SetParent(transform);
        vehiclePool.Add(vehicle);
        return vehicle;
    }

    /// <summary>
    /// Obtiene un vehículo del pool, creando uno nuevo si es necesario
    /// </summary>
    public GameObject GetVehicleFromPool()
    {
        // Buscar un vehículo inactivo
        foreach (GameObject vehicle in vehiclePool)
        {
            if (vehicle != null && !vehicle.activeInHierarchy)
            {
                return vehicle;
            }
        }

        // Si no hay vehículos disponibles y el pool es expandible, crear uno nuevo
        if (expandablePool)
        {
            Debug.Log("Pool de vehículos expandido automáticamente");
            return CreateNewPoolObject();
        }

        Debug.LogWarning("No hay vehículos disponibles en el pool y no está configurado como expandible");
        return null;
    }

    /// <summary>
    /// Devuelve un vehículo al pool (lo desactiva)
    /// </summary>
    public void ReturnVehicleToPool(GameObject vehicle)
    {
        if (vehicle == null) return;
        
        // Verificar que el vehículo pertenece a este pool
        if (IsVehicleFromPool(vehicle))
        {
            // Detener cualquier comportamiento activo
            AutoMovement movement = vehicle.GetComponent<AutoMovement>();
            if (movement != null)
            {
                // Resetear el componente de movimiento
                movement.enabled = false;
                movement.enabled = true;
            }
            
            // Desactivar el objeto
            vehicle.SetActive(false);
        }
    }

    /// <summary>
    /// Verifica si un vehículo pertenece a este pool
    /// </summary>
    public bool IsVehicleFromPool(GameObject vehicle)
    {
        return vehiclePool.Contains(vehicle);
    }

    /// <summary>
    /// Limpia todos los vehículos activos, devolviéndolos al pool
    /// </summary>
    public void ClearActiveVehicles()
    {
        foreach (GameObject vehicle in vehiclePool)
        {
            if (vehicle != null && vehicle.activeInHierarchy)
            {
                ReturnVehicleToPool(vehicle);
            }
        }
    }
    
    /// <summary>
    /// Configura un vehículo para interactuar con el puente
    /// </summary>
    public void ConfigureBridgeCollision(GameObject vehicle)
    {
        if (vehicle == null || bridgeGrid == null) return;
        
        VehicleBridgeCollision collision = vehicle.GetComponent<VehicleBridgeCollision>();
        if (collision != null)
        {
            collision.bridgeGrid = bridgeGrid;
        }
    }
}
