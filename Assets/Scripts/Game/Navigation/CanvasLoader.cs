using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CanvasLoader: Script genérico para cambiar entre Canvas.
/// Asignar este script a un Button y configurar en el inspector:
///  - currentRootCanvas (Canvas que está actualmente activo / raíz UI actual)
///  - targetCanvas (Canvas que se desea mostrar)
/// Al presionar el botón: desactiva el canvas actual y activa el target.
/// Opcional: puede mantener activo el actual si keepCurrentActive está marcado.
/// </summary>
[RequireComponent(typeof(Button))]
public class CanvasLoader : MonoBehaviour
{
    [Header("Referencias Canvas")]
    [Tooltip("Canvas que actualmente está activo y debe desactivarse (puede ser un GameObject padre raíz de la UI)." )]
    [SerializeField] private GameObject currentRootCanvas;

    [Tooltip("Canvas objetivo que se desea activar.")]
    [SerializeField] private GameObject targetCanvas;

    [Header("Opciones")]
    [Tooltip("Si se activa, no se desactiva el currentRootCanvas al cambiar.")]
    [SerializeField] private bool keepCurrentActive = false;

    [Tooltip("Desactivar también todos los canvases hermanos del target dentro del mismo padre (limpia la UI)." )]
    [SerializeField] private bool deactivateSiblings = true;

    [Tooltip("Imprimir logs de depuración.")]
    [SerializeField] private bool debugLogs = false;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(SwitchCanvas);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(SwitchCanvas);
    }

    /// <summary>
    /// Realiza el cambio de canvas.
    /// </summary>
    public void SwitchCanvas()
    {
        if (targetCanvas == null)
        {
            if (debugLogs) Debug.LogWarning($"[CanvasLoader] {name}: targetCanvas no asignado");
            return;
        }

        if (deactivateSiblings && targetCanvas.transform.parent != null)
        {
            var parent = targetCanvas.transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i).gameObject;
                if (child != targetCanvas && child.activeSelf)
                {
                    child.SetActive(false);
                }
            }
        }

        if (!keepCurrentActive && currentRootCanvas != null && currentRootCanvas != targetCanvas)
        {
            if (currentRootCanvas.activeSelf)
            {
                currentRootCanvas.SetActive(false);
            }
        }

        if (!targetCanvas.activeSelf)
            targetCanvas.SetActive(true);

        if (debugLogs)
            Debug.Log($"[CanvasLoader] {name}: Activado '{targetCanvas.name}' {(keepCurrentActive ? "manteniendo" : "desactivando")} el actual.");
    }

    #region API Pública
    public void SetCurrentCanvas(GameObject go) => currentRootCanvas = go;
    public void SetTargetCanvas(GameObject go) => targetCanvas = go;
    public void SetKeepCurrentActive(bool value) => keepCurrentActive = value;
    public void SetDeactivateSiblings(bool value) => deactivateSiblings = value;
    #endregion
}
