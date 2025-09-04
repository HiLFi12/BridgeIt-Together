
//using UnityEngine;
//using BridgeItTogether.Gameplay.Abstractions;
//using BridgeItTogether.Gameplay.AutoControllers;

///// <summary>
///// Carroza Real: aplica triple daño secuencial por bloque (guardia delantero, carroza, guardia trasero).
///// Cada elemento consume 1 uso del bloque cuando pasa por el mismo cuadrante.
///// </summary>
//[DisallowMultipleComponent]
//public class RoyalCarriageSplitAction : AutoController
//{
//    [Header("Referencia a la grilla")]
//    [Tooltip("Si no se asigna, se buscará en escena")] 
//    [SerializeField] private BridgeConstructionGrid explicitBridgeGrid;
//    [Header("Colisión triple por lista de colliders")]
//    [Tooltip("Usar la lista de colliders propios para aplicar triple impacto en momentos distintos (carro+guardias)")]
//    [SerializeField] private bool useColliderListForTriple = true;
//    [Tooltip("Colliders locales que representan: carroza y guardias. Cada uno aplicará impacto al entrar en un bloque nuevo.")]
//    [SerializeField] private Collider[] impactColliders;
//    [Tooltip("Objetos desde los que tomar colliders si la lista anterior está vacía")]
//    [SerializeField] private GameObject[] impactColliderObjects;
//    [Tooltip("Filtrar por layer para considerar colliders propios válidos")]
//    [SerializeField] private LayerMask impactCollidersLayerMask = 0;
//    [Tooltip("Además de los contactos, proyecta por bounds todos los colliders permitidos sobre la grilla para asegurar el triple impacto")]
//    [SerializeField] private bool alsoUseBoundsProjection = true;
//    [Tooltip("Desplazamiento vertical aplicado al punto de proyección (desde el minY de los bounds)")]
//    [SerializeField] private float boundsProjectionYOffset = 0.02f;

//    [Header("Asignación de carroza")]
//    [Tooltip("Collider de la carroza. Solo este impacta 2 filas; el resto impacta 1 cuadrante.")]
//    [SerializeField] private Collider carriageCollider;

//    [Header("Debug")]
//    [SerializeField] private bool logDebug;
//    [SerializeField] private bool logContactDetails; // logs por contacto

//    // (sin estado de marcadores; el triple impacto se basa en colliders)

//    // Cache y dedupe
//    // Último punto usado para gizmos
//    private System.Collections.Generic.Dictionary<Collider, Vector3> _lastPointByCollider = new System.Collections.Generic.Dictionary<Collider, Vector3>(4);
//    // Dedupe por collider y fila (z): guarda último x impactado por cada z
//    private System.Collections.Generic.Dictionary<Collider, System.Collections.Generic.Dictionary<int, int>> _lastColumnByColliderRow = new System.Collections.Generic.Dictionary<Collider, System.Collections.Generic.Dictionary<int, int>>(4);

//    protected override void Awake()
//    {
//    base.Awake();
//        if (useColliderListForTriple)
//        {
//            // Desactivar handler base para puente y usar manejo personalizado
//            usarColisionPuente = false;
//        }
//        EnsureImpactColliderList();

//    if (logDebug) PrintImpactColliders();
//    }

//    public void Execute(int quadrantX = -1, int quadrantZ = -1) { /* compat: no-op */ }

//    /// <summary>
//    /// Notificación desde el handler base cuando la carroza impacta un cuadrante.
//    /// Aplica impactos de los guardias en el MISMO cuadrante para totalizar 3 usos por bloque.
//    /// </summary>
//    public void OnCarriageImpactQuadrant(int x, int z) { /* compat: ya no se usa con lista de colliders */ }

//    /// <summary>
//    /// Permite inyectar la grilla vía mensajería (compatible con otros sistemas del proyecto).
//    /// </summary>
//    public override void SetBridgeGrid(BridgeConstructionGrid grid)
//    {
//        explicitBridgeGrid = grid;
//        base.SetBridgeGrid(grid);
//    }

