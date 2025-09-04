using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleBridgeCollision : MonoBehaviour
{    [Header("Referencias")]
    public BridgeConstructionGrid bridgeGrid;
      [Header("Configuración")]
    public string bridgeQuadrantTag = "BridgeQuadrant";
    public string vehicleTag = "Vehicle";
    public bool debugMode = true;
    [Header("Control de Colisiones")]
    public float collisionCooldown = 1.0f; // Tiempo en segundos antes de poder impactar el mismo cuadrante otra vez
      // Diccionario para rastrear las colisiones recientes por cuadrante
    private System.Collections.Generic.Dictionary<string, float> recentCollisions = new System.Collections.Generic.Dictionary<string, float>();
    
    private void Update()
    {
        // Limpiar colisiones antiguas cada pocos segundos para evitar memory leaks
        if (Time.time % 5.0f < Time.deltaTime) // Aproximadamente cada 5 segundos
        {
            CleanupOldCollisions();
        }
    }
      private void Start()
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
            
            if (bridgeGrid == null)
            {
                Debug.LogError("No se ha encontrado BridgeConstructionGrid. El vehículo no dañará el puente.");
            }
            else
            {
                Debug.Log("BridgeConstructionGrid encontrado automáticamente: " + bridgeGrid.name);
            }
        }
        else 
        {
            Debug.Log("BridgeConstructionGrid asignado: " + bridgeGrid.name);
        }
        
        bool tagExists = false;
        try 
        {
            GameObject testObj = new GameObject();
            testObj.tag = bridgeQuadrantTag;
            Destroy(testObj);
            tagExists = true;
        }
        catch (UnityException)
        {
            tagExists = false;
        }
          if (!tagExists)
        {
            Debug.LogError("El tag '" + bridgeQuadrantTag + "' no existe en el proyecto. Las colisiones con el puente no funcionarán.");
        }
        else
        {
            if (debugMode) Debug.Log("Tag '" + bridgeQuadrantTag + "' verificado correctamente.");
        }
        
        // Verificar que el tag de vehículo existe
        bool vehicleTagExists = false;
        try 
        {
            GameObject testVehicleObj = new GameObject();
            testVehicleObj.tag = vehicleTag;
            Destroy(testVehicleObj);
            vehicleTagExists = true;
        }
        catch (UnityException)
        {
            vehicleTagExists = false;
        }
        
        if (!vehicleTagExists)
        {
            Debug.LogError("El tag '" + vehicleTag + "' no existe en el proyecto. Solo los objetos con este tag podrán dañar el puente.");
        }        else
        {
            if (debugMode) Debug.Log("Tag '" + vehicleTag + "' verificado correctamente.");
        }
    }
      /// <summary>
    /// Limpia las colisiones antiguas del diccionario para evitar memory leaks
    /// </summary>
    private void CleanupOldCollisions()
    {
        float currentTime = Time.time;
        var keysToRemove = new System.Collections.Generic.List<string>();
        
        foreach (var kvp in recentCollisions)
        {
            if (currentTime - kvp.Value > collisionCooldown * 2) // Limpiar colisiones que son el doble del tiempo de cooldown
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (string key in keysToRemove)
        {
            recentCollisions.Remove(key);
        }
        
        if (debugMode && keysToRemove.Count > 0)
        {
            Debug.Log($"Limpiadas {keysToRemove.Count} colisiones antiguas del registro.");
        }
    }
      /// <summary>
    /// Verifica si una colisión con un cuadrante específico es válida (no está en cooldown)
    /// </summary>
    private bool IsCollisionValid(int x, int z)
    {
        string quadrantKey = $"{x}_{z}";
        float currentTime = Time.time;
        
        if (recentCollisions.ContainsKey(quadrantKey))
        {
            float timeSinceLastCollision = currentTime - recentCollisions[quadrantKey];
            if (timeSinceLastCollision < collisionCooldown)
            {
                if (debugMode) Debug.Log($"Colisión con cuadrante [{x},{z}] ignorada. Cooldown activo. Tiempo restante: {collisionCooldown - timeSinceLastCollision:F2}s");
                return false;
            }
        }
        
        // Registrar esta colisión
        recentCollisions[quadrantKey] = currentTime;
        if (debugMode) Debug.Log($"Colisión con cuadrante [{x},{z}] registrada. Próxima colisión permitida en {collisionCooldown}s");
        return true;
    }
    
    /// <summary>
    /// Verifica si este objeto o alguno de sus padres tiene el tag de vehículo
    /// </summary>
    private bool IsVehicleOrChildOfVehicle()
    {
        // Primero verificar el objeto actual
        if (gameObject.CompareTag(vehicleTag))
        {
            if (debugMode) Debug.Log("Este objeto " + gameObject.name + " es un vehículo.");
            return true;
        }
        
        // Buscar en los padres
        Transform currentParent = transform.parent;
        while (currentParent != null)
        {
            if (currentParent.CompareTag(vehicleTag))
            {
                if (debugMode) Debug.Log("Objeto padre " + currentParent.name + " es un vehículo.");
                return true;
            }
            currentParent = currentParent.parent;
        }
        
        if (debugMode) Debug.Log("Este objeto " + gameObject.name + " no es un vehículo ni hijo de uno.");
        return false;
    }
    
    /// <summary>
    /// Procesa el impacto del vehículo en un cuadrante específico si la colisión es válida
    /// </summary>
    private void ProcessVehicleImpact(int x, int z)
    {
        if (x >= 0 && x < bridgeGrid.gridWidth && z >= 0 && z < bridgeGrid.gridLength)
        {
            if (IsCollisionValid(x, z))
            {
                bridgeGrid.OnVehicleImpact(x, z);
                Debug.Log($"Vehículo impactó cuadrante {x},{z}");

                // Intentar ejecutar una acción probabilística si el vehículo (o un componente en sus padres)
                // posee un componente que herede de ProbabilityAction
                ProbabilityAction prob = GetComponentInParent<ProbabilityAction>();
                if (prob != null)
                {
                    if (debugMode) Debug.Log($"ProbabilityAction encontrado en {prob.gameObject.name}. Intentando ejecutar con probabilidad {prob.probability}.");
                    prob.TryExecuteOnQuadrant(x, z);
                }
            }
        }
        else
        {
            if (debugMode) Debug.LogWarning("Cuadrante [" + x + "," + z + "] fuera de los límites de la grilla.");
        }
    }  
    
    /// <summary>
    /// Lógica de procesamiento de colisiones extraída para reutilización
    /// </summary>
    private void ProcessCollisionLogic(Collision collision)
    {
        if (debugMode) Debug.Log("Colisión detectada con: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        if (bridgeGrid == null)
        {
            if (debugMode) Debug.LogWarning("BridgeGrid nulo. Saltando colisión.");
            return;
        }
        
        // Verificar que ESTE objeto O su padre es un vehículo
        if (!IsVehicleOrChildOfVehicle())
        {
            if (debugMode) Debug.Log("Este objeto " + gameObject.name + " no es un vehículo ni hijo de uno. No se aplicará daño.");
            return;
        }
          // Verificar que el objeto con el que colisionamos es un cuadrante del puente
        if (collision.gameObject.CompareTag(bridgeQuadrantTag))
        {
            if (debugMode) Debug.Log("Vehículo colisionó con un cuadrante del puente: " + collision.gameObject.name);
            
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 localPos = hitPoint - bridgeGrid.transform.position;
            
            int x = Mathf.FloorToInt(localPos.x / bridgeGrid.quadrantSize);
            int z = Mathf.FloorToInt(localPos.z / bridgeGrid.quadrantSize);
            
            if (debugMode) Debug.Log("Punto de impacto: " + hitPoint + " -> Cuadrante: [" + x + "," + z + "]");
            
            ProcessVehicleImpact(x, z);
        }
    }    private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other);
    }
    
    /// <summary>
    /// Lógica de procesamiento de triggers extraída para reutilización
    /// </summary>
    private void ProcessTriggerLogic(Collider other)
    {
        if (debugMode) Debug.Log("Trigger detectado con: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");
        
        if (bridgeGrid == null)
        {
            if (debugMode) Debug.LogWarning("BridgeGrid nulo. Saltando trigger.");
            return;
        }
        
        // Verificar que ESTE objeto O su padre es un vehículo
        if (!IsVehicleOrChildOfVehicle())
        {
            if (debugMode) Debug.Log("Este objeto " + gameObject.name + " no es un vehículo ni hijo de uno. No se aplicará daño.");
            return;
        }
        
        GameObject targetObject = other.gameObject;
        string targetName = targetObject.name;
        
        if (targetName.StartsWith("Layer_"))
        {
            if (debugMode) Debug.Log("Colisión con capa de puente: " + targetName);
            
            Transform parent = targetObject.transform.parent;
            if (parent != null && parent.name.StartsWith("Quadrant_"))
            {
                string parentName = parent.name;
                if (debugMode) Debug.Log("Cuadrante padre: " + parentName);
                
                string[] parts = parentName.Split('_');
                if (parts.Length == 3)
                {                    if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int z))
                    {
                        if (debugMode) Debug.Log("Procesando colisión con cuadrante [" + x + "," + z + "]");
                        ProcessVehicleImpact(x, z);
                        return;
                    }
                }
            }
        }
            
        if (other.CompareTag(bridgeQuadrantTag))
        {
            if (debugMode) Debug.Log("Trigger con un cuadrante del puente confirmado.");
            
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 localPos = hitPoint - bridgeGrid.transform.position;
            
            int x = Mathf.FloorToInt(localPos.x / bridgeGrid.quadrantSize);
            int z = Mathf.FloorToInt(localPos.z / bridgeGrid.quadrantSize);
              if (debugMode) Debug.Log("Punto de trigger: " + hitPoint + " -> Cuadrante: [" + x + "," + z + "]");
            
            ProcessVehicleImpact(x, z);
        }
    }
    
    /// <summary>
    /// Método estático para que los objetos hijos puedan reportar colisiones al padre vehículo
    /// </summary>
    public static void HandleCollisionFromChild(GameObject childObject, Collision collision)
    {
        VehicleBridgeCollision vehicleScript = FindVehicleScriptInParents(childObject);
        if (vehicleScript != null)
        {
            vehicleScript.HandleCollision(collision);
        }
    }
    
    /// <summary>
    /// Método estático para que los objetos hijos puedan reportar triggers al padre vehículo
    /// </summary>
    public static void HandleTriggerFromChild(GameObject childObject, Collider other)
    {
        VehicleBridgeCollision vehicleScript = FindVehicleScriptInParents(childObject);
        if (vehicleScript != null)
        {
            vehicleScript.HandleTrigger(other);
        }
    }
    
    /// <summary>
    /// Busca el componente VehicleBridgeCollision en el objeto o sus padres
    /// </summary>
    private static VehicleBridgeCollision FindVehicleScriptInParents(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            VehicleBridgeCollision script = current.GetComponent<VehicleBridgeCollision>();
            if (script != null)
            {
                return script;
            }
            current = current.parent;
        }
        return null;
    }
    
    /// <summary>
    /// Maneja la colisión (puede ser llamado desde el método OnCollisionEnter o desde un hijo)
    /// </summary>
    public void HandleCollision(Collision collision)
    {
        if (debugMode) Debug.Log("Colisión procesada por VehicleBridgeCollision en: " + gameObject.name);
        ProcessCollisionLogic(collision);
    }
    
    /// <summary>
    /// Maneja el trigger (puede ser llamado desde el método OnTriggerEnter o desde un hijo)
    /// </summary>
    public void HandleTrigger(Collider other)
    {
        if (debugMode) Debug.Log("Trigger procesado por VehicleBridgeCollision en: " + gameObject.name);
        ProcessTriggerLogic(other);
    }
}