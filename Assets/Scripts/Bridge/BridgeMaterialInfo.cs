using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este componente almacena información sobre un material de construcción del puente
public class BridgeMaterialInfo : MonoBehaviour
{    [Header("Información del Material")]
    public int layerIndex = 0; // 0: Base, 1: Soporte, 2: Estructura, 3: Superficie
    public BridgeQuadrantSO.EraType era; // Era histórica del material
    public BridgeQuadrantSO.MaterialType materialType = BridgeQuadrantSO.MaterialType.Adoquin; // Tipo de material
    
    private void Start()
    {
        // Asegurar que el tag es correcto según el layerIndex
        UpdateTag();
    }
    
    private void UpdateTag()
    {
        switch (layerIndex)
        {
            case 0:
                gameObject.tag = "BridgeLayer0"; // Base
                break;
            case 1:
                gameObject.tag = "BridgeLayer1"; // Soporte
                break;
            case 2:
                gameObject.tag = "BridgeLayer2"; // Estructura
                break;
            case 3:
                gameObject.tag = "BridgeLayer3"; // Superficie
                break;
            default:
                gameObject.tag = "BridgeLayer0"; // Por defecto, asignamos Base
                break;
        }
        
        Debug.Log($"BridgeMaterialInfo inicializado: {gameObject.name}, LayerIndex: {layerIndex}, Era: {era}, Tag: {gameObject.tag}");
    }
} 