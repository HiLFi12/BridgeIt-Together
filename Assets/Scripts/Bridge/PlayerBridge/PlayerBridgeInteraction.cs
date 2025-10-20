using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerObjectHolder))]
public class PlayerBridgeInteraction : MonoBehaviour
{
    [Header("Referencias")]
    // ...existing code...
    // Reemplazo de un solo grid por múltiples
    [SerializeField] private BridgeConstructionGrid[] bridgeGrids;
    [SerializeField, Tooltip("Si está activo, busca automáticamente todos los BridgeConstructionGrid en la escena si la lista está vacía.")]
    private bool autoFindBridgeGrids = true;

    public Transform buildPoint;
    
    [Header("Configuración")]
    public float interactionRange = 2f;
    public LayerMask bridgeLayer;
    
    // Referencias internas
    private PlayerObjectHolder objectHolder;

    // Selección dinámica de objetivo
    private BridgeConstructionGrid currentTargetGrid;
    
    // Cache para la construcción
    private int targetX = -1;
    private int targetZ = -1;
    private int currentLayerIndex = 0;
    
    private void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        
        if ((bridgeGrids == null || bridgeGrids.Length == 0) && autoFindBridgeGrids)
        {
#if UNITY_2023_1_OR_NEWER
            bridgeGrids = Object.FindObjectsByType<BridgeConstructionGrid>(FindObjectsSortMode.None);
#else
            bridgeGrids = Object.FindObjectsOfType<BridgeConstructionGrid>();
#endif
        }

        if (bridgeGrids == null || bridgeGrids.Length == 0)
        {
            Debug.LogError($"¡No se ha encontrado BridgeConstructionGrid! El jugador {gameObject.name} no podrá interactuar con el puente.");
        }
        
