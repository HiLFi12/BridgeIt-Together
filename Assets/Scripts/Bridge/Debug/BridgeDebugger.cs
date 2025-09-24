using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Agregar este componente a cualquier objeto de la escena para depurar el sistema de puentes
public class BridgeDebugger : MonoBehaviour
{
    [Header("Referencias")]
    public BridgeConstructionGrid bridgeGrid;
    
    [Header("Depuración")]
    public bool enableDebugging = true;
    public KeyCode testBuildKey = KeyCode.T;
    public int testX = 0;
    public int testZ = 0;
    public int testLayer = 0;
    public GameObject testMaterialPrefab;
    
    [Header("Prueba de Impacto")]
    public KeyCode testImpactKey = KeyCode.Y;
    
    [Header("Prueba de Posicionamiento")]
    public KeyCode testPositionKey = KeyCode.U;
    public float alturaAjuste = 0.05f;
    
    [Header("Variables de Estado (Solo Lectura)")]
    [SerializeField] private string gridStatus = "No inicializado";
    [SerializeField] private int completeQuadrants = 0;
    [SerializeField] private int damagedQuadrants = 0;
    [SerializeField] private int incompleteQuadrants = 0;
    
    private void Start()
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
            if (bridgeGrid == null)
            {
                Debug.LogError("No se encontró BridgeConstructionGrid en la escena.");
                gridStatus = "ERROR: Grid no encontrada";
                return;
            }
        }
        
        gridStatus = $"Grid {bridgeGrid.gridWidth}x{bridgeGrid.gridLength}";
        
        Debug.Log("BridgeDebugger iniciado. Usa las teclas:\n" +
                 "- T: Probar construcción\n" +
                 "- Y: Probar impactos\n" +
                 "- U: Probar posicionamiento visual");
    }
    
    private void Update()
    {
        if (!enableDebugging || bridgeGrid == null)
            return;
            
        // Probar construcción con la tecla T
        if (Input.GetKeyDown(testBuildKey))
        {
            TestBuildLayer();
        }
        
        // Probar impacto con la tecla Y
        if (Input.GetKeyDown(testImpactKey))
        {
            TestVehicleImpact();
        }
        
        // Probar posicionamiento con la tecla U
        if (Input.GetKeyDown(testPositionKey))
        {
            TestVisualPositioning();
        }
        
        // Actualizar estadísticas cada frame
        UpdateStatistics();
    }
    
    private void TestBuildLayer()
    {
        if (testMaterialPrefab == null)
        {
            Debug.LogError("Falta asignar un prefab de material de prueba.");
            return;
        }
        
        Debug.Log($"Probando construcción en cuadrante [{testX},{testZ}], capa {testLayer}...");
        bool success = bridgeGrid.TryBuildLayer(testX, testZ, testLayer, testMaterialPrefab);
        
        if (success)
        {
            Debug.Log($"¡Construcción exitosa en cuadrante [{testX},{testZ}], capa {testLayer}!");
            
            // Avanzar automáticamente a la siguiente capa
            testLayer = (testLayer + 1) % 4;
        }
        else
        {
            Debug.LogWarning($"No se pudo construir en el cuadrante [{testX},{testZ}], capa {testLayer}.");
        }
    }
    
    private void TestVehicleImpact()
    {
        Debug.Log($"Probando impacto de vehículo en cuadrante [{testX},{testZ}]...");
        bridgeGrid.OnVehicleImpact(testX, testZ);
    }
    
    // Nuevo método para probar el posicionamiento visual de prefabs
    private void TestVisualPositioning()
    {
        if (bridgeGrid == null || bridgeGrid.defaultQuadrantSO == null)
        {
            Debug.LogError("No puedo probar: bridgeGrid o defaultQuadrantSO no asignados");
            return;
        }
        
        Debug.Log("Probando posicionamiento visual de cuadrantes...");
        
        // Calcular posición del cuadrante actual
        Vector3 cuadrantePos = bridgeGrid.transform.position + 
                              new Vector3(testX * bridgeGrid.quadrantSize, 0, testZ * bridgeGrid.quadrantSize);
        
        // Calcular centro del cuadrante
        Vector3 centroCuadrante = cuadrantePos + 
                                new Vector3(bridgeGrid.quadrantSize/2, 0, bridgeGrid.quadrantSize/2);
        
        // Crear objetos visuales para diagnóstico
        GameObject diagnostico = new GameObject($"Diagnostico_Cuadrante_{testX}_{testZ}");
        diagnostico.transform.position = cuadrantePos;
        
        // Crear una esfera roja en la esquina
        GameObject esquina = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esquina.name = "Esquina";
        esquina.transform.SetParent(diagnostico.transform);
        esquina.transform.position = cuadrantePos;
        esquina.transform.localScale = Vector3.one * 0.1f;
        Renderer rendererEsquina = esquina.GetComponent<Renderer>();
        if (rendererEsquina != null)
            rendererEsquina.material.color = Color.red;
        
        // Crear una esfera verde en el centro
        GameObject centro = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centro.name = "Centro";
        centro.transform.SetParent(diagnostico.transform);
        centro.transform.position = centroCuadrante;
        centro.transform.localScale = Vector3.one * 0.1f;
        Renderer rendererCentro = centro.GetComponent<Renderer>();
        if (rendererCentro != null)
            rendererCentro.material.color = Color.green;
        
        // Crear objetos para cada capa en la posición correcta
        var so = bridgeGrid.defaultQuadrantSO;
        for (int i = 0; i < so.requiredLayers.Length; i++)
        {
            if (so.requiredLayers[i].visualPrefab != null)
            {
                // Calcular posición para la capa con el ajuste de altura
                Vector3 layerPos = centroCuadrante + new Vector3(0, alturaAjuste * i, 0);
                
                // Crear un cubo azul en la posición donde debería ir la capa
                GameObject indicadorCapa = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicadorCapa.name = $"IndicadorCapa_{i}";
                indicadorCapa.transform.SetParent(diagnostico.transform);
                indicadorCapa.transform.position = layerPos;
                indicadorCapa.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                Renderer rendererIndicador = indicadorCapa.GetComponent<Renderer>();
                if (rendererIndicador != null)
                    rendererIndicador.material.color = Color.blue;
                
                // Crear una instancia del prefab visual para ver cómo se vería
                if (i == testLayer)
                {
                    GameObject capaPrueba = Instantiate(so.requiredLayers[i].visualPrefab, layerPos, Quaternion.identity);
                    capaPrueba.name = $"Capa_{i}_Visual";
                    capaPrueba.transform.SetParent(diagnostico.transform);
                    
                    // Asignar material si es posible
                    Renderer rendererCapa = capaPrueba.GetComponent<Renderer>();
                    if (rendererCapa == null)
                        rendererCapa = capaPrueba.GetComponentInChildren<Renderer>();
                        
                    if (rendererCapa != null && so.requiredLayers[i].material != null)
                        rendererCapa.material = so.requiredLayers[i].material;
                }
            }
        }
        
        Debug.Log("Objetos de diagnóstico visual creados. Objetos de color:");
        Debug.Log(" - Rojo: Esquina del cuadrante (origen)");
        Debug.Log(" - Verde: Centro del cuadrante");
        Debug.Log(" - Azul: Posiciones de cada capa");
        Debug.Log($" - Visual de capa {testLayer}: Muestra cómo debería verse el prefab visual");
        
        // El objeto de diagnóstico se autodestruirá después de 10 segundos
        Destroy(diagnostico, 10f);
    }
    
    private void UpdateStatistics()
    {
        // Reiniciar contadores
        completeQuadrants = 0;
        damagedQuadrants = 0;
        incompleteQuadrants = 0;
        
        // Buscar un método para inspeccionar cuadrantes
        // Como no tenemos acceso directo al estado interno, usamos reflexión
        // Nota: Esto es solo para debugging
        System.Type gridType = bridgeGrid.GetType();
        System.Reflection.FieldInfo gridField = gridType.GetField("constructionGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (gridField != null)
        {
            object grid = gridField.GetValue(bridgeGrid);
            if (grid != null)
            {
                // La grilla está inicializada, actualizar estadísticas
                gridStatus = $"Grid {bridgeGrid.gridWidth}x{bridgeGrid.gridLength} activa";
            }
            else
            {
                gridStatus = "Grid no inicializada";
            }
        }
        
        // Esta parte es más difícil de implementar con reflexión,
        // pero para debugging básico ya tenemos la visualización de Gizmos
    }
    
    private void OnGUI()
    {
        if (!enableDebugging)
            return;
            
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUILayout.Label("==== DEPURADOR DE PUENTES ====");
        GUILayout.Label($"Estado de grilla: {gridStatus}");
        GUILayout.Label($"Cuadrante seleccionado: [{testX},{testZ}]");
        GUILayout.Label($"Capa actual: {testLayer}");
        GUILayout.Label($"Tecla {testBuildKey} para construir");
        GUILayout.Label($"Tecla {testImpactKey} para simular impacto");
        GUILayout.Label($"Tecla {testPositionKey} para test visual");
        GUILayout.EndArea();
    }
} 