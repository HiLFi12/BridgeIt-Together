using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de configuración de carril para el spawner
/// </summary>
public enum TipoCarril
{
    DobleCarril,    // Spawneo desde ambos lados (comportamiento original)
    SoloIzquierda,  // Solo spawneo desde el lado izquierdo
    SoloDerecha     // Solo spawneo desde el lado derecho
}

/// <summary>
/// Posición del carril para carriles de una sola dirección
/// </summary>
public enum PosicionCarril
{
    Inferior,   // Carril inferior (valor Z menor)
    Superior,   // Carril superior (valor Z mayor)
    Random      // Selección aleatoria entre inferior y superior
}

/// <summary>
/// Tipo de vehículo a spawnear
/// </summary>
public enum TipoVehiculo
{
    Auto1,     // Vehículo Auto1
    Auto2,     // Vehículo Auto2
    Auto3,     // Vehículo Auto3
    Auto4,     // Vehículo Auto4
    Auto5,     // Vehículo Auto5
    Random      // Selección aleatoria entre Auto1 y Auto5
}

/// <summary>
/// Configuración para una ronda específica de spawneo
/// </summary>
[System.Serializable]
public class RondaConfig
{
    [Header("Configuración de la Ronda")]
    public string nombreRonda = "Ronda"; // Nombre descriptivo de la ronda
    public int cantidadAutos = 5; // Cantidad de autos a spawnear en esta ronda
    public float tiempoEntreAutos = 10f; // Tiempo entre cada auto en esta ronda
    
    [Header("Configuración de Carril (Opcional - sobrescribe configuración global)")]
    public bool sobrescribirTipoCarril = false; // Si esta ronda debe usar un tipo de carril específico
    public TipoCarril tipoCarrilRonda = TipoCarril.DobleCarril; // Tipo de carril para esta ronda
    
    [Header("Configuración Individual de Carriles (Solo para carriles de una dirección)")]
    [Tooltip("Solo aplica cuando tipoCarrilRonda es SoloIzquierda o SoloDerecha. Define el carril (inferior/superior) para cada auto.")]
    public PosicionCarril[] posicionesCarrilPorAuto = new PosicionCarril[0]; // Posición de carril para cada auto individual
    
    [Header("Configuración Individual de Vehículos")]
    [Tooltip("Define el tipo de vehículo (Auto1/Auto2/Auto3/Auto4/Auto5/Random) para cada auto en esta ronda.")]
    public TipoVehiculo[] tiposVehiculoPorAuto = new TipoVehiculo[0]; // Tipo de vehículo para cada auto individual
    
    /// <summary>
    /// Obtiene el tipo de vehículo para un auto específico. Si no hay configuración individual, usa el vehículo normal por defecto.
    /// Si la configuración es Random, selecciona aleatoriamente entre Normal y AutoDoble.
    /// </summary>
        public TipoVehiculo ObtenerTipoVehiculoParaAuto(int indiceAuto)
        {
            TipoVehiculo tipoConfigurado = TipoVehiculo.Auto1; // Por defecto
            
            if (tiposVehiculoPorAuto != null && indiceAuto < tiposVehiculoPorAuto.Length)
            {
                tipoConfigurado = tiposVehiculoPorAuto[indiceAuto];
            }
            
            // Si la configuración es Random, elegir aleatoriamente
            if (tipoConfigurado == TipoVehiculo.Random)
            {
                return (TipoVehiculo)Random.Range(0, 5); // Auto1 to Auto5
            }
            
            return tipoConfigurado;
        }    /// <summary>
    /// Obtiene la posición de carril para un auto específico. Si no hay configuración individual, usa el carril inferior por defecto.
    /// Si la configuración es Random, selecciona el carril opuesto al último usado para evitar colisiones.
    /// </summary>
    public PosicionCarril ObtenerPosicionCarrilParaAuto(int indiceAuto, PosicionCarril ultimoCarrilUsado)
    {
        PosicionCarril posicionConfigurada = PosicionCarril.Inferior; // Por defecto
        
        if (posicionesCarrilPorAuto != null && indiceAuto < posicionesCarrilPorAuto.Length)
        {
            posicionConfigurada = posicionesCarrilPorAuto[indiceAuto];
        }
        
        // Si la configuración es Random, elegir el carril opuesto al último usado
        if (posicionConfigurada == PosicionCarril.Random)
        {
            // Alternar entre Superior e Inferior basado en el último carril usado
            PosicionCarril carrilSeleccionado = (ultimoCarrilUsado == PosicionCarril.Inferior) ? PosicionCarril.Superior : PosicionCarril.Inferior;
            Debug.Log($"[AutoGenerator] Carril Random para auto {indiceAuto}: Último usado={ultimoCarrilUsado}, Seleccionado={carrilSeleccionado}");
            return carrilSeleccionado;
        }
        
        return posicionConfigurada;
    }
    
    /// <summary>
    /// Valida y ajusta los arrays de posiciones de carril y tipos de vehículo para que coincidan con la cantidad de autos
    /// </summary>
    public void ValidarConfiguracionCarriles()
    {
        // Validar y ajustar posiciones de carril
        if (posicionesCarrilPorAuto == null || posicionesCarrilPorAuto.Length != cantidadAutos)
        {
            // Crear o redimensionar el array
            PosicionCarril[] nuevasConfigs = new PosicionCarril[cantidadAutos];
            
            // Copiar configuraciones existentes si las hay
            if (posicionesCarrilPorAuto != null)
            {
                for (int i = 0; i < Mathf.Min(posicionesCarrilPorAuto.Length, cantidadAutos); i++)
                {
                    nuevasConfigs[i] = posicionesCarrilPorAuto[i];
                }
            }
            
            // Llenar el resto con valores por defecto (alternando entre inferior, superior y random)
            for (int i = (posicionesCarrilPorAuto?.Length ?? 0); i < cantidadAutos; i++)
            {
                int resto = i % 3;
                nuevasConfigs[i] = resto == 0 ? PosicionCarril.Inferior : 
                                  resto == 1 ? PosicionCarril.Superior : 
                                              PosicionCarril.Random;
            }
            
            posicionesCarrilPorAuto = nuevasConfigs;
        }
        
        // Validar y ajustar tipos de vehículo
        if (tiposVehiculoPorAuto == null || tiposVehiculoPorAuto.Length != cantidadAutos)
        {
            // Crear o redimensionar el array
            TipoVehiculo[] nuevosConfigs = new TipoVehiculo[cantidadAutos];
            
            // Copiar configuraciones existentes si las hay
            if (tiposVehiculoPorAuto != null)
            {
                for (int i = 0; i < Mathf.Min(tiposVehiculoPorAuto.Length, cantidadAutos); i++)
                {
                    nuevosConfigs[i] = tiposVehiculoPorAuto[i];
                }
            }
            
            // Llenar el resto con valores por defecto (alternando entre Normal, AutoDoble y Random)
            for (int i = (tiposVehiculoPorAuto?.Length ?? 0); i < cantidadAutos; i++)
            {
                int resto = i % 6;
                nuevosConfigs[i] = resto == 0 ? TipoVehiculo.Auto1 : 
                                  resto == 1 ? TipoVehiculo.Auto2 : 
                                  resto == 2 ? TipoVehiculo.Auto3 :
                                  resto == 3 ? TipoVehiculo.Auto4 :
                                  resto == 4 ? TipoVehiculo.Auto5 :
                                              TipoVehiculo.Random;
            }
            
            tiposVehiculoPorAuto = nuevosConfigs;
        }
    }
}

