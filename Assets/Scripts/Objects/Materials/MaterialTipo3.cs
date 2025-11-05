using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class MaterialTipo3 : MonoBehaviour, IHitable, IUIActivatable
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
        
        materialInfo.layerIndex = 2;
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Metal; // MaterialTipo3 es metal
        
        gameObject.tag = "BridgeLayer2";
    }

    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
} 