using UnityEngine;
using TMPro;

public class Paso13FinishBridge : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Objetos a Activar")]
    [SerializeField] private GameObject carSpawner; // Se activa cuando el puente está completamente construido

    [Header("Chequeo")]
    [Tooltip("Intervalo (segundos) entre chequeos de estado del puente.")]
    [SerializeField] private float intervaloChequeo = 0.5f;
    [SerializeField] private bool debugLogs = false;

    private float _siguienteChequeo;
    private bool _completado;

    private static readonly string[] LayerPrefixes = { "Layer_0_", "Layer_1_", "Layer_2_" };

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
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

    // Busca todos los cuadrantes (tag BridgeQuadrant) y valida que tengan las 3 capas activas.
    private bool EsPuenteCompleto()
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

        if (debugLogs) Debug.Log("[Paso13] Todos los cuadrantes están completos.", this);
        return true;
    }

    private static bool CuadranteCompleto(Transform raiz)
    {
        // Considera completo si existen (y están activos en jerarquía) objetos de capa con los prefijos esperados.
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

    // Llama esto desde tu flujo de construcción (por ejemplo, tras TryBuildLayer)
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

        gameObject.SetActive(false);
    }
}
