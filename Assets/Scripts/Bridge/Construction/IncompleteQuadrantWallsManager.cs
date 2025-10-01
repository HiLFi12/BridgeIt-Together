using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coloca "paredes invisibles" en cada cuadrante INCOMPLETO de uno o más BridgeConstructionGrid.
/// - Si el cuadrante NO está construido hasta la capa final (capa 4, índice 3), se activa una pared invisible que bloquea el paso.
/// - Si el cuadrante está completo (capa 4 construida), la pared invisible se desactiva.
/// - Soporta múltiples BridgeConstructionGrid en la escena.
/// 
/// Nota sobre colisiones solo con el jugador:
/// Este script crea BoxCollider(s) sólidos. Para que bloqueen únicamente al jugador (y no a otros objetos),
/// asigna el Layer del GameObject de pared a un layer que SOLO colisione con el layer del jugador en la matriz de colisiones del proyecto.
/// Puedes configurar el layer por nombre en 'wallLayerName'. Si está vacío, se usará el layer actual del GameObject creado.
/// </summary>
[DisallowMultipleComponent]
public class IncompleteQuadrantWallsManager : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("BridgeConstructionGrid a gestionar. Si está vacío, se auto-detectan todos en la escena (incluyendo inactivos).")]
    [SerializeField] private BridgeConstructionGrid[] grids;

    [Header("Wall Shape")]
    [Tooltip("Altura de la pared invisible.")]
    [SerializeField] private float wallHeight = 2.0f;

    [Tooltip("Offset vertical del centro del collider. Útil si el suelo no es y=0.")]
    [SerializeField] private float wallCenterYOffset = 1.0f;

    [Tooltip("Margen para que la pared no toque exactamente los bordes del cuadrante (evita solapes).")]
    [SerializeField] private float shrinkMargin = 0.02f;

    [Header("Layer/Tag Opcionales")]
    [Tooltip("Nombre del Layer a aplicar a las paredes. Déjalo vacío para no cambiar el layer.")]
    [SerializeField] private string wallLayerName = string.Empty;

    [Header("Update")]
    [Tooltip("Intervalo (segundos) para evaluar el estado de los cuadrantes y activar/desactivar paredes.")]
    [SerializeField] private float evaluationInterval = 0.25f;

    [Tooltip("Mostrar Gizmos del área de pared en el editor.")]
    [SerializeField] private bool showGizmos = false;

    // Mapa de paredes por grid
    private readonly Dictionary<BridgeConstructionGrid, GameObject[,]> wallsByGrid = new();
    private readonly Dictionary<BridgeConstructionGrid, float> lastSizeByGrid = new();

    private int wallLayerIndex = -1;
    private float evalTimer;

    private void Awake()
    {
        if (grids == null || grids.Length == 0)
        {
            grids = FindObjectsOfType<BridgeConstructionGrid>(includeInactive: true);
        }

        if (!string.IsNullOrWhiteSpace(wallLayerName))
        {
            int idx = LayerMask.NameToLayer(wallLayerName);
            if (idx >= 0) wallLayerIndex = idx;
            else Debug.LogWarning($"[IncompleteQuadrantWallsManager] Layer '{wallLayerName}' no existe. Se mantendrá el layer por defecto de las paredes.", this);
        }

        // Construir estructuras por grid
        foreach (var grid in grids)
        {
            if (grid == null) continue;
            BuildWallsForGrid(grid);
        }

        // Evaluación inicial
        EvaluateAll();
    }

    private void Update()
    {
        evalTimer -= Time.deltaTime;
        if (evalTimer <= 0f)
        {
            evalTimer = Mathf.Max(0.05f, evaluationInterval);
            EvaluateAll();
        }
    }

    private void BuildWallsForGrid(BridgeConstructionGrid grid)
    {
        if (wallsByGrid.ContainsKey(grid)) return;

        var parent = GetOrCreateParentForGrid(grid);

        int w = Mathf.Max(0, grid.gridWidth);
        int l = Mathf.Max(0, grid.gridLength);
        var map = new GameObject[w, l];

        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < l; z++)
            {
                var wallGO = new GameObject($"InvisibleWall_{x}_{z}");
                wallGO.transform.SetParent(parent, worldPositionStays: false);

                // Posición inicial (se ajustará también en Evaluate si cambia quadrantSize)
                Vector3 center = GetQuadrantCenter(grid, x, z);
                wallGO.transform.position = new Vector3(center.x, wallCenterYOffset, center.z);

                var box = wallGO.AddComponent<BoxCollider>();
                box.isTrigger = false; // sólido para bloquear

                // Asignar layer si se configuró
                if (wallLayerIndex >= 0) wallGO.layer = wallLayerIndex;

                // Guardar
                map[x, z] = wallGO;
            }
        }

        wallsByGrid[grid] = map;
        lastSizeByGrid[grid] = grid.quadrantSize;

        // Ajustar tamaños al tamaño actual del cuadrante
        ResizeWallsForGrid(grid);
    }

    private Transform GetOrCreateParentForGrid(BridgeConstructionGrid grid)
    {
        // Crear contenedor bajo el grid
        string parentName = "InvisibleWallsContainer";
        var existing = grid.transform.Find(parentName);
        if (existing != null) return existing;

        var go = new GameObject(parentName);
        go.transform.SetParent(grid.transform, worldPositionStays: false);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }

    private void EvaluateAll()
    {
        foreach (var grid in grids)
        {
            if (grid == null) continue;

            // Si cambió el tamaño de cuadrante, re-posicionar y re-escalar
            if (!Mathf.Approximately(lastSizeByGrid.GetValueOrDefault(grid, grid.quadrantSize), grid.quadrantSize))
            {
                ResizeWallsForGrid(grid);
                lastSizeByGrid[grid] = grid.quadrantSize;
            }

            UpdateGridWallsActivity(grid);
        }
    }

    private void ResizeWallsForGrid(BridgeConstructionGrid grid)
    {
        if (!wallsByGrid.TryGetValue(grid, out var map) || map == null) return;

        int w = map.GetLength(0);
        int l = map.GetLength(1);
        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < l; z++)
            {
                var wall = map[x, z];
                if (wall == null) continue;

                Vector3 center = GetQuadrantCenter(grid, x, z);
                wall.transform.position = new Vector3(center.x, wallCenterYOffset, center.z);

                var box = wall.GetComponent<BoxCollider>();
                if (box != null)
                {
                    float size = Mathf.Max(0.01f, grid.quadrantSize - shrinkMargin);
                    box.size = new Vector3(size, wallHeight, size);
                    box.center = new Vector3(0f, 0f, 0f); // usar transform.position como centro
                }
            }
        }
    }

    private void UpdateGridWallsActivity(BridgeConstructionGrid grid)
    {
        if (!wallsByGrid.TryGetValue(grid, out var map) || map == null) return;

        int w = map.GetLength(0);
        int l = map.GetLength(1);
        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < l; z++)
            {
                var so = grid.GetQuadrantSO(x, z);
                bool complete = false;
                if (so != null && so.requiredLayers != null && so.requiredLayers.Length > 0)
                {
                    int last = so.requiredLayers.Length - 1; // capa final (4ta)
                    complete = so.requiredLayers[last].isCompleted;
                }

                var wall = map[x, z];
                if (wall != null)
                {
                    // Activo cuando NO está completo, inactivo cuando sí lo está
                    bool shouldBeActive = !complete;
                    if (wall.activeSelf != shouldBeActive)
                    {
                        wall.SetActive(shouldBeActive);
                    }
                }
            }
        }
    }

    private static Vector3 GetQuadrantCenter(BridgeConstructionGrid grid, int x, int z)
    {
        // Mismo cálculo que usa el grid para posicionar cuadrantes
        Vector3 basePos = grid.transform.position + new Vector3(x * grid.quadrantSize, 0f, z * grid.quadrantSize);
        return basePos + new Vector3(grid.quadrantSize * 0.5f, 0f, grid.quadrantSize * 0.5f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || grids == null) return;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
        foreach (var grid in grids)
        {
            if (grid == null) continue;
            int w = Mathf.Max(0, grid.gridWidth);
            int l = Mathf.Max(0, grid.gridLength);
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < l; z++)
                {
                    var center = GetQuadrantCenter(grid, x, z);
                    Vector3 size = new Vector3(Mathf.Max(0.01f, grid.quadrantSize - shrinkMargin), wallHeight, Mathf.Max(0.01f, grid.quadrantSize - shrinkMargin));
                    Gizmos.DrawCube(new Vector3(center.x, wallCenterYOffset, center.z), size);
                }
            }
        }
    }
#endif
}
