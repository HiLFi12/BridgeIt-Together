using UnityEngine;

/// <summary>
/// Script de configuración para el sistema de navegación de escenas.
/// Coloca este script en la primera escena de tu juego (típicamente el menú principal).
/// </summary>
public class SceneNavigationSetup : MonoBehaviour
{
    [Header("Navigation System Setup")]
    [SerializeField] private GameObject sceneNavigatorPrefab;
    [SerializeField] private GameObject navigationEventsPrefab;
    [SerializeField] private bool autoSetupOnStart = true;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupNavigationSystem();
        }
    }

    /// <summary>
    /// Configura el sistema de navegación
    /// </summary>
    public void SetupNavigationSystem()
    {
        // Crear SceneNavigator si no existe
        if (SceneNavigator.Instance == null)
        {
            if (sceneNavigatorPrefab != null)
            {
                Instantiate(sceneNavigatorPrefab);
            }
            else
            {
                // Crear un GameObject simple con el SceneNavigator
                GameObject navigatorObj = new GameObject("SceneNavigator");
                navigatorObj.AddComponent<SceneNavigator>();
            }
        }

        // Crear SceneNavigationEvents si no existe
        if (SceneNavigationEvents.Instance == null)
        {
            if (navigationEventsPrefab != null)
            {
                Instantiate(navigationEventsPrefab);
            }
            else
            {
                // Crear un GameObject simple con SceneNavigationEvents
                GameObject eventsObj = new GameObject("SceneNavigationEvents");
                eventsObj.AddComponent<SceneNavigationEvents>();
            }
        }

        Debug.Log("Sistema de navegación configurado correctamente");
    }

    #region Inspector Buttons (Solo en Editor)
    
    #if UNITY_EDITOR
    [ContextMenu("Setup Navigation System")]
    private void SetupFromMenu()
    {
        SetupNavigationSystem();
    }

    [ContextMenu("Test Navigation - Go to Level Selector")]
    private void TestLevelSelector()
    {
        SceneNavigator.NavigateToLevelSelector();
    }

    [ContextMenu("Test Navigation - Go to Credits")]
    private void TestCredits()
    {
        SceneNavigator.NavigateToCredits();
    }
    #endif

    #endregion
}
