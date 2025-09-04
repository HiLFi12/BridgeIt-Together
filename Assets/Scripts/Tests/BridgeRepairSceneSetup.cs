using UnityEngine;

/// <summary>
/// Herramienta para configurar automáticamente la escena para el sistema de reparación de puentes
/// </summary>
public class BridgeRepairSceneSetup : MonoBehaviour
{
    [Header("Configuración Automática")]
    [SerializeField] private bool autoConfigureOnStart = true;
    
    [Header("Referencias de Prefabs")]
    [SerializeField] private GameObject bridgeQuadrantPrefab;
    [SerializeField] private BridgeQuadrantSO prehistoricQuadrantSO;
    [SerializeField] private GameObject genericObject3Prefab;
    
    [Header("Configuración de Grilla")]
    [SerializeField] private int gridWidth = 3;
    [SerializeField] private int gridLength = 5;
    [SerializeField] private float quadrantSize = 1f;
    
    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigurarEscenaCompleta();
        }
    }
    
    /// <summary>
    /// Configura automáticamente toda la escena para el sistema de reparación
    /// </summary>
    [ContextMenu("Configurar Escena Completa")]
    public void ConfigurarEscenaCompleta()
    {
        Debug.Log("=== INICIANDO CONFIGURACIÓN AUTOMÁTICA DE ESCENA ===");
        
        // 1. Configurar BridgeGrid
        ConfigurarBridgeGrid();
        
        // 2. Configurar GenericObject3
        ConfigurarGenericObject3();
        
        // 3. Configurar jugadores
        ConfigurarJugadores();
        
        // 4. Añadir BridgeRepairTest
        ConfigurarBridgeRepairTest();
        
        Debug.Log("=== CONFIGURACIÓN AUTOMÁTICA COMPLETADA ===");
        Debug.Log("INSTRUCCIONES:");
        Debug.Log("1. Verifica que todos los prefabs estén asignados en el inspector");
        Debug.Log("2. Ejecuta la escena y sigue el flujo de prueba");
        Debug.Log("3. Usa los métodos del BridgeRepairTest para verificar el funcionamiento");
    }
    
    private void ConfigurarBridgeGrid()
    {
        // Buscar BridgeConstructionGrid existente
        BridgeConstructionGrid existingGrid = FindObjectOfType<BridgeConstructionGrid>();
        
        if (existingGrid == null)
        {
            // Crear nuevo BridgeGrid
            GameObject bridgeGridObj = new GameObject("BridgeGrid");
            BridgeConstructionGrid grid = bridgeGridObj.AddComponent<BridgeConstructionGrid>();
            
            // Configurar propiedades
            grid.gridWidth = gridWidth;
            grid.gridLength = gridLength;
            grid.quadrantSize = quadrantSize;
            grid.showDebugGrid = true;
            
            // Crear QuadrantContainer
            GameObject container = new GameObject("QuadrantContainer");
            container.transform.SetParent(bridgeGridObj.transform);
            
            Debug.Log("✓ BridgeConstructionGrid creado y configurado");
        }
        else
        {
            Debug.Log("✓ BridgeConstructionGrid ya existe en la escena");
        }
    }
    
    private void ConfigurarGenericObject3()
    {
        // Verificar si ya existe GenericObject3 en la escena
        GenericObject3 existing = FindObjectOfType<GenericObject3>();
        
        if (existing == null && genericObject3Prefab != null)
        {
            // Instanciar GenericObject3 cerca del origen
            Vector3 position = new Vector3(-2, 0, 0);
            GameObject instance = Instantiate(genericObject3Prefab, position, Quaternion.identity);
            instance.name = "GenericObject3_AdoquinGenerator";
            
            Debug.Log($"✓ GenericObject3 instanciado en posición {position}");
        }
        else if (existing != null)
        {
            Debug.Log("✓ GenericObject3 ya existe en la escena");
        }
        else
        {
            Debug.LogWarning("⚠ GenericObject3 prefab no asignado. Asígnalo en el inspector.");
        }
    }
    
    private void ConfigurarJugadores()
    {
        // Buscar objetos que podrían ser jugadores
        GameObject[] possiblePlayers = GameObject.FindGameObjectsWithTag("Player");
        
        if (possiblePlayers.Length == 0)
        {
            // Crear jugadores de prueba
            for (int i = 0; i < 2; i++)
            {
                GameObject player = new GameObject($"Player{i + 1}");
                player.tag = "Player";
                
                // Posicionar jugadores
                player.transform.position = new Vector3(i * 4 - 2, 0, -3);
                
                // Añadir componentes necesarios
                PlayerObjectHolder holder = player.AddComponent<PlayerObjectHolder>();
                PlayerBridgeInteraction interaction = player.AddComponent<PlayerBridgeInteraction>();
                
                // Configurar BuildPoint
                GameObject buildPoint = new GameObject("BuildPoint");
                buildPoint.transform.SetParent(player.transform);
                buildPoint.transform.localPosition = new Vector3(0, 0, 1);
                
                // Buscar BridgeGrid para asignar
                BridgeConstructionGrid grid = FindObjectOfType<BridgeConstructionGrid>();
                if (grid != null)
                {
                    // Nota: Necesitarás asignar manualmente bridgeGrid y buildPoint en el inspector
                    Debug.Log($"✓ Player{i + 1} creado. Asigna BridgeGrid y BuildPoint en el inspector.");
                }
            }
        }
        else
        {
            Debug.Log($"✓ {possiblePlayers.Length} jugador(es) encontrados en la escena");
            
            // Verificar que tengan PlayerBridgeInteraction
            foreach (GameObject player in possiblePlayers)
            {
                if (player.GetComponent<PlayerBridgeInteraction>() == null)
                {
                    player.AddComponent<PlayerObjectHolder>();
                    player.AddComponent<PlayerBridgeInteraction>();
                    Debug.Log($"✓ PlayerBridgeInteraction añadido a {player.name}");
                }
            }
        }
    }
    
    private void ConfigurarBridgeRepairTest()
    {
        // Verificar si ya existe BridgeRepairTest
        BridgeRepairTest existing = FindObjectOfType<BridgeRepairTest>();
        
        if (existing == null)
        {
            // Crear objeto con BridgeRepairTest
            GameObject testObj = new GameObject("BridgeRepairTest");
            BridgeRepairTest test = testObj.AddComponent<BridgeRepairTest>();
            
            Debug.Log("✓ BridgeRepairTest añadido a la escena");
            Debug.Log("  Asigna testQuadrant y materialPrefabsSO en el inspector para ejecutar las pruebas");
        }
        else
        {
            Debug.Log("✓ BridgeRepairTest ya existe en la escena");
        }
    }
    
    /// <summary>
    /// Crear una escena de prueba rápida con todo configurado
    /// </summary>
    [ContextMenu("Crear Escena de Prueba Rápida")]
    public void CrearEscenaPruebaRapida()
    {
        Debug.Log("=== CREANDO ESCENA DE PRUEBA RÁPIDA ===");
        
        // Limpiar escena (opcional - comentar si no quieres borrar objetos existentes)
        // LimpiarEscena();
        
        // Configurar todo
        ConfigurarEscenaCompleta();
        
        // Añadir cámara si no existe
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 5, -10);
            camObj.transform.rotation = Quaternion.Euler(30, 0, 0);
            
            Debug.Log("✓ Cámara principal añadida");
        }
        
        // Añadir luz si no existe
        if (FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            Debug.Log("✓ Luz direccional añadida");
        }
        
        Debug.Log("=== ESCENA DE PRUEBA RÁPIDA COMPLETADA ===");
        Debug.Log("LISTO PARA PROBAR: Ejecuta la escena y el sistema debería funcionar automáticamente");
    }
    
    private void LimpiarEscena()
    {
        // Eliminar objetos no esenciales (usar con cuidado)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != gameObject && obj.transform.parent == null)
            {
                if (obj.name != "Main Camera" && obj.name != "Directional Light")
                {
                    DestroyImmediate(obj);
                }
            }
        }
        
        Debug.Log("✓ Escena limpiada");
    }
}
