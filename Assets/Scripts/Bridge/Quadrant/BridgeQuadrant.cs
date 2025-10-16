using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BridgeQuadrant : MonoBehaviour
{
    // Este script se usa principalmente como placeholder para identificar los cuadrantes del puente
    // La lógica principal está en BridgeConstructionGrid y BridgeQuadrantSO

    [Header("UI de Capas")]
    [Tooltip("UI que se muestra cuando se puede construir la capa 0 (Base)")]
    [SerializeField] private Image layer0UI;

    [Tooltip("UI que se muestra cuando se puede construir la capa 1 (Soporte)")]
    [SerializeField] private Image layer1UI;

    [Tooltip("UI que se muestra cuando se puede construir la capa 2 (Estructura)")]
    [SerializeField] private Image layer2UI;

    private int currentLayer = -1; // -1 significa vacío, 0-2 son las capas construidas

    public Image GetLayerUI(int layerIndex)
    {
        return layerIndex switch
        {
            0 => layer0UI,
            1 => layer1UI,
            2 => layer2UI,
            _ => null
        };
    }

    public bool CanBuildLayer(int layerIndex)
    {
        // Solo se puede construir una capa si la anterior está construida
        // Capa 0 se puede construir si el cuadrante está vacío (currentLayer == -1)
        // Capa 1 se puede construir si la capa 0 está construida (currentLayer == 0)
        // Capa 2 se puede construir si la capa 1 está construida (currentLayer == 1)

        if (layerIndex == 0)
            return currentLayer == -1;

        return currentLayer == layerIndex - 1;
    }

    public void SetCurrentLayer(int layer)
    {
        currentLayer = layer;
    }

    public int GetCurrentLayer()
    {
        return currentLayer;
    }

    private void Awake()
    {
        // Asegurar que este objeto tenga el tag correcto
        if (gameObject.tag != "BridgeQuadrant")
        {
            gameObject.tag = "BridgeQuadrant";
            Debug.Log($"Se ha establecido automáticamente el tag BridgeQuadrant en {gameObject.name}");
        }
    }
}
