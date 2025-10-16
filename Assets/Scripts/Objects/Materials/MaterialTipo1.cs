using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class MaterialTipo1 : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración del material")]
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 0;
    
    public int UIIndex => uiIndex;
    
    private void Start()
    {
        EnsureBridgeMaterialInfo();
    }
    
    private void EnsureBridgeMaterialInfo()
    {
        BridgeMaterialInfo materialInfo = GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = gameObject.AddComponent<BridgeMaterialInfo>();
        }

        materialInfo.layerIndex = 0;
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Wood; // MaterialTipo1 es madera

        gameObject.tag = "BridgeLayer0";
    }
    
    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }
 
    public void CombinarConResina()
    {
        Debug.Log("Palo de madera listo para ser combinado con resina.");
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}