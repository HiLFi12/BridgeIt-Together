using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    public class DefaultBridgeCollisionHandler : IBridgeCollisionHandler
    {
        private GameObject owner;
        private Transform ownerTransform;
        private BridgeConstructionGrid grid;
        private BridgeCollisionSettings settings;
        private readonly Dictionary<string, float> recent = new Dictionary<string, float>();

        public void Initialize(GameObject owner, BridgeCollisionSettings settings)
        {
            this.owner = owner;
            this.ownerTransform = owner != null ? owner.transform : null;
            this.settings = settings ?? new BridgeCollisionSettings();
        }

        public void SetGrid(BridgeConstructionGrid grid)
        {
            this.grid = grid;
        }

        public void Tick()
        {
            // Limpieza liviana del cooldown
            if (recent.Count == 0) return;
            if (Time.frameCount % 300 != 0) return;
            float t = Time.time;
            var toRemove = new List<string>();
            foreach (var kv in recent)
            {
                if (t - kv.Value > settings.collisionCooldown * 2f)
                    toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove) recent.Remove(k);
        }

        public void HandleCollision(Collision collision)
        {
            if (grid == null) { if (settings.debug) Debug.LogWarning("[BridgeHandler] Grid nulo"); return; }
            if (!IsOwnerVehicle()) return;
            if (collision.gameObject.CompareTag(settings.bridgeQuadrantTag))
            {
                Vector3 hitPoint = collision.contacts[0].point;
                ProcessImpactAtPoint(hitPoint);
            }
        }

        public void HandleTrigger(Collider other)
        {
            if (grid == null) { if (settings.debug) Debug.LogWarning("[BridgeHandler] Grid nulo (trigger)"); return; }
            if (!IsOwnerVehicle()) return;

            GameObject targetObject = other.gameObject;
            string targetName = targetObject.name;

            if (targetName.StartsWith("Layer_"))
            {
                Transform parent = targetObject.transform.parent;
                if (parent != null && parent.name.StartsWith("Quadrant_"))
                {
                    string[] parts = parent.name.Split('_');
                    if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int z))
                    {
                        ProcessImpactAtQuadrant(x, z);
                        return;
                    }
                }
            }

            if (other.CompareTag(settings.bridgeQuadrantTag))
            {
                Vector3 hitPoint = other.ClosestPoint(ownerTransform.position);
                ProcessImpactAtPoint(hitPoint);
            }
        }

        private bool IsOwnerVehicle()
        {
            if (owner == null) return false;
            if (!string.IsNullOrEmpty(settings.vehicleTag) && owner.CompareTag(settings.vehicleTag)) return true;
            Transform p = owner.transform.parent;
            while (p != null)
            {
                if (!string.IsNullOrEmpty(settings.vehicleTag) && p.CompareTag(settings.vehicleTag)) return true;
                p = p.parent;
            }
            return false;
        }

        private void ProcessImpactAtPoint(Vector3 worldPoint)
        {
            Vector3 localPos = worldPoint - grid.transform.position;
            int x = Mathf.FloorToInt(localPos.x / grid.quadrantSize);
            int z = Mathf.FloorToInt(localPos.z / grid.quadrantSize);
            ProcessImpactAtQuadrant(x, z);
        }

        private void ProcessImpactAtQuadrant(int x, int z)
        {
            if (x < 0 || x >= grid.gridWidth || z < 0 || z >= grid.gridLength)
            {
                if (settings.debug) Debug.LogWarning($"[BridgeHandler] Cuadrante fuera de límites [{x},{z}]");
                return;
            }
            if (!IsCollisionValid(x, z))
            {
                if (settings.debug) Debug.Log($"[BridgeHandler] Ignorado por cooldown [{x},{z}]");
                return;
            }
            grid.OnVehicleImpact(x, z);
            var prob = ownerTransform != null ? ownerTransform.GetComponentInParent<ProbabilityAction>() : null;
            if (prob != null) prob.TryExecuteOnQuadrant(x, z);
        }

        private bool IsCollisionValid(int x, int z)
        {
            string key = $"{x}_{z}";
            float t = Time.time;
            if (recent.TryGetValue(key, out float last) && (t - last) < settings.collisionCooldown)
            {
                return false;
            }
            recent[key] = t;
            return true;
        }
    }
}