//    /// <summary>
//    /// Ejecuta el daño en el cuadrante correspondiente a un punto del mundo.
//    /// </summary>
//    public void ExecuteAtWorldPoint(Vector3 worldPoint) { /* compat: no-op */ }

//    /// <summary>
//    /// Desactiva el modo continuo cuando ya no se necesita (por ejemplo, al salir del puente).
//    /// </summary>
//    public void DeactivateContinuous() { /* compat: no-op */ }

//    private BridgeConstructionGrid ResolveGrid()
//    {
//        // Priorizar el grid configurado explícitamente o por AutoController
//        if (explicitBridgeGrid != null) return explicitBridgeGrid;
//        if (bridgeGridRef != null) return bridgeGridRef;
//        explicitBridgeGrid = Object.FindObjectOfType<BridgeConstructionGrid>();
//        return explicitBridgeGrid;
//    }

//    private static void WorldToQuadrant(BridgeConstructionGrid grid, Vector3 worldPos, out int x, out int z)
//    {
//        Vector3 local = worldPos - grid.transform.position;
//        x = Mathf.FloorToInt(local.x / grid.quadrantSize);
//        z = Mathf.FloorToInt(local.z / grid.quadrantSize);
//    }

//    // (sin cálculo de cuadrante por marcadores)

//    private Vector3 GetMoveAxisXZ()
//    {
//    // Usar la dirección propia del transform
//    Vector3 forward = transform.forward;
//        Vector3 axis = new Vector3(forward.x, 0f, forward.z);
//        if (axis.sqrMagnitude < 1e-4f)
//        {
//            // Intentar con el Rigidbody local del host (propio o del base)
//            var rbLocal = rb != null ? rb : GetComponent<Rigidbody>();
//            if (rbLocal == null) rbLocal = GetComponentInParent<Rigidbody>();
//            if (rbLocal != null)
//            {
//                Vector3 v = rbLocal.linearVelocity; // Unity 6 API
//                axis = new Vector3(v.x, 0f, v.z);
//            }
//        }
//        if (axis.sqrMagnitude < 1e-4f)
//        {
//            axis = Vector3.right; // fallback estable
//        }
//        return axis.normalized;
//    }

//    // ===== Manejo de colisiones personalizado para lista de colliders =====
//    private void OnCollisionEnter(Collision collision)
//    {
//        if (logDebug)
//        {
//            Debug.Log($"[RoyalCarriageSplitAction] OnCollisionEnter con {collision.gameObject.name}, contactos: {collision.contactCount}", this);
//        }
//        // Filtro de colisiones (global)
//        if (collisionFilter != null && collisionFilter.ShouldIgnore(collision.collider))
//        {
//            collisionFilter.ApplyIgnore(collision.collider);
//            return;
//        }

//        // Prioridad: jugador (usar lógica del base)
//        if (usarColisionJugador && collision.gameObject.CompareTag("Player"))
//        {
//            if (playerHandler == null)
//            {
//                playerHandler = new DefaultPlayerCollisionHandler();
//                SyncPlayerSettingsFromLegacyFields();
//                playerHandler.Initialize(this.gameObject, this, playerCollisionSettings);
//            }
//            Vector3 hitPoint = transform.position;
//            if (collision.contactCount > 0)
//            {
//                hitPoint = collision.GetContact(0).point;
//            }
//            playerHandler.HandleHit(collision.gameObject, hitPoint);
//            return;
//        }

//        // Puente
//        if (!useColliderListForTriple)
//        {
//            // Delegar al handler del base si no usamos el modo de lista
//            if (bridgeHandler != null) bridgeHandler.HandleCollision(collision);
//            return;
//        }

//        // Modo lista: procesar todos los contactos
//        ProcessBridgeCollisionContacts(collision);
//    }

//    // Procesar también contactos persistentes para detectar cuando otro collider propio empieza a tocar
//    private void OnCollisionStay(Collision collision)
//    {
//        if (!useColliderListForTriple) return;
//        // Ignorar jugador aquí (ya manejado en Enter)
//        if (usarColisionJugador && collision.gameObject.CompareTag("Player")) return;

