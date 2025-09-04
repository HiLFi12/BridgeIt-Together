using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.Spawning
{
    /// <summary>
    /// Adaptador para cumplir la interfaz IVehiclePool usando el componente existente VehiclePool.
    /// Facilita pruebas y reduce acoplamiento (DIP).
    /// </summary>
    [DisallowMultipleComponent]
    public class VehiclePoolAdapter : MonoBehaviour, IVehiclePoolService
    {
        private VehiclePool pool;

        private void Awake()
        {
            pool = GetComponent<VehiclePool>();
            if (pool == null) pool = gameObject.AddComponent<VehiclePool>();
        }

        public void Initialize(GameObject defaultPrefab, int initialSize, bool expandable, BridgeConstructionGrid bridgeGrid)
        {
            pool.Initialize(defaultPrefab, initialSize, expandable, bridgeGrid);
        }

    public GameObject GetVehicleFromPool(GameObject prefab = null)
        {
            if (prefab == null) return pool.GetVehicleFromPool();
            return pool.GetVehicleFromPool(prefab);
        }

        public void ReturnVehicleToPool(GameObject vehicle) => pool.ReturnVehicleToPool(vehicle);
        public void ClearActiveVehicles() => pool.ClearActiveVehicles();
        public int GetActiveVehicleCount() => pool.GetActiveVehicleCount();
        public bool IsVehicleFromPool(GameObject vehicle) => pool.IsVehicleFromPool(vehicle);
    }
}
