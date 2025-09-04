using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerObjectHolder))]
public class PlayerBridgeInteraction : MonoBehaviour
{
    [Header("Referencias")]
    public BridgeConstructionGrid bridgeGrid;
    public Transform buildPoint;
    
    [Header("Configuración")]
    public float interactionRange = 2f;
    public LayerMask bridgeLayer;
    
    // Referencias internas
    private PlayerObjectHolder objectHolder;
    
    // Cache para la construcción
    private int targetX = -1;
    private int targetZ = -1;
    private int currentLayerIndex = 0;
    
    private void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        
        if (bridgeGrid == null)
        {
            // Intentar encontrar la rejilla de puente en la escena
            bridgeGrid = Object.FindFirstObjectByType<BridgeConstructionGrid>();
            
            if (bridgeGrid == null)
            {
                Debug.LogError($"¡No se ha encontrado BridgeConstructionGrid! El jugador {gameObject.name} no podrá interactuar con el puente.");
            }
        }
        
        if (buildPoint == null)
        {
            Debug.LogError($"¡El punto de construcción (buildPoint) no está asignado en {gameObject.name}! Crea un Transform vacío como hijo del jugador y asígnalo.");
        }
    }
    
    // Esta función debe ser llamada desde el sistema de interacción del jugador
    public void TryBuildBridge()
    {
        if (bridgeGrid == null)
        {
            Debug.LogWarning($"No hay una rejilla de puente asignada al jugador {gameObject.name}.");
            return;
        }
        
        if (objectHolder == null)
        {
            Debug.LogError($"No se encuentra el componente PlayerObjectHolder en {gameObject.name}.");
            return;
        }
        
        if (!objectHolder.HasObjectInHand())
        {
            Debug.Log($"El jugador {gameObject.name} no tiene ningún objeto en la mano para construir.");
            return;
        }
        
        // Encontrar cuadrante más cercano
        Vector3 buildPos = buildPoint != null ? buildPoint.position : transform.position + transform.forward;
        bool foundTarget = FindTargetQuadrant(buildPos);
        
        if (foundTarget)
        {
            Debug.Log($"Cuadrante objetivo encontrado en [{targetX},{targetZ}], intentando construir");            // Intentar construir
            GameObject objectInHand = objectHolder.GetHeldObject();
              if (objectInHand != null)
            {
                BridgeMaterialInfo materialInfo = objectInHand.GetComponent<BridgeMaterialInfo>();
                
                if (materialInfo != null && materialInfo.materialType == BridgeQuadrantSO.MaterialType.Adoquin)
                {
                    BridgeQuadrantSO targetQuadrant = bridgeGrid.GetQuadrantSO(targetX, targetZ);
                    //BridgeQuadrantSO targetQuadrant = bridgeGrid.GetQuadrantSO(targetX, targetZ);
                      if (targetQuadrant != null && targetQuadrant.IsDamaged())
                    {
                        bool repairSuccess = TryRepairQuadrant(targetQuadrant, objectInHand);
                        
                        if (repairSuccess)
                        {
                            objectHolder.UseHeldObject();
                            Debug.Log("Cuadrante reparado exitosamente con adoquín.");
                        }
                        else
                        {
                            Debug.LogWarning("No se pudo reparar el cuadrante con este material.");
                        }
                        
                        return; // Salir temprano, ya manejamos la reparación
                    }                }
                
                // Si llegamos aquí, proceder con construcción normal
                // Si el material tiene información, usar su layerIndex
                int materialLayerIndex = 0; // Por defecto, capa base
                
                if (materialInfo != null)
                {
                    materialLayerIndex = materialInfo.layerIndex;
                    Debug.Log($"Usando layerIndex {materialLayerIndex} del material {objectInHand.name}");
                }
                else
                {
                    // Verificar si el tag del objeto indica su tipo
                    if (objectInHand.tag.StartsWith("BridgeLayer"))
                    {
                        string layerStr = objectInHand.tag.Substring(11); // Obtener el número después de "BridgeLayer"
                        if (int.TryParse(layerStr, out int layer))
                        {
                            materialLayerIndex = layer;
                            Debug.Log($"Usando layerIndex {materialLayerIndex} del tag {objectInHand.tag}");
                        }
                    }
                }
                
                // CRÍTICO: Siempre obtener el índice de capa correcto basado en el estado del cuadrante
                int correctLayerIndex = GetNextCorrectLayerIndex(targetX, targetZ);
                
                // Si recibimos -1, significa que no hay más capas para construir en este cuadrante
                if (correctLayerIndex == -1)
                {
                    Debug.LogWarning($"El cuadrante [{targetX},{targetZ}] ya tiene todas sus capas completas. No se puede construir más.");
                    return;
                }
                
                // Si el material no coincide con la capa que se espera construir, mostrar error
                if (materialLayerIndex != correctLayerIndex)
                {
                    Debug.LogError($"ERROR DE TIPO: El material es para la capa {materialLayerIndex}, pero la siguiente capa a construir es {correctLayerIndex}");
                    return;
                }
                
                // Forzar que siempre usemos el índice correcto, que debe coincidir con el tipo de material
                Debug.Log($"Índice de capa determinado automaticamente: {correctLayerIndex}, Material correcto: {materialLayerIndex == correctLayerIndex}");
                
                // Intentar construir usando el índice correcto
                bool success = bridgeGrid.TryBuildLayer(targetX, targetZ, correctLayerIndex, objectInHand);
                
                if (success)
                {
                    // El objeto se utilizó, indicar al holder que ya no tiene nada
                    objectHolder.UseHeldObject();
                    
                    // Actualizar el índice de capa para la próxima construcción
                    currentLayerIndex = GetNextCorrectLayerIndex(targetX, targetZ);
                    Debug.Log($"Construcción exitosa. Siguiente capa a construir: {currentLayerIndex}");
                }
                else
                {
                    Debug.LogWarning($"No se pudo construir en el cuadrante [{targetX},{targetZ}] con capa {correctLayerIndex}");
                }
            }
            else
            {
                Debug.LogWarning("GetHeldObject devolvió null a pesar de que HasObjectInHand es true.");
            }
        }
        else
        {
            Debug.Log($"No se encontró un cuadrante objetivo dentro del rango {interactionRange}.");
        }
    }
    
    /// <summary>
    /// Intenta reparar un cuadrante dañado usando MaterialTipo4 (adoquín)
    /// </summary>
    /// <param name="quadrant">El cuadrante a reparar</param>
    /// <param name="materialObject">El objeto material que se está usando</param>
    /// <returns>True si la reparación fue exitosa</returns>
    private bool TryRepairQuadrant(BridgeQuadrantSO quadrant, GameObject materialObject)
    {
        if (quadrant == null)
        {
            Debug.LogError("Cuadrante nulo - no se puede reparar.");
            return false;
        }
        
        if (materialObject == null)
        {
            Debug.LogError("Material nulo - no se puede reparar.");
            return false;
        }
        
        // Verificar si el cuadrante realmente está dañado
        if (!quadrant.IsDamaged())
        {
            Debug.Log("Este cuadrante no está dañado - no necesita reparación.");
            return false;
        }
        
        // Verificar que el material sea del tipo correcto (adoquín/MaterialTipo4)
        BridgeMaterialInfo materialInfo = materialObject.GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null || materialInfo.materialType != BridgeQuadrantSO.MaterialType.Adoquin)
        {
            Debug.LogWarning("Este material no puede reparar cuadrantes de puente. Se necesita adoquín.");
            return false;
        }
        
        // Intentar reparar usando TryAddLayer del ScriptableObject
        // La reparación cuenta como agregar MaterialTipo4 a la última capa dañada
        bool reparacionExitosa = quadrant.TryAddLayer(BridgeQuadrantSO.MaterialType.Adoquin, 1);
        
        if (reparacionExitosa)
        {
            Debug.Log("Cuadrante reparado exitosamente con adoquín.");
            return true;
        }
        else
        {
            Debug.LogWarning("No se pudo reparar el cuadrante. Puede que no necesite este tipo de material.");
            return false;
        }
    }

    // Método mejorado para obtener el siguiente índice de capa correcto basado en el estado del cuadrante
    private int GetNextCorrectLayerIndex(int x, int z)
    {
        if (!bridgeGrid.IsValidQuadrant(x, z))
            return 0;
        
        // Busca la primera capa no completada
        BridgeQuadrantSO quadrantSO = GetQuadrantSO(x, z);
        if (quadrantSO == null)
        {
            Debug.LogError("No se pudo obtener el ScriptableObject del cuadrante.");
            return 0;
        }
            
        // CRÍTICO: Primero verificar si el cuadrante está vacío (ninguna capa completada)
        bool cuadranteVacio = true;
        for (int i = 0; i < quadrantSO.requiredLayers.Length; i++)
        {
            if (quadrantSO.requiredLayers[i].isCompleted)
            {
                cuadranteVacio = false;
                break;
            }
        }
        
        // Si el cuadrante está vacío, siempre empezar con la capa 0
        if (cuadranteVacio)
        {
            Debug.Log("Cuadrante vacío detectado. Forzando construcción de capa 0.");
            return 0;
        }
        
        // Buscar la primera capa no completada para cuadrantes parcialmente construidos
        for (int i = 0; i < quadrantSO.requiredLayers.Length; i++)
        {
            if (!quadrantSO.requiredLayers[i].isCompleted)
            {
                Debug.Log($"Primera capa incompleta: {i}");
                return i;
            }
        }
        
        // Si todas las capas están completas, no hay más capas para construir
        Debug.Log("Todas las capas están completas. No se puede construir más en este cuadrante.");
        return -1; // Valor especial para indicar que no hay más capas para construir
    }
    
    // Método auxiliar para obtener el SO del cuadrante
    private BridgeQuadrantSO GetQuadrantSO(int x, int z)
    {
        // Usa reflexión para acceder al campo private constructionGrid
        System.Type gridType = bridgeGrid.GetType();
        System.Reflection.FieldInfo gridField = gridType.GetField("constructionGrid", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (gridField == null)
            return null;
            
        object grid = gridField.GetValue(bridgeGrid);
        if (grid == null)
            return null;
            
        // Acceder al elemento [x,z]
        System.Type elementType = grid.GetType().GetElementType();
        object element = ((System.Array)grid).GetValue(x, z);
        if (element == null)
            return null;
            
        // Obtener el campo quadrantSO
        System.Reflection.FieldInfo soField = elementType.GetField("quadrantSO");
        if (soField == null)
            return null;
            
        return soField.GetValue(element) as BridgeQuadrantSO;
    }
    
    // Para otros efectos específicos de era
    public void TryInteractWithQuadrant()
    {
        if (bridgeGrid == null)
        {
            Debug.LogWarning("No hay una rejilla de puente asignada.");
            return;
        }
        
        Debug.Log($"Jugador {gameObject.name} intentando interactuar con el puente...");
            
        // Encontrar cuadrante más cercano
        Vector3 buildPos = buildPoint != null ? buildPoint.position : transform.position + transform.forward;
        bool foundTarget = FindTargetQuadrant(buildPos);
        
        if (foundTarget)
        {
            Debug.Log($"Cuadrante objetivo encontrado en {targetX},{targetZ}");
            
            // Verificar tipo de interacción según el objeto que tenga el jugador
            GameObject heldObject = objectHolder.GetHeldObject();
            
            if (heldObject != null)
            {
                Debug.Log($"Objeto sostenido: {heldObject.name}");
                
                // Ejemplos de interacciones específicas de era
                // Esto debería refinarse con un sistema más robusto de tipos de objetos
                string objectType = heldObject.tag;
                
                switch (objectType)
                {
                    case "Heater":
                        // Para la era industrial
                        bridgeGrid.ApplyHeat(targetX, targetZ);
                        Debug.Log("Aplicando calor al cuadrante");
                        break;
                    case "Battery":
                        // Para la era futurista
                        bridgeGrid.ReplaceBattery(targetX, targetZ);
                        objectHolder.UseHeldObject(); // Consimir la batería
                        Debug.Log("Reemplazando batería del cuadrante");
                        break;
                    default:
                        // Intento de construcción normal
                        Debug.Log("Intento de construcción normal con objeto genérico");
                        TryBuildBridge();
                        break;
                }
            }
            else
            {
                Debug.Log("No hay objeto en la mano. Solo se puede interactuar con el puente teniendo un objeto.");
            }
        }
        else
        {
            Debug.Log($"No se encontró un cuadrante objetivo dentro del rango {interactionRange}.");
        }
    }
    
    // Buscar el cuadrante más cercano en rango
    private bool FindTargetQuadrant(Vector3 position)
    {
        // Intentamos hacer un raycast hacia abajo para detectar el cuadrante
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, interactionRange, bridgeLayer))
        {
            // Calcular la posición en la grilla (esto asume que la grilla está alineada con los ejes globales)
            Vector3 localPos = hit.point - bridgeGrid.transform.position;
            
            int x = Mathf.FloorToInt(localPos.x / bridgeGrid.quadrantSize);
            int z = Mathf.FloorToInt(localPos.z / bridgeGrid.quadrantSize);
            
            // Verificar validez
            if (x >= 0 && x < bridgeGrid.gridWidth && z >= 0 && z < bridgeGrid.gridLength)
            {
                targetX = x;
                targetZ = z;
                return true;
            }
        }
        
        // Alternativamente, buscar el cuadrante más cercano si el raycast falla
        float minDistance = float.MaxValue;
        bool found = false;
        
        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                Vector3 quadrantPos = bridgeGrid.transform.position + 
                                      new Vector3(x * bridgeGrid.quadrantSize, 0, z * bridgeGrid.quadrantSize);
                
                float distance = Vector3.Distance(position, quadrantPos);
                
                if (distance < interactionRange && distance < minDistance)
                {
                    minDistance = distance;
                    targetX = x;
                    targetZ = z;
                    found = true;
                }
            }
        }
        
        return found;
    }
    
    // Posibilidad de mostrar visualmente el cuadrante objetivo
    private void OnDrawGizmos()
    {
        if (bridgeGrid != null && targetX >= 0 && targetZ >= 0)
        {
            Vector3 targetPos = bridgeGrid.transform.position + 
                               new Vector3(targetX * bridgeGrid.quadrantSize, 0.1f, targetZ * bridgeGrid.quadrantSize);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(targetPos + new Vector3(bridgeGrid.quadrantSize/2, 0, bridgeGrid.quadrantSize/2), 
                               new Vector3(bridgeGrid.quadrantSize, 0.2f, bridgeGrid.quadrantSize));
        }
        
        // Dibujar el rango de interacción
        if (buildPoint != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f); // Cyan transparente
            Gizmos.DrawSphere(buildPoint.position, interactionRange);
        }
    }
}