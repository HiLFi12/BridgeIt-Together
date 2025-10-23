using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // <-- añadido

public class Paso4Drop : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt; // <-- añadido

    [Header("UI Inputs de Drop")]
    [SerializeField] private GameObject uiDropP1;
    [SerializeField] private GameObject uiDropP2;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si los items se parentan a una mano/holder específico, asignarlo aquí para cada jugador.")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject proximoPaso;

    [Header("Input System (opcional)")]
    [SerializeField] private InputActionReference dropP1; // Acción Drop del Player 1 (opcional)
    [SerializeField] private InputActionReference dropP2; // Acción Drop del Player 2 (opcional)

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool _p1HoldingPrev;
    private bool _p2HoldingPrev;
    private bool _p1Dropped;
    private bool _p2Dropped;
    private bool _completado;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true); // <-- añadido

        if (uiDropP1) uiDropP1.SetActive(true);
        if (uiDropP2) uiDropP2.SetActive(true);

        // Baseline: si ya están sosteniendo algo al entrar, cuenta como "held"
        _p1HoldingPrev = EstaSosteniendoAlgo(raizChequeoP1 ? raizChequeoP1 : jugador1);
        _p2HoldingPrev = EstaSosteniendoAlgo(raizChequeoP2 ? raizChequeoP2 : jugador2);
        _p1Dropped = false;
        _p2Dropped = false;
        _completado = false;

        // Suscripción opcional a acciones Drop (no usado para validar, solo para acelerar chequeos)
        if (dropP1?.action != null)
        {
            dropP1.action.performed += OnDropActionPerformedP1;
            if (!dropP1.action.enabled) dropP1.action.Enable();
        }
        if (dropP2?.action != null)
        {
            dropP2.action.performed += OnDropActionPerformedP2;
            if (!dropP2.action.enabled) dropP2.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (dropP1?.action != null) dropP1.action.performed -= OnDropActionPerformedP1;
        if (dropP2?.action != null) dropP2.action.performed -= OnDropActionPerformedP2;
    }

    private void Update()
    {
        if (_completado) return;

        ChequearJugador(ref _p1HoldingPrev, ref _p1Dropped, raizChequeoP1 ? raizChequeoP1 : jugador1, 1);
        ChequearJugador(ref _p2HoldingPrev, ref _p2Dropped, raizChequeoP2 ? raizChequeoP2 : jugador2, 2);

        if (_p1Dropped && _p2Dropped)
            CompletarPaso();
    }

    public void NotificarPickup(int playerIndex, bool agarrado)
    {
        if (playerIndex == 1)
        {
            if (_p1HoldingPrev && !agarrado) _p1Dropped = true;
            _p1HoldingPrev = agarrado;
        }
        else if (playerIndex == 2)
        {
            if (_p2HoldingPrev && !agarrado) _p2Dropped = true;
            _p2HoldingPrev = agarrado;
        }

        if (debugLogs)
            Debug.Log($"[Paso4Drop] NotificarPickup p{playerIndex} agarrado={agarrado} -> dropped P1={_p1Dropped}, P2={_p2Dropped}", this);

        if (_p1Dropped && _p2Dropped)
            CompletarPaso();
    }

    private void ChequearJugador(ref bool holdingPrev, ref bool dropped, Transform raiz, int playerIndex)
    {
        bool holdingNow = EstaSosteniendoAlgo(raiz);

        if (holdingPrev && !holdingNow)
        {
            dropped = true;
            if (debugLogs) Debug.Log($"[Paso4Drop] Player {playerIndex} soltó el objeto.", this);
        }

        holdingPrev = holdingNow;
    }

    private static bool EstaSosteniendoAlgo(Transform raiz)
    {
        if (raiz == null) return false;

        var monos = raiz.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i];
            if (mb != null && mb.GetType().Name == "BridgeMaterialInfo")
                return true;
        }

        var pickups = raiz.GetComponentsInChildren<BridgeMaterialPickup>(true);
        return pickups != null && pickups.Length > 0;
    }

    private void OnDropActionPerformedP1(InputAction.CallbackContext _) { }
    private void OnDropActionPerformedP2(InputAction.CallbackContext _) { }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false); // <-- añadido
        if (uiDropP1) uiDropP1.SetActive(false);
        if (uiDropP2) uiDropP2.SetActive(false);

        gameObject.SetActive(false);
    }
}