//        if (logContactDetails && logDebug)
//        {
//            Debug.Log($"[RoyalCarriageSplitAction] OnCollisionStay con {collision.gameObject.name}, contactos: {collision.contactCount}", this);
//        }
//        ProcessBridgeCollisionContacts(collision);
//    }

//    // Itera contactos y aplica un impacto por collider propio permitido cuando entra a un nuevo bloque
//    private void ProcessBridgeCollisionContacts(Collision collision)
//    {
//        var grid = ResolveGrid();
//        if (grid == null) return;

//        var processed = new System.Collections.Generic.HashSet<Collider>();
//        int n = collision.contactCount;
//        for (int i = 0; i < n; i++)
//        {
//            var cp = collision.GetContact(i);
//            var selfCol = cp.thisCollider;
//            if (selfCol == null) continue;
//            if (!processed.Add(selfCol)) continue; // evitar duplicados del mismo collider en el mismo frame
//            if (!IsImpactColliderAllowed(selfCol)) continue;

//            Vector3 worldPoint = cp.point;
//            WorldToQuadrant(grid, worldPoint, out int x, out int z);
//            if (!grid.IsValidQuadrant(x, z) || grid.GetQuadrantSO(x, z) == null) continue;

//            // Aplica impacto en hasta 2 filas según el ancho del collider
//            ApplyRowImpactsForCollider(grid, selfCol, worldPoint, x, z, /*fromBounds*/ false);
//        }

//        // Proyección por bounds para el resto de colliders permitidos (por si alguno no genera contacto propio)
//        if (alsoUseBoundsProjection)
//        {
//            // Asegurar lista
//            EnsureImpactColliderList();
//            if (impactColliders != null && impactColliders.Length > 0)
//            {
//                for (int i = 0; i < impactColliders.Length; i++)
//                {
//                    var c = impactColliders[i];
//                    if (c == null) continue;
//                    if (!IsImpactColliderAllowed(c)) continue;

//                    // Si ya fue procesado por contacto en este frame, saltar
//                    if (processed.Contains(c)) continue;

//                    var b = c.bounds;
//                    // Tomar el punto más bajo del collider (minY) y ajustar un poco arriba para evitar falsas negativas
//                    Vector3 projPoint = new Vector3(b.center.x, b.min.y + boundsProjectionYOffset, b.center.z);
//                    WorldToQuadrant(grid, projPoint, out int px, out int pz);
//                    if (!grid.IsValidQuadrant(px, pz) || grid.GetQuadrantSO(px, pz) == null) continue;

//                    // Aplica impacto en hasta 2 filas usando proyección de bounds
//                    ApplyRowImpactsForCollider(grid, c, projPoint, px, pz, /*fromBounds*/ true);
//                }
//            }
//        }
//    }

//    // Aplica impactos en 1 o 2 filas (z) para el mismo x según el ancho del collider (sus bounds sobre el eje Z de la grilla)
//    private void ApplyRowImpactsForCollider(BridgeConstructionGrid grid, Collider col, Vector3 refPoint, int baseX, int baseZ, bool fromBounds)
//    {
//        bool isCarriage = IsCarriageCollider(col);

//        // Determinar filas cubiertas por los bounds del collider
//        var b = col.bounds;
//        int zMin = Mathf.FloorToInt((b.min.z - grid.transform.position.z) / grid.quadrantSize);
//        int zMax = Mathf.FloorToInt((b.max.z - grid.transform.position.z) / grid.quadrantSize);

//        // Candidatos reales dentro de la grilla
//        if (zMax < zMin) { int tmp = zMin; zMin = zMax; zMax = tmp; }
//        // Asegurar inclusión del baseZ
//        if (baseZ < zMin) zMin = baseZ;
//        if (baseZ > zMax) zMax = baseZ;

//        // Construir lista de filas candidatas
//        var candidateRows = new System.Collections.Generic.List<int>(3);
//        for (int z = zMin; z <= zMax; z++)
//        {
//            if (grid.IsValidQuadrant(baseX, z) && grid.GetQuadrantSO(baseX, z) != null)
//            {
//                candidateRows.Add(z);
//            }
//        }

