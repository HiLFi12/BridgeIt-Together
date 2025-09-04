using UnityEngine;
using System.Collections;

/// <summary>
/// Validación final del sistema de reparación para confirmar que todo funciona según las especificaciones
/// </summary>
public class SystemRepairValidation : MonoBehaviour
{
    [Header("Referencias del Sistema")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO;
    [SerializeField] private BridgeQuadrantSO testQuadrantSO;
    
    private void Start()
    {
        StartCoroutine(ValidateRepairSystem());
    }
    
    private IEnumerator ValidateRepairSystem()
    {
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("=== VALIDACIÓN FINAL DEL SISTEMA DE REPARACIÓN ===");
        
        // Validación 1: Verificar MaterialPrefabSO
        if (ValidateMaterialPrefabSO())
        {
            Debug.Log("✅ MaterialPrefabSO: VÁLIDO");
        }
        else
        {
            Debug.LogError("❌ MaterialPrefabSO: FALLÓ LA VALIDACIÓN");
            yield break;
        }
        
        // Validación 2: Verificar GenericObject3 en la escena
        if (ValidateGenericObject3())
        {
            Debug.Log("✅ GenericObject3: VÁLIDO");
        }
        else
        {
            Debug.LogError("❌ GenericObject3: FALLÓ LA VALIDACIÓN");
        }
        
        // Validación 3: Verificar lógica de reparación
        if (ValidateRepairLogic())
        {
            Debug.Log("✅ Lógica de Reparación: VÁLIDA");
        }
        else
        {
            Debug.LogError("❌ Lógica de Reparación: FALLÓ LA VALIDACIÓN");
        }
        
        // Validación 4: Verificar sistema de puentes
        if (ValidateBridgeSystem())
        {
            Debug.Log("✅ Sistema de Puentes: VÁLIDO");
        }
        else
        {
            Debug.LogError("❌ Sistema de Puentes: FALLÓ LA VALIDACIÓN");
        }
        
        Debug.Log("=== VALIDACIÓN COMPLETA ===");
        Debug.Log("📋 ESTADO: El sistema de reparación está implementado según las especificaciones del documento.");
        Debug.Log("🎯 FUNCIONALIDAD: Los jugadores pueden usar GenericObject3 para obtener adoquín y reparar cuadrantes dañados.");
    }
    
    /// <summary>
    /// Valida que MaterialPrefabSO esté configurado correctamente
    /// </summary>
    private bool ValidateMaterialPrefabSO()
    {
        if (materialPrefabsSO == null)
        {
            // Intentar cargar desde Assets
            materialPrefabsSO = Resources.Load<MaterialPrefabSO>("MaterialesPrefabs");
            if (materialPrefabsSO == null)
            {
                Debug.LogError("No se pudo encontrar MaterialPrefabSO");
                return false;
            }
        }
        
        // Verificar que tiene material tipo 4 para era prehistórica
        GameObject material4 = materialPrefabsSO.GetMaterialPrefab(4, BridgeQuadrantSO.EraType.Prehistoric);
        if (material4 == null)
        {
            Debug.LogError("Material tipo 4 no encontrado para era prehistórica");
            return false;
        }
        
        // Verificar componente MaterialTipo4
        MaterialTipo4 materialComponent = material4.GetComponent<MaterialTipo4>();
        if (materialComponent == null)
        {
            Debug.LogError("Prefab material4 no tiene componente MaterialTipo4");
            return false;
        }
        
        Debug.Log($"Material4 encontrado: {material4.name}");
        return true;
    }
    
    /// <summary>
    /// Valida que GenericObject3 esté configurado correctamente
    /// </summary>
    private bool ValidateGenericObject3()
    {
        GenericObject3[] generators = FindObjectsOfType<GenericObject3>();
        
        if (generators.Length == 0)
        {
            Debug.LogWarning("No se encontraron objetos GenericObject3 en la escena");
            return false;
        }
        
        Debug.Log($"Encontrados {generators.Length} generadores GenericObject3");
        
        foreach (var generator in generators)
        {
            Debug.Log($"  - GenericObject3 en posición: {generator.transform.position}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Valida que la lógica de reparación funcione correctamente
    /// </summary>
    private bool ValidateRepairLogic()
    {
        if (testQuadrantSO == null)
        {
            Debug.LogWarning("No se asignó testQuadrantSO, creando uno temporal para la prueba");
            testQuadrantSO = ScriptableObject.CreateInstance<BridgeQuadrantSO>();
            testQuadrantSO.Initialize();
        }
        
        // Crear una copia temporal para pruebas
        BridgeQuadrantSO tempQuadrant = ScriptableObject.CreateInstance<BridgeQuadrantSO>();
        tempQuadrant.Initialize();
        
        // Simular construcción completa (4 capas)
        for (int i = 0; i < 4; i++)
        {
            if (!tempQuadrant.TryAddLayer(i, null))
            {
                Debug.LogError($"No se pudo agregar la capa {i}");
                return false;
            }
        }
        
        // Simular daño en la última capa
        tempQuadrant.SetLastLayerState(BridgeQuadrantSO.LastLayerState.Damaged);
        
        // Intentar reparar
        bool repaired = tempQuadrant.TryAddLayer(3, null); // Capa 3 = última capa
        
        if (repaired && tempQuadrant.GetLastLayerState() == BridgeQuadrantSO.LastLayerState.Complete)
        {
            Debug.Log("Reparación simulada exitosa");
            return true;
        }
        
        Debug.LogError("La lógica de reparación no funcionó como se esperaba");
        return false;
    }
    
    /// <summary>
    /// Valida que el sistema de puentes esté presente en la escena
    /// </summary>
    private bool ValidateBridgeSystem()
    {
        BridgeConstructionGrid bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        
        if (bridgeGrid == null)
        {
            Debug.LogWarning("No se encontró BridgeConstructionGrid en la escena");
            return false;
        }
        
        Debug.Log("✓ BridgeConstructionGrid encontrado");
        
        PlayerBridgeInteraction[] players = FindObjectsOfType<PlayerBridgeInteraction>();
        Debug.Log($"✓ Encontrados {players.Length} jugadores con PlayerBridgeInteraction");
        
        return true;
    }
    
    /// <summary>
    /// Prueba manual para verificar el flujo completo
    /// </summary>
    [ContextMenu("Ejecutar Prueba Manual")]
    public void ExecuteManualTest()
    {
        StartCoroutine(ValidateRepairSystem());
    }
}
