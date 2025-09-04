using System.Collections.Generic;
using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    [System.Serializable]
    public class BridgeCollisionSettings
    {
        public string vehicleTag = "Vehicle";
        public string bridgeQuadrantTag = "BridgeQuadrant";
        public float collisionCooldown = 1.0f;
        public bool debug = true;
    }

    public interface IBridgeCollisionHandler
    {
        void Initialize(GameObject owner, BridgeCollisionSettings settings);
        void SetGrid(BridgeConstructionGrid grid);
        void Tick();
        void HandleCollision(Collision collision);
        void HandleTrigger(Collider other);
    }
}
