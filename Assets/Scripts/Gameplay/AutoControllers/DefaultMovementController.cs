using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    public class DefaultMovementController : IMovementController
    {
        private GameObject owner;
        private Transform ownerTransform;
        private Rigidbody rb;
        private float velocidadBase;
        private float multiplicadorVelocidad;
        private bool usarPhysics;
        private Vector3 direccionMovimiento = Vector3.right;
        private float velocidadActual;
        private float velocidadOriginal;
    // Nota: La corrección de rotación ahora la hace únicamente AutoController.

        public void Initialize(GameObject owner, Rigidbody rb, float velocidadBase, float multiplicadorVelocidad, bool usarPhysics)
        {
            this.owner = owner;
            this.ownerTransform = owner != null ? owner.transform : null;
            this.rb = rb;
            this.velocidadBase = Mathf.Max(0f, velocidadBase);
            this.velocidadOriginal = this.velocidadBase;
            this.multiplicadorVelocidad = multiplicadorVelocidad;
            this.usarPhysics = usarPhysics;
            this.velocidadActual = GetSpeed();
        }

        public void SetDirection(Vector3 direction)
        {
            direccionMovimiento = direction;
        }

        public void SetSpeed(float speed)
        {
            velocidadBase = Mathf.Max(0f, speed);
            velocidadActual = GetSpeed();
        }

        public float GetSpeed() => velocidadBase * multiplicadorVelocidad;

        public void TickUpdate()
        {
            if (!usarPhysics)
            {
                ownerTransform.Translate(direccionMovimiento.normalized * velocidadActual * Time.deltaTime, Space.World);
            }
        }

        public void TickFixed()
        {
            if (usarPhysics && rb != null)
            {
                var targetVelocity = direccionMovimiento.normalized * velocidadActual;
                var vel = rb.linearVelocity;
                rb.linearVelocity = new Vector3(targetVelocity.x, vel.y, targetVelocity.z);
            }
        }

        public void Reset()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            velocidadActual = velocidadOriginal;
        }
    }
}
