using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

/// <summary>
/// Clase que representa un material tipo 4 (para la capa 4 del puente).
/// En la era prehistórica, este material es un adoquín.
/// </summary>
public class MaterialTipo4 : MonoBehaviour, IHitable
{
    [Header("Configuración del material")]
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    
    private void Start()
    {
        // Asegurarse de que tiene un componente BridgeMaterialInfo
        EnsureBridgeMaterialInfo();
    }
    
    private void EnsureBridgeMaterialInfo()
    {
        BridgeMaterialInfo materialInfo = GetComponent<BridgeMaterialInfo>();
        if (materialInfo == null)
        {
            materialInfo = gameObject.AddComponent<BridgeMaterialInfo>();
        }
        
        // Configurar el material como tipo 4 (índice 3 corresponde a la capa 4)
        materialInfo.layerIndex = 3;
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Adoquin; // Solo MaterialTipo4 es adoquín
        
        // Asegurarnos de que el objeto tenga el tag correcto
        gameObject.tag = "BridgeLayer3";
    }

    public void OnLaunched(Vector3 targetPosition)
    {
        
    }
} 