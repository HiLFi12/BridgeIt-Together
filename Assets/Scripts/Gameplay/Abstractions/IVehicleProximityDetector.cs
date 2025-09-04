using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    public interface IVehicleProximityDetector
    {
        void Initialize(GameObject owner, float detectionDistance);
        bool IsVehicleAhead(Vector3 direction);
    }
}
