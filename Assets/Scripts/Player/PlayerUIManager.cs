using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    private static List<PlayerUIManager> _allManagers = new List<PlayerUIManager>();
    private static Dictionary<int, int> _sharedActiveCount = new Dictionary<int, int>();
    
    // Diccionario estático para contar cuántos jugadores tienen activado cada cuadrante individual
    // Key: (index, quadrant) → Value: cantidad de jugadores que lo tienen activado
    private static Dictionary<(int, BridgeQuadrant), int> _sharedQuadrantCount = new Dictionary<(int, BridgeQuadrant), int>();

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
    
    // Diccionario para rastrear qué cuadrantes activó este jugador específico
    private Dictionary<int, HashSet<BridgeQuadrant>> _myActivatedQuadrants = new Dictionary<int, HashSet<BridgeQuadrant>>();
    
    // Referencia al componente Player
    private Player _player;

    private void Awake()
    {
        if (!_allManagers.Contains(this))
        {
            _allManagers.Add(this);
            Debug.Log($"PlayerUIManager registrado: {gameObject.name}. Total managers: {_allManagers.Count}");
        }
        
        // Obtener referencia al componente Player
        _player = GetComponent<Player>();
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

    private void Update()
    {
        if (_player == null) return;
        
        // Verificar si interactionUI o BuildUI están activos
        bool interactionUIActive = (_player.InteractionKeyUI != null && _player.InteractionKeyUI.gameObject.activeInHierarchy) ||
                                    (_player.InteractionPadUI != null && _player.InteractionPadUI.gameObject.activeInHierarchy);
        
        bool buildUIActive = (_player.BuildKeyUI != null && _player.BuildKeyUI.gameObject.activeInHierarchy) ||
                            (_player.BuildPadUI != null && _player.BuildPadUI.gameObject.activeInHierarchy);
        
        // Si interactionUI o BuildUI están activos, desactivar todas las playerUI
        if (interactionUIActive || buildUIActive)
        {
            for (int i = 0; i < uiGroups.Count; i++)
            {
                UIGroup group = uiGroups[i];
                if (group.playerUI != null && group.playerUI.gameObject.activeInHierarchy)
                {
                    group.playerUI.gameObject.SetActive(false);
                }
            }
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
            // Solo activar playerUI si las UIs de interacción y construcción NO están activas
            if (_player != null)
            {
                bool interactionUIActive = (_player.InteractionKeyUI != null && _player.InteractionKeyUI.gameObject.activeInHierarchy) ||
                                            (_player.InteractionPadUI != null && _player.InteractionPadUI.gameObject.activeInHierarchy);
                
                bool buildUIActive = (_player.BuildKeyUI != null && _player.BuildKeyUI.gameObject.activeInHierarchy) ||
                                    (_player.BuildPadUI != null && _player.BuildPadUI.gameObject.activeInHierarchy);
                
                // Solo activar si ninguna de las UIs está activa
                if (!interactionUIActive && !buildUIActive)
                {
                    group.playerUI.gameObject.SetActive(true);
                }
            }
            else
            {
                // Si no hay referencia al player, activar normalmente
                group.playerUI.gameObject.SetActive(true);
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
                    var key = (index, quadrant);
                    
                    // Decrementar el contador compartido
                    if (_sharedQuadrantCount.ContainsKey(key))
                    {
                        _sharedQuadrantCount[key]--;
                        
                        // Solo apagar la UI si ningún jugador la está usando (contador <= 0)
                        if (_sharedQuadrantCount[key] <= 0)
                        {
                            Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                            if (layerUI != null)
                            {
                                layerUI.gameObject.SetActive(false);
                            }
                            
                            // Limpiar del diccionario
                            _sharedQuadrantCount.Remove(key);
                            
                            Debug.Log($"PlayerUIManager ({gameObject.name}): Apagado cuadrante {quadrant.name} (contador llegó a 0)");
                        }
                        else
                        {
                            Debug.Log($"PlayerUIManager ({gameObject.name}): Cuadrante {quadrant.name} mantiene UI activa (contador: {_sharedQuadrantCount[key]})");
                        }
                    }
                }
            }
            
            // Limpiar el registro de cuadrantes activados por este jugador
            myQuadrants.Clear();
            
            Debug.Log($"PlayerUIManager ({gameObject.name}): Procesados cuadrantes del puente para índice {index}");
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
            
            // Cache de cuadrantes completos para este grid (para optimizar búsquedas)
            Dictionary<(int x, int z), bool> completionCache = new Dictionary<(int, int), bool>();
            foreach (BridgeQuadrant q in quadrants)
            {
                if (q != null)
                {
                    int qx = q.GetX();
                    int qz = q.GetZ();
                    if (qx != -1 && qz != -1)
                    {
                        completionCache[(qx, qz)] = q.GetCurrentLayer() >= 2;
                    }
                }
            }
            
            List<BridgeQuadrant> allQuadrantsInGrid = quadrants;
            
            int leftEdgeX = 0;
            int rightEdgeX = grid.gridWidth - 1;
            
            float leftEdgePosX = grid.transform.position.x + (leftEdgeX + 0.5f) * grid.QuadrantStepX;
            float rightEdgePosX = grid.transform.position.x + (rightEdgeX + 0.5f) * grid.QuadrantStepX;
            
            float distToLeftEdge = Mathf.Abs(playerPos.x - leftEdgePosX);
            float distToRightEdge = Mathf.Abs(playerPos.x - rightEdgePosX);
            
            bool playerNearLeftEdge = distToLeftEdge < distToRightEdge;
            
            Debug.Log($"Jugador en X={playerPos.x:F1}, Borde izq X={leftEdgePosX:F1}, Borde der X={rightEdgePosX:F1}, Player cerca del borde: {(playerNearLeftEdge ? "IZQUIERDO" : "DERECHO")}");
            
            // Determinar desde qué lado comenzar el recorrido y en qué dirección
            int startX = playerNearLeftEdge ? leftEdgeX : rightEdgeX;
            int direction = playerNearLeftEdge ? 1 : -1; // 1 = hacia derecha, -1 = hacia izquierda
            int activatedCount = 0;
            
            // HashSet para evitar activar el mismo cuadrante múltiples veces
            HashSet<(int x, int z)> alreadyActivated = new HashSet<(int, int)>();
            
            // PASO 1: Recorrer por FILAS Z primero, y para cada fila, buscar la primera columna X disponible
            // Esto asegura que se active AL MENOS UN cuadrante por fila Z (el más cercano al jugador en X)
            for (int z = 0; z < grid.gridLength; z++)
            {
                bool foundInThisRow = false;
                
                // Para cada columna X desde el borde del jugador
                for (int x = startX; x >= 0 && x < grid.gridWidth; x += direction)
                {
                    BridgeQuadrant quadrant = FindQuadrantAt(allQuadrantsInGrid, x, z);
                    
                    if (quadrant != null && quadrant.CanBuildLayer(group.bridgeLayer))
                    {
                        if (!IsQuadrantReachableFromPlayerSide(grid, x, z, playerNearLeftEdge, completionCache))
                        {
                            Debug.Log($"  ✗ Cuadrante [{x},{z}] no es alcanzable desde el lado del jugador");
                            continue;
                        }
                        
                        if (ActivateQuadrantUI(quadrant, group, index, myQuadrants, x, z, alreadyActivated))
                        {
                            foundInThisRow = true;
                            activatedCount++;
                            Debug.Log($"  ✓ Activado cuadrante [{x},{z}] (primer disponible en fila Z={z})");
                            
                            // IMPORTANTE: Detener la búsqueda en esta fila una vez encontrado el primero
                            break;
                        }
                    }
                }
                
                if (!foundInThisRow)
                {
                    Debug.Log($"  ✗ Fila Z={z} no tiene cuadrantes accesibles desde borde X={startX}");
                }
            }
            
            Debug.Log($"Paso 1 completado: {activatedCount} cuadrantes activados desde borde X={startX}");
            
            // PASO 2: Activar cuadrantes adyacentes a todos los cuadrantes COMPLETOS
            int adjacentActivatedCount = ActivateAdjacentQuadrants(grid, allQuadrantsInGrid, completionCache, group, index, myQuadrants, alreadyActivated, playerNearLeftEdge);
            activatedCount += adjacentActivatedCount;
            
            Debug.Log($"Paso 2 completado: {adjacentActivatedCount} cuadrantes adyacentes activados");
            Debug.Log($"Total activados: {activatedCount} cuadrantes desde borde X={startX}");
        }
    }
    
    /// <summary>
    /// Activa la UI de un cuadrante específico.
    /// </summary>
    /// <returns>True si se activó exitosamente, false si ya estaba activado o no se pudo activar</returns>
    private bool ActivateQuadrantUI(BridgeQuadrant quadrant, UIGroup group, int index, HashSet<BridgeQuadrant> myQuadrants, int x, int z, HashSet<(int, int)> alreadyActivated)
    {
        // Verificar si ya fue activado
        if (alreadyActivated.Contains((x, z)))
        {
            return false;
        }
        
        Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
        if (layerUI == null)
        {
            return false;
        }
        
        // Incrementar el contador compartido para este cuadrante
        var key = (index, quadrant);
        if (!_sharedQuadrantCount.ContainsKey(key))
        {
            _sharedQuadrantCount[key] = 0;
        }
        _sharedQuadrantCount[key]++;
        
        // Solo activar la UI si no está activa
        if (!layerUI.gameObject.activeInHierarchy)
        {
            layerUI.gameObject.SetActive(true);
        }
        
        // Registrar este cuadrante como activado por ESTE jugador
        myQuadrants.Add(quadrant);
        alreadyActivated.Add((x, z));
        
        Debug.Log($"      UI activada para [{x},{z}], contador compartido: {_sharedQuadrantCount[key]}");
        return true;
    }
    
    /// <summary>
    /// Activa los cuadrantes adyacentes a todos los cuadrantes completos.
    /// Solo activa los que sean alcanzables desde el lado específico del jugador.
    /// VALIDACIÓN CRÍTICA: Solo procesa cuadrantes completos que el jugador pueda alcanzar.
    /// </summary>
    private int ActivateAdjacentQuadrants(BridgeConstructionGrid grid, List<BridgeQuadrant> allQuadrantsInGrid, 
        Dictionary<(int, int), bool> completionCache, UIGroup group, int index, 
        HashSet<BridgeQuadrant> myQuadrants, HashSet<(int, int)> alreadyActivated, bool playerNearLeftEdge)
    {
        int activatedCount = 0;
        
        Debug.Log($"Buscando cuadrantes adyacentes a cuadrantes completos (desde lado {(playerNearLeftEdge ? "IZQUIERDO" : "DERECHO")})...");
        
        // Recorrer todos los cuadrantes completos
        foreach (var kvp in completionCache)
        {
            if (!kvp.Value) continue; // Solo procesar cuadrantes completos
            
            int completeX = kvp.Key.Item1;
            int completeZ = kvp.Key.Item2;
            
            // VALIDACIÓN PRIMERA: Verificar que el cuadrante completo sea alcanzable por el jugador
            // Si el jugador no puede llegar caminando hasta este cuadrante completo, no mostrar sus adyacentes
            if (!IsCompleteQuadrantReachableByPlayer(grid, completeX, completeZ, playerNearLeftEdge, completionCache))
            {
                Debug.Log($"  ✗ Cuadrante completo [{completeX},{completeZ}] NO es alcanzable por el jugador, ignorando sus adyacentes");
                continue;
            }
            
            Debug.Log($"  ✓ Cuadrante completo [{completeX},{completeZ}] es alcanzable por el jugador, verificando sus adyacentes");
            
            // Verificar los 4 cuadrantes adyacentes (arriba, abajo, izquierda, derecha)
            int[] adjacentX = { completeX - 1, completeX + 1, completeX, completeX };
            int[] adjacentZ = { completeZ, completeZ, completeZ - 1, completeZ + 1 };
            
            for (int i = 0; i < 4; i++)
            {
                int adjX = adjacentX[i];
                int adjZ = adjacentZ[i];
                
                // Validar que esté dentro de los límites del grid
                if (adjX < 0 || adjX >= grid.gridWidth || adjZ < 0 || adjZ >= grid.gridLength)
                {
                    continue;
                }
                
                // Saltar si ya fue activado
                if (alreadyActivated.Contains((adjX, adjZ)))
                {
                    continue;
                }
                
                BridgeQuadrant adjacentQuadrant = FindQuadrantAt(allQuadrantsInGrid, adjX, adjZ);
                
                if (adjacentQuadrant != null && adjacentQuadrant.CanBuildLayer(group.bridgeLayer))
                {
                    // VALIDACIÓN CRÍTICA: Verificar que el cuadrante adyacente sea alcanzable desde el lado del jugador
                    if (!IsQuadrantReachableFromPlayerSide(grid, adjX, adjZ, playerNearLeftEdge, completionCache))
                    {
                        Debug.Log($"    ✗ Cuadrante adyacente [{adjX},{adjZ}] NO es alcanzable desde el lado del jugador (vecino de [{completeX},{completeZ}])");
                        continue;
                    }
                    
                    if (ActivateQuadrantUI(adjacentQuadrant, group, index, myQuadrants, adjX, adjZ, alreadyActivated))
                    {
                        activatedCount++;
                        Debug.Log($"    ✓ Activado cuadrante adyacente [{adjX},{adjZ}] (vecino de [{completeX},{completeZ}])");
                    }
                }
            }
        }
        
        return activatedCount;
    }
    
    /// <summary>
    /// Verifica si un cuadrante es alcanzable desde el lado específico del jugador.
    /// </summary>
    private bool IsQuadrantReachableFromPlayerSide(BridgeConstructionGrid grid, int x, int z, bool fromLeftSide, Dictionary<(int, int), bool> completionCache)
    {
        // Si es el borde del jugador, siempre es alcanzable
        if ((fromLeftSide && x == 0) || (!fromLeftSide && x == grid.gridWidth - 1))
        {
            Debug.Log($"  → Cuadrante [{x},{z}] es el borde del jugador, ALCANZABLE");
            return true;
        }
        
        // Para cuadrantes internos, verificar si tienen un vecino completo EN LA DIRECCIÓN del jugador
        int previousX = fromLeftSide ? x - 1 : x + 1;
        
        // Verificar vecino en la dirección del jugador (el que ya debería estar construido)
        if (IsQuadrantCompleteAt(previousX, z, completionCache))
        {
            Debug.Log($"  → Cuadrante [{x},{z}] es ALCANZABLE porque el vecino [{previousX},{z}] está completo");
            return true;
        }
        
        // También verificar vecinos en Z (arriba/abajo)
        if (IsQuadrantCompleteAt(x, z - 1, completionCache))
        {
            Debug.Log($"  → Cuadrante [{x},{z}] es ALCANZABLE porque el vecino [{x},{z - 1}] está completo");
            return true;
        }
        
        if (IsQuadrantCompleteAt(x, z + 1, completionCache))
        {
            Debug.Log($"  → Cuadrante [{x},{z}] es ALCANZABLE porque el vecino [{x},{z + 1}] está completo");
            return true;
        }
        
        Debug.Log($"  → Cuadrante [{x},{z}] NO es alcanzable (ningún vecino completo en dirección correcta)");
        return false;
    }
    
    /// <summary>
    /// Verifica si un cuadrante en las coordenadas dadas está completo usando el cache.
    /// </summary>
    private bool IsQuadrantCompleteAt(int x, int z, Dictionary<(int, int), bool> completionCache)
    {
        if (completionCache.TryGetValue((x, z), out bool isComplete))
        {
            return isComplete;
        }
        
        return false;
    }
    
    /// <summary>
    /// Verifica si un cuadrante completo es alcanzable por el jugador caminando desde su borde.
    /// Usa un algoritmo de búsqueda para determinar si existe un camino de cuadrantes completos
    /// desde el borde del jugador hasta el cuadrante objetivo.
    /// </summary>
    private bool IsCompleteQuadrantReachableByPlayer(BridgeConstructionGrid grid, int targetX, int targetZ, 
        bool fromLeftSide, Dictionary<(int, int), bool> completionCache)
    {
        // Si es el borde del jugador, siempre es alcanzable
        int playerEdgeX = fromLeftSide ? 0 : grid.gridWidth - 1;
        if (targetX == playerEdgeX)
        {
            return true;
        }
        
        // BFS (Breadth-First Search) para encontrar un camino de cuadrantes completos
        // desde el borde del jugador hasta el cuadrante objetivo
        Queue<(int x, int z)> queue = new Queue<(int, int)>();
        HashSet<(int x, int z)> visited = new HashSet<(int, int)>();
        
        // Iniciar búsqueda desde todos los cuadrantes completos en el borde del jugador
        for (int z = 0; z < grid.gridLength; z++)
        {
            if (IsQuadrantCompleteAt(playerEdgeX, z, completionCache))
            {
                queue.Enqueue((playerEdgeX, z));
                visited.Add((playerEdgeX, z));
            }
        }
        
        // Si no hay cuadrantes completos en el borde, el target no es alcanzable
        if (queue.Count == 0)
        {
            Debug.Log($"    → No hay cuadrantes completos en el borde X={playerEdgeX}");
            return false;
        }
        
        // Búsqueda BFS
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentX = current.x;
            int currentZ = current.z;
            
            // Si llegamos al objetivo, es alcanzable
            if (currentX == targetX && currentZ == targetZ)
            {
                Debug.Log($"    → Camino encontrado desde borde X={playerEdgeX} hasta [{targetX},{targetZ}]");
                return true;
            }
            
            // Explorar vecinos completos (4 direcciones)
            int[] neighborX = { currentX - 1, currentX + 1, currentX, currentX };
            int[] neighborZ = { currentZ, currentZ, currentZ - 1, currentZ + 1 };
            
            for (int i = 0; i < 4; i++)
            {
                int nx = neighborX[i];
                int nz = neighborZ[i];
                
                // Validar límites
                if (nx < 0 || nx >= grid.gridWidth || nz < 0 || nz >= grid.gridLength)
                {
                    continue;
                }
                
                // Si ya fue visitado, saltar
                if (visited.Contains((nx, nz)))
                {
                    continue;
                }
                
                // Solo considerar cuadrantes completos como caminables
                if (IsQuadrantCompleteAt(nx, nz, completionCache))
                {
                    queue.Enqueue((nx, nz));
                    visited.Add((nx, nz));
                }
            }
        }
        
        // No se encontró camino
        Debug.Log($"    → NO existe camino desde borde X={playerEdgeX} hasta [{targetX},{targetZ}]");
        return false;
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
    /// Método estático para refrescar todas las UIs de cuadrantes activas cuando el estado del puente cambia.
    /// Se llama desde BridgeQuadrantSO cuando un cuadrante se destruye.
    /// </summary>
    public static void RefreshAllActiveQuadrantUIs()
    {
        Debug.Log($"RefreshAllActiveQuadrantUIs llamado. Total managers: {_allManagers.Count}");
        
        foreach (PlayerUIManager manager in _allManagers)
        {
            if (manager != null)
            {
                manager.RefreshMyActiveQuadrantUIs();
            }
        }
    }

    /// <summary>
    /// Refresca las UIs de cuadrantes que este jugador tiene activas actualmente.
    /// </summary>
    private void RefreshMyActiveQuadrantUIs()
    {
        // Encontrar todos los índices que tienen UIs activas (contador > 0)
        List<int> activeIndices = new List<int>();
        
        foreach (var kvp in _sharedActiveCount)
        {
            if (kvp.Value > 0)
            {
                // Verificar si este manager tiene cuadrantes activados para este índice
                if (_myActivatedQuadrants.ContainsKey(kvp.Key) && _myActivatedQuadrants[kvp.Key].Count > 0)
                {
                    activeIndices.Add(kvp.Key);
                }
            }
        }
        
        Debug.Log($"RefreshMyActiveQuadrantUIs ({gameObject.name}): Refrescando {activeIndices.Count} índices activos");
        
        // Para cada índice activo, desactivar y reactivar los cuadrantes
        foreach (int index in activeIndices)
        {
            // Desactivar los cuadrantes actuales
            TurnOffBridgeQuadrantsOnly(index);
            
            // Reactivar con el estado actualizado
            TurnOnBridgeQuadrantsOnly(index);
        }
    }
}



