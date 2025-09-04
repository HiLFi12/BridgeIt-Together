using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento del auto con soporte para doble carril
/// </summary>
public class AutoMovement : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private Vector3 direccionMovimiento = Vector3.right;
    [SerializeField] private bool usarPhysics = true;
    [SerializeField] private float distanciaDeteccionVehiculo = 3f; // Distancia para detectar otros vehículos
    [SerializeField] private float velocidadReducida = 2f; // Velocidad cuando hay un vehículo cerca
    
    private Vector3 correccionRotacion = Vector3.zero;
    private Rigidbody rb;
    private float velocidadOriginal;
    private bool vehiculoCercaDetectado = false;
      private void Start()
    {
        velocidadOriginal = velocidad;
          if (usarPhysics)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                // Freeze rotation on X and Z axes to prevent rolling/tumbling
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                // Apply constraints to existing Rigidbody
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
        
        CorregirRotacion();
    }    private void Update()
    {
        // Verificar si el juego ha terminado antes de hacer cualquier cosa
        if (GameConditionManager.Instance != null && GameConditionManager.Instance.IsJuegoTerminado())
        {
            return; // No hacer nada si el juego ha terminado
        }
        
        DetectarVehiculosCercanos();
        
        if (!usarPhysics)
        {
            transform.Translate(direccionMovimiento * velocidad * Time.deltaTime, Space.World);
        }
    }
    
    private void DetectarVehiculosCercanos()
    {
        // Detectar vehículos en la misma dirección de movimiento
        Vector3 direccionDeteccion = direccionMovimiento.normalized;
        Ray ray = new Ray(transform.position, direccionDeteccion);
        
        RaycastHit[] hits = Physics.RaycastAll(ray, distanciaDeteccionVehiculo);
        bool vehiculoDetectado = false;
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != this.gameObject && 
                hit.collider.GetComponent<AutoMovement>() != null)
            {
                vehiculoDetectado = true;
                break;
            }
        }
        
        // Ajustar velocidad según detección
        if (vehiculoDetectado && !vehiculoCercaDetectado)
        {
            velocidad = velocidadReducida;
            vehiculoCercaDetectado = true;
        }
        else if (!vehiculoDetectado && vehiculoCercaDetectado)
        {
            velocidad = velocidadOriginal;
            vehiculoCercaDetectado = false;
        }
    }
      private void FixedUpdate()
    {
        // Verificar si el juego ha terminado antes de aplicar física
        if (GameConditionManager.Instance != null && GameConditionManager.Instance.IsJuegoTerminado())
        {
            // Si el juego ha terminado, asegurar que el vehículo esté completamente detenido
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }
        
        if (usarPhysics && rb != null)
        {
            Vector3 targetVelocity = direccionMovimiento * velocidad;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
    }    public void SetVelocidad(float nuevaVelocidad)
    {
        velocidadOriginal = nuevaVelocidad;
        if (!vehiculoCercaDetectado)
        {
            velocidad = nuevaVelocidad;
        }
    }
      public void SetCorreccionRotacion(Vector3 rotacion)
    {
        correccionRotacion = rotacion;
        CorregirRotacion();
    }
    
    /// <summary>
    /// Aplica corrección automática de rotación basada en la dirección de movimiento
    /// </summary>
    public void AplicarCorreccionAutomatica()
    {
        // Lógica específica para AutoDoble
        if (gameObject.name.Contains("AutoDoble"))
        {
            // AutoDoble rota según la dirección de movimiento
            if (direccionMovimiento.x < 0)
            {
                // Moviéndose hacia la izquierda (Solo izquierda): -90 grados
                correccionRotacion = new Vector3(0, -90, 0);
            }
            else
            {
                // Moviéndose hacia la derecha (Solo derecha): 90 grados
                correccionRotacion = new Vector3(0, 90, 0);
            }
        }
        else
        {
            // Auto normal: Si se mueve hacia la izquierda, necesita rotar 180 grados en Y
            if (direccionMovimiento.x < 0)
            {
                correccionRotacion = new Vector3(0, 180, 0);
            }
            else
            {
                correccionRotacion = Vector3.zero;
            }
        }
        
        Debug.Log($"[AutoMovement] AplicarCorreccionAutomatica en {gameObject.name}: dirección={direccionMovimiento}, rotación={correccionRotacion}");
        
        CorregirRotacion();
    }    private void CorregirRotacion()
    {
        Vector3 rotacionCorreccion = correccionRotacion;
        
        // Intentar encontrar el modelo hijo BasicVehicle
        Transform modeloAuto = transform.Find("BasicVehicle");
        
        if (modeloAuto == null)
        {
            // Si no se encuentra BasicVehicle, usar el primer hijo que tenga un MeshRenderer
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform hijo = transform.GetChild(i);
                if (hijo.GetComponent<MeshRenderer>() != null || hijo.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    modeloAuto = hijo;
                    break;
                }
            }
        }
        
        // Si aún no se encuentra, usar el transform raíz
        if (modeloAuto == null)
        {
            modeloAuto = transform;
        }
        
        // Aplicar la rotación local al modelo
        modeloAuto.localRotation = Quaternion.Euler(rotacionCorreccion);
        
        Debug.Log($"[AutoMovement] Aplicando corrección de rotación {rotacionCorreccion} al modelo: {modeloAuto.name} en vehículo {gameObject.name}");
    }
    
public void SetDireccionMovimiento(Vector3 nuevaDireccion)
    {
        direccionMovimiento = nuevaDireccion;
    }
    
    public void SetDireccion(Vector3 nuevaDireccion)
    {
        SetDireccionMovimiento(nuevaDireccion);
    }

    /// <summary>
    /// Resetea completamente el estado del auto cuando vuelve al pool
    /// </summary>
    public void ResetearAuto()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        
        velocidad = velocidadOriginal;
        vehiculoCercaDetectado = false;
        
        // Resetear rotación local del modelo
        CorregirRotacion();
    }
    
    /// <summary>
    /// Función de debugging para verificar el estado del auto
    /// </summary>
    [ContextMenu("Debug Auto State")]
    public void DebugAutoState()
    {
        Debug.Log($"=== ESTADO DEL AUTO {gameObject.name} ===");
        Debug.Log($"Posición: {transform.position}");
        Debug.Log($"Rotación: {transform.rotation.eulerAngles}");
        Debug.Log($"Dirección de movimiento: {direccionMovimiento}");
        Debug.Log($"Velocidad: {velocidad}");
        Debug.Log($"Usar Physics: {usarPhysics}");
        
        if (rb != null)
        {
            Debug.Log($"Rigidbody velocity: {rb.linearVelocity}");
            Debug.Log($"Rigidbody angularVelocity: {rb.angularVelocity}");
            Debug.Log($"Rigidbody constraints: {rb.constraints}");
        }
        
        // Verificar modelo hijo
        Transform modelo = transform.Find("BasicVehicle");
        if (modelo != null)
        {
            Debug.Log($"Modelo BasicVehicle encontrado - Posición local: {modelo.localPosition}, Rotación local: {modelo.localRotation.eulerAngles}");
        }
        else
        {
            Debug.Log("Modelo BasicVehicle NO encontrado");
        }
    }
}