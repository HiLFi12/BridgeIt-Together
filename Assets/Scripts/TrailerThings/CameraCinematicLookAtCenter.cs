using UnityEngine;

public class CameraCinematicLookAtCenter : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform centerPoint;

    [Header("Movimiento alrededor del centro")]
    [SerializeField] private float swayAmplitude = 0.5f; // radio del movimiento leve
    [SerializeField] private float swaySpeed = 0.5f;     // velocidad del movimiento

    [Header("Rotación hacia el centro")]
    [SerializeField] private float lookLerpSpeed = 2f;   // qué tan rápido rota hacia el centro

    private Vector3 baseOffset;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (centerPoint == null && targetCamera != null)
        {
            // Si no se asigna un punto, usar el origen como referencia
            GameObject go = new GameObject("CameraCenterPoint");
            go.transform.position = Vector3.zero;
            centerPoint = go.transform;
        }

        if (targetCamera != null && centerPoint != null)
        {
            baseOffset = targetCamera.transform.position - centerPoint.position;
        }
    }

    private void Update()
    {
        if (targetCamera == null || centerPoint == null) return;

        // Movimiento leve (sway) alrededor del centro en un plano horizontal
        float t = Time.time * swaySpeed;
        Vector3 swayOffset = new Vector3(Mathf.Sin(t), 0f, Mathf.Cos(t)) * swayAmplitude;

        // Posición final de la cámara
        Vector3 desiredPosition = centerPoint.position + baseOffset + swayOffset;
        targetCamera.transform.position = desiredPosition;

        // Rotar suavemente para mirar siempre al punto central
        Vector3 dirToCenter = centerPoint.position - targetCamera.transform.position;
        if (dirToCenter.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToCenter.normalized, Vector3.up);
            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                targetRot,
                lookLerpSpeed * Time.deltaTime
            );
        }
    }
}
