using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Paso2Dash : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;           // Texto TMP que debe verse al entrar al Paso 2

    [Header("UI Inputs de Dash")]
    [SerializeField] private GameObject uiDashP1;            // UI que muestra la tecla/botón de Dash del Player 1
    [SerializeField] private GameObject uiDashP2;            // UI que muestra la tecla/botón de Dash del Player 2

    [Header("Siguiente Paso")]
    [SerializeField] private TMP_Text proximoTextoTMP;       // Texto del paso siguiente (se enciende al completar este paso)
    [SerializeField] private GameObject proximoPaso;         // GameObject del siguiente paso a activar

    [Header("Input System (opcional)")]
    [SerializeField] private InputActionReference dashP1;    // Acción Dash del Player 1
    [SerializeField] private InputActionReference dashP2;    // Acción Dash del Player 2

    private bool _inicializado;
    private bool _completado;

    private void OnEnable() => Inicializar();
    private void Start() => Inicializar();

    private void OnDisable()
    {
        if (dashP1?.action != null) dashP1.action.performed -= OnDashPerformed;
        if (dashP2?.action != null) dashP2.action.performed -= OnDashPerformed;
    }

    private void Inicializar()
    {
        if (_inicializado) _inicializado = false; // permitir inicializar correctamente si se reusa
        // Mostrar elementos del paso actual
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (uiDashP1) uiDashP1.SetActive(true);
        if (uiDashP2) uiDashP2.SetActive(true);

        // Asegurar que el texto del próximo paso esté apagado hasta completar
        if (proximoTextoTMP) proximoTextoTMP.gameObject.SetActive(false);

        // Suscribir a acciones de dash (si se asignan en el inspector)
        if (dashP1?.action != null)
        {
            dashP1.action.performed += OnDashPerformed;
            if (!dashP1.action.enabled) dashP1.action.Enable();
        }
        if (dashP2?.action != null)
        {
            dashP2.action.performed += OnDashPerformed;
            if (!dashP2.action.enabled) dashP2.action.Enable();
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext _)
    {
        CompletarPaso();
    }

    // Alternativa: llamar esto desde el script de Player cuando hace dash.
    public void NotificarDash()
    {
        CompletarPaso();
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);
        if (proximoTextoTMP) proximoTextoTMP.gameObject.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (uiDashP1) uiDashP1.SetActive(false);
        if (uiDashP2) uiDashP2.SetActive(false);

        gameObject.SetActive(false);
    }
}
