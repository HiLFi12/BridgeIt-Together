using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Paso6Build : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si los items se parentan a una mano/holder específico, asignarlo aquí para cada jugador.")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject proximoPaso;

    [Header("Input System (opcional)")]
    [SerializeField] private InputActionReference buildP1; // Acción Build del Player 1 (opcional)
    [SerializeField] private InputActionReference buildP2; // Acción Build del Player 2 (opcional)

    [Header("Parámetros")]
    [Tooltip("Ventana de tiempo (segundos) para asociar 'dejar de sostener' con una acción de Build.")]
    [SerializeField] private float ventanaIntentoBuild = 0.6f;
    [SerializeField] private bool debugLogs = false;

    private bool _p1Built;
    private bool _p2Built;
    private bool _completado;

    private bool _p1HoldingPrev;
    private bool _p2HoldingPrev;

    private float _p1BuildIntentUntil;
    private float _p2BuildIntentUntil;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);

        // Estado inicial: normalmente vienen sosteniendo material del paso 5
        _p1HoldingPrev = EstaSosteniendoAlgo(raizChequeoP1 ? raizChequeoP1 : jugador1);
        _p2HoldingPrev = EstaSosteniendoAlgo(raizChequeoP2 ? raizChequeoP2 : jugador2);
        _p1Built = false;
        _p2Built = false;
        _completado = false;
        _p1BuildIntentUntil = 0f;
        _p2BuildIntentUntil = 0f;

        // Suscripción a acciones Build (opcional)
        if (buildP1?.action != null)
        {
            buildP1.action.performed += OnBuildP1;
            if (!buildP1.action.enabled) buildP1.action.Enable();
        }
        if (buildP2?.action != null)
        {
            buildP2.action.performed += OnBuildP2;
            if (!buildP2.action.enabled) buildP2.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (buildP1?.action != null) buildP1.action.performed -= OnBuildP1;
        if (buildP2?.action != null) buildP2.action.performed -= OnBuildP2;
    }

    private void Update()
    {
        if (_completado) return;

        ChequearJugador(ref _p1Built, ref _p1HoldingPrev, ref _p1BuildIntentUntil, raizChequeoP1 ? raizChequeoP1 : jugador1, 1);
        ChequearJugador(ref _p2Built, ref _p2HoldingPrev, ref _p2BuildIntentUntil, raizChequeoP2 ? raizChequeoP2 : jugador2, 2);

        if (_p1Built && _p2Built)
            CompletarPaso();
    }

    private void ChequearJugador(ref bool built, ref bool holdingPrev, ref float buildIntentUntil, Transform raiz, int playerIndex)
    {
        if (built) return;

        bool holdingNow = EstaSosteniendoAlgo(raiz);

        // Considerar "construyó" cuando deja de sostener dentro de la ventana tras un intento de Build
        if (holdingPrev && !holdingNow)
        {
            bool dentroVentana = Time.time <= buildIntentUntil;
            if (dentroVentana)
            {
                built = true;
                if (debugLogs) Debug.Log($"[Paso6Build] Player {playerIndex} construyó (soltó tras Build).", this);
            }
            else if (debugLogs)
            {
                Debug.Log($"[Paso6Build] Player {playerIndex} dejó de sostener fuera de ventana (posible drop).", this);
            }
        }

        holdingPrev = holdingNow;
    }

    private void OnBuildP1(InputAction.CallbackContext _)
    {
        _p1BuildIntentUntil = Time.time + ventanaIntentoBuild;
        if (debugLogs) Debug.Log("[Paso6Build] Intento Build P1", this);
    }

    private void OnBuildP2(InputAction.CallbackContext _)
    {
        _p2BuildIntentUntil = Time.time + ventanaIntentoBuild;
        if (debugLogs) Debug.Log("[Paso6Build] Intento Build P2", this);
    }

    // Llamar directamente desde tu flujo de construcción (PlayerBridgeInteraction / Grid) si prefieres eventos.
    public void NotificarConstruccion(int playerIndex)
    {
        if (playerIndex == 1) _p1Built = true;
        else if (playerIndex == 2) _p2Built = true;

        if (debugLogs) Debug.Log($"[Paso6Build] NotificarConstruccion p{playerIndex}", this);

        if (_p1Built && _p2Built)
            CompletarPaso();
    }

    // Alternativa: si tu Player notifica intención de construir al presionar la tecla
    public void NotificarIntentoBuild(int playerIndex)
    {
        if (playerIndex == 1) _p1BuildIntentUntil = Time.time + ventanaIntentoBuild;
        else if (playerIndex == 2) _p2BuildIntentUntil = Time.time + ventanaIntentoBuild;
    }

    private static bool EstaSosteniendoAlgo(Transform raiz)
    {
        if (raiz == null) return false;

        // Detectar por BridgeMaterialInfo (nombre de tipo para evitar dependencias directas)
        var monos = raiz.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i];
            if (mb != null && mb.GetType().Name == "BridgeMaterialInfo")
                return true;
        }

        // Fallback: BridgeMaterialPickup
        var pickups = raiz.GetComponentsInChildren<BridgeMaterialPickup>(true);
        return pickups != null && pickups.Length > 0;
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);
        if (textoPrompt) textoPrompt.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