//        if (candidateRows.Count == 0)
//        {
//            // fallback: solo la fila base si es válida
//            if (grid.IsValidQuadrant(baseX, baseZ) && grid.GetQuadrantSO(baseX, baseZ) != null)
//            {
//                candidateRows.Add(baseZ);
//            }
//        }

//        // Elegir filas a impactar: carroza hasta 2 filas; guardias 1 fila
//        System.Collections.Generic.List<int> rowsToHit = new System.Collections.Generic.List<int>(2);
//        if (!isCarriage)
//        {
//            // Guardias: 1 sola fila, preferentemente la base
//            int zToHit = baseZ;
//            if (!(grid.IsValidQuadrant(baseX, zToHit) && grid.GetQuadrantSO(baseX, zToHit) != null))
//            {
//                if (candidateRows.Count > 0) zToHit = candidateRows[0];
//            }
//            rowsToHit.Add(zToHit);
//        }
//        else
//        {
//            // Carroza: hasta 2 filas
//            if (candidateRows.Count <= 1)
//            {
//                rowsToHit.Add(candidateRows[0]);
//            }
//            else if (candidateRows.Count == 2)
//            {
//                rowsToHit.Add(candidateRows[0]);
//                rowsToHit.Add(candidateRows[1]);
//            }
//            else
//            {
//                // Asegurar baseZ
//                if (!candidateRows.Contains(baseZ)) candidateRows.Add(baseZ);
//                rowsToHit.Add(baseZ);
//                // Elegir la otra fila más cercana al refPoint.z
//                float localZ = refPoint.z - grid.transform.position.z;
//                float bestDist = float.MaxValue;
//                int bestRow = baseZ;
//                for (int i = 0; i < candidateRows.Count; i++)
//                {
//                    int zc = candidateRows[i];
//                    if (zc == baseZ) continue;
//                    float centerZ = (zc + 0.5f) * grid.quadrantSize;
//                    float d = Mathf.Abs(centerZ - localZ);
//                    if (d < bestDist)
//                    {
//                        bestDist = d;
//                        bestRow = zc;
//                    }
//                }
//                if (!rowsToHit.Contains(bestRow)) rowsToHit.Add(bestRow);
//            }
//        }

//        // Aplicar impactos con dedupe por (collider, z) en la columna x actual
//        for (int i = 0; i < rowsToHit.Count; i++)
//        {
//            int z = rowsToHit[i];
//            if (HasAlreadyImpacted(col, baseX, z))
//            {
//                continue;
//            }

//            // Usar la lógica heredada del AutoController para aplicar fallPoint si existe
//            ReportBridgeImpact(grid, baseX, z, refPoint);
//            MarkImpacted(col, baseX, z, refPoint);

//            if (logDebug)
//            {
//                if (fromBounds && logContactDetails)
//                    Debug.Log($"[RoyalCarriageSplitAction] (Bounds) Impacto {col.name} en ({baseX},{z}) punto {refPoint}", this);
//                else if (!fromBounds)
//                    Debug.Log($"[RoyalCarriageSplitAction] Impacto {col.name} en ({baseX},{z}) punto {refPoint}", this);
//            }
//        }
//    }

//    private bool HasAlreadyImpacted(Collider c, int x, int z)
//    {
//        if (!_lastColumnByColliderRow.TryGetValue(c, out var byRow)) return false;
//        if (!byRow.TryGetValue(z, out int lastX)) return false;
//        return lastX == x;
//    }

//    private void MarkImpacted(Collider c, int x, int z, Vector3 point)
//    {
//        if (!_lastColumnByColliderRow.TryGetValue(c, out var byRow))
//        {
//            byRow = new System.Collections.Generic.Dictionary<int, int>(4);
//            _lastColumnByColliderRow[c] = byRow;
//        }
//        byRow[z] = x;
//        _lastPointByCollider[c] = point;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        // Filtro de colisiones (global)
//        if (collisionFilter != null && collisionFilter.ShouldIgnore(other))
//        {
//            collisionFilter.ApplyIgnore(other);
//            return;
//        }

