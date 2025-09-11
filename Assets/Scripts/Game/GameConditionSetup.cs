using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script para configurar automáticamente el sistema de condiciones de victoria/derrota
/// </summary>
public class GameConditionSetup : MonoBehaviour
{
    [Header("Configuración Automática")]
    [SerializeField] private bool configurarAlIniciar = true;
    [SerializeField] private bool buscarTriggersExistentes = true;
    [SerializeField] private bool crearGameManagerSiNoExiste = true;
    
    [Header("Configuración de Triggers")]
    [SerializeField] private string tagTriggerVictoria = "PassTrigger";
    [SerializeField] private string tagTriggerDerrota = "FallTrigger";
    [SerializeField] private string tagVehiculo = "Vehicle";
    
    [Header("Condiciones de Juego")]
    [SerializeField] private int vehiculosParaVictoria = 10;
    [SerializeField] private int vehiculosParaDerrota = 3;
    
    [Header("Referencias Manuales (Opcional)")]
    [SerializeField] private Collider[] triggersVictoriaPersonalizados;
    [SerializeField] private Collider[] triggersDerrotaPersonalizados;
    
    private void Start()
    {
        if (configurarAlIniciar)
        {
            ConfigurarSistemaCompleto();
        }
    }
    
    #region Configuración Automática
    
    /// <summary>
    /// Configura todo el sistema de condiciones de victoria/derrota
    /// </summary>
    [ContextMenu("Configurar Sistema Completo")]
    public void ConfigurarSistemaCompleto()
    {
        Debug.Log("🔧 Iniciando configuración del sistema de condiciones...");
        
        // 1. Verificar/crear tags
        VerificarYCrearTags();
        
        // 2. Configurar GameConditionManager
        ConfigurarGameManager();
        
        // 3. Configurar triggers
        ConfigurarTriggers();
        
        Debug.Log("✅ Sistema de condiciones configurado correctamente!");
    }
    
    /// <summary>
    /// Verifica y crea los tags necesarios
    /// </summary>
    private void VerificarYCrearTags()
    {
        Debug.Log("📋 Verificando tags necesarios...");
        
        #if UNITY_EDITOR
        // Obtener el asset de tags
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        // Verificar y crear tags si no existen
        VerificarYCrearTag(tagsProp, tagTriggerVictoria);
        VerificarYCrearTag(tagsProp, tagTriggerDerrota);
        VerificarYCrearTag(tagsProp, tagVehiculo);
        
        tagManager.ApplyModifiedProperties();
        #endif
        
        Debug.Log("✅ Verificación de tags completada");
    }
    
    #if UNITY_EDITOR
    private void VerificarYCrearTag(SerializedProperty tagsProp, string tagName)
    {
        // Verificar si el tag ya existe
        bool tagExists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty tag = tagsProp.GetArrayElementAtIndex(i);
            if (tag.stringValue == tagName)
            {
                tagExists = true;
                break;
            }
        }
        
