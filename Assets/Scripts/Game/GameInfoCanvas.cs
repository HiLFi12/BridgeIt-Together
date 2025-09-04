using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Prefab completo de Canvas para mostrar la información del juego
/// Incluye contador de autos caídos y autos que pasaron
/// </summary>
public class GameInfoCanvas : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoAutosCaidos;
    [SerializeField] private TextMeshProUGUI textoAutosPasaron;
    [SerializeField] private TextMeshProUGUI textoEstadoJuego;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private GameObject panelFinJuego;
    
    [Header("Configuración")]
    [SerializeField] private bool mostrarSoloAutosCaidos = false;
    [SerializeField] private Color colorPeligro = Color.red;
    [SerializeField] private Color colorExito = Color.green;
    [SerializeField] private Color colorNormal = Color.white;
    
    private GameConditionUI gameConditionUI;
    private GameConditionManager gameManager;
    
    private void Start()
    {
        ConfigurarUI();
        ConectarEventos();
    }

    private void OnEnable()
    {
        // Forzar actualización una frame después para asegurar que GameConditionManager haya inicializado sus valores
        StartCoroutine(ForzarActualizacionUnFrame());
    }

    private System.Collections.IEnumerator ForzarActualizacionUnFrame()
    {
        yield return null; // esperar un frame
        if (gameConditionUI != null)
        {
            gameConditionUI.ForzarActualizacion();
        }
    }
    
    private void ConfigurarUI()
    {
        // Buscar o crear GameConditionUI
        gameConditionUI = GetComponent<GameConditionUI>();
        if (gameConditionUI == null)
        {
            gameConditionUI = gameObject.AddComponent<GameConditionUI>();
        }
        
        // Buscar GameConditionManager
        gameManager = GameConditionManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameConditionManager>();
        }
        
        // Configurar referencias en GameConditionUI
        if (gameConditionUI != null)
        {
            gameConditionUI.ConfigurarReferencias(
                textoAutosPasaron,
                textoAutosCaidos,
                textoEstadoJuego,
                botonReiniciar
            );
            
            // Configurar mensajes personalizados (formatos)
            gameConditionUI.ConfigurarMensajes(
                "Vehicles remaining: {0}/{1}",
                "Vehicles fallen: {0}/{1}",
                "LEVEL COMPLETED!",
                "TOO MANY CARS FELL!",
                ""
            );
        }
        
        // Ocultar panel de fin de juego y botón inicialmente
        if (panelFinJuego != null)
            panelFinJuego.SetActive(false);
        
        if (botonReiniciar != null)
            botonReiniciar.gameObject.SetActive(false);
        
        // No establecer textos estáticos aquí para evitar que sean sobrescritos por otros setup.
        // Solicitar al GameConditionUI que actualice los textos iniciales según el GameConditionManager.
        if (gameConditionUI != null)
        {
            gameConditionUI.ForzarActualizacion();
        }
        
        // Ocultar contador de victoria si solo queremos mostrar autos caídos
        if (mostrarSoloAutosCaidos && textoAutosPasaron != null)
        {
            textoAutosPasaron.gameObject.SetActive(false);
        }
    }
    
    private void ConectarEventos()
    {
        if (gameManager == null) return;
        
        // Suscribirse a eventos
        gameManager.OnDerrotaProgreso.AddListener(OnAutosCaidosActualizado);
        gameManager.OnVictoriaProgreso.AddListener(OnAutosPasaronActualizado);
        gameManager.OnDerrota.AddListener(OnJuegoTerminado);
        gameManager.OnVictoria.AddListener(OnJuegoTerminado);
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (gameManager != null)
        {
            gameManager.OnDerrotaProgreso.RemoveListener(OnAutosCaidosActualizado);
            gameManager.OnVictoriaProgreso.RemoveListener(OnAutosPasaronActualizado);
            gameManager.OnDerrota.RemoveListener(OnJuegoTerminado);
            gameManager.OnVictoria.RemoveListener(OnJuegoTerminado);
        }
    }
    
    private void OnAutosCaidosActualizado(int cantidad)
    {
        if (textoAutosCaidos == null || gameManager == null) return;
        
        int meta = gameManager.GetMetaDerrota();
        textoAutosCaidos.text = $"Vehicles fallen: {cantidad}/{meta}";
        
        // Cambiar color si está cerca del límite
        if (cantidad >= meta - 1)
        {
            textoAutosCaidos.color = colorPeligro;
        }
        else if (cantidad >= meta * 0.6f)
        {
            textoAutosCaidos.color = Color.yellow;
        }
        else
        {
            textoAutosCaidos.color = colorNormal;
        }
    }
    
    private void OnAutosPasaronActualizado(int cantidad)
    {
        if (textoAutosPasaron == null || gameManager == null) return;
        // Mostrar vehículos restantes: cantidad = vehiculosRestantes
        int meta = gameManager.GetMetaVictoria();
        int restantes = cantidad;
        textoAutosPasaron.text = $"Vehicles remaining: {restantes}/{meta}";

        // Cambiar color si está cerca de la victoria
        if (restantes <= Mathf.Max(1, meta / 5)) // cerca cuando queda 20% o 1
        {
            textoAutosPasaron.color = colorExito;
        }
        else
        {
            textoAutosPasaron.color = colorNormal;
        }
    }
    
    private void OnJuegoTerminado()
    {
        if (panelFinJuego != null)
            panelFinJuego.SetActive(true);
        
        if (botonReiniciar != null)
            botonReiniciar.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Método público para configurar manualmente las referencias
    /// </summary>
    public void ConfigurarReferenciasManualmente(TextMeshProUGUI autosCaidos, TextMeshProUGUI autosPasaron, 
        TextMeshProUGUI estado, Button reiniciar, GameObject panelFin = null)
    {
        textoAutosCaidos = autosCaidos;
        textoAutosPasaron = autosPasaron;
        textoEstadoJuego = estado;
        botonReiniciar = reiniciar;
        panelFinJuego = panelFin;
        
        ConfigurarUI();
        ConectarEventos();
    }
    
    /// <summary>
    /// Alterna la visibilidad del contador de autos que pasaron
    /// </summary>
    public void MostrarSoloAutosCaidos(bool soloAutosCaidos)
    {
        mostrarSoloAutosCaidos = soloAutosCaidos;
        
        if (textoAutosPasaron != null)
        {
            textoAutosPasaron.gameObject.SetActive(!soloAutosCaidos);
        }
    }
}
