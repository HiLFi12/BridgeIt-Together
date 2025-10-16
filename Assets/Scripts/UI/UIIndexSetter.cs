using UnityEngine;

public class UIIndexSetter : MonoBehaviour, IUIActivatable
{
    [SerializeField] private int uiIndex = -1;

    public int UIIndex => uiIndex;

    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }
}

