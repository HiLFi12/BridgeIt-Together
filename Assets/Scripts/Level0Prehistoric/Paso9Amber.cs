using UnityEngine;
using TMPro;

public class Paso9Amber : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Flechas hacia el source (corteza resistente)")]
    [SerializeField] private GameObject flechaSourceP1;
    [SerializeField] private GameObject flechaSourceP2;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si los items se parentan a una mano/holder específico, asignarlo aquí para cada jugador.")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject proximoPaso;

    [Header("Parámetros")]
    [Tooltip("layerIndex del material requerido (0=Base, 1=Soporte, 2=Superficie).")]
    [Range(0, 2)]
    [SerializeField] private int tipoMaterialObjetivo = 1; // Amber/Layer 1
    [Tooltip("Requiere haber visto el PaloIgnifugo encendido durante este paso antes de aceptar el material.")]
    [SerializeField] private bool requerirPaloEncendidoPrevio = true;
    [SerializeField] private bool debugLogs = false;

    private bool _vistoEncendidoP1, _vistoEncendidoP2;
    private bool _p1Tiene, _p2Tiene;
    private bool _completado;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (flechaSourceP1) flechaSourceP1.SetActive(true);
        if (flechaSourceP2) flechaSourceP2.SetActive(true);

        _vistoEncendidoP1 = _vistoEncendidoP2 = false;
        _p1Tiene = _p2Tiene = false;
        _completado = false;
    }

    private void Update()
    {
        if (_completado) return;

        var raiz1 = raizChequeoP1 ? raizChequeoP1 : jugador1;
        var raiz2 = raizChequeoP2 ? raizChequeoP2 : jugador2;

        // Trackear si ya vimos el palo encendido en este paso
        if (raiz1)
        {
            var palo1 = raiz1.GetComponentInChildren<PaloIgnifugo>(true);
            if (palo1 && palo1.EstaEncendido()) _vistoEncendidoP1 = true;

            bool tieneL1 = JugadorTieneMaterialDeLayer(raiz1, tipoMaterialObjetivo, debugLogs);
            _p1Tiene = requerirPaloEncendidoPrevio ? (_vistoEncendidoP1 && tieneL1) : tieneL1;
        }

        if (raiz2)
        {
            var palo2 = raiz2.GetComponentInChildren<PaloIgnifugo>(true);
            if (palo2 && palo2.EstaEncendido()) _vistoEncendidoP2 = true;

            bool tieneL1 = JugadorTieneMaterialDeLayer(raiz2, tipoMaterialObjetivo, debugLogs);
            _p2Tiene = requerirPaloEncendidoPrevio ? (_vistoEncendidoP2 && tieneL1) : tieneL1;
        }

        if (debugLogs)
            Debug.Log($"[Paso9Amber] P1Tiene={_p1Tiene} (vistoEnc={_vistoEncendidoP1}) | P2Tiene={_p2Tiene} (vistoEnc={_vistoEncendidoP2})", this);

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }

    // Úsalo si tu flujo no parenta el item al jugador (llamar al agarrar/soltar)
    public void NotificarPickup(int playerIndex, BridgeMaterialPickup pickup, bool agarrado)
    {
        if (pickup == null) return;
        bool esObjetivo = pickup.layerIndex == tipoMaterialObjetivo;

        if (playerIndex == 1 && esObjetivo) _p1Tiene = requerirPaloEncendidoPrevio ? (_vistoEncendidoP1 && agarrado) : agarrado;
        else if (playerIndex == 2 && esObjetivo) _p2Tiene = requerirPaloEncendidoPrevio ? (_vistoEncendidoP2 && agarrado) : agarrado;

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }

    // Llama esto desde el PaloIgnifugo cuando cambie su estado, si prefieres eventos.
    public void NotificarEstadoPalo(int playerIndex, bool encendido)
    {
        if (playerIndex == 1 && encendido) _vistoEncendidoP1 = true;
        else if (playerIndex == 2 && encendido) _vistoEncendidoP2 = true;
    }

    private static bool JugadorTieneMaterialDeLayer(Transform raiz, int layerIndexObjetivo, bool debug)
    {
        if (raiz == null) return false;

        // 1) BridgeMaterialPickup (principal)
        var pickups = raiz.GetComponentsInChildren<BridgeMaterialPickup>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            var p = pickups[i];
            if (p != null && p.layerIndex == layerIndexObjetivo)
            {
                if (debug) Debug.Log($"[Paso9Amber] Pickup {p.gameObject.name} -> layerIndex={p.layerIndex}");
                return true;
            }
        }

        // 2) BridgeMaterialInfo (si también lo usan para materiales de mano)
        var monos = raiz.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i]; if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name != "BridgeMaterialInfo") continue;

            // Intentar leer un layerIndex si lo expone (opcional)
            var f = t.GetField("layerIndex") ?? t.GetField("LayerIndex");
            if (f != null && f.FieldType == typeof(int) && (int)f.GetValue(mb) == layerIndexObjetivo)
                return true;

            var p = t.GetProperty("layerIndex") ?? t.GetProperty("LayerIndex");
            if (p != null && p.PropertyType == typeof(int) && (int)p.GetValue(mb) == layerIndexObjetivo)
                return true;
        }

        return false;
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (flechaSourceP1) flechaSourceP1.SetActive(false);
        if (flechaSourceP2) flechaSourceP2.SetActive(false);

        gameObject.SetActive(false);
    }
}
