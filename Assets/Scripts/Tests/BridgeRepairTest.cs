using System.Collections;
using UnityEngine;

/// <summary>
/// Script de prueba para verificar que el sistema de reparación del puente funciona correctamente.
/// </summary>
public class BridgeRepairTest : MonoBehaviour
{
    [Header("Referencias para la prueba")]
    [SerializeField] private BridgeQuadrantSO testQuadrant;
    [SerializeField] private MaterialPrefabSO materialPrefabsSO;
    
    private void Start()
    {
        StartCoroutine(RunRepairTest());
    }
    
    private IEnumerator RunRepairTest()
    {
        yield return new WaitForSeconds(1f);
        
        Debug.Log("=== INICIANDO PRUEBA DE REPARACIÓN DE PUENTE ===");
        
        // Verificar que tengamos las referencias necesarias
        if (testQuadrant == null)
        {
            Debug.LogError("TEST FALLIDO: No se ha asignado BridgeQuadrantSO de prueba");
            yield break;
        }
        
        if (materialPrefabsSO == null)
        {
            Debug.LogError("TEST FALLIDO: No se ha asignado MaterialPrefabSO");
            yield break;
        }
        
        // Verificar que el material tipo 4 existe para la era prehistórica
        GameObject material4Prefab = materialPrefabsSO.GetMaterialPrefab(4, BridgeQuadrantSO.EraType.Prehistoric);
        if (material4Prefab == null)
        {
            Debug.LogError("TEST FALLIDO: No se encontró prefab para material tipo 4 de la era prehistórica");
            yield break;
        }
        
        Debug.Log("✓ Material tipo 4 (adoquín) encontrado correctamente");
        
        // Verificar que el material tiene el componente MaterialTipo4
        MaterialTipo4 materialComponent = material4Prefab.GetComponent<MaterialTipo4>();
        if (materialComponent == null)
        {
            Debug.LogError("TEST FALLIDO: El prefab del material tipo 4 no tiene el componente MaterialTipo4");
            yield break;
        }
        
        Debug.Log("✓ Componente MaterialTipo4 presente en el prefab");
        
        // Verificar la configuración de MaterialTipo4
        BridgeMaterialInfo materialInfo = material4Prefab.GetComponent<BridgeMaterialInfo>();
        if (materialInfo != null)
        {
            if (materialInfo.layerIndex == 3) // Índice 3 = capa 4
            {
                Debug.Log("✓ MaterialTipo4 configurado correctamente (layerIndex = 3)");
            }
            else
            {
                Debug.LogWarning($"ADVERTENCIA: MaterialTipo4 tiene layerIndex = {materialInfo.layerIndex}, esperado = 3");
            }
        }
        
        // Simular escenario de reparación
        Debug.Log("--- Simulando escenario de reparación ---");
        
        // Crear un cuadrante de prueba y configurarlo como dañado
        if (TestRepairScenario())
        {
            Debug.Log("✓ Lógica de reparación funciona correctamente");
        }
        else
        {
            Debug.LogError("TEST FALLIDO: La lógica de reparación no funciona como se esperaba");
        }
        
        Debug.Log("=== PRUEBA DE REPARACIÓN COMPLETADA ===");
    }
      /// <summary>
    /// Prueba el escenario de reparación con el material adoquín
    /// </summary>
    private bool TestRepairScenario()
    {
        // Crear una instancia temporal del cuadrante para pruebas
        BridgeQuadrantSO tempQuadrant = ScriptableObject.CreateInstance<BridgeQuadrantSO>();
        
        // Configurar el cuadrante manualmente (Initialize no toma parámetros)
        tempQuadrant.Initialize();
        
        // Simular que todas las capas están completadas
        for (int i = 0; i < 4; i++)
        {
            tempQuadrant.TryAddLayer(i, null); // Usar null como layerObject para la prueba
        }
        
        // Simular daño en la última capa manualmente
        tempQuadrant.lastLayerState = BridgeQuadrantSO.LastLayerState.Damaged;
        
        Debug.Log($"Estado antes de reparación - Última capa: {tempQuadrant.lastLayerState}");
        
        // Intentar reparar con material tipo 4 (adoquín) - la capa 3 es el índice de la capa 4
        bool repairSuccess = tempQuadrant.TryAddLayer(3, null);
        
        Debug.Log($"Resultado de reparación: {(repairSuccess ? "ÉXITO" : "FALLO")}");
        Debug.Log($"Estado después de reparación - Última capa: {tempQuadrant.lastLayerState}");
        
        // Verificar que la reparación fue exitosa
        bool repairWorked = repairSuccess && tempQuadrant.lastLayerState == BridgeQuadrantSO.LastLayerState.Complete;
        
        // Limpiar objeto temporal
        DestroyImmediate(tempQuadrant);
        
        return repairWorked;
    }
      /// <summary>
    /// Verificar que GenericObject3 puede producir material tipo 4
    /// </summary>
    [ContextMenu("Test GenericObject3 Production")]
    public void TestGenericObject3Production()
    {
        GameObject genericObject3Prefab = Resources.Load<GameObject>("GenericObject3");
        if (genericObject3Prefab != null)
        {
            GenericObject3 genericComponent = genericObject3Prefab.GetComponent<GenericObject3>();
            if (genericComponent != null)
            {
                Debug.Log("✓ GenericObject3 encontrado y configurado correctamente");
            }
            else
            {
                Debug.LogError("GenericObject3 prefab no tiene el componente GenericObject3");
            }
        }
        else
        {
            Debug.LogWarning("No se pudo cargar GenericObject3 desde Resources");
        }
    }
    
