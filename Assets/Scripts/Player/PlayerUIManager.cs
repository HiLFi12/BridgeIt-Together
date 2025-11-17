using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    private static List<PlayerUIManager> _allManagers = new List<PlayerUIManager>();
    private static Dictionary<int, int> _sharedActiveCount = new Dictionary<int, int>();

    [System.Serializable]
    public class UIGroup
    {
        public string name;
        
        [Tooltip("UI que se muestra en el canvas del jugador")]
        public Image playerUI;

        [Tooltip("UI de otros objetos (se puede usar junto con useBridgeQuadrants)")]
        public Image[] othersUI;
        
        [Header("Bridge Quadrants")]
        [Tooltip("¿Este grupo usa UI de BridgeQuadrants?")]
        public bool useBridgeQuadrants = false;
        
        [Tooltip("Índice de la capa del puente (0=Base, 1=Soporte, 2=Estructura)")]
        [Range(0, 2)]
        public int bridgeLayer = 0;
        
        [HideInInspector]
        public List<BridgeQuadrant> registeredQuadrants = new List<BridgeQuadrant>();
    }

    [Header("Grupos de UI")]
    [SerializeField] private List<UIGroup> uiGroups = new List<UIGroup>();
    
    [Header("Referencias")]
    [Tooltip("Referencia al componente Player para verificar UIs de interacción/construcción")]
    [SerializeField] private Player player;
    
    // Diccionario para rastrear qué cuadrantes activó este jugador específico
    private Dictionary<int, HashSet<BridgeQuadrant>> _myActivatedQuadrants = new Dictionary<int, HashSet<BridgeQuadrant>>();
    
    // Índice de UI que debería estar activo (pero puede estar oculto por interacción/construcción)
    private int _pendingPlayerUIIndex = -1;

    private void Awake()
    {
        if (!_allManagers.Contains(this))
        {
            _allManagers.Add(this);
            Debug.Log($"PlayerUIManager registrado: {gameObject.name}. Total managers: {_allManagers.Count}");
        }
        
        // Auto-asignar Player si no está asignado
        if (player == null)
        {
            player = GetComponent<Player>();
        }
    }

    private void OnDestroy()
    {
        _allManagers.Remove(this);
    }

    private void Start()
    {
        RegisterBridgeQuadrants();
        
        for (int i = 0; i < uiGroups.Count; i++)
        {
            if (!_sharedActiveCount.ContainsKey(i))
            {
                _sharedActiveCount[i] = 0;
            }
            TurnOffUIInternal(i);
        }
    }

    private void RegisterBridgeQuadrants()
    {
        BridgeQuadrant[] allQuadrants = FindObjectsOfType<BridgeQuadrant>();
        Debug.Log($"PlayerUIManager ({gameObject.name}): Encontrados {allQuadrants.Length} cuadrantes en total");
        for (int q = 0; q < allQuadrants.Length; q++)
        {
            var quad = allQuadrants[q];
            string uiInfo = $"Quadrant '{quad.gameObject.name}': layer0UI={(quad.GetLayerUI(0)!=null)}, layer1UI={(quad.GetLayerUI(1)!=null)}, layer2UI={(quad.GetLayerUI(2)!=null)}";
            Debug.Log(uiInfo);
        }
        for (int i = 0; i < uiGroups.Count; i++)
        {
            UIGroup group = uiGroups[i];
            if (group.useBridgeQuadrants)
            {
                group.registeredQuadrants.Clear();
                int foundCount = 0;
                foreach (BridgeQuadrant quadrant in allQuadrants)
                {
                    Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                    if (layerUI != null)
                    {
                        group.registeredQuadrants.Add(quadrant);
                        foundCount++;
                    }
                }
                Debug.Log($"PlayerUIManager ({gameObject.name}): Grupo[{i}] '{group.name}' Registrados {foundCount} de {allQuadrants.Length} cuadrantes para la capa {group.bridgeLayer}");
            }
        }
    }

    public void TurnOnUI(int index)
    {
        if (!IsValidIndex(index)) return;

        if (!_sharedActiveCount.ContainsKey(index))
        {
            _sharedActiveCount[index] = 0;
        }

        _sharedActiveCount[index]++;
        
        Debug.Log($"TurnOnUI({index}) llamado por {gameObject.name}. Conteo: {_sharedActiveCount[index]}");
        
        // Activar la playerUI SOLO de este manager
        TurnOnPlayerUIOnly(index);
        
        // Los cuadrantes del puente se activan SOLO para este manager (basado en su posición)
        // Los othersUI se activan para todos solo la primera vez
        if (_sharedActiveCount[index] == 1)
        {
            // Activar othersUI compartidos en todos los managers
            foreach (PlayerUIManager manager in _allManagers)
            {
                if (manager != null)
                {
                    manager.TurnOnSharedUIOthersOnly(index);
                }
            }
        }
        
        // Activar cuadrantes del puente solo para ESTE manager
        TurnOnBridgeQuadrantsOnly(index);
    }

    public void TurnOffUI(int index)
    {
        if (!IsValidIndex(index)) return;

        if (!_sharedActiveCount.ContainsKey(index))
        {
            _sharedActiveCount[index] = 0;
            return;
        }

        _sharedActiveCount[index]--;
        
        if (_sharedActiveCount[index] < 0)
        {
            _sharedActiveCount[index] = 0;
        }
        
        Debug.Log($"TurnOffUI({index}) llamado por {gameObject.name}. Conteo: {_sharedActiveCount[index]}");
        
        // Desactivar la playerUI SOLO de este manager
        TurnOffPlayerUIOnly(index);
        
        // Desactivar cuadrantes del puente solo para ESTE manager
        TurnOffBridgeQuadrantsOnly(index);
        
        // Desactivar las UI compartidas (othersUI) solo cuando nadie las use
        if (_sharedActiveCount[index] == 0)
        {
            foreach (PlayerUIManager manager in _allManagers)
            {
                if (manager != null)
                {
                    manager.TurnOffSharedUIOthersOnly(index);
                }
            }
        }
    }

    private void TurnOnPlayerUIOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.playerUI != null)
        {
            // Guardar el índice pendiente
            _pendingPlayerUIIndex = index;
            
            // Solo activar si no hay UIs de interacción/construcción activas
            if (!IsInteractionOrBuildUIActive())
            {
                group.playerUI.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log($"PlayerUIManager ({gameObject.name}): playerUI[{index}] no se activa porque hay UI de interacción/construcción activa");
            }
        }
    }

    private void TurnOffPlayerUIOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.playerUI != null)
        {
            group.playerUI.gameObject.SetActive(false);
            
            // Limpiar el índice pendiente si coincide
            if (_pendingPlayerUIIndex == index)
            {
                _pendingPlayerUIIndex = -1;
            }
        }
    }

    /// <summary>
    /// Activa solo los cuadrantes del puente para ESTE jugador específico.
    /// </summary>
    private void TurnOnBridgeQuadrantsOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.useBridgeQuadrants)
        {
            // Inicializar el HashSet para este índice si no existe
            if (!_myActivatedQuadrants.ContainsKey(index))
            {
                _myActivatedQuadrants[index] = new HashSet<BridgeQuadrant>();
            }
            
            Debug.Log($"PlayerUIManager ({gameObject.name}): Activando BridgeQuadrants para grupo[{index}] '{group.name}' capa {group.bridgeLayer}, registrados: {group.registeredQuadrants.Count}");
            Vector3 playerPos = transform.position;
            
            // Activar solo los cuadrantes accesibles desde el lado del jugador
            // y registrarlos en _myActivatedQuadrants
            ActivateAccessibleQuadrantsFromPlayerSide(group, playerPos, index);
        }
    }
    
    /// <summary>
    /// Desactiva solo los cuadrantes del puente que ESTE jugador activó.
    /// </summary>
    private void TurnOffBridgeQuadrantsOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.useBridgeQuadrants && _myActivatedQuadrants.ContainsKey(index))
        {
            // Solo apagar los cuadrantes que ESTE jugador activó
            HashSet<BridgeQuadrant> myQuadrants = _myActivatedQuadrants[index];
            
            foreach (BridgeQuadrant quadrant in myQuadrants)
            {
                if (quadrant != null)
                {
                    Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                    if (layerUI != null)
                    {
                        layerUI.gameObject.SetActive(false);
                    }
                }
            }
            
            // Limpiar el registro de cuadrantes activados por este jugador
            myQuadrants.Clear();
            
            Debug.Log($"PlayerUIManager ({gameObject.name}): Desactivados cuadrantes del puente para índice {index}");
        }
    }

    /// <summary>
    /// Activa solo los othersUI compartidos (no los cuadrantes del puente).
    /// </summary>
    private void TurnOnSharedUIOthersOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];
        
        if (group.othersUI != null && group.othersUI.Length > 0)
        {
            foreach (Image otherUI in group.othersUI)
            {
                if (otherUI != null)
                {
                    otherUI.gameObject.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Desactiva solo los othersUI compartidos (no los cuadrantes del puente).
    /// </summary>
    private void TurnOffSharedUIOthersOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];
        
        if (group.othersUI != null && group.othersUI.Length > 0)
        {
            foreach (Image otherUI in group.othersUI)
            {
                if (otherUI != null)
                {
                    otherUI.gameObject.SetActive(false);
                }
            }
        }
    }

    private void TurnOnSharedUIOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.useBridgeQuadrants)
        {
            // Inicializar el HashSet para este índice si no existe
            if (!_myActivatedQuadrants.ContainsKey(index))
            {
                _myActivatedQuadrants[index] = new HashSet<BridgeQuadrant>();
            }
            
            Debug.Log($"PlayerUIManager ({gameObject.name}): Activando BridgeQuadrants para grupo[{index}] '{group.name}' capa {group.bridgeLayer}, registrados: {group.registeredQuadrants.Count}");
            Vector3 playerPos = transform.position;
            
            // Activar solo los cuadrantes accesibles desde el lado del jugador
            ActivateAccessibleQuadrantsFromPlayerSide(group, playerPos, index);
        }
        
        if (group.othersUI != null && group.othersUI.Length > 0)
        {
            foreach (Image otherUI in group.othersUI)
            {
                if (otherUI != null)
                {
                    otherUI.gameObject.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Activa solo los cuadrantes accesibles desde el lado del jugador.
    /// Recorre el puente desde el borde más cercano hacia el centro, columna por columna,
    /// y se detiene cuando encuentra cuadrantes no construibles.
    /// </summary>
    private void ActivateAccessibleQuadrantsFromPlayerSide(UIGroup group, Vector3 playerPos, int index)
    {
        if (group.registeredQuadrants.Count == 0) return;
        
        // Obtener el HashSet de cuadrantes activados por este jugador
        HashSet<BridgeQuadrant> myQuadrants = _myActivatedQuadrants[index];
        
        // Agrupar cuadrantes por grid
        Dictionary<BridgeConstructionGrid, List<BridgeQuadrant>> quadrantsByGrid = new Dictionary<BridgeConstructionGrid, List<BridgeQuadrant>>();
        
        foreach (BridgeQuadrant quadrant in group.registeredQuadrants)
        {
            if (quadrant != null && quadrant.grid != null)
            {
                if (!quadrantsByGrid.ContainsKey(quadrant.grid))
                {
                    quadrantsByGrid[quadrant.grid] = new List<BridgeQuadrant>();
                }
                quadrantsByGrid[quadrant.grid].Add(quadrant);
            }
        }
        
        // Procesar cada grid
        foreach (var kvp in quadrantsByGrid)
        {
            BridgeConstructionGrid grid = kvp.Key;
            List<BridgeQuadrant> quadrants = kvp.Value;
            
            // Determinar de qué lado está el jugador
            int leftEdgeX = 0;
            int rightEdgeX = grid.gridWidth - 1;
            
            float leftEdgePosX = grid.transform.position.x + (leftEdgeX + 0.5f) * grid.QuadrantStepX;
            float rightEdgePosX = grid.transform.position.x + (rightEdgeX + 0.5f) * grid.QuadrantStepX;
            
            float distToLeftEdge = Mathf.Abs(playerPos.x - leftEdgePosX);
            float distToRightEdge = Mathf.Abs(playerPos.x - rightEdgePosX);
            
            bool playerNearLeftEdge = distToLeftEdge < distToRightEdge;
            
            Debug.Log($"Jugador en X={playerPos.x:F1}, Borde izq X={leftEdgePosX:F1}, Borde der X={rightEdgePosX:F1}, Player cerca del borde: {(playerNearLeftEdge ? "IZQUIERDO" : "DERECHO")}");
            
            // Recorrer desde el borde del jugador hacia el centro, columna por columna
            int startX = playerNearLeftEdge ? leftEdgeX : rightEdgeX;
            int direction = playerNearLeftEdge ? 1 : -1; // 1 = hacia derecha, -1 = hacia izquierda
            int activatedCount = 0;
            
            // Para cada columna desde el borde del jugador
            for (int x = startX; x >= 0 && x < grid.gridWidth; x += direction)
            {
                bool columnHasAccessible = false;
                
                // Para cada fila en esta columna
                for (int z = 0; z < grid.gridLength; z++)
                {
                    BridgeQuadrant quadrant = FindQuadrantAt(quadrants, x, z);
                    
                    if (quadrant != null && quadrant.CanBuildLayer(group.bridgeLayer))
                    {
                        Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                        if (layerUI != null)
                        {
                            layerUI.gameObject.SetActive(true);
                            columnHasAccessible = true;
                            activatedCount++;
                            
                            // Registrar este cuadrante como activado por ESTE jugador
                            myQuadrants.Add(quadrant);
                            
                            Debug.Log($"  ✓ Activado cuadrante [{x},{z}]");
                        }
                    }
                }
                
                // Si esta columna no tiene ningún cuadrante accesible, detener la búsqueda
                // (el puente no ha progresado hasta aquí todavía)
                if (!columnHasAccessible && x != startX)
                {
                    Debug.Log($"  ✗ Columna {x} no tiene cuadrantes accesibles, deteniendo búsqueda");
                    break;
                }
            }
            
            Debug.Log($"Total activados: {activatedCount} cuadrantes desde borde X={startX}");
        }
    }
    
    /// <summary>
    /// Encuentra un cuadrante en la lista por sus coordenadas X,Z.
    /// </summary>
    private BridgeQuadrant FindQuadrantAt(List<BridgeQuadrant> quadrants, int x, int z)
    {
        foreach (BridgeQuadrant quadrant in quadrants)
        {
            if (quadrant == null) continue;
            
            string[] parts = quadrant.gameObject.name.Split('_');
            if (parts.Length < 3) continue;
            
            if (int.TryParse(parts[1], out int quadX) && int.TryParse(parts[2], out int quadZ))
            {
                if (quadX == x && quadZ == z)
                    return quadrant;
            }
        }
        return null;
    }

    private void TurnOffSharedUIOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.useBridgeQuadrants)
        {
            foreach (BridgeQuadrant quadrant in group.registeredQuadrants)
            {
                if (quadrant != null)
                {
                    Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                    if (layerUI != null)
                    {
                        layerUI.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        if (group.othersUI != null && group.othersUI.Length > 0)
        {
            foreach (Image otherUI in group.othersUI)
            {
                if (otherUI != null)
                {
                    otherUI.gameObject.SetActive(false);
                }
            }
        }
    }

    private void TurnOnUIInternal(int index)
    {
        // Este método ya no se usa, pero lo mantengo para compatibilidad
        TurnOnPlayerUIOnly(index);
        TurnOnSharedUIOnly(index);
    }

    private void TurnOffUIInternal(int index)
    {
        TurnOffPlayerUIOnly(index);
        TurnOffSharedUIOnly(index);
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < uiGroups.Count;
    }
    
    public void RefreshBridgeQuadrants()
    {
        RegisterBridgeQuadrants();
    }

    public void RefreshHeldObjectUI(int index)
    {
        // Apagar todas las UIs del canvas del player
        for (int i = 0; i < uiGroups.Count; i++)
        {
            TurnOffPlayerUIOnly(i);
        }
        // Activar la UI del índice deseado
        TurnOnPlayerUIOnly(index);
    }
    
    /// <summary>
    /// Verifica si alguna UI de interacción o construcción está activa.
    /// </summary>
    private bool IsInteractionOrBuildUIActive()
    {
        if (player == null)
            return false;
        
        return player.IsInteractionUIActive || player.IsBuildUIActive;
    }
    
    /// <summary>
    /// Método llamado por Player cuando cambia el estado de las UIs de interacción/construcción.
    /// Refresca la playerUI pendiente según corresponda.
    /// </summary>
    public void OnInteractionOrBuildUIChanged()
    {
        if (_pendingPlayerUIIndex < 0 || !IsValidIndex(_pendingPlayerUIIndex))
            return;
        
        UIGroup group = uiGroups[_pendingPlayerUIIndex];
        if (group.playerUI == null)
            return;
        
        // Si hay UI de interacción/construcción activa, ocultar playerUI
        if (IsInteractionOrBuildUIActive())
        {
            if (group.playerUI.gameObject.activeInHierarchy)
            {
                group.playerUI.gameObject.SetActive(false);
                Debug.Log($"PlayerUIManager ({gameObject.name}): Ocultando playerUI[{_pendingPlayerUIIndex}] por UI de interacción/construcción");
            }
        }
        // Si no hay UI de interacción/construcción, mostrar playerUI pendiente
        else
        {
            if (!group.playerUI.gameObject.activeInHierarchy)
            {
                group.playerUI.gameObject.SetActive(true);
                Debug.Log($"PlayerUIManager ({gameObject.name}): Mostrando playerUI[{_pendingPlayerUIIndex}] porque ya no hay UI de interacción/construcción");
            }
        }
    }
}