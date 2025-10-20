using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MaterialesPrefabs", menuName = "Bridge/Material Prefabs")]
public class MaterialPrefabSO : ScriptableObject
{
    [System.Serializable]
    public class MaterialPorEra
    {
        public BridgeQuadrantSO.EraType era;
        public GameObject prefab;
    }
    
    [Header("Material Tipo 1 (Base)")]
    [Tooltip("Prefabs para el material tipo 1 (capa base) por cada era.")]
    public List<MaterialPorEra> materialesTipo1;
    
    [Header("Material Tipo 2 (Soporte)")]
    [Tooltip("Prefabs para el material tipo 2 (capa soporte) por cada era.")]
    public List<MaterialPorEra> materialesTipo2;
    
    [Header("Material Tipo 3 (Superficie)")]
    [Tooltip("Prefabs para el material tipo 3 (capa superior) por cada era.")]
    [FormerlySerializedAs("materialesTipo4")]
    public List<MaterialPorEra> materialesTipo3;
    
    public GameObject GetMaterialPrefab(int tipo, BridgeQuadrantSO.EraType era)
    {
        List<MaterialPorEra> lista = null;
        
        switch (tipo)
        {
            case 1: lista = materialesTipo1; break;
            case 2: lista = materialesTipo2; break;
            case 3: lista = materialesTipo3; break;
            case 4:
                Debug.LogWarning("MaterialPrefabSO: El material tipo 4 está obsoleto. Usando la configuración del material tipo 3 (superficie).", this);
                lista = materialesTipo3;
                break;
            default: 
                Debug.LogError($"Tipo de material no válido: {tipo}. Debe ser 1, 2 o 3.", this);
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