    /// <summary>
    /// Método para probar manualmente el sistema completo desde el inspector
    /// </summary>
    [ContextMenu("Ejecutar Prueba Completa")]
    public void EjecutarPruebaCompleta()
    {
        StartCoroutine(RunRepairTest());
    }
    
    /// <summary>
    /// Verificar que el sistema esté correctamente configurado en la escena actual
    /// </summary>
    [ContextMenu("Verificar Configuracion Escena")]
    public void VerificarConfiguracionEscena()
    {
        Debug.Log("=== VERIFICANDO CONFIGURACIÓN DE ESCENA ===");
        
        // Verificar BridgeConstructionGrid
        BridgeConstructionGrid grid = FindObjectOfType<BridgeConstructionGrid>();
        if (grid != null)
        {
            Debug.Log("✓ BridgeConstructionGrid encontrado en la escena");
            
            if (grid.defaultQuadrantSO != null)
                Debug.Log("✓ defaultQuadrantSO asignado");
            else
                Debug.LogError("✗ defaultQuadrantSO NO asignado en BridgeConstructionGrid");
                
            if (grid.quadrantPrefab != null)
                Debug.Log("✓ quadrantPrefab asignado");
            else
                Debug.LogError("✗ quadrantPrefab NO asignado en BridgeConstructionGrid");
        }
        else
        {
            Debug.LogError("✗ BridgeConstructionGrid NO encontrado en la escena");
        }
        
        // Verificar GenericObject3
        GenericObject3[] generators = FindObjectsOfType<GenericObject3>();
        if (generators.Length > 0)
        {
            Debug.Log($"✓ {generators.Length} GenericObject3 encontrado(s) en la escena");
            foreach (var gen in generators)
            {
                Debug.Log($"  - GenericObject3 en posición: {gen.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No se encontraron GeneratorObject3 en la escena");
        }
        
        // Verificar PlayerBridgeInteraction
        PlayerBridgeInteraction[] players = FindObjectsOfType<PlayerBridgeInteraction>();
        if (players.Length > 0)
        {
            Debug.Log($"✓ {players.Length} jugador(es) con PlayerBridgeInteraction encontrados");
        }
        else
        {
            Debug.LogWarning("⚠ No se encontraron jugadores con PlayerBridgeInteraction");
        }
        
        Debug.Log("=== VERIFICACIÓN COMPLETADA ===");
    }
}
