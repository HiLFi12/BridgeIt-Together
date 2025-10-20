using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

/// <summary>
/// Clase que representa el material de la capa superior del puente (nuevo tipo 3).
/// En la era prehistórica, este material es un adoquín.
/// </summary>
public class MaterialTipo4 : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración del material")]
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;

    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 0;
    public int UIIndex => uiIndex;

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
        
        // Configurar el material como superficie (índice 2)
        materialInfo.layerIndex = 2;
        materialInfo.era = era;
        materialInfo.materialType = BridgeQuadrantSO.MaterialType.Adoquin; // Superficie usa adoquín
        
        // Asegurarnos de que el objeto tenga el tag correcto
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
