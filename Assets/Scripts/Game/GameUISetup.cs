using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script de utilidad para configurar automáticamente el UI del juego
/// </summary>
public class GameUISetup : MonoBehaviour
{
    [Header("Configuración Automática")]
    [SerializeField] private bool configurarAutomaticamente = true;
    [SerializeField] private bool crearElementosSiFaltan = true;
    
    [Header("Referencias Requeridas")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GameConditionUI gameConditionUI;
    
    [Header("Configuración de Estilo")]
    [SerializeField] private Font fontePrincipal;
    [SerializeField] private Color colorTexto = Color.white;
    [SerializeField] private int tamañoFuente = 24;
    
    private void Start()
    {
        if (configurarAutomaticamente)
        {
            ConfigurarUI();
        }
    }
    
    [ContextMenu("Configurar UI")]
    public void ConfigurarUI()
    {
        // Buscar Canvas si no está asignado
        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null && crearElementosSiFaltan)
            {
                CrearCanvas();
            }
        }
        
        // Buscar GameConditionUI si no está asignado
        if (gameConditionUI == null)
        {
            gameConditionUI = FindFirstObjectByType<GameConditionUI>();
            if (gameConditionUI == null && crearElementosSiFaltan)
            {
                CrearGameConditionUI();
            }
        }
        
        // Crear elementos UI si no existen
        if (crearElementosSiFaltan)
        {
            CrearElementosUI();
        }
        
        Debug.Log("GameUISetup: Configuración de UI completada");
    }
    
    private void CrearCanvas()
    {
        GameObject canvasObj = new GameObject("Game UI Canvas");
        targetCanvas = canvasObj.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        targetCanvas.sortingOrder = 100;
        
        // Agregar CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Agregar GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("GameUISetup: Canvas creado automáticamente");
    }
    
    private void CrearGameConditionUI()
    {
        if (targetCanvas == null) return;
        
        GameObject uiObj = new GameObject("GameConditionUI");
        uiObj.transform.SetParent(targetCanvas.transform, false);
        gameConditionUI = uiObj.AddComponent<GameConditionUI>();
        
        Debug.Log("GameUISetup: GameConditionUI creado automáticamente");
    }
    
    private void CrearElementosUI()
    {
        if (targetCanvas == null || gameConditionUI == null) return;
        
        // Crear panel principal para la información del juego
        GameObject panelPrincipal = CrearPanel("Panel Info Juego", targetCanvas.transform);
        ConfigurarRectTransform(panelPrincipal.GetComponent<RectTransform>(), 
            new Vector2(10, 10), new Vector2(400, 150), Vector2.zero, Vector2.zero);
        
        // Crear texto para autos caídos (derrota)
        GameObject textoDerrota = CrearTexto("Texto Autos Caidos", panelPrincipal.transform, 
            "", colorTexto, tamañoFuente);
        ConfigurarRectTransform(textoDerrota.GetComponent<RectTransform>(),
            new Vector2(10, 10), new Vector2(380, 40), Vector2.zero, new Vector2(0, 0));
        
        // Crear texto para autos que pasaron (victoria)
        GameObject textoVictoria = CrearTexto("Texto Autos Pasaron", panelPrincipal.transform,
            "", colorTexto, tamañoFuente);
        ConfigurarRectTransform(textoVictoria.GetComponent<RectTransform>(),
            new Vector2(10, 60), new Vector2(380, 40), Vector2.zero, new Vector2(0, 0));
        
        // Crear texto de estado del juego
        GameObject textoEstado = CrearTexto("Texto Estado", panelPrincipal.transform,
            "", colorTexto, tamañoFuente - 4);
        ConfigurarRectTransform(textoEstado.GetComponent<RectTransform>(),
            new Vector2(10, 110), new Vector2(280, 30), Vector2.zero, new Vector2(0, 0));
        
        // Crear botón de reinicio (inicialmente oculto)
        GameObject botonReiniciar = CrearBoton("Boton Reiniciar", panelPrincipal.transform, "Reiniciar");
        ConfigurarRectTransform(botonReiniciar.GetComponent<RectTransform>(),
            new Vector2(300, 110), new Vector2(80, 30), Vector2.zero, new Vector2(0, 0));
        botonReiniciar.SetActive(false);
        
        // Configurar las referencias en GameConditionUI
        gameConditionUI.ConfigurarReferencias(
            textoVictoria.GetComponent<TextMeshProUGUI>(),
            textoDerrota.GetComponent<TextMeshProUGUI>(),
            textoEstado.GetComponent<TextMeshProUGUI>(),
            botonReiniciar.GetComponent<Button>()
        );
        
        Debug.Log("GameUISetup: Elementos UI creados y configurados");
    }
    
    private GameObject CrearPanel(string nombre, Transform padre)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(padre, false);
        
        Image imagen = panel.AddComponent<Image>();
        imagen.color = new Color(0, 0, 0, 0.7f); // Fondo semi-transparente
        
        return panel;
    }
    
    private GameObject CrearTexto(string nombre, Transform padre, string texto, Color color, int tamaño)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        
        TextMeshProUGUI textComponent = obj.AddComponent<TextMeshProUGUI>();
        textComponent.text = texto;
        textComponent.color = color;
        textComponent.fontSize = tamaño;
        textComponent.fontStyle = FontStyles.Bold;
        
        return obj;
    }
    
    private GameObject CrearBoton(string nombre, Transform padre, string textoBoton)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        
        Image imagen = obj.AddComponent<Image>();
        imagen.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button boton = obj.AddComponent<Button>();
        
        // Crear texto del botón
        GameObject textoObj = new GameObject("Text");
        textoObj.transform.SetParent(obj.transform, false);
        
        TextMeshProUGUI texto = textoObj.AddComponent<TextMeshProUGUI>();
        texto.text = textoBoton;
        texto.color = Color.white;
        texto.fontSize = 16;
        texto.alignment = TextAlignmentOptions.Center;
        
        // Configurar el texto para llenar el botón
        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;
        
        return obj;
    }
    
    private void ConfigurarRectTransform(RectTransform rect, Vector2 posicion, Vector2 tamaño, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }
}
