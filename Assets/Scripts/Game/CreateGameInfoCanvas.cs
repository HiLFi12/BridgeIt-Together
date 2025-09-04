using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script para crear automáticamente el Canvas de información del juego
/// Ejecutar desde el menú contextual o automáticamente al iniciar
/// </summary>
public class CreateGameInfoCanvas : MonoBehaviour
{
    [Header("Configuración de Creación")]
    [SerializeField] private bool crearAlIniciar = false;
    [SerializeField] private bool sobreescribirExistente = false;
    
    [Header("Configuración Visual")]
    [SerializeField] private Color colorFondo = new Color(0, 0, 0, 0.7f);
    [SerializeField] private Color colorTexto = Color.white;
    [SerializeField] private int tamañoFuentePrincipal = 18;
    [SerializeField] private int tamañoFuenteSecundario = 14;
    
    [Header("Posicionamiento")]
    [SerializeField] private Vector2 posicionPanel = new Vector2(20, -20);
    // Aumentado ancho/alto para dar más espacio y evitar wrapping
    [SerializeField] private Vector2 tamañoPanel = new Vector2(380, 70);
    // Separar un poco más del borde y del texto inferior
    [SerializeField] private Vector2 posicionTextoAutosCaidos = new Vector2(12, -12);
    [SerializeField] private Vector2 posicionTextoAutosPasaron = new Vector2(12, -36);
    
    private void Start()
    {
        if (crearAlIniciar)
        {
            CrearCanvasInformacion();
        }
    }
    
    [ContextMenu("Crear Canvas de Información")]
    public void CrearCanvasInformacion()
    {
        // Verificar si ya existe
        Canvas canvasExistente = FindFirstObjectByType<Canvas>();
        GameInfoCanvas infoCanvasExistente = FindFirstObjectByType<GameInfoCanvas>();
        
        if (infoCanvasExistente != null && !sobreescribirExistente)
        {
            Debug.Log("CreateGameInfoCanvas: Ya existe un GameInfoCanvas. Activar 'sobreescribirExistente' para reemplazarlo.");
            return;
        }
        
        // Crear Canvas principal si no existe
        Canvas canvas = canvasExistente;
        if (canvas == null)
        {
            canvas = CrearCanvasPrincipal();
        }
        
        // Destruir canvas existente si se requiere sobreescribir
        if (infoCanvasExistente != null && sobreescribirExistente)
        {
            DestroyImmediate(infoCanvasExistente.gameObject);
        }
        
        // Crear el Canvas de información
        GameObject canvasInfo = CrearCanvasInfo(canvas);
        
        Debug.Log("CreateGameInfoCanvas: Canvas de información del juego creado exitosamente!");
    }
    
    private Canvas CrearCanvasPrincipal()
    {
        GameObject canvasObj = new GameObject("Game Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    private GameObject CrearCanvasInfo(Canvas parentCanvas)
    {
        // Crear objeto principal
        GameObject canvasInfo = new GameObject("Game Info Canvas");
        canvasInfo.transform.SetParent(parentCanvas.transform, false);
        
        // Agregar script principal
        GameInfoCanvas script = canvasInfo.AddComponent<GameInfoCanvas>();
        
        // Crear panel de fondo
        GameObject panel = new GameObject("Info Panel");
        panel.transform.SetParent(canvasInfo.transform, false);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = colorFondo;
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1); // Esquina superior izquierda
        panelRect.anchorMax = new Vector2(0, 1); // Esquina superior izquierda
        panelRect.anchoredPosition = posicionPanel;
        panelRect.sizeDelta = tamañoPanel;
        
        // Crear texto de autos caídos
        GameObject textoAutosCaidos = CrearTexto("Texto Autos Caidos", canvasInfo.transform, 
            "", colorTexto, tamañoFuentePrincipal);
        // Aumentar ancho para evitar que la línea se corte y dar altura suficiente
        ConfigurarTextoEnPanel(textoAutosCaidos, posicionTextoAutosCaidos, new Vector2(360, 26));

        // Crear texto de autos que pasaron
        GameObject textoAutosPasaron = CrearTexto("Texto Autos Pasaron", canvasInfo.transform,
            "", colorTexto, tamañoFuentePrincipal);
        ConfigurarTextoEnPanel(textoAutosPasaron, posicionTextoAutosPasaron, new Vector2(360, 26));
        
        // Crear panel de fin de juego (inicialmente oculto)
        GameObject panelFinJuego = new GameObject("Panel Fin Juego");
        panelFinJuego.transform.SetParent(canvasInfo.transform, false);
        panelFinJuego.SetActive(false);
        
        // Configurar referencias en el script
        script.ConfigurarReferenciasManualmente(
            textoAutosCaidos.GetComponent<TextMeshProUGUI>(),
            textoAutosPasaron.GetComponent<TextMeshProUGUI>(),
            null, // Sin texto de estado
            null, // Sin botón de reinicio
            panelFinJuego
        );
        
        return canvasInfo;
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
        textComponent.alignment = TextAlignmentOptions.Left;
        
        return obj;
    }
    
    private GameObject CrearBoton(string nombre, Transform padre, string textoBoton)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        
        Image imagen = obj.AddComponent<Image>();
        imagen.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        
        Button boton = obj.AddComponent<Button>();
        
        // Crear texto del botón
        GameObject textoObj = new GameObject("Text");
        textoObj.transform.SetParent(obj.transform, false);
        
        TextMeshProUGUI texto = textoObj.AddComponent<TextMeshProUGUI>();
        texto.text = textoBoton;
        texto.color = Color.white;
        texto.fontSize = 16;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        
        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;
        
        return obj;
    }
    
    private void ConfigurarTextoEnPanel(GameObject texto, Vector2 posicion, Vector2 tamaño)
    {
        RectTransform rect = texto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); // Anclado a la parte superior izquierda
        rect.anchorMax = new Vector2(0, 1); // Anclado a la parte superior izquierda
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }
    
    private void ConfigurarBotonEnPanel(GameObject boton, Vector2 posicion, Vector2 tamaño)
    {
        RectTransform rect = boton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); // Anclado a la parte superior izquierda
        rect.anchorMax = new Vector2(0, 1); // Anclado a la parte superior izquierda
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }
    
    [ContextMenu("Eliminar Canvas Existente")]
    public void EliminarCanvasExistente()
    {
        GameInfoCanvas[] canvases = FindObjectsByType<GameInfoCanvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            DestroyImmediate(canvas.gameObject);
        }
        Debug.Log("CreateGameInfoCanvas: Canvas eliminados");
    }
}
