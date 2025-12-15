using System.Collections;
using UnityEngine;

namespace Gameplay.AutoControllers
{
    [DisallowMultipleComponent]
    public class VehicleRotator : MonoBehaviour
    {
        [Header("Detección de Suelo")]
        [Tooltip("Radio de la esfera para detectar el suelo")]
        [SerializeField] private float detectionRadius = 0.5f;
        
        [Tooltip("Offset desde el centro del objeto para la detección")]
        [SerializeField] private Vector3 detectionOffset = Vector3.down * 0.5f;
        
        [Tooltip("LayerMask que representa el suelo")]
        [SerializeField] private LayerMask groundLayerMask;
        
        [Header("Rotación en Caída")]
        [Tooltip("Grados a rotar en el eje X cuando pierde contacto con el suelo")]
        [SerializeField] private float rotationDegrees = 90f;
        
        [Tooltip("Velocidad de rotación (grados por segundo)")]
        [SerializeField] private float rotationSpeed = 180f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        
        // Estado
        private bool isGrounded;
        private bool isRotating;
        private float targetRotation;
        private float currentXRotation;
        private Collider[] detectionBuffer = new Collider[5];

        private void Awake()
        {
            currentXRotation = transform.rotation.eulerAngles.x;
        }

        private void FixedUpdate()
        {
            CheckGroundStatus();
        }

        private void CheckGroundStatus()
        {
            Vector3 sphereCenter = transform.position + detectionOffset;
            int hits = Physics.OverlapSphereNonAlloc(sphereCenter, detectionRadius, detectionBuffer, groundLayerMask);

            bool wasGrounded = isGrounded;
            isGrounded = hits > 0;

            // Si estaba en el suelo y dejó de estarlo, iniciar rotación
            if (wasGrounded && !isGrounded && !isRotating)
            {
                StartRotation();
            }
        }

        private void StartRotation()
        {
            targetRotation = currentXRotation + rotationDegrees;
            isRotating = true;
            StartCoroutine(RotateCoroutine());
        }

        private IEnumerator RotateCoroutine()
        {
            float startRotation = currentXRotation;
            float rotationProgress = 0f;

            while (rotationProgress < 1f)
            {
                rotationProgress += (rotationSpeed / Mathf.Abs(rotationDegrees)) * Time.deltaTime;
                rotationProgress = Mathf.Clamp01(rotationProgress);

                currentXRotation = Mathf.Lerp(startRotation, targetRotation, rotationProgress);
                
                // Aplicar la rotación manteniendo Y y Z actuales
                Vector3 currentEuler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(currentXRotation, currentEuler.y, currentEuler.z);

                yield return null;
            }

            // Asegurar que llegamos exactamente al target
            currentXRotation = targetRotation;
            Vector3 finalEuler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentXRotation, finalEuler.y, finalEuler.z);

            isRotating = false;
        }

        public bool IsGrounded() => isGrounded;
        public bool IsRotating() => isRotating;

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Vector3 sphereCenter = transform.position + detectionOffset;
            
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(sphereCenter, detectionRadius);
        }
    }
}

