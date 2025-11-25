using UnityEngine;
using System;

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
    [Tooltip("Radio del SphereCast vertical para detectar cuadrantes por física.")]
    [SerializeField] private float spherecastRadius = 0.22f;
    [Tooltip("Offset hacia arriba para iniciar el SphereCast (evita false negatives por contacto con el suelo/cuerpo).")]
    [SerializeField] private float spherecastUpOffset = 0.3f;
    [Tooltip("Extra de alcance vertical del SphereCast por diferencias de altura.")]
    [SerializeField] private float spherecastExtraDistance = 1.0f;
    
    // Referencias internas
    private PlayerObjectHolder objectHolder;

    // Selección dinámica de objetivo
    private BridgeConstructionGrid currentTargetGrid;
    
    // Cache para la construcción
    private int targetX = -1;
    private int targetZ = -1;
    private int currentLayerIndex = 0;
    
    // Event fired when TryBuildBridge is executed
    public event Action OnTryBuildAttempt;

    // Event fired when a build attempt finishes: bool indicates success
    public event Action<bool> OnBuildResult;

    // Event fired when a repair attempt finishes: bool indicates success
    public event Action<bool> OnRepairResult;

    [Header("Ghost Preview")]
    [Tooltip("Material para la silueta fantasma; si no se asigna, se creará uno transparente en runtime.")]
    [SerializeField] private Material previewMaterial;
    [SerializeField, Range(0f,1f)] private float previewAlpha = 0.45f;
    [SerializeField] private Color previewTint = new Color(0.1f, 0.8f, 1f, 0.45f);
    [SerializeField] private bool showPreview = true;

    // Estado del ghost
    private GameObject ghostInstance;
    private BridgeConstructionGrid ghostGrid;
    private int ghostX = -1, ghostZ = -1, ghostLayer = -1;

    private void Start()
    {
        objectHolder = GetComponent<PlayerObjectHolder>();
        
        if ((bridgeGrids == null || bridgeGrids.Length ==0) && autoFindBridgeGrids)
        {
#if UNITY_2023_1_OR_NEWER
            bridgeGrids = UnityEngine.Object.FindObjectsByType<BridgeConstructionGrid>(FindObjectsSortMode.None);
#else
            bridgeGrids = FindObjectsOfType<BridgeConstructionGrid>();
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

        // Suscribirse a eventos del holder para gestionar el ghost
        if (objectHolder != null)
        {
            objectHolder.OnPickedUp += HandlePickedUp;
            objectHolder.OnDropped += HandleDropped;
            objectHolder.OnUsed += HandleUsed;
        }

        // Suscribirse al resultado de construcción para ocultar ghost al construir
        OnBuildResult += HandleBuildResult;
    }

    private void OnDestroy()
    {
        if (objectHolder != null)
        {
            objectHolder.OnPickedUp -= HandlePickedUp;
            objectHolder.OnDropped -= HandleDropped;
            objectHolder.OnUsed -= HandleUsed;
        }
        OnBuildResult -= HandleBuildResult;
        DestroyGhost();
    }

    private void Update()
    {
        // Actualizar la vista previa cada frame para seguir la posición/selección
        UpdateGhostPreview();
    }
    
    // Esta función debe ser llamada desde el sistema de interacción del jugador
    public void TryBuildBridge()
    {
        // Notify listeners that a build attempt is being executed
        OnTryBuildAttempt?.Invoke();

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
        
        // Reparación de última capa (superficie): aceptar adoquín o, como fallback, cualquier material de la capa superior
        {
            BridgeQuadrantSO targetQuadrant = currentTargetGrid.GetQuadrantSO(targetX, targetZ);
            if (targetQuadrant != null && targetQuadrant.NeedsRepair())
            {
                int lastIdx = (targetQuadrant.requiredLayers != null && targetQuadrant.requiredLayers.Length > 0)
                    ? targetQuadrant.requiredLayers.Length - 1
                    : 2;

                bool canAttemptRepair = false;
                if (materialInfo != null)
                {
                    canAttemptRepair =
                        materialInfo.materialType == BridgeQuadrantSO.MaterialType.Adoquin ||
                        materialInfo.layerIndex == lastIdx;
                }
                else if (objectInHand != null && objectInHand.tag == $"BridgeLayer{lastIdx}")
                {
                    canAttemptRepair = true;
                }

                if (canAttemptRepair)
                {
                    bool repairSuccess = TryRepairQuadrant(targetQuadrant, objectInHand);
                    if (repairSuccess)
                    {
                        objectHolder.UseHeldObject();
                        Debug.Log("Cuadrante reparado exitosamente con material de superficie (adoquín).");
                    }
                    else
                    {
                        Debug.LogWarning("No se pudo reparar el cuadrante con este material de superficie.");
                    }

                    // Notify listeners about the repair result
                    try
                    {
                        OnRepairResult?.Invoke(repairSuccess);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"OnRepairResult listener threw an exception: {ex}");
                    }

                    return; // reparación ya manejada (éxito o fallo)
                }
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

        // Notify listeners about the build result
        try
        {
            OnBuildResult?.Invoke(success);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"OnBuildResult listener threw an exception: {ex}");
        }

        // Si construyó, el UseHeldObject ocultará el ghost; si falló, lo mantenemos/actualizamos
    }
    
    /// <summary>
    /// Intenta reparar un cuadrante dañado usando material de superficie (adoquín)
    /// </summary>
    private bool TryRepairQuadrant(BridgeQuadrantSO quadrant, GameObject materialObject)
    {
        if (quadrant == null || materialObject == null) return false;
    if (!quadrant.NeedsRepair()) return false;

        BridgeMaterialInfo materialInfo = materialObject.GetComponent<BridgeMaterialInfo>();
        int lastIdx = (quadrant.requiredLayers != null && quadrant.requiredLayers.Length > 0)
            ? quadrant.requiredLayers.Length - 1
            : 2;

        // Aceptar reparación si: es adoquín, o es material de la capa superior (por compatibilidad)
        bool isValidRepairMaterial = false;
        if (materialInfo != null)
        {
            isValidRepairMaterial =
                materialInfo.materialType == BridgeQuadrantSO.MaterialType.Adoquin ||
                materialInfo.layerIndex == lastIdx;
        }
        else if (materialObject.tag == $"BridgeLayer{lastIdx}")
        {
            isValidRepairMaterial = true;
        }

        if (!isValidRepairMaterial) return false;

        return quadrant.TryAddLayer(BridgeQuadrantSO.MaterialType.Adoquin, 1);
    }

    // ===== Ghost Preview =====
    private void HandlePickedUp(GameObject obj)
    {
        // Forzar actualización inmediata al recoger
        UpdateGhostPreview(true);
    }

    private void HandleDropped(GameObject obj)
    {
        DestroyGhost();
    }

    private void HandleUsed(GameObject obj)
    {
        DestroyGhost();
    }

    private void HandleBuildResult(bool success)
    {
        if (success) DestroyGhost();
    }

    private void EnsurePreviewMaterial()
    {
        if (previewMaterial != null) return;

        // Crear un material transparente simple si no hay uno asignado
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        if (sh == null)
        {
            // Como último recurso, usar el primer shader disponible
            sh = Shader.Find("Diffuse");
        }
        var mat = new Material(sh);
        // Intentar configurar transparencia básica
        mat.color = new Color(previewTint.r, previewTint.g, previewTint.b, previewAlpha);
        previewMaterial = mat;
    }

    private bool EsMaterialDeConstruccionValido(GameObject obj)
    {
        if (obj == null) return false;

        // Excluir explícitos (herramientas / consumibles / interactuables especiales)
        string tag = obj.tag;
        if (tag == "Heater" || tag == "Battery" || tag == "Coal" || tag == "Torch")
            return false;

        // 1) Si es un MaterialBaseInteractable, respetar su gating (ready) mediante PuedeConstruirse
        var interactable = obj.GetComponent<MaterialBaseInteractable>();
        if (interactable != null)
            return interactable.PuedeConstruirse;

        // 2) Compatibilidad: si solo tiene BridgeMaterialInfo, permitir
        var info = obj.GetComponent<BridgeMaterialInfo>();
        if (info != null) return true;

        // 3) Compatibilidad: tags BridgeLayerX válidos
        if (!string.IsNullOrEmpty(tag) && tag.StartsWith("BridgeLayer"))
        {
            int parsed;
            if (int.TryParse(tag.Substring("BridgeLayer".Length), out parsed))
                return true;
        }

        return false;
    }

    private void UpdateGhostPreview(bool force = false)
    {
        if (!showPreview) { DestroyGhost(); return; }
        if (objectHolder == null || !objectHolder.HasObjectInHand()) { DestroyGhost(); return; }
        if (bridgeGrids == null || bridgeGrids.Length == 0 || buildPoint == null) { DestroyGhost(); return; }

        GameObject inHand = objectHolder.GetHeldObject();
        // NUEVO: si no es un material válido, ocultar preview y salir
        if (!EsMaterialDeConstruccionValido(inHand))
        {
            DestroyGhost();
            return;
        }

        // Encontrar cuadrante objetivo
        BridgeConstructionGrid g;
        int x, z;
        if (!FindTargetQuadrantAllGrids(buildPoint.position, out g, out x, out z))
        {
            DestroyGhost();
            return;
        }

        var so = GetQuadrantSO(g, x, z);
        if (so == null || so.requiredLayers == null || so.requiredLayers.Length == 0)
        {
            DestroyGhost();
            return;
        }

        // Material en mano
        int matLayerIndex = 0;
        BridgeMaterialInfo matInfo = inHand.GetComponent<BridgeMaterialInfo>();
        if (matInfo != null) matLayerIndex = matInfo.layerIndex;
        else if (inHand.tag.StartsWith("BridgeLayer"))
        {
            var s = inHand.tag.Substring(11);
            int li;
            if (int.TryParse(s, out li)) matLayerIndex = li;
        }

        int nextLayer = GetNextCorrectLayerIndex(g, x, z);
        bool isRepair = false;
    if (nextLayer == -1 && so.NeedsRepair())
        {
            int lastIdx = Mathf.Max(0, so.requiredLayers.Length - 1);
            if (matInfo != null)
            {
                isRepair = matInfo.materialType == BridgeQuadrantSO.MaterialType.Adoquin || matInfo.layerIndex == lastIdx;
            }
            else if (inHand.tag == $"BridgeLayer{lastIdx}")
            {
                isRepair = true;
            }
            nextLayer = isRepair ? lastIdx : -1;
        }

        if (nextLayer < 0)
        {
            DestroyGhost();
            return;
        }

        // Validar coincidencia capa vs material (si no es reparación)
        if (!isRepair && matLayerIndex != nextLayer)
        {
            DestroyGhost();
            return;
        }

        // Obtener prefab visual
        var layer = so.requiredLayers[nextLayer];
        GameObject prefab = (layer != null) ? layer.visualPrefab : null;
        if (prefab == null)
        {
            DestroyGhost();
            return;
        }

        // Posición / escala
        float layerHeight = (nextLayer < g.layerHeights.Length) ? g.layerHeights[nextLayer] : (0.5f * nextLayer);
        float cx = g.QuadrantStepX;
        float cz = g.QuadrantStepZ;
        Vector3 pos = g.transform.position + new Vector3(x * cx + cx * 0.5f, layerHeight, z * cz + cz * 0.5f);

        Vector3 layerScaleCfg = (nextLayer < g.layerScales.Length) ? g.layerScales[nextLayer] : Vector3.one;
        Vector3 baseScaleCfg = g.usarTamañoPorEje ? new Vector3(g.quadrantSizeX, g.quadrantSizeY, g.quadrantSizeZ)
                                                  : new Vector3(g.quadrantSize, 1f, g.quadrantSize);
        Vector3 finalScale = g.layerScaleMode == BridgeConstructionGrid.LayerScaleMode.RelativeToQuadrantSize
            ? Vector3.Scale(baseScaleCfg, layerScaleCfg)
            : layerScaleCfg;

        if (!force && ghostInstance != null && ghostGrid == g && ghostX == x && ghostZ == z && ghostLayer == nextLayer)
        {
            ghostInstance.transform.position = pos;
            ghostInstance.transform.localScale = finalScale;
            return;
        }

        DestroyGhost();
        EnsurePreviewMaterial();

        ghostInstance = Instantiate(prefab);
        ghostInstance.name = $"GhostPreview_{x}_{z}_L{nextLayer}";
        ghostInstance.transform.position = pos;
        ghostInstance.transform.rotation = prefab.transform.localRotation;
        ghostInstance.transform.localScale = finalScale;
        ghostGrid = g; ghostX = x; ghostZ = z; ghostLayer = nextLayer;

        var rends = ghostInstance.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            try
            {
                r.sharedMaterial = previewMaterial;
                var c = previewTint; c.a = previewAlpha;
                if (r.sharedMaterial.HasProperty("_BaseColor")) r.sharedMaterial.SetColor("_BaseColor", c);
                else if (r.sharedMaterial.HasProperty("_Color")) r.sharedMaterial.color = c;
            }
            catch { }
        }
        var cols = ghostInstance.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) { c.enabled = false; }
    }

    private void DestroyGhost()
    {
        if (ghostInstance != null)
        {
            try { Destroy(ghostInstance); } catch { }
            ghostInstance = null;
        }
        ghostGrid = null; ghostX = ghostZ = ghostLayer = -1;
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

        // Parámetros para mejorar la tolerancia en bordes
        const float epsilon = 0.001f;

        // 0) Mapeo directo por coordenadas XZ (no depende de colliders). Priorizar este camino.
        if (TryDirectMap(position, out grid, out xSel, out zSel))
        {
            return true;
        }

        // 1) Intento con SphereCast hacia abajo para mayor robustez en bordes
        Vector3 castOrigin = position + Vector3.up * Mathf.Max(0f, spherecastUpOffset);
        float castDistance = interactionRange + Mathf.Max(0f, spherecastExtraDistance);
        if (Physics.SphereCast(castOrigin, Mathf.Max(0.01f, spherecastRadius), Vector3.down, out var hit, castDistance, bridgeLayer))
        {
            var hitGrid = hit.collider.GetComponentInParent<BridgeConstructionGrid>();
            if (hitGrid != null)
            {
                // Convertir a espacio local de la grilla y clamping para evitar problemas en los bordes
                Vector3 localPos = hit.point - hitGrid.transform.position;
                float maxX = Mathf.Max(0f, hitGrid.gridWidth * hitGrid.QuadrantStepX - epsilon);
                float maxZ = Mathf.Max(0f, hitGrid.gridLength * hitGrid.QuadrantStepZ - epsilon);
                localPos.x = Mathf.Clamp(localPos.x, 0f, maxX);
                localPos.z = Mathf.Clamp(localPos.z, 0f, maxZ);

                int xi = Mathf.FloorToInt(localPos.x / hitGrid.QuadrantStepX);
                int zi = Mathf.FloorToInt(localPos.z / hitGrid.QuadrantStepZ);

                xi = Mathf.Clamp(xi, 0, hitGrid.gridWidth - 1);
                zi = Mathf.Clamp(zi, 0, hitGrid.gridLength - 1);

                // Usar el centro del cuadrante para la comprobación de distancia (reduce sesgo lateral)
                Vector3 center = hitGrid.transform.position + new Vector3((xi + 0.5f) * hitGrid.QuadrantStepX, 0f, (zi + 0.5f) * hitGrid.QuadrantStepZ);
                if (DistanceXZ(position, center) <= interactionRange + 0.1f)
                {
                    grid = hitGrid;
                    xSel = xi;
                    zSel = zi;
                    return true;
                }
            }
        }

        // 2) Fallback: búsqueda por proximidad usando centros de cuadrante (más simétrico que usar esquinas)
        float minDistance = float.MaxValue;
        bool found = false;

        foreach (var g in bridgeGrids)
        {
            if (g == null) continue;

            for (int x = 0; x < g.gridWidth; x++)
            {
                for (int z = 0; z < g.gridLength; z++)
                {
                    Vector3 center = g.transform.position + new Vector3((x + 0.5f) * g.QuadrantStepX, 0f, (z + 0.5f) * g.QuadrantStepZ);
                    float distance = DistanceXZ(position, center);
                    if (distance <= interactionRange + 0.1f && distance < minDistance)
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

    // Distancia en plano XZ (ignora diferencias de altura para una detección simétrica)
    private static float DistanceXZ(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    // Mapeo directo: convierte la posición al cuadrante por coordenadas, sin depender de colliders
    private bool TryDirectMap(Vector3 position, out BridgeConstructionGrid gSel, out int xiSel, out int ziSel)
    {
        gSel = null; xiSel = -1; ziSel = -1;
        float best = float.MaxValue;

        foreach (var g in bridgeGrids)
        {
            if (g == null) continue;

            Vector3 local = position - g.transform.position;
            // Fuera de la proyección de la grilla
            if (local.x < 0f || local.z < 0f) continue;
            float maxX = g.gridWidth * g.QuadrantStepX;
            float maxZ = g.gridLength * g.QuadrantStepZ;
            if (local.x >= maxX || local.z >= maxZ) continue;

            int xi = Mathf.Clamp(Mathf.FloorToInt(local.x / g.QuadrantStepX), 0, g.gridWidth - 1);
            int zi = Mathf.Clamp(Mathf.FloorToInt(local.z / g.QuadrantStepZ), 0, g.gridLength - 1);

            Vector3 center = g.transform.position + new Vector3((xi + 0.5f) * g.QuadrantStepX, 0f, (zi + 0.5f) * g.QuadrantStepZ);
            float d = DistanceXZ(position, center);
            if (d <= interactionRange + 0.1f && d < best)
            {
                best = d; gSel = g; xiSel = xi; ziSel = zi;
            }
        }

        return gSel != null;
    }
    
    // Gizmos: usa el grid seleccionado
    private void OnDrawGizmos()
    {
        if (currentTargetGrid != null && targetX >= 0 && targetZ >= 0)
        {
            Vector3 targetPos = currentTargetGrid.transform.position +
                                new Vector3(targetX * currentTargetGrid.QuadrantStepX, 0.1f, targetZ * currentTargetGrid.QuadrantStepZ);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(targetPos + new Vector3(currentTargetGrid.QuadrantStepX/2, 0, currentTargetGrid.QuadrantStepZ/2),
                                new Vector3(currentTargetGrid.QuadrantStepX, 0.2f, currentTargetGrid.QuadrantStepZ));
        }

        if (buildPoint != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawSphere(buildPoint.position, interactionRange);
        }
    }
}
