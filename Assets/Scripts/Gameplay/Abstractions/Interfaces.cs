using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    public interface IVehiclePoolService
    {
        GameObject GetVehicleFromPool(GameObject prefab = null);
        void ReturnVehicleToPool(GameObject vehicle);
        void ClearActiveVehicles();
        int GetActiveVehicleCount();
        bool IsVehicleFromPool(GameObject vehicle);
        void Initialize(GameObject defaultPrefab, int initialSize, bool expandable, BridgeConstructionGrid bridgeGrid);
    }

    public interface IPlatformProvider
    {
        bool TryGetPlatforms(out Transform izquierda, out Transform derecha);
    }
}
