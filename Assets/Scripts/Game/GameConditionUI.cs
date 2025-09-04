using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager para mostrar el progreso de las condiciones de victoria y derrota
/// </summary>
public class GameConditionUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoVictoria;
    [SerializeField] private TextMeshProUGUI textoDerrota;
    [SerializeField] private TextMeshProUGUI textoEstadoJuego;
    [SerializeField] private Button botonReiniciar;
    
    [Header("Configuración")]
    [SerializeField] private bool actualizarAutomaticamente = true;
    [SerializeField] private Color colorVictoria = Color.green;
    [SerializeField] private Color colorDerrota = Color.red;
    [SerializeField] private Color colorNormal = Color.white;
    
    [Header("Mensajes")]
    [SerializeField] private string formatoVictoria = "Vehicles remaining: {0}/{1}";
    [SerializeField] private string formatoDerrota = "Vehicles fallen: {0}/{1}";
    [SerializeField] private string mensajeVictoria = "¡VICTORIA! ¡Bien hecho!";
    [SerializeField] private string mensajeDerrota = "DERROTA - Demasiados vehículos cayeron";
    [SerializeField] private string mensajeJuegoActivo = "";
    
    // Referencias
    private GameConditionManager gameManager;
    
    #region Unity Events
    
    private void Start()
    {
        // Buscar el GameConditionManager
        gameManager = GameConditionManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameConditionManager>();
        }
        
        if (gameManager == null)
        {
            Debug.LogError("GameConditionUI: No se encontró GameConditionManager");
            return;
        }
        
        // Configurar eventos
        ConfigurarEventos();
        
        // Configurar botón de reinicio
        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.AddListener(ReiniciarJuego);
        }
        
        // Actualización inicial
        ActualizarUI();
    }
    
    private void Update()
    {
        if (actualizarAutomaticamente && gameManager != null)
        {
            ActualizarUI();
        }
    }
    
    #endregion
    
    #region Configuración de Eventos
    
    private void ConfigurarEventos()
    {
        if (gameManager == null) return;
        
        // Suscribirse a eventos de progreso
        gameManager.OnVictoriaProgreso.AddListener(OnProgresoVictoriaActualizado);
        gameManager.OnDerrotaProgreso.AddListener(OnProgresoDerrotaActualizado);
        
        // Suscribirse a eventos de fin de juego
        gameManager.OnVictoria.AddListener(OnVictoria);
        gameManager.OnDerrota.AddListener(OnDerrota);
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de eventos para evitar memory leaks
        if (gameManager != null)
        {
            gameManager.OnVictoriaProgreso.RemoveListener(OnProgresoVictoriaActualizado);
            gameManager.OnDerrotaProgreso.RemoveListener(OnProgresoDerrotaActualizado);
            gameManager.OnVictoria.RemoveListener(OnVictoria);
            gameManager.OnDerrota.RemoveListener(OnDerrota);
        }
    }
    
    #endregion
    
    #region Callbacks de Eventos
    
    private void OnProgresoVictoriaActualizado(int progreso)
    {
        ActualizarTextoVictoria();
    }
    
    private void OnProgresoDerrotaActualizado(int progreso)
    {
        ActualizarTextoDerrota();
    }
    
    private void OnVictoria()
    {
        ActualizarEstadoJuego(mensajeVictoria, colorVictoria);
        MostrarBotonReiniciar();
    }
    
    private void OnDerrota()
    {
        ActualizarEstadoJuego(mensajeDerrota, colorDerrota);
        MostrarBotonReiniciar();
    }
    
    #endregion
    
    #region Actualización de UI
    
    private void ActualizarUI()
    {
        ActualizarTextoVictoria();
        ActualizarTextoDerrota();
        ActualizarEstadoJuegoActual();
    }
      private void ActualizarTextoVictoria()
    {
        if (textoVictoria == null || gameManager == null) return;
        int restantes = gameManager.GetVehiculosRestantes();
        int meta = gameManager.GetMetaVictoria();

        string texto = string.Format(formatoVictoria, restantes, meta);
        textoVictoria.text = texto;

        // Cambiar color si está cerca de la victoria (cuando quedan pocos)
        if (restantes <= Mathf.Max(1, meta * 0.2f))
        {
            textoVictoria.color = colorVictoria;
        }
        else
        {
            textoVictoria.color = colorNormal;
        }
    }
    
    private void ActualizarTextoDerrota()
    {
        if (textoDerrota == null || gameManager == null) return;
        
        int progreso = gameManager.GetProgresoDerrota();
        int meta = gameManager.GetMetaDerrota();
        
        string texto = string.Format(formatoDerrota, progreso, meta);
        
        textoDerrota.text = texto;
        
        // Cambiar color si está cerca de la derrota (un vehículo antes de la meta)
        if (progreso >= meta - 1)
        {
            textoDerrota.color = colorDerrota;
        }
        else
        {
            textoDerrota.color = colorNormal;
        }
    }
    
    private void ActualizarEstadoJuegoActual()
    {
        if (gameManager == null) return;
        
        if (gameManager.IsJuegoTerminado())
        {
            // El estado ya fue actualizado por los eventos OnVictoria/OnDerrota
            return;
        }
        
        if (gameManager.IsJuegoActivo())
        {
            ActualizarEstadoJuego(mensajeJuegoActivo, colorNormal);
            OcultarBotonReiniciar();
        }
    }
    
    private void ActualizarEstadoJuego(string mensaje, Color color)
    {
        if (textoEstadoJuego != null)
        {
            textoEstadoJuego.text = mensaje;
            textoEstadoJuego.color = color;
        }
    }
    
    #endregion
    
    #region Manejo de Botones
    
    private void MostrarBotonReiniciar()
    {
        if (botonReiniciar != null)
        {
            botonReiniciar.gameObject.SetActive(true);
        }
    }
    
    private void OcultarBotonReiniciar()
    {
        if (botonReiniciar != null)
        {
            botonReiniciar.gameObject.SetActive(false);
        }
    }
    
    private void ReiniciarJuego()
    {
        if (gameManager != null)
        {
            gameManager.ReiniciarJuego();
            ActualizarUI();
        }
    }
    
    #endregion
    
    #region Métodos Públicos
    
    /// <summary>
    /// Fuerza una actualización manual de la UI
    /// </summary>
    public void ForzarActualizacion()
    {
        ActualizarUI();
    }
    
    /// <summary>
    /// Configura las referencias de texto manualmente
    /// </summary>
    public void ConfigurarReferencias(TextMeshProUGUI victoria, TextMeshProUGUI derrota, TextMeshProUGUI estado, Button reiniciar)
    {
        textoVictoria = victoria;
        textoDerrota = derrota;
        textoEstadoJuego = estado;
        botonReiniciar = reiniciar;
        
        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarJuego);
        }
    }
    
    /// <summary>
    /// Configura los mensajes de la UI
    /// </summary>
    public void ConfigurarMensajes(string formatoVict, string formatoDerr, string mensVict, string mensDerr, string mensActivo)
    {
        formatoVictoria = formatoVict;
        formatoDerrota = formatoDerr;
        mensajeVictoria = mensVict;
        mensajeDerrota = mensDerr;
        mensajeJuegoActivo = mensActivo;
    }
    
    #endregion
    
    #region Context Menu para Testing
    
    [ContextMenu("Actualizar UI")]
    public void ContextActualizarUI()
    {
        ForzarActualizacion();
    }
    
    [ContextMenu("Simular Victoria")]
    public void ContextSimularVictoria()
    {
        OnVictoria();
    }
    
    [ContextMenu("Simular Derrota")]
    public void ContextSimularDerrota()
    {
        OnDerrota();
    }
    
    #endregion
}