/// <summary>
/// Generador de autos que instancia un auto cada cierto tiempo alternando entre ambos lados del mapa
/// La lógica de pooling se ha separado completamente en la clase VehiclePool
/// </summary>
public class AutoGenerator : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject[] prefabsAutos = new GameObject[5]; // Prefabs de autos Auto1 a Auto5
    [SerializeField] private float tiempoEntreAutos = 10f; // Tiempo entre cada auto generado
    [SerializeField] private float[] velocidadesAutos = new float[5] { 5f, 6f, 7f, 8f, 9f }; // Velocidades para Auto1 a Auto5
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // Referencia al grid del puente
    
    [Header("Configuración de Spawn")]
    [SerializeField] private Transform puntoSpawnIzquierdo; // Punto de spawn del lado izquierdo
    [SerializeField] private Transform puntoSpawnDerecho; // Punto de spawn del lado derecho
    [SerializeField] private bool iniciarDesdeIzquierda = true; // Determina si el primer auto viene de la izquierda
    
    [Header("Configuración de Doble Carril")]
    [SerializeField] private float separacionCarriles = 12f; // Separación entre carriles (en unidades Z)
    [SerializeField] private TipoCarril tipoCarril = TipoCarril.DobleCarril; // Tipo de configuración de carril
    
    [Header("Configuración de Rondas")]
    [SerializeField] private bool usarSistemaRondas = false; // Habilita el sistema de rondas
    [SerializeField] private float tiempoEsperaEntreRondas = 3f; // Tiempo de espera antes de iniciar cada ronda
    [SerializeField] private RondaConfig[] configuracionRondas = new RondaConfig[]
    {
        new RondaConfig { 
            nombreRonda = "Ronda 1 - Fácil (Solo Auto1)", 
            cantidadAutos = 3, 
            tiempoEntreAutos = 8f,
            tiposVehiculoPorAuto = new TipoVehiculo[] { 
                TipoVehiculo.Auto1, TipoVehiculo.Auto1, TipoVehiculo.Auto1 
            }
        },
        new RondaConfig { 
            nombreRonda = "Ronda 2 - Mixta (Auto1 + Auto2)", 
            cantidadAutos = 5, 
            tiempoEntreAutos = 6f, 
            sobrescribirTipoCarril = true, 
            tipoCarrilRonda = TipoCarril.SoloIzquierda,
            posicionesCarrilPorAuto = new PosicionCarril[] { 
                PosicionCarril.Inferior, PosicionCarril.Superior, PosicionCarril.Inferior, 
                PosicionCarril.Superior, PosicionCarril.Inferior 
            },
            tiposVehiculoPorAuto = new TipoVehiculo[] { 
                TipoVehiculo.Auto1, TipoVehiculo.Auto2, TipoVehiculo.Auto1, 
                TipoVehiculo.Auto2, TipoVehiculo.Auto1 
            }
        },
        new RondaConfig { 
            nombreRonda = "Ronda 3 - Caótica (Aleatorio)", 
            cantidadAutos = 5, 
            tiempoEntreAutos = 4f, 
            sobrescribirTipoCarril = true, 
            tipoCarrilRonda = TipoCarril.SoloDerecha,
            posicionesCarrilPorAuto = new PosicionCarril[] { 
                PosicionCarril.Superior, PosicionCarril.Random, 
                PosicionCarril.Inferior, PosicionCarril.Random,
                PosicionCarril.Random
            },
            tiposVehiculoPorAuto = new TipoVehiculo[] { 
                TipoVehiculo.Random, TipoVehiculo.Auto3, 
                TipoVehiculo.Random, TipoVehiculo.Auto4,
                TipoVehiculo.Random
            }
        }
    }; // Configuración de cada ronda
    [SerializeField] private bool loopearRondas = true; // Si repetir las rondas cuando termine la última
      [Header("Configuración del Pool")]
    [SerializeField] private int poolSize = 10; // Tamaño inicial del pool de autos
    [SerializeField] private bool poolExpandible = true; // Si el pool puede crecer automáticamente
    
    [Header("Debug")]
    [SerializeField] private bool mostrarDebugInfo = true; // Si mostrar información de debug en consola
    
    [Header("Configuración de Triggers de Retorno")]
    [SerializeField] private Collider[] triggersRetorno; // Array de triggers que devuelven vehículos al pool
    [SerializeField] private bool activarTriggersAlIniciar = true; // Si activar automáticamente los triggers al iniciar
    
    // Variable para alternar el lado de spawn
    private bool spawnDesdeIzquierda;
    
    // Referencia al sistema de pool de vehículos (SEPARADO)
    private VehiclePool vehiclePool;
    
    // Componente para manejar triggers
    private VehicleReturnTriggerManager triggerManager;
    
    // Variables para el sistema de rondas
    private int rondaActual = 0; // Índice de la ronda actual
    private int autosSpawneadosEnRonda = 0; // Cantidad de autos spawneados en la ronda actual
    private int autosQueDebenVolverAlPool = 0; // Cantidad de autos que deben volver al pool antes de continuar
    private bool esperandoFinDeRonda = false; // Si estamos esperando que termine la ronda actual
    private bool esperandoInicioDeRonda = false; // Si estamos esperando para iniciar la siguiente ronda
    private Coroutine corrutinGeneracion; // Referencia a la corrutina de generación para poder controlarla
    private float tiempoEsperaMaximoRonda = 60f; // Tiempo máximo de espera por ronda antes de forzar avance
    
    // Variables para el control de carriles alternos
    private PosicionCarril ultimoCarrilUsado = PosicionCarril.Inferior; // Track del último carril usado para evitar repetición
    
    private void Start()
    {
        // Inicializar la dirección de spawn
        spawnDesdeIzquierda = iniciarDesdeIzquierda;
        
        // Configurar puntos de spawn
        SetupSpawnPoints();
        
        // Configurar bridge grid
        SetupBridgeGrid();
        
        // Inicializar el pool de vehículos
        InitializeVehiclePool();
        
        // Inicializar el sistema de triggers de retorno
        InitializeTriggerSystem();
        
        // Inicializar sistema de rondas si está habilitado
        if (usarSistemaRondas)
        {
            InitializeRoundSystem();
        }
        
        // Iniciar la generación de autos
        IniciarGeneracion();
    }
    
    /// <summary>
    /// Configura los puntos de spawn
    /// </summary>
    private void SetupSpawnPoints()
    {
        if (puntoSpawnIzquierdo == null)
        {
            puntoSpawnIzquierdo = transform;
            Debug.LogWarning("No se ha asignado un punto de spawn izquierdo. Se usará la posición del generador.");
        }
        
        if (puntoSpawnDerecho == null)
        {
            puntoSpawnDerecho = transform;
            Debug.LogWarning("No se ha asignado un punto de spawn derecho. Se usará la posición del generador.");
        }
    }
    
    /// <summary>
    /// Configura la referencia al bridge grid
    /// </summary>
    private void SetupBridgeGrid()
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindFirstObjectByType<BridgeConstructionGrid>();
            if (bridgeGrid == null)
            {
                Debug.LogWarning("No se encontró BridgeConstructionGrid en la escena. Los autos no podrán interactuar con el puente.");
            }
        }
    }
    
    /// <summary>
    /// Inicializa el sistema de pool de vehículos (DELEGADO A VehiclePool)
    /// </summary>
    private void InitializeVehiclePool()
    {
        if (prefabsAutos == null || prefabsAutos.Length == 0 || prefabsAutos[0] == null)
        {
            Debug.LogError("No se ha asignado el prefab de auto Auto1 al generador");
            return;
        }
        
        // Obtener o crear el componente VehiclePool
        vehiclePool = GetComponent<VehiclePool>();
        if (vehiclePool == null)
        {
            vehiclePool = gameObject.AddComponent<VehiclePool>();
        }
        
        // Inicializar el pool con las configuraciones del generador
        vehiclePool.Initialize(prefabsAutos[0], poolSize, poolExpandible, bridgeGrid);
    }
    
    /// <summary>
    /// Inicializa el sistema de triggers para devolver vehículos al pool
    /// </summary>
    private void InitializeTriggerSystem()
    {
        triggerManager = GetComponent<VehicleReturnTriggerManager>();
        if (triggerManager == null)
        {
            triggerManager = gameObject.AddComponent<VehicleReturnTriggerManager>();
        }
        
        triggerManager.Initialize(this, triggersRetorno, activarTriggersAlIniciar);
    }
    
    /// <summary>
    /// Corrutina que genera autos de forma continua
    /// </summary>
    private IEnumerator GenerarAutos()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreAutos);
            
            GenerarAuto();
            
            // Alternar el lado para el próximo spawn
            spawnDesdeIzquierda = !spawnDesdeIzquierda;
        }
    }
      /// <summary>
    /// Genera un solo auto usando el pool (corregido para usar lógica original)
    /// </summary>
    private void GenerarAuto()
    {
        // DELEGAR AL POOL: Obtener un auto del pool
        GameObject auto = vehiclePool.GetVehicleFromPool();
        if (auto == null)
        {
            Debug.LogError("No se pudo obtener un auto del pool");
            return;
        }

        
        // LÓGICA DE GENERACIÓN: Configurar posición (usar variable directa como en lógica original)
        ConfigurarPosicionAuto(auto);
        
        // LÓGICA DE GENERACIÓN: Activar el auto
        auto.SetActive(true);
        
        // LÓGICA DE GENERACIÓN: Configurar movimiento (usar variable directa como en lógica original)
        ConfigurarMovimientoAuto(auto);
        
        // LÓGICA DE GENERACIÓN: Configurar colisión con el puente
        ConfigurarColisionPuente(auto);
        
        // Los vehículos ahora solo se devuelven al pool mediante triggers de salida
        // Se eliminó el retorno automático por tiempo para permitir múltiples vehículos en escena
    }/// <summary>
    /// Configura la posición y rotación del auto
    /// </summary>
    private void ConfigurarPosicionAuto(GameObject auto)
    {
        Transform puntoSpawn = spawnDesdeIzquierda ? puntoSpawnIzquierdo : puntoSpawnDerecho;
        
        Vector3 posicionSpawn = puntoSpawn.position;
        
        // Aplicar offset de carril solo si el tipo de carril lo requiere
        TipoCarril tipoActual = ObtenerTipoCarrilActual();
        if (tipoActual == TipoCarril.DobleCarril)
        {
            float offsetCarril = spawnDesdeIzquierda ? separacionCarriles / 2 : -separacionCarriles / 2;
            posicionSpawn += new Vector3(0, 0, offsetCarril);
        }
        
        auto.transform.position = posicionSpawn;
        
        // Configurar rotación basada en la dirección de movimiento (restaurado como en versión anterior)
        if (spawnDesdeIzquierda)
        {
            // Auto se mueve hacia la izquierda, debe mirar hacia la izquierda
            auto.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }
        else
        {
            // Auto se mueve hacia la derecha, debe mirar hacia la derecha  
            auto.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }
        
        // Asegurar que el Rigidbody tenga las restricciones correctas
        Rigidbody rb = auto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        

    }    /// <summary>
    /// Configura el movimiento del auto (restaurado a lógica original que funcionaba)
    /// </summary>
    private void ConfigurarMovimientoAuto(GameObject auto)
    {
        AutoMovement movimiento = auto.GetComponent<AutoMovement>();
        if (movimiento != null)
        {
            // Determinar dirección basada en el spawn (RESTAURADO: comportamiento original que funcionaba)
            Vector3 direccion = spawnDesdeIzquierda ? Vector3.left : Vector3.right;
            
            movimiento.SetDireccionMovimiento(direccion);
            movimiento.SetVelocidad(velocidadesAutos.Length > 0 ? velocidadesAutos[0] : 5f);
            // Aplicar corrección automática de rotación del modelo
            movimiento.AplicarCorreccionAutomatica();
            movimiento.enabled = true;
            
            if (mostrarDebugInfo)
            {
                Debug.Log($"[AutoGenerator] DEBUG ConfigurarMovimientoAuto:");
                Debug.Log($"  - Vehículo: {auto.name}");
                Debug.Log($"  - Dirección: {direccion}");
                Debug.Log($"  - Velocidad: {(velocidadesAutos.Length > 0 ? velocidadesAutos[0] : 5f)}");
                Debug.Log($"  - AutoMovement.enabled: {movimiento.enabled}");
                Debug.Log($"  - Posición inicial: {auto.transform.position}");
                Debug.Log($"  - Rotación inicial: {auto.transform.rotation.eulerAngles}");
                
                // Revisar la estructura interna del vehículo
                Debug.Log($"  - Cantidad de hijos: {auto.transform.childCount}");
                for (int i = 0; i < auto.transform.childCount; i++)
                {
                    Transform hijo = auto.transform.GetChild(i);
                    Debug.Log($"    - Hijo {i}: {hijo.name} (activo: {hijo.gameObject.activeSelf})");
                }
            }
        }
        else
        {
            Debug.LogWarning("El prefab de auto no tiene el componente AutoMovement");
        }
    }
    
    /// <summary>
    /// Configura la colisión del auto con el puente
    /// </summary>
    private void ConfigurarColisionPuente(GameObject auto)
    {
        VehicleBridgeCollision vehicleCollision = auto.GetComponent<VehicleBridgeCollision>();
        if (vehicleCollision != null && bridgeGrid != null)
        {
            vehicleCollision.bridgeGrid = bridgeGrid;
        }
        
        // Configurar colisión con jugadores
        VehiclePlayerCollision playerCollision = auto.GetComponent<VehiclePlayerCollision>();
        if (playerCollision == null)
        {
            playerCollision = auto.AddComponent<VehiclePlayerCollision>();

        }
        else
        {

        }
        
        // Configurar las plataformas automáticamente si no están asignadas
        ConfigurarPlataformasParaVehiculo(playerCollision);
    }
      /// <summary>
    /// Retorna manualmente un auto al pool (DELEGADO A VehiclePool)
    /// </summary>
    public void ReturnAutoToPool(GameObject auto)
    {
        if (auto == null || vehiclePool == null) return;
        
        // Notificar al sistema de rondas antes de devolver al pool
        NotificarAutoDevueltoAlPool(auto);
        
        vehiclePool.ReturnVehicleToPool(auto);
    }
    
    /// <summary>
    /// Método para limpiar todos los autos activos (DELEGADO A VehiclePool)
    /// </summary>
    public void ClearActiveAutos()
    {
        if (vehiclePool != null)
        {
            vehiclePool.ClearActiveVehicles();
        }
    }
    
    /// <summary>
    /// Activa o desactiva todos los triggers de retorno
    /// </summary>
    public void SetTriggersActive(bool active)
    {
        if (triggerManager != null)
        {
            triggerManager.SetTriggersActive(active);
        }
    }
    
    /// <summary>
    /// Verifica si un vehículo pertenece al pool (DELEGADO A VehiclePool)
    /// </summary>
    public bool IsVehicleFromPool(GameObject vehicle)
    {
        if (vehiclePool == null) return false;
        return vehiclePool.IsVehicleFromPool(vehicle);
    }
    
    // ===== MÉTODOS PARA MANEJO DE TRIGGERS =====
    
    /// <summary>
    /// Agrega un nuevo trigger al sistema
    /// </summary>
    public void AddReturnTrigger(Collider newTrigger, bool activate = true)
    {
        if (triggerManager != null)
        {
            triggerManager.AddTrigger(newTrigger, activate);
        }
    }
    
    /// <summary>
    /// Remueve un trigger del sistema
    /// </summary>
    public void RemoveReturnTrigger(Collider trigger)
    {
        if (triggerManager != null)
        {
            triggerManager.RemoveTrigger(trigger);
        }
    }
    
    /// <summary>
    /// Obtiene el número de triggers activos
    /// </summary>
    public int GetActiveTriggerCount()
    {
        return triggerManager != null ? triggerManager.GetActiveTriggerCount() : 0;
    }
    
    /// <summary>
    /// Activa o desactiva un trigger específico
    /// </summary>
    public void SetTriggerActive(Collider trigger, bool active)
    {
        if (triggerManager != null)
        {
            triggerManager.SetTriggerActive(trigger, active);
        }
    }
    
    /// <summary>
    /// Reconfigura todos los triggers con un nuevo array
    /// </summary>
    public void ReconfigureTriggers(Collider[] newTriggers, bool activate = true)
    {
        triggersRetorno = newTriggers;
        if (triggerManager != null)
        {
            // Limpiar triggers existentes
            triggerManager.SetTriggersActive(false);
            
            // Reinicializar con los nuevos triggers
            triggerManager.Initialize(this, newTriggers, activate);
        }
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    private void OnDrawGizmos()
    {
        if (puntoSpawnIzquierdo == null || puntoSpawnDerecho == null)
            return;
            
        Vector3 puntoCentral = Vector3.Lerp(puntoSpawnIzquierdo.position, puntoSpawnDerecho.position, 0.5f);
        
        // Dibujar puntos de spawn principales
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(puntoSpawnIzquierdo.position, 0.5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(puntoSpawnDerecho.position, 0.5f);
        
        // Dibujar carriles según el tipo configurado
        TipoCarril tipoActual = ObtenerTipoCarrilActual();
        
        if (tipoActual == TipoCarril.DobleCarril)
        {
            Vector3 carrilIzqOffset = new Vector3(0, 0, separacionCarriles / 2);
            Vector3 carrilDerOffset = new Vector3(0, 0, -separacionCarriles / 2);
            
            Gizmos.color = new Color(0, 1, 0, 0.7f);
            Gizmos.DrawSphere(puntoSpawnIzquierdo.position + carrilIzqOffset, 0.3f);
            
            Gizmos.color = new Color(1, 0, 0, 0.7f);
            Gizmos.DrawSphere(puntoSpawnDerecho.position + carrilDerOffset, 0.3f);
        }
        else if (tipoActual == TipoCarril.SoloIzquierda)
        {
            // Solo mostrar el punto de spawn izquierdo más prominente
            Gizmos.color = new Color(0, 1, 0, 1f);
            Gizmos.DrawSphere(puntoSpawnIzquierdo.position, 0.8f);
            
            // Dibujar carriles superior e inferior para dirección única
            Vector3 carrilSuperior = puntoSpawnIzquierdo.position + new Vector3(0, 0, separacionCarriles / 2);
            Vector3 carrilInferior = puntoSpawnIzquierdo.position + new Vector3(0, 0, -separacionCarriles / 2);
            
            Gizmos.color = new Color(0, 1, 1, 0.8f); // Cyan para carril superior
            Gizmos.DrawSphere(carrilSuperior, 0.4f);
            Gizmos.DrawLine(carrilSuperior, carrilSuperior + Vector3.right * 3f);
            
            Gizmos.color = new Color(0, 0.5f, 1, 0.8f); // Azul para carril inferior
            Gizmos.DrawSphere(carrilInferior, 0.4f);
            Gizmos.DrawLine(carrilInferior, carrilInferior + Vector3.right * 3f);
            
            // Dibujar línea de dirección principal
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawLine(puntoSpawnIzquierdo.position, puntoSpawnIzquierdo.position + Vector3.right * 3f);
        }
        else if (tipoActual == TipoCarril.SoloDerecha)
        {
            // Solo mostrar el punto de spawn derecho más prominente
            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawSphere(puntoSpawnDerecho.position, 0.8f);
            
            // Dibujar carriles superior e inferior para dirección única
            Vector3 carrilSuperior = puntoSpawnDerecho.position + new Vector3(0, 0, separacionCarriles / 2);
            Vector3 carrilInferior = puntoSpawnDerecho.position + new Vector3(0, 0, -separacionCarriles / 2);
            
            Gizmos.color = new Color(1, 1, 0, 0.8f); // Amarillo para carril superior
            Gizmos.DrawSphere(carrilSuperior, 0.4f);
            Gizmos.DrawLine(carrilSuperior, carrilSuperior + Vector3.left * 3f);
            
            Gizmos.color = new Color(1, 0.5f, 0, 0.8f); // Naranja para carril inferior
            Gizmos.DrawSphere(carrilInferior, 0.4f);
            Gizmos.DrawLine(carrilInferior, carrilInferior + Vector3.left * 3f);
            
            // Dibujar línea de dirección principal
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawLine(puntoSpawnDerecho.position, puntoSpawnDerecho.position + Vector3.left * 3f);
        }
        
        // Dibujar punto central
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(puntoCentral, 0.5f);
        
        // Líneas de ruta según el tipo de carril
        if (tipoActual == TipoCarril.DobleCarril || tipoActual == TipoCarril.SoloIzquierda)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(puntoSpawnIzquierdo.position, puntoCentral);
        }
        
        if (tipoActual == TipoCarril.DobleCarril || tipoActual == TipoCarril.SoloDerecha)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(puntoSpawnDerecho.position, puntoCentral);
        }
    }
    
    /// <summary>
    /// Configura las plataformas para el VehiclePlayerCollision
    /// </summary>
    private void ConfigurarPlataformasParaVehiculo(VehiclePlayerCollision playerCollision)
    {
        if (playerCollision == null) return;
        
        // Buscar plataformas en la escena
        GameObject[] plataformas = GameObject.FindGameObjectsWithTag("Platform");
        
        if (plataformas.Length >= 2)
        {
            // Ordenar por posición X para identificar izquierda y derecha
            System.Array.Sort(plataformas, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            
            // Asignar las plataformas usando las propiedades públicas
            playerCollision.PlataformaIzquierda = plataformas[0].transform;
            playerCollision.PlataformaDerecha = plataformas[plataformas.Length - 1].transform;
            

        }
        else
        {
            Debug.LogWarning("No se encontraron suficientes plataformas con tag 'Platform' para configurar VehiclePlayerCollision");
        }
    }
    
    /// <summary>
    /// Inicializa el sistema de rondas y valida las configuraciones de carril
    /// </summary>
    private void InitializeRoundSystem()
    {
        if (configuracionRondas == null || configuracionRondas.Length == 0)
        {
            Debug.LogWarning("[AutoGenerator] Sistema de rondas habilitado pero no hay configuración de rondas definida. Usando modo continuo.");
            usarSistemaRondas = false;
            return;
        }
        
        // Validar y configurar las posiciones de carril para cada ronda
        for (int i = 0; i < configuracionRondas.Length; i++)
        {
            if (configuracionRondas[i] == null)
            {
                Debug.LogError($"[AutoGenerator] Error: La configuración de ronda {i} es null. Desactivando sistema de rondas.");
                usarSistemaRondas = false;
                return;
            }
            configuracionRondas[i].ValidarConfiguracionCarriles();
        }
        
        rondaActual = 0;
        autosSpawneadosEnRonda = 0;
        autosQueDebenVolverAlPool = 0;
        esperandoFinDeRonda = false;
        esperandoInicioDeRonda = true; // Activar espera para la primera ronda
        
        // Inicializar el último carril usado
        ultimoCarrilUsado = PosicionCarril.Inferior;
        
        Debug.Log($"[AutoGenerator] Sistema de rondas inicializado con {configuracionRondas.Length} rondas configuradas.");
    }
    
    /// <summary>
    /// Inicia la generación de autos basada en la configuración (rondas o continuo)
    /// </summary>
    private void IniciarGeneracion()
    {
        if (usarSistemaRondas)
        {
            corrutinGeneracion = StartCoroutine(GenerarAutosPorRondas());
        }
        else
        {
            corrutinGeneracion = StartCoroutine(GenerarAutos());
        }
    }
    
    /// <summary>
    /// Corrutina que genera autos por sistema de rondas
    /// </summary>
    private IEnumerator GenerarAutosPorRondas()
    {
        // Validar que tenemos configuraciones de rondas válidas
        if (configuracionRondas == null || configuracionRondas.Length == 0)
        {
            Debug.LogError("[AutoGenerator] Error: No hay configuraciones de rondas definidas. Desactivando sistema de rondas.");
            usarSistemaRondas = false;
            yield break;
        }
        
        float tiempoInicioEspera = 0f;
        
        while (true)
        {
            // Si estamos esperando para iniciar una nueva ronda
            if (esperandoInicioDeRonda)
            {
                if (tiempoEsperaEntreRondas > 0)
                {
                    yield return new WaitForSeconds(tiempoEsperaEntreRondas);
                }
                
                esperandoInicioDeRonda = false;
            }
              // Si no estamos esperando el fin de ronda y aún quedan autos por spawnear
            if (!esperandoFinDeRonda && !esperandoInicioDeRonda && 
                rondaActual < configuracionRondas.Length && 
                autosSpawneadosEnRonda < configuracionRondas[rondaActual].cantidadAutos)
            {
                GenerarAutoEnRonda();
                autosSpawneadosEnRonda++;
                autosQueDebenVolverAlPool++;
                
                if (mostrarDebugInfo)
                {
                    Debug.Log($"[AutoGenerator] Spawneado {autosSpawneadosEnRonda}/{configuracionRondas[rondaActual].cantidadAutos} autos. Esperando retorno de: {autosQueDebenVolverAlPool}");
                }
                
                // Si completamos los autos de esta ronda, empezar a esperar
                if (autosSpawneadosEnRonda >= configuracionRondas[rondaActual].cantidadAutos)
                {
                    esperandoFinDeRonda = true;
                    tiempoInicioEspera = Time.time;
                    
                    if (mostrarDebugInfo)
                    {
                        Debug.Log($"[AutoGenerator] ⏳ Todos los autos spawneados para {configuracionRondas[rondaActual].nombreRonda}. Esperando que {autosQueDebenVolverAlPool} vehículos regresen al pool.");
                    }
                }
                
                yield return new WaitForSeconds(configuracionRondas[rondaActual].tiempoEntreAutos);
            }
            else if (esperandoFinDeRonda)
            {
                // Verificar timeout para evitar esperas infinitas
                if (Time.time - tiempoInicioEspera > tiempoEsperaMaximoRonda)
                {
                    // Forzar avance si han pasado demasiados segundos
                    ClearActiveAutos();
                    autosQueDebenVolverAlPool = 0;
                    AvanzarSiguienteRonda();
                }
                else
                {
                    // Verificar si los autos activos coinciden con los que esperamos
                    int autosActivos = vehiclePool != null ? vehiclePool.GetActiveVehicleCount() : 0;
                    if (autosActivos == 0 && autosQueDebenVolverAlPool > 0)
                    {
                        // No hay autos activos pero el contador indica que deberían haber algunos
                        // Corregir el contador y avanzar
                        autosQueDebenVolverAlPool = 0;
                        AvanzarSiguienteRonda();
                    }
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // Esperamos un poco antes de verificar si podemos avanzar a la siguiente ronda
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
      /// <summary>
    /// Genera un auto específicamente para el sistema de rondas con configuración individual de carril y tipo de vehículo
    /// </summary>
    private void GenerarAutoEnRonda()
    {
        // Validar que el índice esté dentro del rango
        if (rondaActual >= configuracionRondas.Length)
        {
            Debug.LogError($"[AutoGenerator] Error: Índice de ronda fuera de rango. RondaActual: {rondaActual}, Configuraciones disponibles: {configuracionRondas.Length}");
            return;
        }
        
        RondaConfig rondaActualConfig = configuracionRondas[rondaActual];
        
        // Obtener el tipo de vehículo para este auto específico
        TipoVehiculo tipoVehiculoAuto = rondaActualConfig.ObtenerTipoVehiculoParaAuto(autosSpawneadosEnRonda);
        
        // Determinar qué prefab usar
        GameObject prefabAUsar = ObtenerPrefabSegunTipo(tipoVehiculoAuto);
        if (prefabAUsar == null)
        {
            Debug.LogError($"[AutoGenerator] No se pudo obtener el prefab para el tipo de vehículo: {tipoVehiculoAuto}");
            return;
        }
        
        // DELEGAR AL POOL: Obtener un auto del pool usando el prefab específico
        GameObject auto = vehiclePool.GetVehicleFromPool(prefabAUsar);
        if (auto == null)
        {
            Debug.LogError("[AutoGenerator] No se pudo obtener un auto del pool para la ronda");
            return;
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"[AutoGenerator] Spawneando vehículo {auto.name} de tipo {tipoVehiculoAuto} para {rondaActualConfig.nombreRonda} ({autosSpawneadosEnRonda + 1}/{rondaActualConfig.cantidadAutos})");
        }
        
        // Determinar lado de spawn basado en configuración de carril
        bool spawnearDesdeIzquierda = DebeSpawnearDesdeIzquierda();
        
        // Obtener la configuración de carril individual para este auto
        PosicionCarril posicionCarrilAuto = rondaActualConfig.ObtenerPosicionCarrilParaAuto(autosSpawneadosEnRonda, ultimoCarrilUsado);
        
        // Actualizar el último carril usado para el próximo auto
        ultimoCarrilUsado = posicionCarrilAuto;
        
        // LÓGICA DE GENERACIÓN: Configurar posición (ahora incluye configuración individual de carril)
        ConfigurarPosicionAutoRonda(auto, spawnearDesdeIzquierda, posicionCarrilAuto);
        
        // LÓGICA DE GENERACIÓN: Activar el auto
        auto.SetActive(true);
        
        // LÓGICA DE GENERACIÓN: Configurar movimiento con tipo de vehículo
        ConfigurarMovimientoAutoRonda(auto, spawnearDesdeIzquierda, tipoVehiculoAuto);
        
        // LÓGICA DE GENERACIÓN: Configurar colisión con el puente
        ConfigurarColisionPuente(auto);
        
        // Solo alternar lado si estamos en modo doble carril
        if (ObtenerTipoCarrilActual() == TipoCarril.DobleCarril)
        {
            spawnDesdeIzquierda = !spawnDesdeIzquierda;
        }
    }
    
    /// <summary>
    /// Configura la posición y rotación del auto con soporte para diferentes tipos de carril y configuración individual
    /// </summary>
    private void ConfigurarPosicionAutoRonda(GameObject auto, bool desdeIzquierda, PosicionCarril posicionCarrilIndividual)
    {
        Transform puntoSpawn = desdeIzquierda ? puntoSpawnIzquierdo : puntoSpawnDerecho;
        Vector3 posicionSpawn = puntoSpawn.position;
        
        // Aplicar offset de carril basado en el tipo actual
        TipoCarril tipoActual = ObtenerTipoCarrilActual();
        if (tipoActual == TipoCarril.DobleCarril)
        {
            // En doble carril, usar la lógica original (ignora posicionCarrilIndividual)
            float offsetCarril = desdeIzquierda ? separacionCarriles / 2 : -separacionCarriles / 2;
            posicionSpawn += new Vector3(0, 0, offsetCarril);
        }
        else if (tipoActual == TipoCarril.SoloIzquierda || tipoActual == TipoCarril.SoloDerecha)
        {
            // En carriles de una sola dirección, usar la configuración individual
            // Si es Random, ya se resolvió en ObtenerPosicionCarrilParaAuto()
            float offsetCarril = (posicionCarrilIndividual == PosicionCarril.Superior) ? separacionCarriles / 2 : -separacionCarriles / 2;
            posicionSpawn += new Vector3(0, 0, offsetCarril);
        }
        
        auto.transform.position = posicionSpawn;
        
        // Configurar rotación basada en la dirección de movimiento (restaurado como en versión anterior)
        if (desdeIzquierda)
        {
            // Auto se mueve hacia la izquierda, debe mirar hacia la izquierda
            auto.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }
        else
        {
            // Auto se mueve hacia la derecha, debe mirar hacia la derecha  
            auto.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }
        
        // Asegurar que el Rigidbody tenga las restricciones correctas
        Rigidbody rb = auto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        

    }
    
    /// <summary>
    /// Configura el movimiento del auto con velocidad personalizable (restaurado a lógica original que funcionaba)
    /// </summary>
    private void ConfigurarMovimientoAutoRonda(GameObject auto, bool desdeIzquierda, TipoVehiculo tipoVehiculo)
    {
        AutoMovement movimiento = auto.GetComponent<AutoMovement>();
        if (movimiento != null)
        {
            // Determinar dirección basada en el spawn (RESTAURADO: comportamiento original que funcionaba)
            Vector3 direccion = desdeIzquierda ? Vector3.left : Vector3.right;
            
            // Obtener velocidad según el tipo de vehículo
            float velocidadAUsar = ObtenerVelocidadSegunTipo(tipoVehiculo);
            
            movimiento.SetDireccionMovimiento(direccion);
            movimiento.SetVelocidad(velocidadAUsar);
            // Aplicar corrección automática de rotación del modelo
            movimiento.AplicarCorreccionAutomatica();
            movimiento.enabled = true;
            
            // DEBUG ESPECÍFICO: Información detallada sobre la configuración del vehículo
            if (mostrarDebugInfo)
            {
                Debug.Log($"[AutoGenerator] DEBUG ConfigurarMovimientoAutoRonda:");
                Debug.Log($"  - Vehículo: {auto.name}");
                Debug.Log($"  - Tipo: {tipoVehiculo}");
                Debug.Log($"  - Dirección: {direccion}");
                Debug.Log($"  - Velocidad: {velocidadAUsar}");
                Debug.Log($"  - AutoMovement.enabled: {movimiento.enabled}");
                Debug.Log($"  - Posición inicial: {auto.transform.position}");
                Debug.Log($"  - Rotación inicial: {auto.transform.rotation.eulerAngles}");
                
                // Revisar la estructura interna del vehículo
                Debug.Log($"  - Cantidad de hijos: {auto.transform.childCount}");
                for (int i = 0; i < auto.transform.childCount; i++)
                {
                    Transform hijo = auto.transform.GetChild(i);
                    Debug.Log($"    - Hijo {i}: {hijo.name} (activo: {hijo.gameObject.activeSelf})");
                }
            }
        }
        else
        {
            Debug.LogWarning($"El auto {auto.name} no tiene componente AutoMovement");
        }
    }
      /// <summary>
    /// Método llamado directamente cuando un vehículo es devuelto al pool por cualquier medio
    /// </summary>
    public void NotificarAutoDevueltoAlPool(GameObject vehiculo)
    {
        if (usarSistemaRondas && esperandoFinDeRonda && IsVehicleFromPool(vehiculo))
        {
            autosQueDebenVolverAlPool--;
            
            if (mostrarDebugInfo)
            {
                Debug.Log($"[AutoGenerator] Vehículo {vehiculo.name} devuelto al pool. Quedan: {autosQueDebenVolverAlPool}");
            }
            
            if (autosQueDebenVolverAlPool <= 0)
            {
                AvanzarSiguienteRonda();
            }
        }
    }
      /// <summary>
    /// Avanza a la siguiente ronda
    /// </summary>
    private void AvanzarSiguienteRonda()
    {
        // Validar que tenemos configuraciones válidas
        if (configuracionRondas == null || configuracionRondas.Length == 0)
        {
            Debug.LogError("[AutoGenerator] Error: No hay configuraciones de rondas válidas para avanzar.");
            return;
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"[AutoGenerator] Completando ronda {rondaActual}: {configuracionRondas[rondaActual].nombreRonda}");
        }
        
        // Verificar si hay evento sorpresa después de esta ronda
        VerificarEventoSorpresa(rondaActual + 1); // +1 porque rondaActual se incrementa después
        
        rondaActual++;
        
        // Si llegamos al final de las rondas
        if (rondaActual >= configuracionRondas.Length)
        {
            if (loopearRondas)
            {
                rondaActual = 0;
                Debug.Log($"[AutoGenerator] Reiniciando rondas desde el principio (ronda {rondaActual}).");
            }
            else
            {
                Debug.Log("[AutoGenerator] 🎉 Todas las rondas completadas. Deteniendo generación.");
                
                // Notificar al GameConditionManager que todas las rondas terminaron
                NotificarTodasLasRondasCompletadas();
                
                StopCoroutine(corrutinGeneracion);
                return;
            }
        }
        
        // Resetear contadores para la nueva ronda
        autosSpawneadosEnRonda = 0;
        autosQueDebenVolverAlPool = 0;
        esperandoFinDeRonda = false;
        esperandoInicioDeRonda = true; // Activar espera para la siguiente ronda
        
        // Resetear el último carril usado para que la nueva ronda comience con alternancia limpia
        ultimoCarrilUsado = PosicionCarril.Inferior;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"[AutoGenerator] Avanzando a ronda {rondaActual}: {configuracionRondas[rondaActual].nombreRonda}");
        }
    }
    
    /// <summary>
    /// Determina el tipo de carril a usar (global o específico de la ronda)
    /// </summary>
    private TipoCarril ObtenerTipoCarrilActual()
    {
        // Verificar que el sistema de rondas esté activo y que los arrays sean válidos
        if (usarSistemaRondas && 
            configuracionRondas != null && 
            configuracionRondas.Length > 0 && 
            rondaActual >= 0 && 
            rondaActual < configuracionRondas.Length &&
            configuracionRondas[rondaActual].sobrescribirTipoCarril)
        {
            return configuracionRondas[rondaActual].tipoCarrilRonda;
        }
        return tipoCarril;
    }
    
    /// <summary>
    /// Determina si debe spawnear desde la izquierda basado en el tipo de carril
    /// </summary>
    private bool DebeSpawnearDesdeIzquierda()
    {
        TipoCarril tipoActual = ObtenerTipoCarrilActual();
        
        switch (tipoActual)
        {
            case TipoCarril.SoloIzquierda:
                return true;
            case TipoCarril.SoloDerecha:
                return false;
            case TipoCarril.DobleCarril:
            default:
                return spawnDesdeIzquierda; // Usar la alternancia normal
        }
    }
    
    // ===== MÉTODOS PÚBLICOS PARA CONTROL DE RONDAS =====
    
    /// <summary>
    /// Inicia el sistema de rondas manualmente
    /// </summary>
    public void IniciarSistemaRondas()
    {
        if (!usarSistemaRondas)
        {
            Debug.LogWarning("[AutoGenerator] El sistema de rondas no está habilitado. Actívalo en el inspector.");
            return;
        }
        
        // Ejecutar diagnóstico en modo debug
        DiagnosticarSistemaRondas();
        
        // Detener la generación actual si está corriendo
        if (corrutinGeneracion != null)
        {
            StopCoroutine(corrutinGeneracion);
        }
        
        // Reiniciar sistema de rondas
        InitializeRoundSystem();
        
        // Solo iniciar la corrutina si el sistema se inicializó correctamente
        if (usarSistemaRondas) // InitializeRoundSystem puede desactivar usarSistemaRondas si hay errores
        {
            corrutinGeneracion = StartCoroutine(GenerarAutosPorRondas());
            Debug.Log("[AutoGenerator] Sistema de rondas iniciado correctamente.");
        }
        else
        {
            Debug.LogError("[AutoGenerator] No se pudo iniciar el sistema de rondas debido a errores de configuración.");
        }
    }
    
    /// <summary>
    /// Detiene el sistema de rondas y vuelve al modo continuo
    /// </summary>
    public void DetenerSistemaRondas()
    {
        if (corrutinGeneracion != null)
        {
            StopCoroutine(corrutinGeneracion);
        }
        
        usarSistemaRondas = false;
        corrutinGeneracion = StartCoroutine(GenerarAutos());

    }
    
    /// <summary>
    /// Fuerza el avance a la siguiente ronda
    /// </summary>
    public void ForzarSiguienteRonda()
    {
        if (!usarSistemaRondas)
        {
            Debug.LogWarning("El sistema de rondas no está habilitado.");
            return;
        }
        
        // Limpiar todos los autos activos para forzar el avance
        ClearActiveAutos();
        autosQueDebenVolverAlPool = 0;
        AvanzarSiguienteRonda();
    }
    
    /// <summary>
    /// Obtiene información sobre la ronda actual
    /// </summary>
    public string GetInfoRondaActual()
    {
        if (!usarSistemaRondas || configuracionRondas == null || configuracionRondas.Length == 0)
        {
            return "Sistema de rondas no activo";
        }
        
        if (rondaActual >= configuracionRondas.Length)
        {
            return "Sistema de rondas completado";
        }
        
        RondaConfig ronda = configuracionRondas[rondaActual];
        return $"{ronda.nombreRonda}: {autosSpawneadosEnRonda}/{ronda.cantidadAutos} autos - Esperando retorno: {autosQueDebenVolverAlPool}";
    }
    
    /// <summary>
    /// Configura dinámicamente las rondas desde código
    /// </summary>
    public void ConfigurarRondas(RondaConfig[] nuevasRondas, bool iniciarInmediatamente = false)
    {
        configuracionRondas = nuevasRondas;
        
        if (iniciarInmediatamente)
        {
            usarSistemaRondas = true;
            IniciarSistemaRondas();
        }
    }
    
    /// <summary>
    /// Habilita o deshabilita el sistema de rondas
    /// </summary>
    public void SetSistemaRondasActivo(bool activo)
    {
        bool estabaActivo = usarSistemaRondas;
        usarSistemaRondas = activo;
        
        if (activo && !estabaActivo)
        {
            IniciarSistemaRondas();
        }
        else if (!activo && estabaActivo)
        {
            DetenerSistemaRondas();
        }
    }
    
    /// <summary>
    /// Obtiene el estado actual del sistema de rondas
    /// </summary>
    public bool IsUsandoSistemaRondas()
    {
        return usarSistemaRondas;
    }
    
    /// <summary>
    /// Obtiene el índice de la ronda actual (0-based)
    /// </summary>
    public int GetRondaActual()
    {
        return rondaActual;
    }
    
    /// <summary>
    /// Obtiene el número total de rondas configuradas
    /// </summary>
    public int GetTotalRondas()
    {
        return configuracionRondas?.Length ?? 0;
    }

    /// <summary>
    /// Obtiene el número total de vehículos configurados para este nivel
    /// (suma de cantidadAutos en cada RondaConfig).
    /// Si no hay configuración de rondas devuelve 0.
    /// </summary>
    public int GetTotalVehiclesForLevel()
    {
        if (configuracionRondas == null || configuracionRondas.Length == 0) return 0;

        int total = 0;
        for (int i = 0; i < configuracionRondas.Length; i++)
        {
            var r = configuracionRondas[i];
            if (r != null) total += Mathf.Max(0, r.cantidadAutos);
        }
        return total;
    }
    
    /// <summary>
    /// Notifica al GameConditionManager que todas las rondas han terminado
    /// </summary>
    private void NotificarTodasLasRondasCompletadas()
    {
        // Buscar el GameConditionManager en la escena
        GameConditionManager gameConditionManager = FindFirstObjectByType<GameConditionManager>();
        
        if (gameConditionManager != null)
        {
            Debug.Log("[AutoGenerator] Notificando al GameConditionManager que todas las rondas han terminado.");
            gameConditionManager.NotificarTodasLasRondasCompletadas();
        }
        else
        {
            Debug.LogWarning("[AutoGenerator] No se encontró GameConditionManager para notificar el fin de rondas.");
        }
    }
    
    /// <summary>
    /// Crea automáticamente una configuración de carriles alternada (Inferior-Superior-Inferior-...)
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <param name="iniciarConSuperior">Si empezar con carril superior en lugar de inferior</param>
    /// <returns>Array de posiciones de carril alternadas</returns>
    public static PosicionCarril[] GenerarConfiguracionAlternada(int cantidadAutos, bool iniciarConSuperior = false)
    {
        PosicionCarril[] configuracion = new PosicionCarril[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            bool esPar = (i % 2 == 0);
            configuracion[i] = (esPar != iniciarConSuperior) ? PosicionCarril.Inferior : PosicionCarril.Superior;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Crea una configuración donde todos los autos van por el mismo carril
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <param name="posicion">Posición del carril a usar para todos los autos</param>
    /// <returns>Array de posiciones de carril uniforme</returns>
    public static PosicionCarril[] GenerarConfiguracionUniforme(int cantidadAutos, PosicionCarril posicion)
    {
        PosicionCarril[] configuracion = new PosicionCarril[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            configuracion[i] = posicion;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Crea una configuración completamente aleatoria donde cada auto puede ir por cualquier carril
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <returns>Array de posiciones de carril aleatorias</returns>
    public static PosicionCarril[] GenerarConfiguracionAleatoria(int cantidadAutos)
    {
        PosicionCarril[] configuracion = new PosicionCarril[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            configuracion[i] = PosicionCarril.Random;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Crea una configuración donde todos los autos son del mismo tipo de vehículo
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <param name="tipoVehiculo">Tipo de vehículo a usar para todos los autos</param>
    /// <returns>Array de tipos de vehículo uniforme</returns>
    public static TipoVehiculo[] GenerarConfiguracionVehiculoUniforme(int cantidadAutos, TipoVehiculo tipoVehiculo)
    {
        TipoVehiculo[] configuracion = new TipoVehiculo[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            configuracion[i] = tipoVehiculo;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Crea una configuración donde los tipos de vehículo se alternan entre Auto1 y Auto2
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <param name="iniciarConAuto2">Si empezar con Auto2 en lugar de Auto1</param>
    /// <returns>Array de tipos de vehículo alternados</returns>
    public static TipoVehiculo[] GenerarConfiguracionVehiculoAlternada(int cantidadAutos, bool iniciarConAuto2 = false)
    {
        TipoVehiculo[] configuracion = new TipoVehiculo[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            bool esPar = (i % 2 == 0);
            configuracion[i] = (esPar != iniciarConAuto2) ? TipoVehiculo.Auto1 : TipoVehiculo.Auto2;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Crea una configuración completamente aleatoria donde cada auto puede ser de cualquier tipo
    /// </summary>
    /// <param name="cantidadAutos">Número de autos para los que generar la configuración</param>
    /// <returns>Array de tipos de vehículo aleatorios</returns>
    public static TipoVehiculo[] GenerarConfiguracionVehiculoAleatoria(int cantidadAutos)
    {
        TipoVehiculo[] configuracion = new TipoVehiculo[cantidadAutos];
        for (int i = 0; i < cantidadAutos; i++)
        {
            configuracion[i] = TipoVehiculo.Random;
        }
        return configuracion;
    }
    
    /// <summary>
    /// Método de diagnóstico para verificar el estado del sistema de rondas
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DiagnosticarSistemaRondas()
    {
        Debug.Log("=== DIAGNÓSTICO DEL SISTEMA DE RONDAS ===");
        Debug.Log($"usarSistemaRondas: {usarSistemaRondas}");
        Debug.Log($"configuracionRondas null: {configuracionRondas == null}");
        Debug.Log($"configuracionRondas.Length: {(configuracionRondas?.Length ?? 0)}");
        Debug.Log($"rondaActual: {rondaActual}");
        Debug.Log($"autosSpawneadosEnRonda: {autosSpawneadosEnRonda}");
        Debug.Log($"esperandoFinDeRonda: {esperandoFinDeRonda}");
        Debug.Log($"esperandoInicioDeRonda: {esperandoInicioDeRonda}");
        Debug.Log($"vehiclePool null: {vehiclePool == null}");
        Debug.Log($"corrutinGeneracion null: {corrutinGeneracion == null}");
        
        if (configuracionRondas != null)
        {
            for (int i = 0; i < configuracionRondas.Length; i++)
            {
                var ronda = configuracionRondas[i];
                Debug.Log($"Ronda {i}: {ronda?.nombreRonda ?? "NULL"} - Autos: {ronda?.cantidadAutos ?? 0}");
            }
        }
        Debug.Log("=== FIN DIAGNÓSTICO ===");
    }
    
    /// <summary>
    /// Método debug para verificar el estado del sistema de alternancia de carriles
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DiagnosticarSistemaCarriles()
    {
        Debug.Log("=== DIAGNÓSTICO DEL SISTEMA DE CARRILES ===");
        Debug.Log($"Último carril usado: {ultimoCarrilUsado}");
        Debug.Log($"Tipo de carril actual: {ObtenerTipoCarrilActual()}");
        Debug.Log($"Ronda actual: {rondaActual}");
        Debug.Log($"Autos spawneados en ronda: {autosSpawneadosEnRonda}");
        
        if (usarSistemaRondas && rondaActual < configuracionRondas.Length)
        {
            var ronda = configuracionRondas[rondaActual];
            Debug.Log($"Configuración de ronda actual: {ronda.nombreRonda}");
            Debug.Log($"Posiciones de carril configuradas: {ronda.posicionesCarrilPorAuto?.Length ?? 0}");
            
            if (ronda.posicionesCarrilPorAuto != null)
            {
                for (int i = 0; i < ronda.posicionesCarrilPorAuto.Length; i++)
                {
                    Debug.Log($"  Auto {i}: {ronda.posicionesCarrilPorAuto[i]}");
                }
            }
        }
        Debug.Log("=== FIN DIAGNÓSTICO CARRILES ===");
    }
    
    /// <summary>
    /// Obtiene el prefab correspondiente según el tipo de vehículo
    /// </summary>
    private GameObject ObtenerPrefabSegunTipo(TipoVehiculo tipo)
    {
        switch (tipo)
        {
            case TipoVehiculo.Auto1:
                return prefabsAutos.Length > 0 && prefabsAutos[0] != null ? prefabsAutos[0] : null;
            case TipoVehiculo.Auto2:
                return prefabsAutos.Length > 1 && prefabsAutos[1] != null ? prefabsAutos[1] : prefabsAutos[0];
            case TipoVehiculo.Auto3:
                return prefabsAutos.Length > 2 && prefabsAutos[2] != null ? prefabsAutos[2] : prefabsAutos[0];
            case TipoVehiculo.Auto4:
                return prefabsAutos.Length > 3 && prefabsAutos[3] != null ? prefabsAutos[3] : prefabsAutos[0];
            case TipoVehiculo.Auto5:
                return prefabsAutos.Length > 4 && prefabsAutos[4] != null ? prefabsAutos[4] : prefabsAutos[0];
            case TipoVehiculo.Random:
                int randomIndex = Random.Range(0, prefabsAutos.Length);
                return prefabsAutos[randomIndex] != null ? prefabsAutos[randomIndex] : prefabsAutos[0];
            default:
                return prefabsAutos.Length > 0 ? prefabsAutos[0] : null;
        }
    }
    
    /// <summary>
    /// Obtiene la velocidad correspondiente según el tipo de vehículo
    /// </summary>
    private float ObtenerVelocidadSegunTipo(TipoVehiculo tipo)
    {
        switch (tipo)
        {
            case TipoVehiculo.Auto1:
                return velocidadesAutos.Length > 0 ? velocidadesAutos[0] : 5f;
            case TipoVehiculo.Auto2:
                return velocidadesAutos.Length > 1 ? velocidadesAutos[1] : 6f;
            case TipoVehiculo.Auto3:
                return velocidadesAutos.Length > 2 ? velocidadesAutos[2] : 7f;
            case TipoVehiculo.Auto4:
                return velocidadesAutos.Length > 3 ? velocidadesAutos[3] : 8f;
            case TipoVehiculo.Auto5:
                return velocidadesAutos.Length > 4 ? velocidadesAutos[4] : 9f;
            case TipoVehiculo.Random:
                // Para Random, elegir entre las velocidades aleatoriamente
                int randomIndex = Random.Range(0, velocidadesAutos.Length);
                return velocidadesAutos[randomIndex];
            default:
                return velocidadesAutos.Length > 0 ? velocidadesAutos[0] : 5f;
        }
    }
    
    /// <summary>
    /// Determina el tipo de vehículo basado en el prefab usado
    /// </summary>
    private TipoVehiculo DeterminarTipoVehiculo(GameObject vehiculo)
    {
        if (vehiculo == null) return TipoVehiculo.Auto1;
        
        // Comparar con los prefabs para determinar el tipo
        for (int i = 0; i < prefabsAutos.Length && i < 5; i++)
        {
            if (prefabsAutos[i] != null && vehiculo.name.Contains(prefabsAutos[i].name))
            {
                return (TipoVehiculo)i;
            }
        }
        
        return TipoVehiculo.Auto1;
    }
    
    /// <summary>
    /// Verifica si hay evento sorpresa configurado para después de la ronda especificada
    /// </summary>
    private void VerificarEventoSorpresa(int rondaCompletada)
    {
        // Buscar el componente BridgeSurpriseEvent en la escena
        BridgeSurpriseEvent surpriseEvent = FindObjectOfType<BridgeSurpriseEvent>();
        if (surpriseEvent == null)
        {
            return; // No hay sistema de eventos sorpresa en la escena
        }
        
        // Buscar un evento configurado para esta ronda específica
        EventoSorpresa eventoAEjecutar = surpriseEvent.BuscarEventoPorRonda(rondaCompletada);
        
        // Ejecutar el evento si se encontró uno
        if (eventoAEjecutar != null)
        {
            if (mostrarDebugInfo)
            {
                Debug.Log($"🎭 [AutoGenerator] Ejecutando evento sorpresa '{eventoAEjecutar.nombreEvento}' después de la ronda {rondaCompletada - 1}");
            }
            
            surpriseEvent.EjecutarEventoSorpresa(eventoAEjecutar);
        }
        else
        {
            if (mostrarDebugInfo)
            {
                Debug.Log($"[AutoGenerator] No hay eventos sorpresa configurados para después de la ronda {rondaCompletada - 1}");
            }
        }
    }
}