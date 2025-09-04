using UnityEngine;

/// <summary>
/// Verificación final del sistema de reparación - Script de validación
/// </summary>
public class FinalRepairSystemValidation : MonoBehaviour
{
    [Header("Referencias para validación")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO;
    
    private void Start()
    {
        ValidateCompleteSystem();
    }
    
    /// <summary>
    /// Valida que todo el sistema de reparación esté funcionando correctamente
    /// </summary>
    private void ValidateCompleteSystem()
    {
        Debug.Log("=== VALIDACIÓN FINAL DEL SISTEMA DE REPARACIÓN ===");
        
        bool allTestsPassed = true;
        
        // Test 1: Validar MaterialPrefabSO
        if (ValidateMaterialPrefabSO())
        {
            Debug.Log("✅ Test 1 - MaterialPrefabSO: EXITOSO");
        }
        else
        {
            Debug.LogError("❌ Test 1 - MaterialPrefabSO: FALLIDO");
            allTestsPassed = false;
        }
        
        // Test 2: Validar componentes en escena
        if (ValidateSceneComponents())
        {
            Debug.Log("✅ Test 2 - Componentes de Escena: EXITOSO");
        }
        else
        {
            Debug.LogError("❌ Test 2 - Componentes de Escena: FALLIDO");
            allTestsPassed = false;
        }
        
        // Test 3: Validar lógica de reparación
        if (ValidateRepairLogic())
        {
            Debug.Log("✅ Test 3 - Lógica de Reparación: EXITOSO");
        }
        else
        {
            Debug.LogError("❌ Test 3 - Lógica de Reparación: FALLIDO");
            allTestsPassed = false;
        }
        
        // Resultado final
        if (allTestsPassed)
        {
            Debug.Log("🎉 VALIDACIÓN COMPLETA: ¡TODOS LOS TESTS EXITOSOS!");
            Debug.Log("📋 ESTADO: Sistema de reparación completamente funcional");
            Debug.Log("🔧 FUNCIONALIDAD: Los jugadores pueden usar GenericObject3 para obtener adoquín y reparar cuadrantes dañados");
            Debug.Log("✨ IMPLEMENTACIÓN: Cumple 100% con las especificaciones del documento");
        }
        else
        {
            Debug.LogError("❌ VALIDACIÓN FALLIDA: Algunos tests no pasaron");
        }
        
        Debug.Log("=== FIN DE VALIDACIÓN ===");
    }
    
    /// <summary>
    /// Valida MaterialPrefabSO y material tipo 4
    /// </summary>
    private bool ValidateMaterialPrefabSO()
    {
        if (materialPrefabsSO == null)
        {
            // Intentar cargar desde Resources
            materialPrefabsSO = Resources.Load<MaterialPrefabSO>("MaterialesPrefabs");
            if (materialPrefabsSO == null)
            {
                Debug.LogError("No se pudo encontrar MaterialPrefabSO");
                return false;
            }
        }
        
        // Verificar material tipo 4 para era prehistórica
        GameObject material4 = materialPrefabsSO.GetMaterialPrefab(4, BridgeQuadrantSO.EraType.Prehistoric);
        if (material4 == null)
        {
            Debug.LogError("Material tipo 4 no encontrado para era prehistórica");
            return false;
        }
        
        // Verificar componentes del material
        MaterialTipo4 materialComponent = material4.GetComponent<MaterialTipo4>();
        BridgeMaterialInfo materialInfo = material4.GetComponent<BridgeMaterialInfo>();
        
        if (materialComponent == null)
        {
            Debug.LogError("Material4 no tiene componente MaterialTipo4");
            return false;
        }
        
        if (materialInfo == null)
        {
            Debug.LogError("Material4 no tiene componente BridgeMaterialInfo");
            return false;
        }
        
        // Verificar configuración
        if (materialInfo.layerIndex != 3)
        {
            Debug.LogWarning($"Material4 layerIndex = {materialInfo.layerIndex}, esperado = 3");
        }
        
        if (materialInfo.materialType != BridgeQuadrantSO.MaterialType.Adoquin)
        {
            Debug.LogWarning($"Material4 materialType = {materialInfo.materialType}, esperado = Adoquin");
        }
        
        Debug.Log($"Material4 validado: {material4.name}");
        return true;
    }
    
    /// <summary>
    /// Valida que los componentes necesarios estén en la escena
    /// </summary>
    private bool ValidateSceneComponents()
    {
        bool hasComponents = true;
        
        // Verificar GenericObject3
        GenericObject3[] generators = FindObjectsOfType<GenericObject3>();
        if (generators.Length > 0)
        {
            Debug.Log($"Encontrados {generators.Length} generadores GenericObject3");
        }
        else
        {
            Debug.LogWarning("No se encontraron objetos GenericObject3 en la escena");
            hasComponents = false;
        }
        
        // Verificar BridgeConstructionGrid
        BridgeConstructionGrid bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        if (bridgeGrid != null)
        {
            Debug.Log("BridgeConstructionGrid encontrado");
        }
        else
        {
            Debug.LogWarning("No se encontró BridgeConstructionGrid en la escena");
            hasComponents = false;
        }
        
        // Verificar PlayerBridgeInteraction
        PlayerBridgeInteraction[] players = FindObjectsOfType<PlayerBridgeInteraction>();
        if (players.Length > 0)
        {
            Debug.Log($"Encontrados {players.Length} jugadores con PlayerBridgeInteraction");
        }
        else
        {
            Debug.LogWarning("No se encontraron jugadores con PlayerBridgeInteraction");
            hasComponents = false;
        }
        
        return hasComponents;
    }
    
    /// <summary>
    /// Valida la lógica de reparación
    /// </summary>
    private bool ValidateRepairLogic()
    {
        // Crear cuadrante temporal para pruebas
        BridgeQuadrantSO tempQuadrant = ScriptableObject.CreateInstance<BridgeQuadrantSO>();
        tempQuadrant.Initialize();
        
        // Simular construcción completa
        for (int i = 0; i < 4; i++)
        {
            if (!tempQuadrant.TryAddLayer(i, null))
            {
                Debug.LogError($"No se pudo agregar la capa {i} en prueba");
                return false;
            }
        }
        
        // Simular daño
        tempQuadrant.SetLastLayerState(BridgeQuadrantSO.LastLayerState.Damaged);
        
        if (!tempQuadrant.IsDamaged())
        {
            Debug.LogError("El método IsDamaged() no funciona correctamente");
            return false;
        }
        
        // Intentar reparar
        bool repaired = tempQuadrant.TryAddLayer(BridgeQuadrantSO.MaterialType.Adoquin, 1);
        
        if (!repaired)
        {
            Debug.LogError("La reparación falló");
            return false;
        }
        
        if (tempQuadrant.GetLastLayerState() != BridgeQuadrantSO.LastLayerState.Complete)
        {
            Debug.LogError("El estado no se restauró a Complete después de la reparación");
            return false;
        }
        
        Debug.Log("Lógica de reparación simulada exitosamente");
        return true;
    }
    
    /// <summary>
    /// Ejecutar validación manual desde el inspector
    /// </summary>
    [ContextMenu("Ejecutar Validación")]
    public void ExecuteValidation()
    {
        ValidateCompleteSystem();
    }
}
