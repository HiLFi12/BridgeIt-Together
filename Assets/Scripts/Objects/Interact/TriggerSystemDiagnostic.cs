using UnityEngine;
using System.Reflection;

/// <summary>
/// Script de diagnóstico para identificar problemas con el sistema de triggers
/// </summary>
public class TriggerSystemDiagnostic : MonoBehaviour
{
    void Start()
    {
        DiagnoseSystem();
    }
    
    [ContextMenu("Ejecutar Diagnóstico Completo")]
    public void DiagnoseSystem()
    {
        Debug.Log("=== DIAGNÓSTICO DEL SISTEMA DE TRIGGERS ===");
        
        // 1. Verificar que las clases existen
        CheckClassExists("AutoGenerator");
        CheckClassExists("VehicleReturnTriggerManager");
        CheckClassExists("VehicleReturnTrigger");
        CheckClassExists("AutoMovement");
        CheckClassExists("VehicleBridgeCollision");
        
        // 2. Verificar AutoGenerator en la escena
        CheckAutoGeneratorInScene();
        
        // 3. Verificar métodos públicos
        CheckAutoGeneratorMethods();
        
        Debug.Log("=== FIN DIAGNÓSTICO ===");
    }
    
    void CheckClassExists(string className)
    {
        try
        {
            System.Type type = System.Type.GetType(className);
            if (type != null)
            {
                Debug.Log($"✅ Clase {className} encontrada");
            }
            else
            {
                // Buscar en todos los assemblies
                bool found = false;
                foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(className);
                    if (type != null)
                    {
                        Debug.Log($"✅ Clase {className} encontrada en assembly {assembly.GetName().Name}");
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogError($"❌ Clase {className} NO encontrada");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error verificando clase {className}: {e.Message}");
        }
    }
    
    void CheckAutoGeneratorInScene()
    {
        AutoGenerator autoGen = FindFirstObjectByType<AutoGenerator>();
        if (autoGen != null)
        {
            Debug.Log("✅ AutoGenerator encontrado en la escena");
            
            // Verificar si tiene el campo triggersRetorno
            try
            {
                System.Type type = autoGen.GetType();
                FieldInfo field = type.GetField("triggersRetorno", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    Debug.Log("✅ Campo triggersRetorno encontrado");
                }
                else
                {
                    Debug.LogWarning("⚠️ Campo triggersRetorno no encontrado");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error verificando campos: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ AutoGenerator no encontrado en la escena");
        }
    }
    
    void CheckAutoGeneratorMethods()
    {
        AutoGenerator autoGen = FindFirstObjectByType<AutoGenerator>();
        if (autoGen == null) return;
        
        // Lista de métodos que deberían existir
        string[] methods = {
            "SetTriggersActive",
            "AddReturnTrigger", 
            "RemoveReturnTrigger",
            "GetActiveTriggerCount"
        };
        
        System.Type type = autoGen.GetType();
        
        foreach (string methodName in methods)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                Debug.Log($"✅ Método {methodName} encontrado");
            }
            else
            {
                Debug.LogError($"❌ Método {methodName} NO encontrado");
            }
        }
    }
}
