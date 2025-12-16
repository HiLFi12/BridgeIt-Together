using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BridgeItTogether.Gameplay.SafeZones
{
    [DisallowMultipleComponent]
    public class SafeZoneArea : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(6f, 6f);
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color fillColor = new Color(0f, 1f, 0f, 0.08f);
        [SerializeField] private Color wireColor = new Color(0f, 1f, 0f, 0.6f);

        [Header("Lanzamiento (Parábola Física)")]
        [SerializeField, Range(10f, 80f)] private float launchAngleDeg = 45f;
        [SerializeField, Min(0.1f)] private float launchGravity = 9.81f;
        [SerializeField] private bool kinematicDuringLaunch = true;
        [SerializeField] private float minLaunchDistance = 0.05f;

        private readonly Dictionary<Transform, Coroutine> activeLaunches = new();

        public Vector2 Size
        {
            get => size;
            set => size = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
        }

        /// <summary>
        /// Detiene el lanzamiento activo para el transform dado si existe.
        /// Usado cuando un objeto es recogido durante el vuelo.
        /// </summary>
        public void StopLaunchForObject(Transform target)
        {
            if (target == null) return;
            
            if (activeLaunches.TryGetValue(target, out Coroutine routine))
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
                activeLaunches.Remove(target);
                RestorePhysicsComponents(target);
            }
        }

        public Vector3 GetRandomPointInside()
        {
            Vector2 half = size * 0.5f;
            float rx = Random.Range(-half.x, half.x);
            float rz = Random.Range(-half.y, half.y);
            return new Vector3(transform.position.x + rx, transform.position.y, transform.position.z + rz);
        }

        public float SqrDistanceTo(Vector3 worldPos)
        {
            return (transform.position - worldPos).sqrMagnitude;
        }

        /// <summary>
        /// Lanza un IHitable a esta SafeZone usando una parábola balística.
        /// El caller debe haber deshabilitado CharacterController o configurado Rigidbody previamente.
        /// </summary>
        public void LaunchHitableToZone(Transform target, IHitable hitable)
        {
            if (target == null || activeLaunches.ContainsKey(target)) return;

            Vector3 destino = GetRandomPointInside();
            hitable?.OnLaunched(destino);

            var routine = StartCoroutine(LaunchRoutine(target, destino));
            activeLaunches[target] = routine;
        }

        private IEnumerator LaunchRoutine(Transform target, Vector3 destino)
        {
            if (target == null) yield break;

            Vector3 start = target.position;
            Vector3 end = destino;
            
            // Calcular distancia horizontal y diferencia de altura
            Vector3 horizontalDisplacement = new Vector3(end.x - start.x, 0f, end.z - start.z);
            float horizontalDistance = horizontalDisplacement.magnitude;
            
            if (horizontalDistance < minLaunchDistance)
            {
                activeLaunches.Remove(target);
                RestorePhysicsComponents(target);
                yield break;
            }

            Vector3 horizontalDir = horizontalDisplacement / horizontalDistance;
            float heightDifference = end.y - start.y;
            
            // Calcular velocidad inicial necesaria para llegar EXACTAMENTE al destino
            float g = Mathf.Max(0.01f, launchGravity);
            float angleRad = launchAngleDeg * Mathf.Deg2Rad;
            float tanAngle = Mathf.Tan(angleRad);
            float cosAngle = Mathf.Cos(angleRad);
            
            float denominator = 2f * cosAngle * cosAngle * (horizontalDistance * tanAngle - heightDifference);
            
            if (denominator <= 0f)
            {
                Debug.LogWarning($"[SafeZoneArea] No se puede calcular trayectoria exacta con ángulo {launchAngleDeg}°.");
                activeLaunches.Remove(target);
                RestorePhysicsComponents(target);
                yield break;
            }
            
            float velocitySquared = (g * horizontalDistance * horizontalDistance) / denominator;
            float velocity = Mathf.Sqrt(velocitySquared);
            
            // Calcular tiempo de vuelo
            float totalTime = horizontalDistance / (velocity * cosAngle);
            
            // Ejecutar parábola balística exacta
            float t = 0f;
            Vector3 lastPosition = start;
            
            while (t < totalTime && target != null)
            {
                t += Time.deltaTime;
                float ct = Mathf.Clamp(t, 0f, totalTime);
                
                // Posición horizontal (movimiento uniforme)
                float horizontalProgress = velocity * cosAngle * ct;
                
                // Posición vertical (movimiento parabólico)
                float verticalProgress = velocity * Mathf.Sin(angleRad) * ct - 0.5f * g * ct * ct;
                
                lastPosition = target.position;
                Vector3 newPosition = start + horizontalDir * horizontalProgress;
                newPosition.y = start.y + verticalProgress;
                
                target.position = newPosition;

                yield return null;
            }

            // Calcular velocidad final de la trayectoria para que continúe cayendo naturalmente
            Vector3 finalVelocity = Vector3.zero;
            if (target != null && Time.deltaTime > 0f)
            {
                finalVelocity = (target.position - lastPosition) / Time.deltaTime;
            }

            // Restaurar componentes de física
            if (target != null)
            {
                RestorePhysicsComponents(target, finalVelocity);
                activeLaunches.Remove(target);
            }
        }

        private void RestorePhysicsComponents(Transform target, Vector3 finalVelocity = default)
        {
            if (target == null) return;

            CharacterController charController = target.GetComponent<CharacterController>();
            Rigidbody trb = target.GetComponent<Rigidbody>();

            // Restaurar CharacterController (caerá por gravedad natural)
            if (charController != null)
            {
                charController.enabled = true;
            }

            // Restaurar Rigidbody y aplicar velocidad final para continuar la trayectoria
            if (trb != null)
            {
                trb.isKinematic = false;
                
                // Aplicar velocidad final para que continúe cayendo naturalmente
                if (finalVelocity != default)
                {
#if UNITY_6000_0_OR_NEWER
                    trb.linearVelocity = finalVelocity;
#else
                    trb.velocity = finalVelocity;
#endif
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Vector3 center = transform.position;
            Vector3 sz = new Vector3(size.x, 0.02f, size.y);
            Gizmos.color = fillColor;
            Gizmos.DrawCube(center, sz);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(center, sz);
        }
#endif
    }
}
