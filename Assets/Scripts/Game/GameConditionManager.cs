using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja las condiciones de victoria y derrota del juego basadas en triggers con tags
/// </summary>
public class GameConditionManager : MonoBehaviour
{
    [Header("Configuración de Victoria")]
    [SerializeField] private int vehiculosParaVictoria = 10;
    [SerializeField] private string tagTriggerVictoria = "PassTrigger";
    
    [Header("Configuración de Victoria por Rondas")]
    [SerializeField] private bool usarVictoriaPorRondas = false;
    [SerializeField] private AutoGenerator autoGenerator;
    
    [Header("Canvas de Fin de Juego")]
    [SerializeField] private GameObject victoryCanvasPrefab;
    [SerializeField] private GameObject defeatCanvasPrefab;
    [SerializeField] private Canvas pauseCanvas; // Canvas de pausa en la escena
    private GameObject currentEndGameCanvas;
    
    [Header("Configuración de Niveles")]
    [SerializeField] private string nextLevelSceneName = "";
    [SerializeField] private bool useAutoLevelProgression = false;
    
    [Header("Control de Jugador")]
    [SerializeField] private bool desactivarPlayerControllerEnFinDeJuego = true;
    [SerializeField] private bool desactivarAutoGeneratorEnFinDeJuego = true;
    [SerializeField] private bool activarAnimacionesFinDeJuego = true;
    private PlayerController[] playerControllers;
    private PlayerAnimator[] playerAnimators;
    
    [Header("Configuración de Derrota")]
    [SerializeField] private int vehiculosParaDerrota = 3;
    [SerializeField] private string tagTriggerDerrota = "FallTrigger";
    
    [Header("Configuración de Vehículos")]
    [SerializeField] private string tagVehiculo = "Vehicle";
    
    [Header("Estado del Juego")]
    [SerializeField] private bool juegoActivo = true;
    [SerializeField] private bool mostrarDebugInfo = true;
    
    // Contadores
    private int contadorVictoria = 0;
    private int contadorDerrota = 0;
    // Vehículos restantes por nivel (disponibles para pasar o caer). Se inicializa en ReiniciarJuego
    private int vehiculosRestantes = 0;
    private int vehiculosPasados = 0; // estadística
    
    // Estados del juego
    private bool juegoTerminado = false;
    private bool juegoEnPausa = false;
    private bool todasLasRondasCompletadas = false;
    
    [Header("Eventos")]
    public UnityEvent OnVictoria;
    public UnityEvent OnDerrota;
    public UnityEvent<int> OnVictoriaProgreso;
    public UnityEvent<int> OnDerrotaProgreso;
    
    // Singleton para fácil acceso
    public static GameConditionManager Instance { get; private set; }
    
    #region Unity Events
    
    private void Awake()
    {
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse al evento de carga de escenas para resetear pausa
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Inicializar estado del juego inmediatamente para que otras UIs lo reciban en Start
            ReiniciarJuego();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        VerificarTags();
    }
    
    private void Start()
    {
        if (mostrarDebugInfo)
        {
            Debug.Log($"GameConditionManager inicializado - Victoria: {vehiculosParaVictoria} vehículos, Derrota: {vehiculosParaDerrota} vehículos");
        }
        
        // Inicializar eventos si están nulos
        if (OnVictoria == null) OnVictoria = new UnityEvent();
        if (OnDerrota == null) OnDerrota = new UnityEvent();
        if (OnVictoriaProgreso == null) OnVictoriaProgreso = new UnityEvent<int>();
        if (OnDerrotaProgreso == null) OnDerrotaProgreso = new UnityEvent<int>();
        
        // Configurar sistema de victoria por rondas si está habilitado
        ConfigurarVictoriaPorRondas();
        
        // Encontrar todos los PlayerController y PlayerAnimator en la escena
        playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        playerAnimators = FindObjectsByType<PlayerAnimator>(FindObjectsSortMode.None);
        
        // Buscar todos los triggers existentes y configurarlos
        ConfigurarTriggersExistentes();
        
        // Verificar configuración del canvas de pausa
        VerificarConfiguracionCanvasPausa();
    }
    
    private void Update()
    {
        // Detectar presión de ESC para pausar/reanudar el juego
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoTerminado) return; // No permitir pausa si el juego ya terminó
            
