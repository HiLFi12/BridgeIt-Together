using UnityEngine;

/// <summary>
/// Script de verificación para comprobar si el sistema de triggers está funcionando
/// </summary>
public class TriggerSystemVerification : MonoBehaviour
{
    [Header("Verificación del Sistema")]
    [SerializeField] private AutoGenerator autoGenerator;
    
    [ContextMenu("Verificar Sistema de Triggers")]
    public void VerifyTriggerSystem()
    {
        Debug.Log("=== VERIFICACIÓN DEL SISTEMA DE TRIGGERS ===");
        
        // Verificar AutoGenerator
        if (autoGenerator == null)
        {
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
        }
        
        if (autoGenerator == null)
        {
            Debug.LogError("❌ No se encontró AutoGenerator");
            return;
        }
        else
        {
            Debug.Log("✅ AutoGenerator encontrado");
        }
        
        // Verificar VehicleReturnTriggerManager
        VehicleReturnTriggerManager triggerManager = autoGenerator.GetComponent<VehicleReturnTriggerManager>();
        if (triggerManager == null)
        {
            Debug.LogWarning("⚠️ VehicleReturnTriggerManager no encontrado - se creará automáticamente al iniciar");
        }
        else
        {
            Debug.Log("✅ VehicleReturnTriggerManager encontrado");
        }
        
        // Verificar métodos públicos del sistema
        try
        {
            int triggerCount = autoGenerator.GetActiveTriggerCount();
            Debug.Log($"✅ Método GetActiveTriggerCount() funciona - Triggers activos: {triggerCount}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error en GetActiveTriggerCount(): {e.Message}");
        }
        
        // Verificar componentes necesarios
        bool hasAutoMovement = FindFirstObjectByType<AutoMovement>() != null;
        bool hasVehicleBridgeCollision = FindFirstObjectByType<VehicleBridgeCollision>() != null;
        
        Debug.Log($"AutoMovement en escena: {(hasAutoMovement ? "✅" : "❌")}");
        Debug.Log($"VehicleBridgeCollision en escena: {(hasVehicleBridgeCollision ? "✅" : "❌")}");
        
        Debug.Log("=== FIN VERIFICACIÓN ===");
    }
    
    [ContextMenu("Prueba Rápida de Triggers")]
    public void QuickTriggerTest()
    {
        if (autoGenerator == null)
        {
            Debug.LogError("Asigna un AutoGenerator primero");
            return;
        }
        
        // Crear un trigger de prueba
        GameObject testTrigger = new GameObject("TestTrigger");
        testTrigger.transform.position = transform.position;
        
        BoxCollider col = testTrigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one * 2f;
        
        // Agregar al sistema
        autoGenerator.AddReturnTrigger(col, true);
        
        Debug.Log("✅ Trigger de prueba creado y agregado al sistema");
        
        // Programar destrucción en 5 segundos
        Destroy(testTrigger, 5f);
    }
}