//        // Prioridad: jugador (usar lógica del base)
//        if (usarColisionJugador && other.CompareTag("Player"))
//        {
//            if (playerHandler == null)
//            {
//                playerHandler = new DefaultPlayerCollisionHandler();
//                SyncPlayerSettingsFromLegacyFields();
//                playerHandler.Initialize(this.gameObject, this, playerCollisionSettings);
//            }
//            playerHandler.HandleHit(other.gameObject, other.ClosestPoint(transform.position));
//            return;
//        }

//        if (!useColliderListForTriple)
//        {
//            if (bridgeHandler != null) bridgeHandler.HandleTrigger(other);
//            return;
//        }
//        // En modo lista, ignoramos triggers para evitar ambigüedad del collider propio; usar colisiones físicas.
//    }

//    private void EnsureImpactColliderList()
//    {
//        // Construir lista desde objetos si está vacía
//        if ((impactColliders == null || impactColliders.Length == 0) && impactColliderObjects != null && impactColliderObjects.Length > 0)
//        {
//            var list = new System.Collections.Generic.List<Collider>();
//            for (int i = 0; i < impactColliderObjects.Length; i++)
//            {
//                var go = impactColliderObjects[i];
//                if (go == null) continue;
//                var cols = go.GetComponentsInChildren<Collider>(true);
//                for (int j = 0; j < cols.Length; j++)
//                {
//                    if (cols[j] != null) list.Add(cols[j]);
//                }
//            }
//            impactColliders = list.ToArray();
//        }
//    }

//    [ContextMenu("Toggle Debug Logs")]
//    private void ToggleDebugLogs()
//    {
//        logDebug = !logDebug;
//        Debug.Log($"[RoyalCarriageSplitAction] logDebug = {logDebug}", this);
//        if (logDebug) PrintImpactColliders();
//    }

//    [ContextMenu("Print Impact Colliders")]
//    private void PrintImpactColliders()
//    {
//        EnsureImpactColliderList();
//        if (impactColliders != null && impactColliders.Length > 0)
//        {
//            for (int i = 0; i < impactColliders.Length; i++)
//            {
//                var c = impactColliders[i];
//                Debug.Log($"[RoyalCarriageSplitAction] ImpactCollider[{i}]: {(c != null ? c.name : "null")}", this);
//            }
//        }
//        else
//        {
//            Debug.Log("[RoyalCarriageSplitAction] Lista de colliders vacía. Usa 'Impact Collider Objects' o 'LayerMask'.", this);
//        }
//    }

//#if UNITY_EDITOR
//    private void OnDrawGizmosSelected()
//    {
//        if (!logDebug) return;

//        // Puntos de último impacto por collider
//        Gizmos.color = Color.cyan;
//        foreach (var kvp in _lastPointByCollider)
//        {
//            Gizmos.DrawWireSphere(kvp.Value, 0.15f);
//        }

//        // Dibuja líneas desde la raíz a cada collider registrado
//        if (impactColliders != null)
//        {
//            Gizmos.color = Color.yellow;
//            for (int i = 0; i < impactColliders.Length; i++)
//            {
//                var c = impactColliders[i];
//                if (c == null) continue;
//                Gizmos.DrawLine(transform.position, c.bounds.center);
//                Gizmos.DrawWireCube(c.bounds.center, c.bounds.size * 0.2f);
//            }
//        }
//    }
//#endif

//    private bool IsImpactColliderAllowed(Collider selfCol)
//    {
//        if (selfCol == null) return false;
//        // Por lista explícita
//        if (impactColliders != null && impactColliders.Length > 0)
//        {
//            for (int i = 0; i < impactColliders.Length; i++)
//            {
//                if (impactColliders[i] == selfCol) return true;
//            }
//        }
//    // Por layer
//        if (impactCollidersLayerMask.value != 0)
//        {
//            int layer = selfCol.gameObject.layer;
//            if ((impactCollidersLayerMask.value & (1 << layer)) != 0) return true;
//        }
//        return false;
//    }

//    private bool IsCarriageCollider(Collider c)
//    {
//        if (c == null) return false;
//        if (carriageCollider != null) return c == carriageCollider;
//        // Fallback: si no se asigna, asumir que el primero de la lista es la carroza
//        if (impactColliders != null && impactColliders.Length > 0)
//            return c == impactColliders[0];
//        return false;
//    }
//}