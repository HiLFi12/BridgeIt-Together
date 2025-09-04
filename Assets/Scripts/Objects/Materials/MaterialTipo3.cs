using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class MaterialTipo3 : MonoBehaviour, IHitable
{
    [Header("Configuración del material")]
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    
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

    public void OnLaunched(Vector3 targetPosition)
    {
    }
} 