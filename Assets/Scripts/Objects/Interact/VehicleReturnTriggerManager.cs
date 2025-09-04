using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja los triggers que devuelven vehículos al pool cuando los tocan
/// </summary>
public class VehicleReturnTriggerManager : MonoBehaviour
{
    private AutoGenerator autoGenerator;
    private BridgeItTogether.Gameplay.Abstractions.IVehiclePoolService poolService;
    private Collider[] triggers;
    private Dictionary<Collider, VehicleReturnTrigger> triggerComponents = new Dictionary<Collider, VehicleReturnTrigger>();
    
    /// <summary>
    /// Inicializa el manager con los triggers proporcionados
    /// </summary>
    /// <param name="generator">Referencia al AutoGenerator</param>
    /// <param name="triggerColliders">Array de colliders que actuarán como triggers</param>
    /// <param name="activateOnStart">Si activar los triggers inmediatamente</param>
    public void Initialize(AutoGenerator generator, Collider[] triggerColliders, bool activateOnStart = true)
    {
        autoGenerator = generator;
        poolService = null;
        triggers = triggerColliders;
        
        if (triggers != null && triggers.Length > 0)
        {
            SetupTriggers(activateOnStart);
        }
    }

    /// <summary>
    /// Inicializa el manager usando un servicio de pool genérico (para VehicleSpawner)
    /// </summary>
    /// <param name="pool">Servicio de pool que gestionará el retorno</param>
    /// <param name="triggerColliders">Colliders a usar como triggers</param>
    /// <param name="activateOnStart">Activar inmediatamente</param>
    public void Initialize(BridgeItTogether.Gameplay.Abstractions.IVehiclePoolService pool, Collider[] triggerColliders, bool activateOnStart = true)
    {
        autoGenerator = null;
        poolService = pool;
        triggers = triggerColliders;

        if (triggers != null && triggers.Length > 0)
        {
            SetupTriggers(activateOnStart);
        }
    }
    
    /// <summary>
    /// Configura los triggers para detectar vehículos
    /// </summary>
    /// <param name="activate">Si activar los triggers inmediatamente</param>
    private void SetupTriggers(bool activate)
    {
        foreach (Collider trigger in triggers)
        {
            if (trigger == null) continue;
            
            // Asegurar que el collider es un trigger
            trigger.isTrigger = true;
            
            // Agregar el componente VehicleReturnTrigger
            VehicleReturnTrigger triggerComponent = trigger.GetComponent<VehicleReturnTrigger>();
            if (triggerComponent == null)
            {
                triggerComponent = trigger.gameObject.AddComponent<VehicleReturnTrigger>();
            }
            
            // Configurar el trigger con referencia a este manager
            triggerComponent.Initialize(this);
            triggerComponent.SetActive(activate);
            
            // Almacenar en el diccionario para acceso rápido
            triggerComponents[trigger] = triggerComponent;
            
            Debug.Log($"Trigger configurado en {trigger.name}");
        }
    }    /// <summary>
    /// Callback llamado cuando un vehículo toca un trigger
    /// </summary>
    /// <param name="vehicle">El GameObject del vehículo</param>
    /// <param name="trigger">El trigger que fue tocado</param>
    public void OnVehicleTriggered(GameObject vehicle, Collider trigger)
    {
        if (vehicle == null) return;

        if (autoGenerator != null)
        {
            // Verificar que el objeto tenga los componentes de vehículo
            AutoMovement autoMovement = vehicle.GetComponent<AutoMovement>();
            VehicleBridgeCollision vehicleCollision = vehicle.GetComponent<VehicleBridgeCollision>();
            
            if (autoMovement != null || vehicleCollision != null)
            {
                // Debug: Mostrar información sobre los vehículos activos antes de devolver uno
                VehiclePool pool = autoGenerator.GetComponent<VehiclePool>();
                if (pool != null)
                {
                    int vehiculosAntesDeRetorno = pool.GetActiveVehicleCount();
                    Debug.Log($"Vehículo {vehicle.name} devuelto al pool. Vehículos activos antes: {vehiculosAntesDeRetorno}, después: {vehiculosAntesDeRetorno - 1}");
                }
                else
                {
                    Debug.Log($"Vehículo {vehicle.name} devuelto al pool");
                }
                  // SOLUCIÓN AL PROBLEMA: Limpiar el vehículo de todos los triggers de condición antes de devolverlo al pool
                LimpiarVehiculoDeTriggersDeCondicion(vehicle);
                
                autoGenerator.ReturnAutoToPool(vehicle);
                
                // NOTA: No necesitamos llamar OnAutoReturnedToPool() aquí porque ReturnAutoToPool 
                // ya llama internamente a NotificarAutoDevueltoAlPool que maneja el conteo de rondas
            }
        }
        else if (poolService != null)
        {
            // Limpieza de triggers de condición igual que en el flujo legacy
            LimpiarVehiculoDeTriggersDeCondicion(vehicle);
            poolService.ReturnVehicleToPool(vehicle);
        }
    }
    
