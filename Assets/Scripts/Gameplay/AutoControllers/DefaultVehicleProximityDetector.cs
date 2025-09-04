using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    public class DefaultVehicleProximityDetector : IVehicleProximityDetector
    {
        private GameObject owner;
        private float distance;
    private AutoController ownerAuto;

        public void Initialize(GameObject owner, float detectionDistance)
        {
            this.owner = owner;
            this.distance = detectionDistance;
            this.ownerAuto = owner != null ? owner.GetComponentInParent<AutoController>() : null;
        }

        public bool IsVehicleAhead(Vector3 direction)
        {
            if (owner == null) return false;
            Vector3 dir = direction.normalized;
            Ray ray = new Ray(owner.transform.position, dir);
            RaycastHit[] hits = Physics.RaycastAll(ray, distance);
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                var hitAuto = hit.collider.GetComponentInParent<AutoController>();
                if (hitAuto == null) continue;
                // ignorar mis propios colliders/vehículo
                if (ownerAuto != null && hitAuto == ownerAuto) continue;
                if (hitAuto != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
