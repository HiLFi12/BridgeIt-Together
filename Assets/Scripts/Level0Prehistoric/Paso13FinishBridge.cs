using UnityEngine;
using TMPro;

public class Paso13FinishBridge : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Objetos a Activar")]
    [SerializeField] private GameObject carSpawner; // Se activa cuando el puente está completamente construido

    [Header("Referencias")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // Referencia directa para chequeo óptimo

    [Header("Chequeo")]
    [Tooltip("Intervalo (segundos) entre chequeos de estado del puente.")]
    [SerializeField] private float intervaloChequeo = 0.5f;
    [SerializeField] private bool debugLogs = false;

    [Header("Texto Final")]
    [SerializeField] private TMP_Text textoPromptFinal; // Texto a activar al completar el paso

    private float _siguienteChequeo;
    private bool _completado;

    private static readonly string[] LayerPrefixes = { "Layer_0_", "Layer_1_", "Layer_2_" };

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (textoPromptFinal) textoPromptFinal.gameObject.SetActive(false); // asegurar que inicia apagado
        _completado = false;
        _siguienteChequeo = 0f;
    }

    private void Update()
    {
        if (_completado) return;
        if (Time.time < _siguienteChequeo) return;

        _siguienteChequeo = Time.time + intervaloChequeo;

        if (EsPuenteCompleto())
        {
            CompletarPaso();
        }
    }

    // Wrapper: usa BridgeConstructionGrid si está asignado; si no, fallback por tag.
    private bool EsPuenteCompleto()
    {
        if (bridgeGrid != null)
            return EsPuenteCompletoViaGrid();

        return EsPuenteCompletoPorTag();
    }

    // Chequeo óptimo: usa el SO de cada cuadrante desde la grilla
    private bool EsPuenteCompletoViaGrid()
    {
        if (bridgeGrid.gridWidth <= 0 || bridgeGrid.gridLength <= 0)
        {
            if (debugLogs) Debug.Log("[Paso13] Grid inválida (dimensiones <= 0).", this);
            return false;
        }

        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                var so = bridgeGrid.GetQuadrantSO(x, z);
                if (so == null || so.requiredLayers == null || so.requiredLayers.Length == 0)
                {
                    if (debugLogs) Debug.Log($"[Paso13] SO nulo o sin capas en [{x},{z}].", this);
                    return false;
                }

                int last = so.requiredLayers.Length - 1;
                if (!so.requiredLayers[last].isCompleted)
                {
                    if (debugLogs) Debug.Log($"[Paso13] Cuadrante [{x},{z}] incompleto (capa {last} no completa).", this);
                    return false;
                }
            }
        }

        if (debugLogs) Debug.Log("[Paso13] Bridge completo vía BridgeConstructionGrid.", this);
        return true;
    }

    // Fallback: escaneo por tag de objetos visuales activos
    private bool EsPuenteCompletoPorTag()
    {
        var quadrants = GameObject.FindGameObjectsWithTag("BridgeQuadrant");
        if (quadrants == null || quadrants.Length == 0)
        {
            if (debugLogs) Debug.Log("[Paso13] No se encontraron cuadrantes con tag 'BridgeQuadrant'.", this);
            return false;
        }

        for (int i = 0; i < quadrants.Length; i++)
        {
            var q = quadrants[i];
            if (q == null) return false;
            if (!CuadranteCompleto(q.transform))
            {
                if (debugLogs) Debug.Log($"[Paso13] Cuadrante incompleto: {q.name}", this);
                return false;
            }
        }

        if (debugLogs) Debug.Log("[Paso13] Todos los cuadrantes completos (fallback tag).", this);
        return true;
    }

    private static bool CuadranteCompleto(Transform raiz)
    {
        for (int i = 0; i < LayerPrefixes.Length; i++)
        {
            if (!ExisteHijoActivoConPrefijo(raiz, LayerPrefixes[i]))
                return false;
        }
        return true;
    }

    private static bool ExisteHijoActivoConPrefijo(Transform raiz, string prefix)
    {
        if (raiz == null) return false;
        var transforms = raiz.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (t == null || t == raiz) continue;
            if (t.name.StartsWith(prefix) && t.gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }

    // Llamar esto desde tu flujo de construcción (por ejemplo, tras TryBuildLayer)
    public void NotificarCambioConstruccion()
    {
        if (_completado) return;
        if (EsPuenteCompleto())
            CompletarPaso();
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (carSpawner) carSpawner.SetActive(true);
        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (textoPromptFinal) textoPromptFinal.gameObject.SetActive(true); // encender texto final

        gameObject.SetActive(false);
    }
}
