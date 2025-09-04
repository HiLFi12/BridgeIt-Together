using UnityEngine;

/// <summary>
/// Script helper para configurar automáticamente el sistema de navegación.
/// Coloca este script en cualquier GameObject de tu escena principal y se encargará
/// de configurar todo automáticamente.
/// </summary>
public class NavigationSystemAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createSceneNavigator = true;
    [SerializeField] private bool createSceneNavigatorCanvas = true;
    
    [Header("Audio Settings")]
    [SerializeField] private int menuBGMIndex = 0;
    
    private void Start()
    {
        // Si ya existe SceneNavigatorCanvas, desactivar este script para evitar conflictos
        SceneNavigatorCanvas existingCanvasNavigator = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (existingCanvasNavigator != null)
        {
            Debug.Log("🔧 NavigationSystemAutoSetup: SceneNavigatorCanvas ya existe, desactivando este script para evitar conflictos");
            this.enabled = false;
            return;
        }
        
        if (setupOnStart)
        {
            SetupNavigationSystem();
        }
    }
    
    /// <summary>
    /// Configura automáticamente el sistema de navegación
    /// </summary>
    [ContextMenu("Setup Navigation System")]
    public void SetupNavigationSystem()
    {
        // Verificar si ya existe SceneNavigatorCanvas antes de hacer cualquier cosa
        SceneNavigatorCanvas existingCanvasNavigator = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (existingCanvasNavigator != null)
        {
            Debug.Log("🔧 NavigationSystemAutoSetup: SceneNavigatorCanvas ya existe, no es necesario configurar nada");
            return;
        }
        
        // Buscar AudioManager en la escena
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        
        if (createSceneNavigatorCanvas)
        {
            // Crear o buscar SceneNavigatorCanvas
            SceneNavigatorCanvas canvasNavigator = FindFirstObjectByType<SceneNavigatorCanvas>();
            if (canvasNavigator == null)
            {
                GameObject canvasNavObj;
                
                if (audioManager != null)
                {
                    // Agregar al mismo GameObject que el AudioManager
                    canvasNavObj = audioManager.gameObject;
                    Debug.Log("Agregando SceneNavigatorCanvas al GameObject del AudioManager");
                }
                else
                {
                    // Crear nuevo GameObject
                    canvasNavObj = new GameObject("SceneNavigatorCanvas");
                    Debug.Log("Creando nuevo GameObject para SceneNavigatorCanvas");
                }
                
                canvasNavigator = canvasNavObj.AddComponent<SceneNavigatorCanvas>();
                
                // Configurar BGM
                if (audioManager != null)
                {
                    Debug.Log($"Configurando música de menú con índice: {menuBGMIndex}");
                }
            }
            else
            {
                Debug.Log("SceneNavigatorCanvas ya existe en la escena");
            }
        }
        
        if (createSceneNavigator)
        {
            // Crear o buscar SceneNavigator (para compatibilidad)
            SceneNavigator sceneNavigator = FindFirstObjectByType<SceneNavigator>();
            if (sceneNavigator == null)
            {
                GameObject navObj = new GameObject("SceneNavigator");
                navObj.AddComponent<SceneNavigator>();
                Debug.Log("SceneNavigator creado para compatibilidad");
            }
            else
            {
                Debug.Log("SceneNavigator ya existe en la escena");
            }
        }
        
        // Buscar y configurar eventos si existen
        SceneNavigationEvents navEvents = FindFirstObjectByType<SceneNavigationEvents>();
        if (navEvents == null)
        {
            GameObject eventsObj = new GameObject("SceneNavigationEvents");
            eventsObj.AddComponent<SceneNavigationEvents>();
            Debug.Log("SceneNavigationEvents creado");
        }
        
        Debug.Log("✅ Sistema de navegación configurado correctamente!");
        
        // Reproducir música si está disponible Y no hay SceneNavigatorCanvas
        var sceneNavCanvas = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (audioManager != null && sceneNavCanvas == null)
        {
            audioManager.PlayBGM(menuBGMIndex);
        }
    }
    
    /// <summary>
    /// Busca y asigna automáticamente Canvas a SceneNavigatorCanvas
    /// </summary>
    [ContextMenu("Auto Assign Canvas")]
    public void AutoAssignCanvas()
    {
        // Solo ejecutar si este script está habilitado
        if (!this.enabled)
        {
            Debug.Log("🔧 NavigationSystemAutoSetup: Script deshabilitado, no asignando canvas");
            return;
        }
        
        SceneNavigatorCanvas canvasNavigator = FindFirstObjectByType<SceneNavigatorCanvas>();
        if (canvasNavigator != null)
        {
            canvasNavigator.AutoAssignCanvasReferences();
            Debug.Log("✅ Canvas asignados automáticamente");
        }
        else
        {
            Debug.LogWarning("No se encontró SceneNavigatorCanvas en la escena");
        }
    }
}
