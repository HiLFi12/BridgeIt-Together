using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneLoader ultra simple: asigna el nombre de una escena en el inspector y al pulsar el botón la carga.
/// </summary>
[RequireComponent(typeof(Button))]
public class SceneLoader : MonoBehaviour
{
    [Header("Escena a Cargar")] 
    [Tooltip("Nombre exacto de la escena (Build Settings).")]
    [SerializeField] private string sceneName;

    [Header("Opciones")]
    [SerializeField] private bool debugLogs = false;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(LoadScene);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(LoadScene);
    }

    public void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            if (debugLogs) Debug.LogWarning($"[SceneLoader] {name}: sceneName vacío/no asignado.");
            return;
        }

        if (debugLogs) Debug.Log($"[SceneLoader] Cargando escena '{sceneName}'.");
        SceneManager.LoadScene(sceneName);
    }

    // API pública opcional por si quieres cambiarla en runtime.
    public void SetSceneName(string newName) => sceneName = newName;

    private void OnValidate()
    {
        // No spamear logs; validación ligera.
        if (sceneName != null) sceneName = sceneName.Trim();
    }
}
