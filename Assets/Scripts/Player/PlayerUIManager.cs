using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    [System.Serializable]
    public class UIGroup
    {
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
        
        // Lista dinámica de UI de cuadrantes (se llena automáticamente)
        [HideInInspector]
        public List<BridgeQuadrant> registeredQuadrants = new List<BridgeQuadrant>();
    }

    [Header("Grupos de UI")]
    [SerializeField] private List<UIGroup> uiGroups = new List<UIGroup>();

    private void Start()
    {
        RegisterBridgeQuadrants();
        
        for (int i = 0; i < uiGroups.Count; i++)
        {
            TurnOffUI(i);
        }
    }

    private void RegisterBridgeQuadrants()
    {
        BridgeQuadrant[] allQuadrants = FindObjectsOfType<BridgeQuadrant>();
        
        foreach (UIGroup group in uiGroups)
        {
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
                    }
                }
                
                Debug.Log($"PlayerUIManager: Registrados {foundCount} de {allQuadrants.Length} cuadrantes para la capa {group.bridgeLayer}");
            }
        }
    }

    public void TurnOnUI(int index)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning($"PlayerUIManager: Índice {index} fuera de rango. Total de grupos: {uiGroups.Count}");
            return;
        }

        UIGroup group = uiGroups[index];

        if (group.playerUI != null)
        {
            group.playerUI.gameObject.SetActive(true);
        }

        if (group.useBridgeQuadrants)
        {
            // Activar solo las UI de cuadrantes donde se puede construir esa capa
            foreach (BridgeQuadrant quadrant in group.registeredQuadrants)
            {
                if (quadrant != null && quadrant.CanBuildLayer(group.bridgeLayer))
                {
                    Image layerUI = quadrant.GetLayerUI(group.bridgeLayer);
                    if (layerUI != null)
                    {
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

    public void TurnOffUI(int index)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning($"PlayerUIManager: Índice {index} fuera de rango. Total de grupos: {uiGroups.Count}");
            return;
        }

        UIGroup group = uiGroups[index];

        if (group.playerUI != null)
        {
            group.playerUI.gameObject.SetActive(false);
        }

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

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < uiGroups.Count;
    }
    
    public void RefreshBridgeQuadrants()
    {
        RegisterBridgeQuadrants();
    }
}