    /// <summary>
    /// Limpia un vehículo de todas las listas de vehículos contados en GameConditionTriggers
    /// SOLUCIÓN: Esto previene que los vehículos reutilizados del pool sean ignorados en conteos futuros
    /// </summary>
    /// <param name="vehicle">El vehículo a limpiar</param>
    private void LimpiarVehiculoDeTriggersDeCondicion(GameObject vehicle)
    {
        // Encontrar todos los GameConditionTrigger en la escena
        GameConditionTrigger[] conditionTriggers = FindObjectsByType<GameConditionTrigger>(FindObjectsSortMode.None);
        
        foreach (GameConditionTrigger trigger in conditionTriggers)
        {
            if (trigger != null)
            {
                // Forzar la limpieza del vehículo específico
                trigger.RemoverVehiculoContado(vehicle);
            }
        }
        
        Debug.Log($"🧹 Vehículo {vehicle.name} limpiado de todos los triggers de condición para reutilización del pool");
    }
    
    /// <summary>
    /// Activa o desactiva todos los triggers
    /// </summary>
    /// <param name="active">Estado activo/inactivo</param>
    public void SetTriggersActive(bool active)
    {
        foreach (var triggerComponent in triggerComponents.Values)
        {
            if (triggerComponent != null)
            {
                triggerComponent.SetActive(active);
            }
        }
    }
    
    /// <summary>
    /// Activa o desactiva un trigger específico
    /// </summary>
    /// <param name="trigger">El collider del trigger</param>
    /// <param name="active">Estado activo/inactivo</param>
    public void SetTriggerActive(Collider trigger, bool active)
    {
        if (triggerComponents.ContainsKey(trigger))
        {
            triggerComponents[trigger].SetActive(active);
        }
    }
    
    /// <summary>
    /// Agrega un nuevo trigger al sistema
    /// </summary>
    /// <param name="newTrigger">Nuevo trigger a agregar</param>
    /// <param name="activate">Si activar el trigger inmediatamente</param>
    public void AddTrigger(Collider newTrigger, bool activate = true)
    {
        if (newTrigger == null || triggerComponents.ContainsKey(newTrigger)) return;
        
        // Configurar el trigger
        newTrigger.isTrigger = true;
        
        VehicleReturnTrigger triggerComponent = newTrigger.GetComponent<VehicleReturnTrigger>();
        if (triggerComponent == null)
        {
            triggerComponent = newTrigger.gameObject.AddComponent<VehicleReturnTrigger>();
        }
        
        triggerComponent.Initialize(this);
        triggerComponent.SetActive(activate);
        
        triggerComponents[newTrigger] = triggerComponent;
        
        Debug.Log($"Nuevo trigger agregado: {newTrigger.name}");
    }
    
    /// <summary>
    /// Remueve un trigger del sistema
    /// </summary>
    /// <param name="trigger">Trigger a remover</param>
    public void RemoveTrigger(Collider trigger)
    {
        if (triggerComponents.ContainsKey(trigger))
        {
            VehicleReturnTrigger triggerComponent = triggerComponents[trigger];
            if (triggerComponent != null)
            {
                Destroy(triggerComponent);
            }
            
            triggerComponents.Remove(trigger);
            Debug.Log($"Trigger removido: {trigger.name}");
        }
    }
    
    /// <summary>
    /// Obtiene el número de triggers activos
    /// </summary>
    /// <returns>Cantidad de triggers activos</returns>
    public int GetActiveTriggerCount()
    {
        int count = 0;
        foreach (var triggerComponent in triggerComponents.Values)
        {
            if (triggerComponent != null && triggerComponent.IsActive())
            {
                count++;
            }
        }
        return count;
    }
    
    private void OnDestroy()
    {
        // Limpiar componentes al destruir
        foreach (var triggerComponent in triggerComponents.Values)
        {
            if (triggerComponent != null)
            {
                Destroy(triggerComponent);
            }
        }
        triggerComponents.Clear();
    }
}
