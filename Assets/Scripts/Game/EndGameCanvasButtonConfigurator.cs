using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para configurar automáticamente los botones de canvas de fin de juego.
/// Se agrega a los prefabs de Victory/Defeat canvas para configurar los botones automáticamente.
/// </summary>
public class EndGameCanvasButtonConfigurator : MonoBehaviour
{
    [Header("Button Names - Configurar en los prefabs")]
    [SerializeField] private string nextLevelButtonName = "NextLevelButton";
    [SerializeField] private string restartButtonName = "RestartButton";
    [SerializeField] private string menuButtonName = "MenuButton";
    [SerializeField] private string levelSelectButtonName = "LevelSelectButton";
    
    [Header("Configuración - Se configurará automáticamente")]
    [SerializeField] private bool isVictoryCanvas = true; // Si es false, es defeat canvas
    
    private void Start()
    {
        ConfigurarBotones();
    }
    
    /// <summary>
    /// Configura automáticamente todos los botones del canvas
    /// </summary>
    public void ConfigurarBotones()
    {
        ConfigurarBoton(nextLevelButtonName, MenuButton.NavigationType.NextLevel);
        ConfigurarBoton(restartButtonName, MenuButton.NavigationType.RestartLevel);
        ConfigurarBoton(menuButtonName, MenuButton.NavigationType.MenuFromGame);
        ConfigurarBoton(levelSelectButtonName, MenuButton.NavigationType.LevelSelectFromGame);
        
        Debug.Log($"🎮 Botones configurados en {(isVictoryCanvas ? "Victory" : "Defeat")} Canvas");
    }
    
    /// <summary>
    /// Configura un botón específico con el tipo de navegación correspondiente
    /// </summary>
    /// <param name="buttonName">Nombre del botón a buscar</param>
    /// <param name="navigationType">Tipo de navegación a asignar</param>
    private void ConfigurarBoton(string buttonName, MenuButton.NavigationType navigationType)
    {
        // Buscar el botón por nombre en los hijos de este canvas
        Transform buttonTransform = BuscarEnHijos(transform, buttonName);
        
        if (buttonTransform != null)
        {
            // Verificar si ya tiene MenuButton
            MenuButton menuButton = buttonTransform.GetComponent<MenuButton>();
            
            if (menuButton == null)
            {
                // Agregar el componente MenuButton
                menuButton = buttonTransform.gameObject.AddComponent<MenuButton>();
            }
            
            // Configurar el tipo de navegación
            menuButton.SetNavigationType(navigationType);
            
            Debug.Log($"✅ Botón '{buttonName}' configurado con navegación: {navigationType}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró botón con nombre '{buttonName}' en el canvas");
        }
    }
    
    /// <summary>
    /// Busca recursivamente un objeto hijo por nombre
    /// </summary>
    /// <param name="parent">Transform padre donde buscar</param>
    /// <param name="name">Nombre del objeto a buscar</param>
    /// <returns>Transform del objeto encontrado, o null si no se encuentra</returns>
    private Transform BuscarEnHijos(Transform parent, string name)
    {
        // Verificar si algún hijo directo tiene el nombre
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
        }
        
        // Si no se encuentra, buscar recursivamente en los nietos
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform found = BuscarEnHijos(child, name);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Establece si este es un canvas de victoria o derrota
    /// Llamado automáticamente por GameConditionManager cuando instancia el canvas
    /// </summary>
    /// <param name="victory">true si es canvas de victoria, false si es de derrota</param>
    public void SetIsVictoryCanvas(bool victory)
    {
        isVictoryCanvas = victory;
    }
    
    /// <summary>
    /// Permite cambiar los nombres de los botones si es necesario
    /// </summary>
    public void ConfigurarNombresBotones(string nextLevel, string restart, string menu, string levelSelect)
    {
        nextLevelButtonName = nextLevel;
        restartButtonName = restart;
        menuButtonName = menu;
        levelSelectButtonName = levelSelect;
    }
    
    #region Context Menu para Testing
    
    [ContextMenu("Test - Reconfigurar Botones")]
    public void TestReconfigurarBotones()
    {
        ConfigurarBotones();
    }
    
    [ContextMenu("Test - Listar Botones en Canvas")]
    public void TestListarBotones()
    {
        Button[] botones = GetComponentsInChildren<Button>();
        Debug.Log($"=== BOTONES ENCONTRADOS EN {gameObject.name} ===");
        foreach (Button boton in botones)
        {
            MenuButton menuButton = boton.GetComponent<MenuButton>();
            string navegacion = menuButton != null ? menuButton.ToString() : "Sin MenuButton";
            Debug.Log($"- {boton.name}: {navegacion}");
        }
        Debug.Log($"=== Total: {botones.Length} botones ===");
    }
    
    #endregion
}
