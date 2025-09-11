using UnityEngine;

/// <summary>
/// Script de verificación para comprobar si el sistema de triggers está funcionando
/// </summary>
public class TriggerSystemVerification : MonoBehaviour
{
    [Header("Verificación del Sistema")]
    [SerializeField] private VehicleReturnTriggerManager triggerManager;

    [ContextMenu("Verificar Sistema de Triggers")]
    public void VerifyTriggerSystem()
    {
        Debug.Log("=== VERIFICACIÓN DEL SISTEMA DE TRIGGERS ===");

        // Verificar VehicleReturnTriggerManager
        if (triggerManager == null)
        {
            triggerManager = FindFirstObjectByType<VehicleReturnTriggerManager>();
        }

        if (triggerManager == null)
        {
            Debug.LogError("❌ No se encontró VehicleReturnTriggerManager");
            return;
        }
        else
        {
            Debug.Log("✅ VehicleReturnTriggerManager encontrado");
        }

        // Verificar métodos públicos del sistema
        try
        {
            int triggerCount = triggerManager.GetActiveTriggerCount();
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
        if (triggerManager == null)
        {
            Debug.LogError("Asigna un VehicleReturnTriggerManager primero");
            return;
        }

        // Crear un trigger de prueba
        GameObject testTrigger = new GameObject("TestTrigger");
        testTrigger.transform.position = transform.position;

        BoxCollider col = testTrigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one * 2f;

        // Agregar al sistema
        triggerManager.AddTrigger(col, true);

        Debug.Log("✅ Trigger de prueba creado y agregado al sistema");

        // Programar destrucción en 5 segundos
        Destroy(testTrigger, 5f);
    }
}
