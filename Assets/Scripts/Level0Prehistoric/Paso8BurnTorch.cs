using UnityEngine;
using TMPro;

public class Paso8BurnTorch : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Flechas hacia la fogata (verde)")]
    [SerializeField] private GameObject flechaFogataP1;
    [SerializeField] private GameObject flechaFogataP2;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si el PaloIgnifugo se parenta a una mano/holder, asignar aquí (sino deja null para usar el root del jugador).")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject proximoPaso;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool _vistoApagadoP1, _vistoApagadoP2;
    private bool _encendidoOkP1, _encendidoOkP2;
    private bool _completado;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (flechaFogataP1) flechaFogataP1.SetActive(true);
        if (flechaFogataP2) flechaFogataP2.SetActive(true);

        _vistoApagadoP1 = _vistoApagadoP2 = false;
        _encendidoOkP1 = _encendidoOkP2 = false;
        _completado = false;
    }

    private void Update()
    {
        if (_completado) return;

        var raiz1 = raizChequeoP1 ? raizChequeoP1 : jugador1;
        var raiz2 = raizChequeoP2 ? raizChequeoP2 : jugador2;

        if (raiz1) ChequearJugador(raiz1, 1, ref _vistoApagadoP1, ref _encendidoOkP1);
        if (raiz2) ChequearJugador(raiz2, 2, ref _vistoApagadoP2, ref _encendidoOkP2);

        if (_encendidoOkP1 && _encendidoOkP2)
            CompletarPaso();
    }

    private void ChequearJugador(Transform raiz, int playerIndex, ref bool vistoApagado, ref bool encendidoOk)
    {
        if (encendidoOk || raiz == null) return;

        var palo = raiz.GetComponentInChildren<PaloIgnifugo>(true);
        if (!palo) return;

        bool encendido = palo.EstaEncendido();

        if (!encendido)
        {
            // Marcamos que vimos el palo apagado durante este paso
            if (!vistoApagado && debugLogs) Debug.Log($"[Paso8] P{playerIndex} palo APAGADO.", this);
            vistoApagado = true;
        }
        else if (vistoApagado)
        {
            // Transición válida: apagado -> encendido
            encendidoOk = true;
            if (debugLogs) Debug.Log($"[Paso8] P{playerIndex} palo ENCENDIDO (transición detectada).", this);
        }
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (flechaFogataP1) flechaFogataP1.SetActive(false);
        if (flechaFogataP2) flechaFogataP2.SetActive(false);

        gameObject.SetActive(false);
    }

    // Alternativa por eventos: llama esto desde donde invoques PaloIgnifugo.SetEncendido(...)
    public void NotificarEstadoPalo(int playerIndex, bool encendido)
    {
        if (playerIndex == 1)
        {
            if (!encendido) _vistoApagadoP1 = true;
            else if (_vistoApagadoP1) _encendidoOkP1 = true;
        }
        else if (playerIndex == 2)
        {
            if (!encendido) _vistoApagadoP2 = true;
            else if (_vistoApagadoP2) _encendidoOkP2 = true;
        }

        if (_encendidoOkP1 && _encendidoOkP2)
            CompletarPaso();
    }
}
