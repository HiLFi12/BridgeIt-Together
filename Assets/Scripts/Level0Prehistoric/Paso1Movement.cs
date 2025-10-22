using UnityEngine;
using TMPro;

public class Paso1Movement : MonoBehaviour
{
    [Header("UI Prompt")]
    [SerializeField] private TMP_Text textoPrompt;            // Texto TMP que actúa como guía

    [Header("UI Inputs de Movimiento")]
    [SerializeField] private GameObject uiMovimientoP1;       // UI que muestra el input de movimiento del Player 1
    [SerializeField] private GameObject uiMovimientoP2;       // UI que muestra el input de movimiento del Player 2

    [Header("Referencias a Jugadores")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;

    [Header("Flujo de Tutorial")]
    [SerializeField] private GameObject proximoPaso;          // GameObject del siguiente paso a activar
    [SerializeField] private float umbralMovimiento = 0.02f;  // Mínimo delta para considerar que se movió
    [SerializeField] private int framesIgnoradosIniciales = 2;// Ignorar deltas iniciales (spawn/ajustes)

    private Vector3 _ultimaPosP1;
    private Vector3 _ultimaPosP2;
    private bool _inicializado;
    private int _framesIgnoradosRestantes;

    private void OnEnable() => Inicializar();
    private void Start() => Inicializar();

    private void Inicializar()
    {
        if (_inicializado) return;
        _inicializado = true;

        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (uiMovimientoP1) uiMovimientoP1.SetActive(true);
        if (uiMovimientoP2) uiMovimientoP2.SetActive(true);

        if (jugador1) _ultimaPosP1 = jugador1.position;
        if (jugador2) _ultimaPosP2 = jugador2.position;

        _framesIgnoradosRestantes = Mathf.Max(0, framesIgnoradosIniciales);
    }

    private void Update()
    {
        // Durante los primeros frames, refrescar baseline y no detectar
        if (_framesIgnoradosRestantes > 0)
        {
            if (jugador1) _ultimaPosP1 = jugador1.position;
            if (jugador2) _ultimaPosP2 = jugador2.position;
            _framesIgnoradosRestantes--;
            return;
        }

        bool seMovio = false;
        float umbralSqr = umbralMovimiento * umbralMovimiento;

        if (jugador1)
        {
            var delta1 = jugador1.position - _ultimaPosP1;
            if (delta1.sqrMagnitude >= umbralSqr) seMovio = true;
            _ultimaPosP1 = jugador1.position;
        }

        if (!seMovio && jugador2)
        {
            var delta2 = jugador2.position - _ultimaPosP2;
            if (delta2.sqrMagnitude >= umbralSqr) seMovio = true;
            _ultimaPosP2 = jugador2.position;
        }

        if (seMovio)
            CompletarPaso();
    }

    private void CompletarPaso()
    {
        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (uiMovimientoP1) uiMovimientoP1.SetActive(false);
        if (uiMovimientoP2) uiMovimientoP2.SetActive(false);

        gameObject.SetActive(false);
    }
}
