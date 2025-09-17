using UnityEngine;

public class PlayerLabel : MonoBehaviour
{
    private Camera mainCamera;
    
    [Header("Billboard Settings")]
    [SerializeField] private bool lockY = true; // Mantener el eje Y fijo
    [SerializeField] private bool invertRotation = false; // Invertir la rotación si es necesario

    void Start()
    {
        // Obtener la cámara principal
        mainCamera = Camera.main;
        
        // Si no hay cámara principal, buscar cualquier cámara activa
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Calcular la dirección hacia la cámara
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        
        if (lockY)
        {
            // Mantener el eje Y en 0 para que no se incline hacia arriba/abajo
            directionToCamera.y = 0;
        }

        // Solo rotar si hay una dirección válida
        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera.normalized);
            
            if (invertRotation)
            {
                // Invertir 180 grados si el texto se ve al revés
                targetRotation *= Quaternion.Euler(0, 180, 0);
            }
            
            transform.rotation = targetRotation;
        }
    }
}