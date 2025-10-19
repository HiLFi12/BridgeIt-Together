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

    private void Awake()
    {
        if (!_allManagers.Contains(this))
        {
            _allManagers.Add(this);
            Debug.Log($"PlayerUIManager registrado: {gameObject.name}. Total managers: {_allManagers.Count}");
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
        
        // Activar las UI compartidas (othersUI y bridgeQuadrants) solo la primera vez
        if (_sharedActiveCount[index] == 1)
        {
            foreach (PlayerUIManager manager in _allManagers)
            {
                if (manager != null)
                {
                    manager.TurnOnSharedUIOnly(index);
                }
            }
        }
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
        
        // Desactivar las UI compartidas solo cuando nadie las use
        if (_sharedActiveCount[index] == 0)
        {
            foreach (PlayerUIManager manager in _allManagers)
            {
                if (manager != null)
                {
                    manager.TurnOffSharedUIOnly(index);
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
            group.playerUI.gameObject.SetActive(true);
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

    private void TurnOnSharedUIOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        UIGroup group = uiGroups[index];

        if (group.useBridgeQuadrants)
        {
            Debug.Log($"PlayerUIManager ({gameObject.name}): Activando BridgeQuadrants para grupo[{index}] '{group.name}' capa {group.bridgeLayer}, registrados: {group.registeredQuadrants.Count}");
            foreach (BridgeQuadrant quadrant in group.registeredQuadrants)
            {
                if (quadrant != null && quadrant.CanBuildLayer(group.bridgeLayer))
                {
                    Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                    if (layerUI != null)
                    {
                        Debug.Log($"Activando UI de BridgeQuadrant '{quadrant.gameObject.name}' para capa {group.bridgeLayer} en grupo[{index}] '{group.name}'");
                        layerUI.gameObject.SetActive(true);
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
                    otherUI.gameObject.SetActive(true);
                }
            }
        }
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
}