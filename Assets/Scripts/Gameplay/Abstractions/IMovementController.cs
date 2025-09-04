using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    public interface IMovementController
    {
        void Initialize(GameObject owner, Rigidbody rb, float velocidadBase, float multiplicadorVelocidad, bool usarPhysics);
        void SetDirection(Vector3 direction);
        void SetSpeed(float speed);
        float GetSpeed();
        void TickUpdate();
        void TickFixed();
        void Reset();
    }
}