            if (juegoEnPausa)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }
    
    /// <summary>
    /// Se llama cuando se carga una nueva escena
    /// Reanuda automáticamente el juego para evitar que los niveles inicien pausados
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        
        // Solo reanudar en escenas de nivel (no en Menu)
        if (!sceneName.Equals("Menu", System.StringComparison.OrdinalIgnoreCase))
        {
            // Si el juego estaba pausado, reanudarlo automáticamente
            if (juegoEnPausa)
            {
                juegoEnPausa = false;
                Time.timeScale = 1f;
                
                // Reanudar música si estaba pausada
                AudioManager audioManager = FindFirstObjectByType<AudioManager>();
                if (audioManager != null)
                {
                    audioManager.ResumeMusic();
                }
                
                // No activar canvas de pausa ya que estamos en un nuevo nivel
                if (pauseCanvas != null)
                {
                    pauseCanvas.gameObject.SetActive(false);
                }
            }
            
            // Resetear el estado del juego para el nuevo nivel
            ReiniciarJuego();
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #endregion
    
    #region Configuración Inicial
    
    /// <summary>
    /// Verifica que todos los tags necesarios existan en el proyecto
    /// </summary>
    private void VerificarTags()
    {
        VerificarTag(tagTriggerVictoria);
        VerificarTag(tagTriggerDerrota);
        VerificarTag(tagVehiculo);
    }
    
    /// <summary>
    /// Verifica si un tag específico existe en el proyecto
    /// </summary>
    private void VerificarTag(string tagName)
    {
        try
        {
            GameObject testObj = new GameObject();
            testObj.tag = tagName;
            DestroyImmediate(testObj);
            
            if (mostrarDebugInfo)
            {
                Debug.Log($"✅ Tag '{tagName}' verificado correctamente");
            }
        }
        catch (UnityException)
        {
            Debug.LogError($"❌ El tag '{tagName}' no existe en el proyecto. Por favor, créalo en Tag Manager.");
        }
    }
    
    /// <summary>
    /// Configura automáticamente todos los triggers existentes en la escena
    /// </summary>
    private void ConfigurarTriggersExistentes()
    {
        // Buscar todos los triggers con los tags correspondientes
        GameObject[] triggersVictoria = GameObject.FindGameObjectsWithTag(tagTriggerVictoria);
        GameObject[] triggersDerrota = GameObject.FindGameObjectsWithTag(tagTriggerDerrota);
        
        // Configurar triggers de victoria
        foreach (GameObject triggerObj in triggersVictoria)
        {
            ConfigurarTriggerVictoria(triggerObj);
        }
        
        // Configurar triggers de derrota
        foreach (GameObject triggerObj in triggersDerrota)
        {
            ConfigurarTriggerDerrota(triggerObj);
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Configurados {triggersVictoria.Length} triggers de victoria y {triggersDerrota.Length} triggers de derrota");
        }
    }
    
    #endregion
    
    #region Configuración de Triggers
    
    /// <summary>
    /// Configura un trigger para detectar condiciones de victoria
    /// </summary>
    public void ConfigurarTriggerVictoria(GameObject triggerObj)
    {
        Collider collider = triggerObj.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"El objeto {triggerObj.name} no tiene un Collider. Agregando BoxCollider...");
            collider = triggerObj.AddComponent<BoxCollider>();
        }
        
        // Asegurar que sea un trigger
        collider.isTrigger = true;
        
        // Agregar o configurar el componente GameConditionTrigger
        GameConditionTrigger conditionTrigger = triggerObj.GetComponent<GameConditionTrigger>();
        if (conditionTrigger == null)
        {
            conditionTrigger = triggerObj.AddComponent<GameConditionTrigger>();
        }
        
        conditionTrigger.ConfigurarComoTriggerVictoria(this, tagVehiculo);
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Trigger de victoria configurado: {triggerObj.name}");
        }
    }
    
    /// <summary>
    /// Configura un trigger para detectar condiciones de derrota
    /// </summary>
    public void ConfigurarTriggerDerrota(GameObject triggerObj)
    {
        Collider collider = triggerObj.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"El objeto {triggerObj.name} no tiene un Collider. Agregando BoxCollider...");
            collider = triggerObj.AddComponent<BoxCollider>();
        }
        
        // Asegurar que sea un trigger
        collider.isTrigger = true;
        
        // Agregar o configurar el componente GameConditionTrigger
        GameConditionTrigger conditionTrigger = triggerObj.GetComponent<GameConditionTrigger>();
        if (conditionTrigger == null)
        {
            conditionTrigger = triggerObj.AddComponent<GameConditionTrigger>();
        }
        
        conditionTrigger.ConfigurarComoTriggerDerrota(this, tagVehiculo);
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Trigger de derrota configurado: {triggerObj.name}");
        }
    }
    
    #endregion
    
    #region Manejo de Eventos de Triggers
      /// <summary>
    /// Llamado cuando un vehículo toca un trigger de victoria
    /// </summary>
    public void OnVehiculoPasaPuente(GameObject vehiculo)
    {
        if (!juegoActivo || juegoTerminado || juegoEnPausa) return;
        // Registrar estadística (siempre)
        contadorVictoria++;

        // Nuevo comportamiento: decrementamos los vehículos restantes INMEDIATAMENTE
        vehiculosPasados++;
        if (vehiculosRestantes > 0) vehiculosRestantes--;

        if (mostrarDebugInfo)
        {
            if (usarVictoriaPorRondas)
            {
                Debug.Log($"🚗 Vehículo pasó el puente! Estadística: {contadorVictoria} (victoria por rondas activada). Vehículos restantes: {vehiculosRestantes}/{vehiculosParaVictoria} (pasados: {vehiculosPasados})");
            }
            else
            {
                Debug.Log($"🚗 Vehículo pasó el puente! Vehículos restantes: {vehiculosRestantes}/{vehiculosParaVictoria} (pasados: {vehiculosPasados})");
            }
        }

        // Disparar evento de progreso con la cantidad restante (para mostrar en UI)
        OnVictoriaProgreso?.Invoke(vehiculosRestantes);

        // Verificar condición de victoria: cuando quedan 0 vehículos disponibles y pasa el último
        if (vehiculosRestantes <= 0)
        {
            Victoria();
        }
    }
    
    /// <summary>
    /// Llamado cuando un vehículo toca un trigger de derrota
    /// </summary>
    public void OnVehiculoCae(GameObject vehiculo)
    {
        if (!juegoActivo || juegoTerminado || juegoEnPausa) return;
        // Si un vehículo cae, también decrementa la cantidad de vehículos restantes (ya no cuenta al spawnear)
        contadorDerrota++;
        if (vehiculosRestantes > 0) vehiculosRestantes--;

        if (mostrarDebugInfo)
        {
            Debug.Log($"💥 Vehículo cayó! Progreso de derrota: {contadorDerrota}/{vehiculosParaDerrota} - Vehículo: {vehiculo.name} | Vehículos restantes: {vehiculosRestantes}/{vehiculosParaVictoria}");
        }

        // Disparar eventos de progreso: derrota y victoria (restantes)
        OnDerrotaProgreso?.Invoke(contadorDerrota);
        OnVictoriaProgreso?.Invoke(vehiculosRestantes);

        // Verificar condición de derrota
        if (contadorDerrota >= vehiculosParaDerrota)
        {
            Derrota();
            return;
        }

        // Verificar condición de victoria por si la caída del vehículo fue el último disponible
        if (vehiculosRestantes <= 0)
        {
            Victoria();
        }
    }
    
    #endregion
    
    #region Resultados del Juego
    
    /// <summary>
    /// Ejecuta la secuencia de victoria
    /// </summary>
    private void Victoria()
    {
        if (juegoTerminado) return;
        
        juegoTerminado = true;
        juegoActivo = false;
        
        if (mostrarDebugInfo)
        {
            Debug.Log("🎉 ¡VICTORIA! ¡Has logrado que pasen suficientes vehículos por el puente!");
        }
        
        // Desactivar controles y sistemas de juego
        DesactivarSistemasDeJuego();
        
        // Activar canvas de victoria
        ActivarCanvasVictoria();
        
        // Activar animaciones de victoria
        ActivarAnimacionesVictoria();
        
        OnVictoria?.Invoke();
    }
    
    /// <summary>
    /// Ejecuta la secuencia de victoria por rondas completadas
    /// </summary>
    private void VictoriaPorRondas()
    {
        if (juegoTerminado) return;
        
        juegoTerminado = true;
        juegoActivo = false;
        
        if (mostrarDebugInfo)
        {
            Debug.Log("🎉 ¡VICTORIA POR RONDAS! ¡Has completado todas las rondas configuradas!");
        }
        
        // Desactivar controles y sistemas de juego
        DesactivarSistemasDeJuego();
        
        // Activar canvas de victoria
        ActivarCanvasVictoria();
        
        // Activar animaciones de victoria
        ActivarAnimacionesVictoria();
        
        OnVictoria?.Invoke();
    }
    
    /// <summary>
    /// Ejecuta la secuencia de derrota
    /// </summary>
    private void Derrota()
    {
        if (juegoTerminado) return;
        
        juegoTerminado = true;
        juegoActivo = false;
        
        if (mostrarDebugInfo)
        {
            Debug.Log("💀 ¡DERROTA! Demasiados vehículos han caído del puente.");
        }
        
        // Desactivar controles y sistemas de juego
        DesactivarSistemasDeJuego();
        
        // Activar canvas de derrota
        ActivarCanvasDerrota();
        
        // Activar animaciones de derrota
        ActivarAnimacionesDerrota();
        
        OnDerrota?.Invoke();
    }
    
    #endregion
    
    #region Métodos Públicos
    
    /// <summary>
    /// Reinicia el estado del juego
    /// </summary>
    public void ReiniciarJuego()
    {
        contadorVictoria = 0;
        contadorDerrota = 0;
        juegoTerminado = false;
        juegoActivo = true;
        juegoEnPausa = false; // Resetear estado de pausa
        todasLasRondasCompletadas = false; // Resetear estado de rondas
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        
        // Detener monitoreo de autos detenidos
        StopAllCoroutines();
        
        // Reactivar sistemas de juego
        ReactivarSistemasDeJuego();
        
        // Destruir canvas de fin de juego si existe
        if (currentEndGameCanvas != null)
        {
            Destroy(currentEndGameCanvas);
            currentEndGameCanvas = null;
        }
        
        // Desactivar canvas de pausa si existe (NO destruir, solo desactivar)
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(false);
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log("🔄 Juego reiniciado - Sistemas reactivados y canvas eliminados");
        }
        
        // Intentar obtener el total de vehículos para la victoria desde AutoGenerator si existe
        if (autoGenerator == null)
        {
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
        }

        if (autoGenerator != null)
        {
            int antes = vehiculosParaVictoria;
            int totalDesdeGenerator = autoGenerator.GetTotalVehiclesForLevel();
            if (mostrarDebugInfo) Debug.Log($"GameConditionManager.ReiniciarJuego() - antes vehiculosParaVictoria={antes}, totalDesdeGenerator={totalDesdeGenerator}");
            if (totalDesdeGenerator > 0)
            {
                vehiculosParaVictoria = totalDesdeGenerator;
                if (mostrarDebugInfo) Debug.Log($"GameConditionManager: vehiculosParaVictoria seteado desde AutoGenerator = {vehiculosParaVictoria}");
            }
        }

        // Inicializar y disparar eventos de progreso inicial
        vehiculosRestantes = vehiculosParaVictoria;
        vehiculosPasados = 0;
        OnVictoriaProgreso?.Invoke(vehiculosRestantes);
        OnDerrotaProgreso?.Invoke(0);
    }
    
    /// <summary>
    /// Obtiene el progreso actual de victoria
    /// </summary>
    public int GetProgresoVictoria() => contadorVictoria;
    
    /// <summary>
    /// Obtiene el progreso actual de derrota
    /// </summary>
    public int GetProgresoDerrota() => contadorDerrota;
    
    /// <summary>
    /// Obtiene si el juego está activo
    /// </summary>
    public bool IsJuegoActivo() => juegoActivo && !juegoTerminado;
      /// <summary>
    /// Obtiene si el juego ha terminado
    /// </summary>
    public bool IsJuegoTerminado() => juegoTerminado;
    
    /// <summary>
    /// Obtiene la meta de vehículos para victoria
    /// </summary>
    public int GetMetaVictoria() => vehiculosParaVictoria;
    
    /// <summary>
    /// Obtiene la meta de vehículos para derrota
    /// </summary>
    public int GetMetaDerrota() => vehiculosParaDerrota;

    /// <summary>
    /// Obtiene la cantidad de vehículos restantes para la victoria (no spawn)
    /// </summary>
    public int GetVehiculosRestantes() => vehiculosRestantes;
    
    /// <summary>
    /// Obtiene si está usando victoria por rondas
    /// </summary>
    public bool IsUsandoVictoriaPorRondas() => usarVictoriaPorRondas;
    
    /// <summary>
    /// Obtiene información sobre el progreso de rondas (solo si está usando victoria por rondas)
    /// </summary>
    public string GetInfoProgresoRondas()
    {
        if (!usarVictoriaPorRondas || autoGenerator == null) return "";
        
        return $"Ronda {autoGenerator.GetRondaActual() + 1}/{autoGenerator.GetTotalRondas()}";
    }
    
    /// <summary>
    /// Configura los valores de condiciones de juego
    /// </summary>
    public void ConfigurarCondiciones(int nuevosVehiculosVictoria, int nuevosVehiculosDerrota)
    {
        vehiculosParaVictoria = nuevosVehiculosVictoria;
        vehiculosParaDerrota = nuevosVehiculosDerrota;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Condiciones actualizadas - Victoria: {vehiculosParaVictoria}, Derrota: {vehiculosParaDerrota}");
        }
    }
    
    /// <summary>
    /// Configura el sistema de victoria por rondas
    /// </summary>
    public void ConfigurarVictoriaPorRondas(bool habilitar, AutoGenerator generador = null)
    {
        usarVictoriaPorRondas = habilitar;
        if (generador != null)
        {
            autoGenerator = generador;
        }
        
        if (habilitar)
        {
            ConfigurarVictoriaPorRondas();
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Sistema de victoria por rondas {(habilitar ? "habilitado" : "deshabilitado")}");
        }
    }
    
    /// <summary>
    /// Configura los prefabs de canvas de fin de juego
    /// </summary>
    public void ConfigurarCanvasPrefabs(GameObject victoryPrefab, GameObject defeatPrefab)
    {
        victoryCanvasPrefab = victoryPrefab;
        defeatCanvasPrefab = defeatPrefab;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Canvas Prefabs configurados - Victory: {(victoryPrefab != null ? victoryPrefab.name : "null")}, Defeat: {(defeatPrefab != null ? defeatPrefab.name : "null")}");
        }
    }
    
    /// <summary>
    /// Configura las opciones de desactivación de sistemas
    /// </summary>
    public void ConfigurarDesactivacionSistemas(bool desactivarPlayer, bool desactivarAutoGen)
    {
        desactivarPlayerControllerEnFinDeJuego = desactivarPlayer;
        desactivarAutoGeneratorEnFinDeJuego = desactivarAutoGen;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Configuración de desactivación - Player: {desactivarPlayer}, AutoGenerator: {desactivarAutoGen}");
        }
    }
    
    /// <summary>
    /// Configura todas las opciones de sistemas de fin de juego
    /// </summary>
    public void ConfigurarSistemasFinDeJuego(bool desactivarPlayer, bool desactivarAutoGen, bool activarAnimaciones)
    {
        desactivarPlayerControllerEnFinDeJuego = desactivarPlayer;
        desactivarAutoGeneratorEnFinDeJuego = desactivarAutoGen;
        activarAnimacionesFinDeJuego = activarAnimaciones;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Sistemas de fin de juego configurados - Player: {desactivarPlayer}, AutoGenerator: {desactivarAutoGen}, Animaciones: {activarAnimaciones}");
        }
    }
    
    /// <summary>
    /// Configura el siguiente nivel para la navegación
    /// </summary>
    /// <param name="sceneName">Nombre de la escena del siguiente nivel</param>
    public void ConfigurarSiguienteNivel(string sceneName)
    {
        nextLevelSceneName = sceneName;
        useAutoLevelProgression = false;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Siguiente nivel configurado: {sceneName}");
        }
    }
    
    /// <summary>
    /// Habilita o deshabilita la progresión automática de niveles
    /// </summary>
    /// <param name="enabled">Si habilitar la progresión automática</param>
    public void ConfigurarProgresionAutomatica(bool enabled)
    {
        useAutoLevelProgression = enabled;
        if (enabled)
        {
            nextLevelSceneName = "";
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"Progresión automática de niveles: {(enabled ? "habilitada" : "deshabilitada")}");
        }
    }
    
    /// <summary>
    /// Método público para detener manualmente todos los autos activos
    /// </summary>
    public void DetenerTodosLosAutos()
    {
        DetenerTodosLosAutosActivos();
    }
    
    /// <summary>
    /// Método público para reactivar manualmente el movimiento de todos los autos
    /// </summary>
    public void ReactivarTodosLosAutos()
    {
        ReactivarMovimientoAutos();
    }
    
    /// <summary>
    /// Método público para desactivar manualmente todos los sistemas de juego
    /// </summary>
    public void DesactivarSistemas()
    {
        DesactivarSistemasDeJuego();
    }
    
    /// <summary>
    /// Método público para reactivar manualmente todos los sistemas de juego
    /// </summary>
    public void ReactivarSistemas()
    {
        ReactivarSistemasDeJuego();
    }
    
    #endregion
    
    #region Configuración de Victory por Rondas
    
    /// <summary>
    /// Configura el sistema de victoria por rondas si está habilitado
    /// </summary>
    private void ConfigurarVictoriaPorRondas()
    {
        if (!usarVictoriaPorRondas) return;
        
        // Buscar AutoGenerator automáticamente si no está asignado
        if (autoGenerator == null)
        {
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
            if (autoGenerator == null)
            {
                Debug.LogWarning("[GameConditionManager] No se encontró AutoGenerator. Desactivando victoria por rondas.");
                usarVictoriaPorRondas = false;
                return;
            }
        }
        
        // Validar que el AutoGenerator tenga sistema de rondas habilitado
        if (!autoGenerator.IsUsandoSistemaRondas())
        {
            Debug.LogWarning("[GameConditionManager] El AutoGenerator no tiene sistema de rondas habilitado. Desactivando victoria por rondas.");
            usarVictoriaPorRondas = false;
            return;
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"[GameConditionManager] Sistema de victoria por rondas habilitado. Total de rondas: {autoGenerator.GetTotalRondas()}");
        }
    }
    
    /// <summary>
    /// Método público para que AutoGenerator notifique que todas las rondas han terminado
    /// </summary>
    public void NotificarTodasLasRondasCompletadas()
    {
        if (!usarVictoriaPorRondas || !juegoActivo || juegoTerminado) return;
        
        todasLasRondasCompletadas = true;
        
        if (mostrarDebugInfo)
        {
            Debug.Log("[GameConditionManager] 🎉 Todas las rondas completadas! Activando victoria por rondas.");
        }
        
        VictoriaPorRondas();
    }
    
    #endregion

    #region Métodos Privados

    /// <summary>
    /// Desactiva todos los sistemas de juego y controles de jugador
    /// </summary>
    private void DesactivarSistemasDeJuego()
    {
        // Desactivar AutoGenerator y detener autos activos
        if (desactivarAutoGeneratorEnFinDeJuego && autoGenerator != null)
        {
            // Primero detener todos los autos activos
            DetenerTodosLosAutosActivos();
            
            // Luego desactivar el generador para que no spawne más
            autoGenerator.enabled = false;
            if (mostrarDebugInfo)
            {
                Debug.Log("🛑 AutoGenerator desactivado y autos detenidos");
            }
        }
        
        // Desactivar PlayerController
        if (desactivarPlayerControllerEnFinDeJuego && playerControllers != null)
        {
            foreach (PlayerController playerController in playerControllers)
            {
                if (playerController != null)
                {
                    playerController.enabled = false;
                }
            }
            if (mostrarDebugInfo)
            {
                Debug.Log($"🛑 {playerControllers.Length} PlayerController(s) desactivado(s)");
            }
        }
    }
    
    /// <summary>
    /// Detiene todos los autos activos sin desactivarlos, manteniéndolos quietos donde están
    /// </summary>
    private void DetenerTodosLosAutosActivos()
    {
        if (autoGenerator == null) return;
        
        // Buscar todos los vehículos activos en la escena con el tag Vehicle
        GameObject[] vehiculosActivos = GameObject.FindGameObjectsWithTag(tagVehiculo);
        
        int autosDetenidos = 0;
        foreach (GameObject vehiculo in vehiculosActivos)
        {
            if (vehiculo != null && vehiculo.activeInHierarchy)
            {
                // Verificar si el vehículo pertenece al pool del AutoGenerator
                if (autoGenerator.IsVehicleFromPool(vehiculo))
                {
                    DetenerMovimientoVehiculo(vehiculo);
                    autosDetenidos++;
                }
            }
        }
        
        // Iniciar monitoreo continuo para asegurar que permanezcan detenidos
        StartCoroutine(MonitorearAutosDetenidos());
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"🚗 {autosDetenidos} autos detenidos en sus posiciones actuales. Iniciando monitoreo continuo.");
        }
    }
    
    /// <summary>
    /// Corrutina que monitorea continuamente que los autos permanezcan detenidos
    /// </summary>
    private System.Collections.IEnumerator MonitorearAutosDetenidos()
    {
        while (juegoTerminado)
        {
            yield return new WaitForSeconds(0.1f); // Verificar cada 0.1 segundos
            
            if (autoGenerator == null) break;
            
            // Buscar todos los vehículos activos
            GameObject[] vehiculosActivos = GameObject.FindGameObjectsWithTag(tagVehiculo);
            
            foreach (GameObject vehiculo in vehiculosActivos)
            {
                if (vehiculo != null && vehiculo.activeInHierarchy && autoGenerator.IsVehicleFromPool(vehiculo))
                {
                    Rigidbody rb = vehiculo.GetComponent<Rigidbody>();
                    if (rb != null && (!rb.isKinematic || rb.linearVelocity.magnitude > 0.001f))
                    {
                        // El auto se está moviendo cuando no debería, forzar detención
                        if (mostrarDebugInfo)
                        {
                            Debug.Log($"⚠️ Auto {vehiculo.name} se reactivó, forzando detención nuevamente");
                        }
                        DetenerMovimientoVehiculo(vehiculo);
                    }
                }
            }
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log("🔄 Monitoreo de autos detenidos finalizado");
        }
    }
    
    /// <summary>
    /// Detiene el movimiento de un vehículo específico
    /// </summary>
    private void DetenerMovimientoVehiculo(GameObject vehiculo)
    {
        if (vehiculo == null) return;
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"🔧 Deteniendo vehículo: {vehiculo.name}");
        }
        
        // Detener el componente AutoMovement (aunque otros sistemas lo reactiven, 
        // ahora AutoMovement verifica IsJuegoTerminado())
        AutoMovement autoMovement = vehiculo.GetComponent<AutoMovement>();
        if (autoMovement != null)
        {
            if (mostrarDebugInfo)
            {
                Debug.Log($"   - AutoMovement encontrado, desactivando (estaba: {autoMovement.enabled})");
            }
            autoMovement.enabled = false;
            
            // Resetear velocidades internas del AutoMovement
            autoMovement.SetVelocidad(0f);
        }
        else
        {
            if (mostrarDebugInfo)
            {
                Debug.LogWarning($"   - ⚠️ No se encontró AutoMovement en {vehiculo.name}");
            }
        }
        
        // Detener COMPLETAMENTE la física del Rigidbody
        Rigidbody rb = vehiculo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (mostrarDebugInfo)
            {
                Debug.Log($"   - Rigidbody encontrado, congelando COMPLETAMENTE (velocidad actual: {rb.linearVelocity})");
            }
            
            // Parar inmediatamente
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Congelar TODO - posición y rotación
            rb.constraints = RigidbodyConstraints.FreezeAll;
            
            // Desactivar gravedad para evitar que caiga
            rb.useGravity = false;
            
            // Hacer el objeto kinematic para que no responda a fuerzas externas
            rb.isKinematic = true;
        }
        else
        {
            if (mostrarDebugInfo)
            {
                Debug.LogWarning($"   - ⚠️ No se encontró Rigidbody en {vehiculo.name}");
            }
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"✅ Vehículo {vehiculo.name} detenido COMPLETAMENTE (kinematic + congelado)");
        }
    }
    
    /// <summary>
    /// Reactiva todos los sistemas de juego (útil para reinicio)
    /// </summary>
    private void ReactivarSistemasDeJuego()
    {
        // Reactivar AutoGenerator
        if (autoGenerator != null)
        {
            autoGenerator.enabled = true;
            if (mostrarDebugInfo)
            {
                Debug.Log("▶️ AutoGenerator reactivado");
            }
        }
        
        // Reactivar PlayerController
        if (playerControllers != null)
        {
            foreach (PlayerController playerController in playerControllers)
            {
                if (playerController != null)
                {
                    playerController.enabled = true;
                }
            }
            if (mostrarDebugInfo)
            {
                Debug.Log($"▶️ {playerControllers.Length} PlayerController(s) reactivado(s)");
            }
        }
        
        // Reiniciar animaciones a estado de gameplay
        if (playerAnimators != null)
        {
            foreach (PlayerAnimator playerAnimator in playerAnimators)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.ResetToGameplayState();
                }
            }
            if (mostrarDebugInfo)
            {
                Debug.Log($"🔄 {playerAnimators.Length} PlayerAnimator(s) reiniciado(s) a estado de gameplay");
            }
        }
        
        // Reactivar movimiento de autos si están detenidos
        ReactivarMovimientoAutos();
    }
    
    /// <summary>
    /// Reactiva el movimiento de todos los autos que fueron detenidos
    /// </summary>
    private void ReactivarMovimientoAutos()
    {
        if (autoGenerator == null) return;
        
        // Buscar todos los vehículos con el tag Vehicle
        GameObject[] vehiculosActivos = GameObject.FindGameObjectsWithTag(tagVehiculo);
        
        int autosReactivados = 0;
        foreach (GameObject vehiculo in vehiculosActivos)
        {
            if (vehiculo != null && vehiculo.activeInHierarchy)
            {
                // Verificar si el vehículo pertenece al pool del AutoGenerator
                if (autoGenerator.IsVehicleFromPool(vehiculo))
                {
                    ReactivarMovimientoVehiculo(vehiculo);
                    autosReactivados++;
                }
            }
        }
        
        if (mostrarDebugInfo && autosReactivados > 0)
        {
            Debug.Log($"🚗 {autosReactivados} autos reactivados para movimiento");
        }
    }
    
    /// <summary>
    /// Reactiva el movimiento de un vehículo específico
    /// </summary>
    private void ReactivarMovimientoVehiculo(GameObject vehiculo)
    {
        if (vehiculo == null) return;
        
        // Reactivar el componente AutoMovement
        AutoMovement autoMovement = vehiculo.GetComponent<AutoMovement>();
        if (autoMovement != null)
        {
            autoMovement.enabled = true;
            // Restaurar velocidad original si había sido seteada a 0
            autoMovement.ResetearAuto();
        }
        
        // Restaurar la física del Rigidbody a su configuración normal de vehículo
        Rigidbody rb = vehiculo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Restaurar a configuración normal de vehículo
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Restaurar constraints normales para vehículos (solo congelar rotaciones problemáticas)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            // Resetear velocidades para evitar movimientos erráticos
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (mostrarDebugInfo)
        {
            Debug.Log($"🔄 Vehículo {vehiculo.name} reactivado para movimiento normal");
        }
    }
    
    /// <summary>
    /// Activa el canvas de victoria
    /// </summary>
    private void ActivarCanvasVictoria()
    {
        if (victoryCanvasPrefab != null)
        {
            // Destruir canvas anterior si existe
            if (currentEndGameCanvas != null)
            {
                Destroy(currentEndGameCanvas);
            }
            
            // Instanciar nuevo canvas de victoria
            currentEndGameCanvas = Instantiate(victoryCanvasPrefab);
            currentEndGameCanvas.SetActive(true);
            
            // Verificar que los botones estén configurados correctamente
            VerificarBotonesCanvas(currentEndGameCanvas, true);
            
            if (mostrarDebugInfo)
            {
                Debug.Log("🎉 Canvas de Victoria activado y botones verificados");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Victory Canvas Prefab no está asignado en GameConditionManager");
        }
    }
    
    /// <summary>
    /// Activa el canvas de derrota
    /// </summary>
    private void ActivarCanvasDerrota()
    {
        if (defeatCanvasPrefab != null)
        {
            // Destruir canvas anterior si existe
            if (currentEndGameCanvas != null)
            {
                Destroy(currentEndGameCanvas);
            }
            
            // Instanciar nuevo canvas de derrota
            currentEndGameCanvas = Instantiate(defeatCanvasPrefab);
            currentEndGameCanvas.SetActive(true);
            
            // Verificar que los botones estén configurados correctamente
            VerificarBotonesCanvas(currentEndGameCanvas, false);
            
            if (mostrarDebugInfo)
            {
                Debug.Log("💀 Canvas de Derrota activado y botones verificados");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Defeat Canvas Prefab no está asignado en GameConditionManager");
        }
    }
    
    /// <summary>
    /// Activa las animaciones de victoria en todos los PlayerAnimator
    /// </summary>
    private void ActivarAnimacionesVictoria()
    {
        if (activarAnimacionesFinDeJuego && playerAnimators != null)
        {
            foreach (PlayerAnimator playerAnimator in playerAnimators)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerVictoryAnimation();
                }
            }
            if (mostrarDebugInfo)
            {
                Debug.Log($"🎉 Animaciones de victoria activadas en {playerAnimators.Length} PlayerAnimator(s)");
            }
        }
    }
    
    /// <summary>
    /// Activa las animaciones de derrota en todos los PlayerAnimator
    /// </summary>
    private void ActivarAnimacionesDerrota()
    {
        if (activarAnimacionesFinDeJuego && playerAnimators != null)
        {
            foreach (PlayerAnimator playerAnimator in playerAnimators)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerDefeatAnimation();
                }
            }
            if (mostrarDebugInfo)
            {
                Debug.Log($"💀 Animaciones de derrota activadas en {playerAnimators.Length} PlayerAnimator(s)");
            }
        }
    }
    
    /// <summary>
    /// Verifica que los botones del canvas tengan MenuButton configurado correctamente
    /// </summary>
    private void VerificarBotonesCanvas(GameObject canvas, bool isVictory)
    {
        if (canvas == null) return;
        
        MenuButton[] menuButtons = canvas.GetComponentsInChildren<MenuButton>();
        
        if (menuButtons.Length > 0)
        {
            if (mostrarDebugInfo)
            {
                Debug.Log($"🎮 Canvas de {(isVictory ? "Victoria" : "Derrota")} activado con {menuButtons.Length} botón(es) MenuButton configurado(s)");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ El canvas de {(isVictory ? "Victoria" : "Derrota")} no tiene botones MenuButton configurados.");
        }
    }
    
    /// <summary>
    /// Verifica la configuración del canvas de pausa al inicio
    /// </summary>
    private void VerificarConfiguracionCanvasPausa()
    {
        if (pauseCanvas != null)
        {
            Debug.Log($"🔍 INICIO - Canvas de pausa encontrado: '{pauseCanvas.name}'");
            Debug.Log($"🔍 INICIO - Estado inicial: isActiveAndEnabled={pauseCanvas.isActiveAndEnabled}, gameObject.activeSelf={pauseCanvas.gameObject.activeSelf}, enabled={pauseCanvas.enabled}");
            Debug.Log($"🔍 INICIO - sortingOrder: {pauseCanvas.sortingOrder}, renderMode: {pauseCanvas.renderMode}");
            Debug.Log($"🔍 INICIO - worldCamera: {(pauseCanvas.worldCamera != null ? pauseCanvas.worldCamera.name : "null")}");
            
            // Asegurar que el canvas esté desactivado al inicio
            if (pauseCanvas.gameObject.activeSelf)
            {
                Debug.Log("⚠️ Canvas de pausa estaba activo al inicio - desactivándolo");
                pauseCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ INICIO - Pause Canvas no está asignado en GameConditionManager!");
        }
    }

    #endregion

    #region Navigation Methods
    
    /// <summary>
    /// Navega al siguiente nivel
    /// </summary>
    public void NavigateToNextLevel()
    {
        string targetScene = "";
        
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            targetScene = nextLevelSceneName;
        }
        else if (useAutoLevelProgression)
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            targetScene = GetNextLevelName(currentSceneName);
        }
        else
        {
            targetScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        
        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar la escena '{targetScene}': {e.Message}");
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        }
    }
    
    /// <summary>
    /// Intenta detectar automáticamente el nombre del siguiente nivel
    /// </summary>
    private string GetNextLevelName(string currentSceneName)
    {
        if (currentSceneName.Contains("Level"))
        {
            string levelPart = currentSceneName.Substring(currentSceneName.IndexOf("Level") + 5);
            if (int.TryParse(levelPart, out int levelNumber))
            {
                int nextLevelNumber = levelNumber + 1;
                return currentSceneName.Replace($"Level{levelNumber}", $"Level{nextLevelNumber}");
            }
        }
        
        return currentSceneName;
    }
    
    #endregion

    #region Métodos Públicos de Pausa
    
    /// <summary>
    /// Pausa el juego y muestra el canvas de pausa
    /// IGUAL que el sistema de menús: desactiva otros canvas y activa el de pausa
    /// </summary>
    public void PausarJuego()
    {
        if (juegoTerminado || juegoEnPausa) return;
        
        juegoEnPausa = true;
        Time.timeScale = 0f;
        
        // Pausar música de fondo si AudioManager está disponible
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PauseMusic();
        }
        
        // Implementación IGUAL al sistema de menús
        ActivarCanvasPausa();
        
        if (mostrarDebugInfo)
        {
            Debug.Log("⏸️ Juego pausado - Canvas de pausa activado usando sistema igual al menú");
        }
    }
    
    /// <summary>
    /// Reanuda el juego y oculta el canvas de pausa
    /// IGUAL que el sistema de menús: desactiva el canvas de pausa
    /// </summary>
    public void ReanudarJuego()
    {
        if (!juegoEnPausa) return;
        
        juegoEnPausa = false;
        Time.timeScale = 1f;
        
        // Reanudar música de fondo si AudioManager está disponible
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.ResumeMusic();
        }
        
        // Implementación IGUAL al sistema de menús
        DesactivarCanvasPausa();
        
        if (mostrarDebugInfo)
        {
            Debug.Log("▶️ Juego reanudado - Canvas de pausa desactivado usando sistema igual al menú");
        }
    }
    
    /// <summary>
    /// Activa el canvas de pausa IGUAL que SetCanvasActive del menú
    /// </summary>
    private void ActivarCanvasPausa()
    {
        Debug.Log($"🎯 ActivarCanvasPausa llamado - pauseCanvas: {(pauseCanvas != null ? pauseCanvas.name : "null")}");
        
        if (pauseCanvas != null)
        {
            // Debug detallado del estado del canvas ANTES
            Debug.Log($"🔍 ANTES - Canvas '{pauseCanvas.name}': isActiveAndEnabled={pauseCanvas.isActiveAndEnabled}, gameObject.activeSelf={pauseCanvas.gameObject.activeSelf}, enabled={pauseCanvas.enabled}");
            Debug.Log($"🔍 ANTES - Canvas sortingOrder: {pauseCanvas.sortingOrder}, renderMode: {pauseCanvas.renderMode}");
            
            // IGUAL que en SceneNavigatorCanvas.SetCanvasActive()
            pauseCanvas.gameObject.SetActive(true);
            
            // Debug detallado del estado del canvas DESPUÉS
            Debug.Log($"🔍 DESPUÉS - Canvas '{pauseCanvas.name}': isActiveAndEnabled={pauseCanvas.isActiveAndEnabled}, gameObject.activeSelf={pauseCanvas.gameObject.activeSelf}, enabled={pauseCanvas.enabled}");
            
            // Verificar si hay otros canvas que puedan estar tapando
            Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.Log($"🔍 Total de Canvas en escena: {allCanvas.Length}");
            foreach (Canvas canvas in allCanvas)
            {
                if (canvas.gameObject.activeSelf)
                {
                    Debug.Log($"🔍 Canvas activo: '{canvas.name}' - sortingOrder: {canvas.sortingOrder}, renderMode: {canvas.renderMode}");
                }
            }
            
            if (mostrarDebugInfo)
            {
                Debug.Log("✅ Canvas de pausa activado (método igual al menú)");
            }
        }
        else
        {
            if (mostrarDebugInfo)
            {
                Debug.LogWarning("⚠️ Pause Canvas no está asignado en GameConditionManager");
            }
        }
    }
    
    /// <summary>
    /// Desactiva el canvas de pausa IGUAL que SetCanvasActive del menú
    /// </summary>
    private void DesactivarCanvasPausa()
    {
        Debug.Log($"🎯 DesactivarCanvasPausa llamado - pauseCanvas: {(pauseCanvas != null ? pauseCanvas.name : "null")}");
        
        if (pauseCanvas != null)
        {
            // Debug detallado del estado del canvas ANTES
            Debug.Log($"🔍 ANTES DESACTIVAR - Canvas '{pauseCanvas.name}': isActiveAndEnabled={pauseCanvas.isActiveAndEnabled}, gameObject.activeSelf={pauseCanvas.gameObject.activeSelf}");
            
            // IGUAL que en SceneNavigatorCanvas.SetCanvasActive()
            pauseCanvas.gameObject.SetActive(false);
            
            // Debug detallado del estado del canvas DESPUÉS
            Debug.Log($"🔍 DESPUÉS DESACTIVAR - Canvas '{pauseCanvas.name}': isActiveAndEnabled={pauseCanvas.isActiveAndEnabled}, gameObject.activeSelf={pauseCanvas.gameObject.activeSelf}");
            
            if (mostrarDebugInfo)
            {
                Debug.Log("✅ Canvas de pausa desactivado (método igual al menú)");
            }
        }
    }
    
    /// <summary>
    /// Obtiene si el juego está pausado
    /// </summary>
    public bool IsJuegoEnPausa() => juegoEnPausa;
    
    #endregion
    
    #region Métodos de Testing
    
    /// <summary>
    /// Test - Detener todos los autos activos
    /// </summary>
    [ContextMenu("Test - Detener Todos los Autos")]
    public void TestDetenerTodosLosAutos()
    {
        DetenerTodosLosAutos();
        Debug.Log("[TEST] Todos los autos han sido detenidos manualmente");
    }
    
    /// <summary>
    /// Test - Reactivar movimiento de todos los autos
    /// </summary>
    [ContextMenu("Test - Reactivar Todos los Autos")]
    public void TestReactivarTodosLosAutos()
    {
        ReactivarTodosLosAutos();
        Debug.Log("[TEST] Movimiento de todos los autos reactivado manualmente");
    }
    
    /// <summary>
    /// Test - Simular victoria para probar detención de autos
    /// </summary>
    [ContextMenu("Test - Simular Victoria")]
    public void TestSimularVictoria()
    {
        if (juegoTerminado)
        {
            Debug.Log("[TEST] El juego ya ha terminado. Reinicie primero.");
            return;
        }
        
        Debug.Log("[TEST] Simulando victoria...");
        Victoria();
    }
    
    // ...existing code...
    /// <summary>
    /// Test - Simular derrota para probar detención de autos
    /// </summary>
    [ContextMenu("Test - Simular Derrota")]
    public void TestSimularDerrota()
    {
        if (juegoTerminado)
        {
            Debug.Log("[TEST] El juego ya ha terminado. Reinicie primero.");
            return;
        }
        
        Debug.Log("[TEST] Simulando derrota...");
        Derrota();
    }
    
    /// <summary>
    /// Test - Verificar estado de autos activos
    /// </summary>
    [ContextMenu("Test - Verificar Estado Autos")]
    public void TestVerificarEstadoAutos()
    {
        if (autoGenerator == null)
        {
            Debug.Log("[TEST] AutoGenerator no encontrado");
            return;
        }

        GameObject[] vehiculosActivos = GameObject.FindGameObjectsWithTag(tagVehiculo);
        Debug.Log($"[TEST] Encontrados {vehiculosActivos.Length} vehículos con tag '{tagVehiculo}'");

        int autosDelPool = 0;
        int autosMoviendose = 0;
        int autosDetenidos = 0;

        foreach (GameObject vehiculo in vehiculosActivos)
        {
            if (vehiculo != null && vehiculo.activeInHierarchy && autoGenerator.IsVehicleFromPool(vehiculo))
            {
                autosDelPool++;

                AutoMovement autoMovement = vehiculo.GetComponent<AutoMovement>();
                Rigidbody rb = vehiculo.GetComponent<Rigidbody>();

                bool enMovimiento = false;
                if (rb != null && (rb.linearVelocity.magnitude > 0.001f || !rb.isKinematic))
                {
                    enMovimiento = true;
                }

                if (enMovimiento)
                {
                    autosMoviendose++;
                    Debug.Log($"[TEST] 🚗 {vehiculo.name}: MOVIENDOSE - Velocity: {(rb != null ? rb.linearVelocity.magnitude.ToString("F3") : "N/A")}, Kinematic: {(rb != null ? rb.isKinematic : false)}, AutoMovement: {(autoMovement != null ? autoMovement.enabled : false)}");
                }
                else
                {
                    autosDetenidos++;
                    Debug.Log($"[TEST] 🛑 {vehiculo.name}: DETENIDO - Velocity: {(rb != null ? rb.linearVelocity.magnitude.ToString("F3") : "N/A")}, Kinematic: {(rb != null ? rb.isKinematic : false)}, AutoMovement: {(autoMovement != null ? autoMovement.enabled : false)}");
                }
            }
        }

        Debug.Log($"[TEST] Resumen: {autosDelPool} autos del pool, {autosMoviendose} moviéndose, {autosDetenidos} detenidos");
        Debug.Log($"[TEST] Juego terminado: {juegoTerminado}");
    }
    
    #endregion
}
