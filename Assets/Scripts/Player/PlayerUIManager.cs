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

        [Tooltip("UI de otros objetos")]
        public Image[] othersUI;
    }

    [Header("Grupos de UI")]
    [SerializeField] private List<UIGroup> uiGroups = new List<UIGroup>();

    private void Start()
    {
        for (int i = 0; i < uiGroups.Count; i++)
        {
            TurnOffUI(i);
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
        else
        {
            Debug.LogWarning($"PlayerUIManager: PlayerUI en índice {index} es null");
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

    public void 
        TurnOffUI(int index)
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
}