        if (buildPoint == null)
        {
            Debug.LogError($"¡El punto de construcción (buildPoint) no está asignado en {gameObject.name}! Crea un Transform vacío como hijo del jugador y asígnalo.");
        }
    }
    
    // Esta función debe ser llamada desde el sistema de interacción del jugador
    public void TryBuildBridge()
    {
        if (bridgeGrids == null || bridgeGrids.Length == 0)
        {
            Debug.LogWarning($"No hay grids de puente disponibles para el jugador {gameObject.name}.");
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
        
        // Encontrar cuadrante y grid objetivo
        Vector3 buildPos = buildPoint != null ? buildPoint.position : transform.position + transform.forward;
        if (!FindTargetQuadrantAllGrids(buildPos, out currentTargetGrid, out targetX, out targetZ))
        {
            Debug.Log($"No se encontró un cuadrante objetivo dentro del rango {interactionRange}.");
            return;
        }

        Debug.Log($"Cuadrante objetivo [{targetX},{targetZ}] en grid '{currentTargetGrid.name}', intentando construir");

        GameObject objectInHand = objectHolder.GetHeldObject();
        if (objectInHand == null)
        {
            Debug.LogWarning("GetHeldObject devolvió null a pesar de que HasObjectInHand es true.");
            return;
        }

        BridgeMaterialInfo materialInfo = objectInHand.GetComponent<BridgeMaterialInfo>();
        
        // Reparación con adoquín si corresponde
        if (materialInfo != null && materialInfo.materialType == BridgeQuadrantSO.MaterialType.Adoquin)
        {
            BridgeQuadrantSO targetQuadrant = currentTargetGrid.GetQuadrantSO(targetX, targetZ);
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
                return; // reparación ya manejada
            }
        }

        // Construcción normal
        int materialLayerIndex = 0; // default
        if (materialInfo != null)
        {
            materialLayerIndex = materialInfo.layerIndex;
            Debug.Log($"Usando layerIndex {materialLayerIndex} del material {objectInHand.name}");
        }
        else if (objectInHand.tag.StartsWith("BridgeLayer"))
        {
            string layerStr = objectInHand.tag.Substring(11);
            if (int.TryParse(layerStr, out int layer))
                materialLayerIndex = layer;
        }

        int correctLayerIndex = GetNextCorrectLayerIndex(currentTargetGrid, targetX, targetZ);
        if (correctLayerIndex == -1)
        {
            Debug.LogWarning($"El cuadrante [{targetX},{targetZ}] ya tiene todas sus capas completas.");
            return;
        }
        if (materialLayerIndex != correctLayerIndex)
        {
            Debug.LogError($"ERROR DE TIPO: Material para capa {materialLayerIndex}, siguiente capa es {correctLayerIndex}");
            return;
        }

        bool success;
        if (MotivationBuffManager.Active)
        {
            success = false;
            int gridLengthLocal = currentTargetGrid.gridLength;
            for (int zIter = 0; zIter < gridLengthLocal; zIter++)
            {
                BridgeQuadrantSO columnQuadrant = currentTargetGrid.GetQuadrantSO(targetX, zIter);
                int maxLayerIndex = (columnQuadrant != null && columnQuadrant.requiredLayers != null && columnQuadrant.requiredLayers.Length > 0)
                    ? columnQuadrant.requiredLayers.Length - 1
                    : correctLayerIndex;

                for (int layer = correctLayerIndex; layer <= maxLayerIndex; layer++)
                {
                    bool layerBuilt = currentTargetGrid.TryBuildLayer(targetX, zIter, layer, objectInHand);
                    success = success || layerBuilt;
                    if (!MotivationBuffManager.Active) break;
                }
            }
        }
        else
        {
            success = currentTargetGrid.TryBuildLayer(targetX, targetZ, correctLayerIndex, objectInHand);
        }

        if (success)
        {
            objectHolder.UseHeldObject();
            currentLayerIndex = GetNextCorrectLayerIndex(currentTargetGrid, targetX, targetZ);
            Debug.Log(MotivationBuffManager.Active ? "Construcción motivada de columna completa." : $"Construcción exitosa. Siguiente capa: {currentLayerIndex}");
        }
        else
        {
            Debug.LogWarning($"No se pudo construir en [{targetX},{targetZ}] capa {correctLayerIndex}");
        }
    }
    
    /// <summary>
    /// Intenta reparar un cuadrante dañado usando material de superficie (adoquín)
    /// </summary>
    private bool TryRepairQuadrant(BridgeQuadrantSO quadrant, GameObject materialObject)
    {
        if (quadrant == null || materialObject == null) return false;
        if (!quadrant.IsDamaged()) return false;

        BridgeMaterialInfo materialInfo = materialObject.GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null || materialInfo.materialType != BridgeQuadrantSO.MaterialType.Adoquin) return false;

        return quadrant.TryAddLayer(BridgeQuadrantSO.MaterialType.Adoquin, 1);
    }

    // Obtiene el siguiente índice de capa correcto usando el grid seleccionado
    private int GetNextCorrectLayerIndex(BridgeConstructionGrid grid, int x, int z)
    {
        if (grid == null || !grid.IsValidQuadrant(x, z)) return 0;

        BridgeQuadrantSO quadrantSO = GetQuadrantSO(grid, x, z);
        if (quadrantSO == null)
        {
            Debug.LogError("No se pudo obtener el ScriptableObject del cuadrante.");
            return 0;
        }

        bool cuadranteVacio = true;
        for (int i = 0; i < quadrantSO.requiredLayers.Length; i++)
        {
            if (quadrantSO.requiredLayers[i].isCompleted)
            {
                cuadranteVacio = false;
                break;
            }
        }
        if (cuadranteVacio) return 0;

        for (int i = 0; i < quadrantSO.requiredLayers.Length; i++)
        {
            if (!quadrantSO.requiredLayers[i].isCompleted)
                return i;
        }
        return -1;
    }
    
    // Acceso al SO del cuadrante (misma reflexión, pero parametrizada por grid)
    private BridgeQuadrantSO GetQuadrantSO(BridgeConstructionGrid grid, int x, int z)
    {
        System.Type gridType = grid.GetType();
        var gridField = gridType.GetField("constructionGrid",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (gridField == null) return null;

        object gridObj = gridField.GetValue(grid);
        if (gridObj == null) return null;

        System.Type elementType = gridObj.GetType().GetElementType();
        object element = ((System.Array)gridObj).GetValue(x, z);
        if (element == null) return null;

        var soField = elementType.GetField("quadrantSO");
        if (soField == null) return null;

        return soField.GetValue(element) as BridgeQuadrantSO;
    }
    
    // Interacciones de era (usando el grid seleccionado dinámicamente)
    public void TryInteractWithQuadrant()
    {
        if (bridgeGrids == null || bridgeGrids.Length == 0)
        {
            Debug.LogWarning("No hay grids de puente disponibles.");
            return;
        }
        
        Debug.Log($"Jugador {gameObject.name} intentando interactuar con el puente...");

        Vector3 buildPos = buildPoint != null ? buildPoint.position : transform.position + transform.forward;
        if (!FindTargetQuadrantAllGrids(buildPos, out currentTargetGrid, out targetX, out targetZ))
        {
            Debug.Log($"No se encontró un cuadrante objetivo dentro del rango {interactionRange}.");
            return;
        }

        GameObject heldObject = objectHolder.GetHeldObject();
        if (heldObject != null)
        {
            string objectType = heldObject.tag;
            switch (objectType)
            {
                case "Heater":
                    currentTargetGrid.ApplyHeat(targetX, targetZ);
                    Debug.Log("Aplicando calor al cuadrante");
                    break;
                case "Battery":
                    currentTargetGrid.ReplaceBattery(targetX, targetZ);
                    objectHolder.UseHeldObject();
                    Debug.Log("Reemplazando batería del cuadrante");
                    break;
                default:
                    TryBuildBridge();
                    break;
            }
        }
        else
        {
            Debug.Log("No hay objeto en la mano. Solo se puede interactuar con el puente teniendo un objeto.");
        }
    }
    
    /// <summary>
    /// Indica si hay un cuadrante objetivo válido dentro de rango desde el buildPoint, en cualquier grid.
    /// </summary>
    public bool HasTargetQuadrantInRange()
    {
        if ((bridgeGrids == null || bridgeGrids.Length == 0) || buildPoint == null) return false;
        return FindTargetQuadrantAllGrids(buildPoint.position, out _, out _, out _);
    }

    // Selección de cuadrante entre TODOS los grids disponibles
    private bool FindTargetQuadrantAllGrids(Vector3 position, out BridgeConstructionGrid grid, out int xSel, out int zSel)
    {
        grid = null;
        xSel = -1;
        zSel = -1;

        if (bridgeGrids == null || bridgeGrids.Length == 0) return false;

        // 1) Intento por raycast directo al layer del puente
        if (Physics.Raycast(position, Vector3.down, out var hit, interactionRange, bridgeLayer))
        {
            var hitGrid = hit.collider.GetComponentInParent<BridgeConstructionGrid>();
            if (hitGrid != null)
            {
                Vector3 localPos = hit.point - hitGrid.transform.position;
                int xi = Mathf.FloorToInt(localPos.x / hitGrid.quadrantSize);
                int zi = Mathf.FloorToInt(localPos.z / hitGrid.quadrantSize);

                if (xi >= 0 && xi < hitGrid.gridWidth && zi >= 0 && zi < hitGrid.gridLength)
                {
                    grid = hitGrid;
                    xSel = xi;
                    zSel = zi;
                    return true;
                }
            }
        }

        // 2) Fallback: búsqueda por proximidad en todos los grids
        float minDistance = float.MaxValue;
        bool found = false;

        foreach (var g in bridgeGrids)
        {
            if (g == null) continue;

            for (int x = 0; x < g.gridWidth; x++)
            {
                for (int z = 0; z < g.gridLength; z++)
                {
                    Vector3 quadrantPos = g.transform.position + new Vector3(x * g.quadrantSize, 0, z * g.quadrantSize);
                    float distance = Vector3.Distance(position, quadrantPos);
                    if (distance < interactionRange && distance < minDistance)
                    {
                        minDistance = distance;
                        grid = g;
                        xSel = x;
                        zSel = z;
                        found = true;
                    }
                }
            }
        }

        return found;
    }
    
    // Gizmos: usa el grid seleccionado
    private void OnDrawGizmos()
    {
        if (currentTargetGrid != null && targetX >= 0 && targetZ >= 0)
        {
            Vector3 targetPos = currentTargetGrid.transform.position +
                                new Vector3(targetX * currentTargetGrid.quadrantSize, 0.1f, targetZ * currentTargetGrid.quadrantSize);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(targetPos + new Vector3(currentTargetGrid.quadrantSize/2, 0, currentTargetGrid.quadrantSize/2),
                                new Vector3(currentTargetGrid.quadrantSize, 0.2f, currentTargetGrid.quadrantSize));
        }

        if (buildPoint != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawSphere(buildPoint.position, interactionRange);
        }
    }
}