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
        CheckClassExists("VehicleReturnTriggerManager");
        CheckClassExists("VehicleReturnTrigger");
        CheckClassExists("AutoMovement");
        CheckClassExists("VehicleBridgeCollision");
        
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
    
}
