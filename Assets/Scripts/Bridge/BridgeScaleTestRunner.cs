using System.Collections;
using UnityEngine;

/// <summary>
/// Script de prueba para verificar que las escalas del puente se aplican correctamente al inicializar
/// Este script puede ser agregado temporalmente a una escena para probar las correcciones
/// </summary>
public class BridgeScaleTestRunner : MonoBehaviour
{
    [Header("Prueba de Escalas del Puente")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private float testDelay = 2f;

    private void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunScaleTest());
        }
    }

    private IEnumerator RunScaleTest()
    {
        // Esperar a que el sistema se inicialice completamente
        yield return new WaitForSeconds(testDelay);

        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }

        if (bridgeGrid == null)
        {
            Debug.LogError("❌ No se encontró BridgeConstructionGrid para probar");
            yield break;
        }

        Debug.Log("🧪 Iniciando prueba de escalas del puente...");

        // Probar la aplicación de escalas después de la inicialización
        TestInitialScales();

        // Probar cambio de escalas en tiempo de ejecución
        yield return new WaitForSeconds(1f);
        TestRuntimeScaleChanges();

        // Probar métodos de preset
        yield return new WaitForSeconds(1f);
        TestPresetMethods();

        Debug.Log("✅ Prueba de escalas del puente completada");
    }

    private void TestInitialScales()
    {
        Debug.Log("🔍 Probando escalas iniciales...");
        
        // Verificar que las escalas configuradas se aplicaron correctamente
        var currentLayerScales = bridgeGrid.layerScales;
        var currentLayerHeights = bridgeGrid.layerHeights;

        Debug.Log($"📏 Escalas configuradas: {string.Join(", ", currentLayerScales)}");
        Debug.Log($"📐 Alturas configuradas: {string.Join(", ", currentLayerHeights)}");

        // Forzar aplicación de escalas para verificar que funciona
        bridgeGrid.ForceApplyScales();
    }

    private void TestRuntimeScaleChanges()
    {
        Debug.Log("🔄 Probando cambios de escala en tiempo de ejecución...");

        // Guardar escalas originales
        Vector3[] originalScales = new Vector3[bridgeGrid.layerScales.Length];
        System.Array.Copy(bridgeGrid.layerScales, originalScales, bridgeGrid.layerScales.Length);

        // Cambiar escalas temporalmente
        bridgeGrid.layerScales[0] = new Vector3(1.5f, 1.5f, 1.5f); // Base más grande
        bridgeGrid.layerScales[1] = new Vector3(0.8f, 2.0f, 0.8f); // Soporte más alto y delgado

        // Aplicar las nuevas escalas
        bridgeGrid.ApplyCurrentScales();

        Debug.Log("✨ Escalas modificadas aplicadas");

        // Restaurar escalas originales
        System.Array.Copy(originalScales, bridgeGrid.layerScales, originalScales.Length);
        bridgeGrid.ApplyCurrentScales();

        Debug.Log("🔙 Escalas originales restauradas");
    }    private void TestPresetMethods()
    {
        Debug.Log("🎨 Probando métodos preset...");

        // Probar preset de puente estándar
        bridgeGrid.SetStandardBridgeHeights();

        // Probar preset de puente alto
        bridgeGrid.SetHighBridgeHeights();

        // Probar preset elegante
        bridgeGrid.SetElegantBridgeScales();

        // Volver a escalas uniformes
        bridgeGrid.SetUniformScales();

        Debug.Log("🎯 Presets probados exitosamente");
    }

    [ContextMenu("Ejecutar Prueba Manual")]
    public void RunTestManually()
    {
        StartCoroutine(RunScaleTest());
    }

    [ContextMenu("Verificar Estado Actual")]
    public void VerifyCurrentState()
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }

        if (bridgeGrid != null)
        {
            Debug.Log("📊 Estado actual del BridgeConstructionGrid:");
            Debug.Log($"   Tamaño de grilla: {bridgeGrid.gridWidth} x {bridgeGrid.gridLength}");
            Debug.Log($"   Tamaño de cuadrante: {bridgeGrid.quadrantSize}");
            Debug.Log($"   Escalas de capas: {string.Join(", ", bridgeGrid.layerScales)}");
            Debug.Log($"   Alturas de capas: {string.Join(", ", bridgeGrid.layerHeights)}");
        }
        else
        {
            Debug.LogError("❌ BridgeConstructionGrid no encontrado");
        }
    }
}