        // Crear el tag si no existe
        if (!tagExists)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = tagName;
            Debug.Log($"📋 Tag '{tagName}' creado");
        }
        else
        {
            Debug.Log($"✅ Tag '{tagName}' ya existe");
        }
    }
    #endif
    
    /// <summary>
    /// Configura el GameConditionManager
    /// </summary>
    private void ConfigurarGameManager()
    {
        Debug.Log("🎮 Configurando GameConditionManager...");
        
        GameConditionManager manager = FindFirstObjectByType<GameConditionManager>();
        
        if (manager == null && crearGameManagerSiNoExiste)
        {
            // Crear un nuevo GameObject con el GameConditionManager
            GameObject managerObj = new GameObject("GameConditionManager");
            manager = managerObj.AddComponent<GameConditionManager>();
            Debug.Log("🎮 GameConditionManager creado");
        }
        if (manager != null)
        {
            // Si hay un RoundController en la escena, lo usamos para victoria por rondas
            RoundController controlador = FindFirstObjectByType<RoundController>();
            if (controlador != null)
            {
                if (controlador.IsUsandoSistemaRondas())
                {
                    manager.ConfigurarVictoriaPorRondas(true, controlador);
                    Debug.Log("✅ GameConditionManager configurado para usar RoundController (victoria por rondas)");
                }
                else
                {
                    Debug.Log("ℹ️ RoundController encontrado pero el sistema de rondas está desactivado: usando condiciones estáticas si se configuraron.");
                    manager.ConfigurarCondiciones(vehiculosParaVictoria, vehiculosParaDerrota);
                }
            }
            else
            {
                // No hay AutoGenerator: configurar las condiciones estáticas
                manager.ConfigurarCondiciones(vehiculosParaVictoria, vehiculosParaDerrota);
                Debug.Log("✅ GameConditionManager configurado con condiciones estáticas");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No se pudo encontrar o crear GameConditionManager");
        }
    }
    
    /// <summary>
    /// Configura todos los triggers
    /// </summary>
    private void ConfigurarTriggers()
    {
        Debug.Log("🎯 Configurando triggers...");
        
        GameConditionManager manager = FindFirstObjectByType<GameConditionManager>();
        if (manager == null)
        {
            Debug.LogError("❌ No se encontró GameConditionManager para configurar triggers");
            return;
        }
        
        int triggersVictoriaConfigurados = 0;
        int triggersDerrotaConfigurados = 0;
        
        // Configurar triggers automáticamente por tag
        if (buscarTriggersExistentes)
        {
            triggersVictoriaConfigurados += ConfigurarTriggersPorTag(tagTriggerVictoria, true, manager);
            triggersDerrotaConfigurados += ConfigurarTriggersPorTag(tagTriggerDerrota, false, manager);
        }
        
        // Configurar triggers manuales
        if (triggersVictoriaPersonalizados != null)
        {
            foreach (Collider trigger in triggersVictoriaPersonalizados)
            {
                if (trigger != null)
                {
                    ConfigurarTriggerIndividual(trigger.gameObject, true, manager);
                    triggersVictoriaConfigurados++;
                }
            }
        }
        
        if (triggersDerrotaPersonalizados != null)
        {
            foreach (Collider trigger in triggersDerrotaPersonalizados)
            {
                if (trigger != null)
                {
                    ConfigurarTriggerIndividual(trigger.gameObject, false, manager);
                    triggersDerrotaConfigurados++;
                }
            }
        }
        
        Debug.Log($"✅ Configurados {triggersVictoriaConfigurados} triggers de victoria y {triggersDerrotaConfigurados} triggers de derrota");
    }
    
    /// <summary>
    /// Configura triggers basándose en su tag
    /// </summary>
    private int ConfigurarTriggersPorTag(string tag, bool esVictoria, GameConditionManager manager)
    {
        GameObject[] triggers = GameObject.FindGameObjectsWithTag(tag);
        
        foreach (GameObject triggerObj in triggers)
        {
            ConfigurarTriggerIndividual(triggerObj, esVictoria, manager);
        }
        
        return triggers.Length;
    }
    
    /// <summary>
    /// Configura un trigger individual
    /// </summary>
    private void ConfigurarTriggerIndividual(GameObject triggerObj, bool esVictoria, GameConditionManager manager)
    {
        if (esVictoria)
        {
            manager.ConfigurarTriggerVictoria(triggerObj);
        }
        else
        {
            manager.ConfigurarTriggerDerrota(triggerObj);
        }
    }
    
    #endregion
    
    #region Métodos de Utilidad
    
    /// <summary>
    /// Convierte triggers existentes para que usen el nuevo sistema
    /// </summary>
    [ContextMenu("Convertir Triggers Existentes")]
    public void ConvertirTriggersExistentes()
    {
        Debug.Log("🔄 Convirtiendo triggers existentes...");
        
        // Buscar VehicleReturnTrigger existentes y sugerir conversión
        VehicleReturnTrigger[] triggersExistentes = FindObjectsByType<VehicleReturnTrigger>(FindObjectsSortMode.None);
        
        foreach (VehicleReturnTrigger trigger in triggersExistentes)
        {
            Debug.Log($"🔍 Encontrado VehicleReturnTrigger en {trigger.gameObject.name}");
            Debug.Log($"   💡 Considera agregar el tag '{tagTriggerVictoria}' o '{tagTriggerDerrota}' según su función");
        }
        
        if (triggersExistentes.Length == 0)
        {
            Debug.Log("ℹ️ No se encontraron triggers existentes del tipo VehicleReturnTrigger");
        }
    }
    
    /// <summary>
    /// Crea triggers de ejemplo en posiciones comunes
    /// </summary>
    [ContextMenu("Crear Triggers de Ejemplo")]
    public void CrearTriggersDeEjemplo()
    {
        Debug.Log("🏗️ Creando triggers de ejemplo...");
        
        // Trigger de victoria (a los lados del puente)
        CrearTriggerEjemplo("TriggerVictoriaIzquierda", new Vector3(-20, 0, 0), tagTriggerVictoria);
        CrearTriggerEjemplo("TriggerVictoriaDerecha", new Vector3(20, 0, 0), tagTriggerVictoria);
        
        // Trigger de derrota (debajo del puente)
        CrearTriggerEjemplo("TriggerDerrota", new Vector3(0, -5, 0), tagTriggerDerrota);
        
        Debug.Log("✅ Triggers de ejemplo creados");
    }
    
    private void CrearTriggerEjemplo(string nombre, Vector3 posicion, string tag)
    {
        GameObject triggerObj = new GameObject(nombre);
        triggerObj.transform.position = posicion;
        triggerObj.tag = tag;
        
        BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(2, 5, 10); // Tamaño estándar para capturar vehículos
        
        // Agregar el componente GameConditionTrigger
        triggerObj.AddComponent<GameConditionTrigger>();
        
        Debug.Log($"🎯 Trigger '{nombre}' creado en {posicion}");
    }
    
    /// <summary>
    /// Muestra información del sistema actual
    /// </summary>
    [ContextMenu("Mostrar Estado del Sistema")]
    public void MostrarEstadoSistema()
    {
        Debug.Log("=== ESTADO DEL SISTEMA DE CONDICIONES ===");
        
        GameConditionManager manager = FindFirstObjectByType<GameConditionManager>();
        if (manager != null)
        {
            Debug.Log($"✅ GameConditionManager encontrado");
            Debug.Log($"   📊 Victoria: {manager.GetProgresoVictoria()}/{manager.GetMetaVictoria()}");
            Debug.Log($"   📊 Derrota: {manager.GetProgresoDerrota()}/{manager.GetMetaDerrota()}");
            Debug.Log($"   🎮 Juego activo: {manager.IsJuegoActivo()}");
        }
        else
        {
            Debug.Log("❌ GameConditionManager no encontrado");
        }
        
        // Contar triggers
        GameObject[] triggersVictoria = GameObject.FindGameObjectsWithTag(tagTriggerVictoria);
        GameObject[] triggersDerrota = GameObject.FindGameObjectsWithTag(tagTriggerDerrota);
        
        Debug.Log($"🎯 Triggers de victoria encontrados: {triggersVictoria.Length}");
        Debug.Log($"🎯 Triggers de derrota encontrados: {triggersDerrota.Length}");
        
        // Contar vehículos
        GameObject[] vehiculos = GameObject.FindGameObjectsWithTag(tagVehiculo);
        Debug.Log($"🚗 Vehículos encontrados: {vehiculos.Length}");
        
        Debug.Log("==========================================");
    }
    
    #endregion
    
    #region Configuración Manual
    
    /// <summary>
    /// Permite configurar manualmente las referencias
    /// </summary>
    public void ConfigurarReferenciasManualmente(Collider[] triggersVict, Collider[] triggersDerr)
    {
        triggersVictoriaPersonalizados = triggersVict;
        triggersDerrotaPersonalizados = triggersDerr;
        
        Debug.Log($"🔧 Referencias manuales configuradas: {triggersVict?.Length ?? 0} victoria, {triggersDerr?.Length ?? 0} derrota");
    }
    
    /// <summary>
    /// Configura los parámetros del juego
    /// </summary>
    public void ConfigurarParametrosJuego(int metaVictoria, int metaDerrota, string tagVeh, string tagVict, string tagDerr)
    {
        vehiculosParaVictoria = metaVictoria;
        vehiculosParaDerrota = metaDerrota;
        tagVehiculo = tagVeh;
        tagTriggerVictoria = tagVict;
        tagTriggerDerrota = tagDerr;
        
        Debug.Log($"🎮 Parámetros configurados: Victoria={metaVictoria}, Derrota={metaDerrota}");
    }
    
    #endregion
}
