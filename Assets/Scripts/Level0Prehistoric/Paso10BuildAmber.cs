using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Paso10BuildAmber : MonoBehaviour
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
    [Tooltip("Layer del material requerido para construir en este paso.")]
    [Range(0, 2)]
    [SerializeField] private int tipoMaterialObjetivo = 1; // Amber/Layer 1
    [Tooltip("Ventana de tiempo (segundos) para asociar 'dejar de sostener' con una acción de Build.")]
    [SerializeField] private float ventanaIntentoBuild = 0.6f;
    [SerializeField] private bool debugLogs = false;

    private bool _p1Built;
    private bool _p2Built;
    private bool _completado;

    private bool _p1HoldingCorrectPrev;
    private bool _p2HoldingCorrectPrev;

    private float _p1BuildIntentUntil;
    private float _p2BuildIntentUntil;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);

        // Estado inicial: deberían venir sosteniendo layer 1 antes de construir
        _p1HoldingCorrectPrev = TieneMaterialDeLayer(raizChequeoP1 ? raizChequeoP1 : jugador1, tipoMaterialObjetivo);
        _p2HoldingCorrectPrev = TieneMaterialDeLayer(raizChequeoP2 ? raizChequeoP2 : jugador2, tipoMaterialObjetivo);
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

        ChequearJugador(ref _p1Built, ref _p1HoldingCorrectPrev, ref _p1BuildIntentUntil, raizChequeoP1 ? raizChequeoP1 : jugador1, 1);
        ChequearJugador(ref _p2Built, ref _p2HoldingCorrectPrev, ref _p2BuildIntentUntil, raizChequeoP2 ? raizChequeoP2 : jugador2, 2);

        if (_p1Built && _p2Built)
            CompletarPaso();
    }

    private void ChequearJugador(ref bool built, ref bool holdingCorrectPrev, ref float buildIntentUntil, Transform raiz, int playerIndex)
    {
        if (built) return;

        bool holdingCorrectNow = TieneMaterialDeLayer(raiz, tipoMaterialObjetivo);

        // Considerar "construyó" cuando deja de sostener el material correcto tras un intento de Build
        if (holdingCorrectPrev && !holdingCorrectNow)
        {
            bool dentroVentana = Time.time <= buildIntentUntil;
            if (dentroVentana)
            {
                built = true;
                if (debugLogs) Debug.Log($"[Paso10BuildAmber] Player {playerIndex} construyó Layer {tipoMaterialObjetivo}.", this);
            }
            else if (debugLogs)
            {
                Debug.Log($"[Paso10BuildAmber] Player {playerIndex} dejó de sostener fuera de ventana (posible drop).", this);
            }
        }

        holdingCorrectPrev = holdingCorrectNow;
    }

    private void OnBuildP1(InputAction.CallbackContext _)
    {
        _p1BuildIntentUntil = Time.time + ventanaIntentoBuild;
        if (debugLogs) Debug.Log("[Paso10BuildAmber] Intento Build P1", this);
    }

    private void OnBuildP2(InputAction.CallbackContext _)
    {
        _p2BuildIntentUntil = Time.time + ventanaIntentoBuild;
        if (debugLogs) Debug.Log("[Paso10BuildAmber] Intento Build P2", this);
    }

    // Notificar desde tu flujo de construcción si prefieres eventos directos.
    public void NotificarConstruccion(int playerIndex) // sin layer param: asume correcto
    {
        if (playerIndex == 1) _p1Built = true;
        else if (playerIndex == 2) _p2Built = true;

        if (_p1Built && _p2Built) CompletarPaso();
    }

    public void NotificarConstruccion(int playerIndex, int layerConstruido) // valida layer 1
    {
        if (layerConstruido != tipoMaterialObjetivo) return;

        if (playerIndex == 1) _p1Built = true;
        else if (playerIndex == 2) _p2Built = true;

        if (_p1Built && _p2Built) CompletarPaso();
    }

    public void NotificarIntentoBuild(int playerIndex)
    {
        if (playerIndex == 1) _p1BuildIntentUntil = Time.time + ventanaIntentoBuild;
        else if (playerIndex == 2) _p2BuildIntentUntil = Time.time + ventanaIntentoBuild;
    }

    private static bool TieneMaterialDeLayer(Transform raiz, int layerIndexObjetivo)
    {
        if (raiz == null) return false;

        // Principal: BridgeMaterialPickup
        var pickups = raiz.GetComponentsInChildren<BridgeMaterialPickup>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            var p = pickups[i];
            if (p != null && p.layerIndex == layerIndexObjetivo)
                return true;
        }

        // Opcional: BridgeMaterialInfo con layer expuesto (si existe)
        var monos = raiz.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i]; if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name != "BridgeMaterialInfo") continue;

            var f = t.GetField("layerIndex") ?? t.GetField("LayerIndex");
            if (f != null && f.FieldType == typeof(int) && (int)f.GetValue(mb) == layerIndexObjetivo) return true;

            var p = t.GetProperty("layerIndex") ?? t.GetProperty("LayerIndex");
            if (p != null && p.PropertyType == typeof(int) && (int)p.GetValue(mb) == layerIndexObjetivo) return true;
        }

        return false;
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
