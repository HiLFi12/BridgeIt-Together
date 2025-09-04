using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeQuadrant : MonoBehaviour
{
    // Este script se usa principalmente como placeholder para identificar los cuadrantes del puente
    // La lógica principal está en BridgeConstructionGrid y BridgeQuadrantSO
    
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