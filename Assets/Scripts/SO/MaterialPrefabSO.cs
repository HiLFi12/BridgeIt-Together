using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialesPrefabs", menuName = "Bridge/Material Prefabs")]
public class MaterialPrefabSO : ScriptableObject
{
    [System.Serializable]
    public class MaterialPorEra
    {
        public BridgeQuadrantSO.EraType era;
        public GameObject prefab;
    }
    
    [Header("Material Tipo 1 (Capa 1)")]
    [Tooltip("Prefabs para el material tipo 1 por cada era.")]
    public List<MaterialPorEra> materialesTipo1;
    
    [Header("Material Tipo 2 (Capa 2)")]
    [Tooltip("Prefabs para el material tipo 2 por cada era.")]
    public List<MaterialPorEra> materialesTipo2;
    
    [Header("Material Tipo 3 (Capa 3)")]
    [Tooltip("Prefabs para el material tipo 3 por cada era.")]
    public List<MaterialPorEra> materialesTipo3;
    
    [Header("Material Tipo 4 (Capa 4)")]
    [Tooltip("Prefabs para el material tipo 4 por cada era.")]
    public List<MaterialPorEra> materialesTipo4;
    
    public GameObject GetMaterialPrefab(int tipo, BridgeQuadrantSO.EraType era)
    {
        List<MaterialPorEra> lista = null;
        
        switch (tipo)
        {
            case 1: lista = materialesTipo1; break;
            case 2: lista = materialesTipo2; break;
            case 3: lista = materialesTipo3; break;
            case 4: lista = materialesTipo4; break;
            default: 
                Debug.LogError($"Tipo de material no válido: {tipo}. Debe ser entre 1 y 4.");
                return null;
        }
        
        if (lista == null) return null;
        
        foreach (MaterialPorEra material in lista)
        {
            if (material.era == era)
                return material.prefab;
        }
        
        Debug.LogWarning($"No se encontró prefab para material tipo {tipo} de la era {era}.");
        return null;
    }
} 