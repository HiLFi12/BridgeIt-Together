using UnityEngine;

/// <summary>
/// Componente que se agrega a cada trigger individual para detectar vehículos
/// Los vehículos se envían al pool, otros objetos se destruyen
/// </summary>
public class VehicleReturnTrigger : MonoBehaviour
{
    [Header("Configuración del Trigger")]
    [SerializeField] private bool destroyNonVehicles = false;
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Tags Protegidos (No se destruirán)")]
    [SerializeField] private string[] additionalProtectedTags = new string[0];
    
    private VehicleReturnTriggerManager manager;
    private bool isActive = true;
    private Collider triggerCollider;
    
    /// <summary>
    /// Inicializa el trigger con referencia al manager
    /// </summary>
    /// <param name="triggerManager">Manager que maneja este trigger</param>
    public void Initialize(VehicleReturnTriggerManager triggerManager)
    {
        manager = triggerManager;
        triggerCollider = GetComponent<Collider>();
        
        // Asegurar que es un trigger
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
    
    /// <summary>
    /// Activa o desactiva este trigger
    /// </summary>
    /// <param name="active">Estado activo/inactivo</param>
    public void SetActive(bool active)
    {
        isActive = active;
        
        // También controlar el collider
        if (triggerCollider != null)
        {
            triggerCollider.enabled = active;
        }
    }
      /// <summary>
    /// Verifica si el trigger está activo
    /// </summary>
    /// <returns>True si está activo</returns>
    public bool IsActive()
    {
        return isActive && triggerCollider != null && triggerCollider.enabled;
    }
    
    /// <summary>
    /// Activa o desactiva la destrucción de objetos no-vehículos
    /// </summary>
    /// <param name="enabled">True para activar destrucción, False para desactivar</param>
    public void SetDestroyNonVehicles(bool enabled)
    {
        destroyNonVehicles = enabled;
        if (showDebugMessages)
        {
            Debug.Log($"Destrucción de no-vehículos {(enabled ? "activada" : "desactivada")} en trigger {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Activa o desactiva los mensajes de debug
    /// </summary>
    /// <param name="enabled">True para mostrar mensajes, False para ocultarlos</param>
    public void SetDebugMessages(bool enabled)
    {
        showDebugMessages = enabled;
    }
    
    /// <summary>
    /// Añade tags adicionales que deben estar protegidos de la destrucción
    /// </summary>
    /// <param name="newProtectedTags">Array de tags a proteger</param>
    public void AddProtectedTags(string[] newProtectedTags)
    {
        if (newProtectedTags == null || newProtectedTags.Length == 0) return;
        
        // Combinar tags existentes con los nuevos
        System.Collections.Generic.List<string> allTags = new System.Collections.Generic.List<string>();
        
        if (additionalProtectedTags != null)
        {
            allTags.AddRange(additionalProtectedTags);
        }
        
        foreach (string tag in newProtectedTags)
        {
            if (!string.IsNullOrEmpty(tag) && !allTags.Contains(tag))
            {
                allTags.Add(tag);
            }
        }
        
        additionalProtectedTags = allTags.ToArray();
        
        if (showDebugMessages)
        {
            Debug.Log($"Tags protegidos actualizados en trigger {gameObject.name}: {string.Join(", ", additionalProtectedTags)}");
        }
    }private void OnTriggerEnter(Collider other)
    {
        // Solo procesar si el trigger está activo
        if (!isActive || manager == null) return;
        
        // Verificar si el objeto que entró es un vehículo
        GameObject targetObject = other.gameObject;
        
    // Buscar componentes de vehículo en el objeto o en su padre
    AutoMovement autoMovement = targetObject.GetComponent<AutoMovement>();
    VehicleBridgeCollision vehicleCollision = targetObject.GetComponent<VehicleBridgeCollision>();
    BridgeItTogether.Gameplay.AutoControllers.AutoController autoController = targetObject.GetComponent<BridgeItTogether.Gameplay.AutoControllers.AutoController>();
        
        // Si no tiene los componentes en sí mismo, buscar en el padre
        if (autoMovement == null && vehicleCollision == null && autoController == null)
        {
            autoMovement = targetObject.GetComponentInParent<AutoMovement>();
            vehicleCollision = targetObject.GetComponentInParent<VehicleBridgeCollision>();
            autoController = targetObject.GetComponentInParent<BridgeItTogether.Gameplay.AutoControllers.AutoController>();
            
            // Si encontramos los componentes en el padre, usar el GameObject padre
            if (autoMovement != null || vehicleCollision != null || autoController != null)
            {
                if (autoController != null) targetObject = autoController.gameObject;
                else targetObject = autoMovement != null ? autoMovement.gameObject : vehicleCollision.gameObject;
            }
        }
        
        // Si tiene componentes de vehículo, enviar al pool
        if (autoMovement != null || vehicleCollision != null || autoController != null)
        {
            manager.OnVehicleTriggered(targetObject, triggerCollider);
        }        else
        {
            // Intento extra: si el collider pertenece a un Rigidbody vinculado a un AutoController, tratarlo como vehículo
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                var ac = rb.GetComponent<BridgeItTogether.Gameplay.AutoControllers.AutoController>()
                         ?? rb.GetComponentInParent<BridgeItTogether.Gameplay.AutoControllers.AutoController>();
                if (ac != null)
                {
                    manager.OnVehicleTriggered(ac.gameObject, triggerCollider);
                    return;
                }
            }

            // Si NO es un vehículo, destruir el objeto (si está habilitado)
            // Por defecto no destruimos para evitar eliminar partes de vehículos u objetos compartidos
            if (destroyNonVehicles)
            {
                DestroyNonVehicleObject(other.gameObject);
            }
            else
            {
                // fallback: desactivar objeto si es temporal, pero nunca si es hijo de un vehículo
                if (!EsParteDeVehiculo(other.gameObject))
                {
                    other.gameObject.SetActive(false);
                    if (showDebugMessages) Debug.Log($"⏸️ Objeto no-vehículo desactivado: {other.gameObject.name}");
                }
                else if (showDebugMessages)
                {
                    Debug.Log($"⚠️ No se desactiva por ser parte de un vehículo: {other.gameObject.name}");
                }
            }
        }
    }
      /// <summary>
    /// Destruye objetos que no son vehículos cuando colisionan con el trigger
    /// </summary>
    /// <param name="obj">El objeto a destruir</param>
    private void DestroyNonVehicleObject(GameObject obj)
    {
        // Verificar que no sea un jugador u otros objetos importantes que no deben destruirse
        if (ShouldDestroyObject(obj))
        {
            if (showDebugMessages)
                Debug.Log($"🗑️ Destruyendo objeto no-vehículo: {obj.name} (Tag: {obj.tag})");
            
            Destroy(obj);
        }
        else
        {
            if (showDebugMessages)
                Debug.Log($"⚠️ Objeto {obj.name} (Tag: {obj.tag}) protegido - no se destruye");
        }
    }
    
    /// <summary>
    /// Determina si un objeto debe ser destruido o no
    /// </summary>
    /// <param name="obj">El objeto a evaluar</param>
    /// <returns>True si debe destruirse, False si debe ignorarse</returns>
    private bool ShouldDestroyObject(GameObject obj)
    {
    if (EsParteDeVehiculo(obj)) return false;
        

        // Lista de tags que NO deben destruirse (tags básicos del sistema)
        string[] defaultProtectedTags = { 
            "Player", 
            "MainCamera", 
            "GameController",
            "UI",
            "BridgeQuadrant",
            "Ground",
            "Platform",
            "Respawn",
            "Finish",
            "EditorOnly"
        };
        
        // Verificar tags protegidos por defecto
        foreach (string protectedTag in defaultProtectedTags)
        {
            if (obj.CompareTag(protectedTag))
            {
                return false;
            }
        }
        
        // Verificar tags adicionales configurados en el inspector
        if (additionalProtectedTags != null)
        {
            foreach (string protectedTag in additionalProtectedTags)
            {
                if (!string.IsNullOrEmpty(protectedTag) && obj.CompareTag(protectedTag))
                {
                    return false;
                }
            }
        }
        
        // No destruir objetos que tengan componentes importantes del sistema
        if (obj.GetComponent<Camera>() != null ||
            obj.GetComponent<Light>() != null ||
            obj.GetComponent<AudioListener>() != null ||
            obj.GetComponentInParent<Canvas>() != null ||
            obj.GetComponent<VehicleReturnTrigger>() != null ||
            obj.GetComponent<VehicleReturnTriggerManager>() != null)
        {
            return false;
        }
        
        // No destruir objetos que sean parte del sistema de puentes
        if (obj.GetComponent<BridgeConstructionGrid>() != null ||
            obj.GetComponentInChildren<BridgeConstructionGrid>() != null ||
            obj.GetComponentInParent<BridgeConstructionGrid>() != null)
        {
            return false;
        }
        
        // Si llegamos aquí, es seguro destruir el objeto
        return true;
    }

    private bool EsParteDeVehiculo(GameObject obj)
    {
        if (obj.GetComponentInParent<BridgeItTogether.Gameplay.AutoControllers.AutoController>() != null)
            return true;
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            var ac = rb.GetComponent<BridgeItTogether.Gameplay.AutoControllers.AutoController>()
                     ?? rb.GetComponentInParent<BridgeItTogether.Gameplay.AutoControllers.AutoController>();
            if (ac != null) return true;
        }
        return false;
    }
    
    // Método opcional para debugging - mostrar el trigger en la escena
    private void OnDrawGizmos()
    {
        if (triggerCollider != null && isActive)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = isActive ? Color.green : Color.gray;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}
