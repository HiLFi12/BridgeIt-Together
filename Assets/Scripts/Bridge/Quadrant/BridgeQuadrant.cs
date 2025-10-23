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

    public BridgeConstructionGrid grid;

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

        bool canBuild = false;
        if (layerIndex == 0)
            canBuild = currentLayer == -1;
        else
            canBuild = currentLayer == layerIndex - 1;

        if (!canBuild) return false;

        // Check reachability
        if (grid == null) return true; // fallback if no grid reference
        int x = GetX();
        int z = GetZ();
        if (x == -1 || z == -1) return true; // fallback if can't parse
        return grid.IsQuadrantReachable(x, z);
    }

    public void SetCurrentLayer(int layer)
    {
        currentLayer = layer;
    }

    public int GetCurrentLayer()
    {
        return currentLayer;
    }

    private int GetX()
    {
        string[] parts = gameObject.name.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[1], out int x)) return x;
        return -1;
    }

    private int GetZ()
    {
        string[] parts = gameObject.name.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int z)) return z;
        return -1;
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
