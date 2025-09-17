// csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class BridgeCompletionManager : MonoBehaviour
{
    [Header("Referencia al Bridge Grid")]
    [Tooltip("Raíz del BridgeGrid. Si está vacío se intentará autodetectar un objeto con componente 'BridgeConstructionGrid'.")]
    [SerializeField] private Transform bridgeGridRoot;

    [Header("Progreso requerido")]
    [Tooltip("Índice/Número de la capa objetivo. Si 'useOneBasedLayerIndex' está activo, 1..4; si no, 0..3.")]
    [SerializeField] [Range(0, 3)] private int targetLayerIndex = 2;
    [Tooltip("Si tus capas se numeran 1..4 en vez de 0..3, activa esto.")]
    [SerializeField] private bool useOneBasedLayerIndex = false;

    private enum CompletionMode { AnyQuadrant, AllQuadrants }
    [SerializeField] private CompletionMode completionMode = CompletionMode.AnyQuadrant;

    [Header("Descubrimiento de cuadrantes")]
    [Tooltip("Arrastra aquí los componentes de cada cuadrante (opcional). Si se llena, se usará esta lista y no el auto-descubrimiento.")]
    [SerializeField] private MonoBehaviour[] manualQuadrants;
    [Tooltip("Nombre exacto del tipo de componente de cuadrante (ej.: 'BridgeQuadrantInstance'). Opcional.")]
    [SerializeField] private string quadrantComponentTypeName = "";
    [Tooltip("Re-descubrir cuadrantes periódicamente si se crean en runtime.")]
    [SerializeField] private bool autoRefreshQuadrants = true;
    [SerializeField] private float refreshInterval = 0.5f;

    [Header("Siguiente nivel")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private float loadDelaySeconds = 0.25f;

    [Header("Depuración")]
    [SerializeField] private bool enableDebugLogs = false;

    private readonly List<MonoBehaviour> quadrantComponents = new List<MonoBehaviour>();
    private float refreshTimer;
    private bool triggered;

    // Nombres comunes conocidos
    private static readonly string[] QuadrantTypeNames = { "BridgeQuadrantInstance", "BridgeQuadrant" };
    private static readonly string[] Method_IsLayerComplete = { "IsLayerComplete", "IsLayerCompleted", "IsLayerFilled", "HasCompletedLayer" };
    private static readonly string[] Prop_CompletedLayers = { "CompletedLayers", "CompleteLayers", "BuiltLayers" };
    private static readonly string[] Prop_LayerStates = { "LayerStates", "Layers" };
    private static readonly string[] Prop_CompletedCount = { "CompletedLayerCount", "BuiltLayerCount", "CompletedCount", "BuiltCount", "CurrentLayer", "CurrentLayerIndex" };
    private static readonly string[] Elem_IsComplete = { "IsComplete", "Complete" };

    private void Awake()
    {
        TryAutoFindBridgeGridRoot();
        BuildQuadrantList(initial: true);
    }

    private void Update()
    {
        if (triggered) return;
        if (string.IsNullOrWhiteSpace(nextSceneName)) return;

        if ((quadrantComponents.Count == 0 || autoRefreshQuadrants) && (manualQuadrants == null || manualQuadrants.Length == 0))
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval)
            {
                refreshTimer = 0f;
                BuildQuadrantList();
            }
        }

        if (quadrantComponents.Count == 0) return;

        int idxForIndexing = useOneBasedLayerIndex ? Mathf.Max(0, targetLayerIndex - 1) : targetLayerIndex;
        int minCompletedThreshold = useOneBasedLayerIndex ? Mathf.Max(0, targetLayerIndex) : targetLayerIndex + 1;

        bool ready = completionMode == CompletionMode.AllQuadrants
            ? AllComplete(idxForIndexing, minCompletedThreshold)
            : AnyComplete(idxForIndexing, minCompletedThreshold);

        if (!ready) return;

        triggered = true;
        if (enableDebugLogs) Debug.Log("[BridgeCompletionManager] Condición cumplida. Cargando siguiente escena...");
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private void TryAutoFindBridgeGridRoot()
    {
        if (bridgeGridRoot != null) return;

        var all = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in all)
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name == "BridgeConstructionGrid")
            {
                bridgeGridRoot = mb.transform;
                if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] BridgeGridRoot autodetectado en: {bridgeGridRoot.name}");
                break;
            }
        }
        if (bridgeGridRoot == null)
        {
            bridgeGridRoot = transform;
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Usando este objeto como raíz: {bridgeGridRoot.name}");
        }
    }

    private void BuildQuadrantList(bool initial = false)
    {
        quadrantComponents.Clear();

        // 1) Manual
        if (manualQuadrants != null && manualQuadrants.Length > 0)
        {
            foreach (var mb in manualQuadrants)
                if (mb != null && !quadrantComponents.Contains(mb))
                    quadrantComponents.Add(mb);

            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Usando {quadrantComponents.Count} cuadrantes manuales.");
            return;
        }

        if (bridgeGridRoot == null) return;

        // 2) Por nombre de tipo específico
        if (!string.IsNullOrWhiteSpace(quadrantComponentTypeName))
        {
            var all = bridgeGridRoot.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                if (mb.GetType().Name == quadrantComponentTypeName && !quadrantComponents.Contains(mb))
                    quadrantComponents.Add(mb);
            }
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Cuadrantes por tipo '{quadrantComponentTypeName}': {quadrantComponents.Count}");
        }

        // 3) Por nombres conocidos
        if (quadrantComponents.Count == 0)
        {
            var all = bridgeGridRoot.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                var tn = mb.GetType().Name;
                for (int i = 0; i < QuadrantTypeNames.Length; i++)
                {
                    if (tn == QuadrantTypeNames[i])
                    {
                        if (!quadrantComponents.Contains(mb))
                            quadrantComponents.Add(mb);
                        break;
                    }
                }
            }
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Cuadrantes por tipos conocidos: {quadrantComponents.Count}");
        }

        // 4) Fallback: cualquier MB que exponga señales de capas
        if (quadrantComponents.Count == 0)
        {
            var all = bridgeGridRoot.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                if (ExposesLayerProgress(mb.GetType()))
                    quadrantComponents.Add(mb);
            }
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Cuadrantes por fallback de reflexión: {quadrantComponents.Count}");
        }
    }

    private bool ExposesLayerProgress(Type type)
    {
        foreach (var name in Method_IsLayerComplete)
            if (type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null) != null)
                return true;

        foreach (var name in Prop_CompletedLayers)
            if (type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                return true;

        foreach (var name in Prop_LayerStates)
            if (type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                return true;

        foreach (var name in Prop_CompletedCount)
            if (type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                return true;

        return false;
        }

    private bool AnyComplete(int idxForIndexing, int minCompletedThreshold)
    {
        foreach (var q in quadrantComponents)
        {
            if (q == null) continue;
            bool ok = IsQuadrantLayerComplete(q, idxForIndexing, minCompletedThreshold, out string dbg);
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] Any: {q.name} -> {ok} {dbg}");
            if (ok) return true;
        }
        return false;
    }

    private bool AllComplete(int idxForIndexing, int minCompletedThreshold)
    {
        bool foundAny = false;
        foreach (var q in quadrantComponents)
        {
            if (q == null) continue;
            foundAny = true;
            bool ok = IsQuadrantLayerComplete(q, idxForIndexing, minCompletedThreshold, out string dbg);
            if (enableDebugLogs) Debug.Log($"[BridgeCompletionManager] All: {q.name} -> {ok} {dbg}");
            if (!ok) return false;
        }
        return foundAny;
    }

    private bool IsQuadrantLayerComplete(MonoBehaviour quadrant, int layerIndexForIndexing, int minCompletedThreshold, out string debugInfo)
    {
        debugInfo = "";
        var type = quadrant.GetType();

        // 1) Método IsLayerComplete(int) -> bool
        foreach (var name in Method_IsLayerComplete)
        {
            var m = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
            if (m != null && m.ReturnType == typeof(bool))
            {
                try
                {
                    bool res = (bool)m.Invoke(quadrant, new object[] { layerIndexForIndexing });
                    debugInfo = $"via {name}({layerIndexForIndexing})={res}";
                    if (res) return true;
                }
                catch { }
            }
        }

        // 2) Propiedad CompletedLayers -> IEnumerable<int>
        foreach (var name in Prop_CompletedLayers)
        {
            var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
            {
                try
                {
                    var value = p.GetValue(quadrant);
                    if (value is System.Collections.IEnumerable en)
                    {
                        foreach (var e in en)
                        {
                            if (e is int idx && idx == layerIndexForIndexing)
                            {
                                debugInfo = $"via {name} contains {layerIndexForIndexing}";
                                return true;
                            }
                        }
                        debugInfo = $"via {name} not contains {layerIndexForIndexing}";
                    }
                }
                catch { }
            }
        }

        // 3) Propiedad LayerStates/Layers -> IList/Array de bool o elementos con IsComplete
        foreach (var name in Prop_LayerStates)
        {
            var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p == null) continue;

            try
            {
                var value = p.GetValue(quadrant);
                if (value == null) continue;

                if (value is Array arr)
                {
                    if (layerIndexForIndexing >= 0 && layerIndexForIndexing < arr.Length)
                    {
                        var el = arr.GetValue(layerIndexForIndexing);
                        if (el is bool b)
                        {
                            debugInfo = $"via {name}[{layerIndexForIndexing}]={b}";
                            if (b) return true;
                        }
                        else
                        {
                            var bt = el?.GetType();
                            var bp = bt?.GetProperty(Elem_IsComplete[0]) ?? bt?.GetProperty(Elem_IsComplete[1]);
                            if (bp != null && bp.PropertyType == typeof(bool))
                            {
                                bool b2 = (bool)bp.GetValue(el);
                                debugInfo = $"via {name}[{layerIndexForIndexing}].{bp.Name}={b2}";
                                if (b2) return true;
                            }
                        }
                    }
                }
                else if (value is System.Collections.IList list)
                {
                    if (layerIndexForIndexing >= 0 && layerIndexForIndexing < list.Count)
                    {
                        var el = list[layerIndexForIndexing];
                        if (el is bool b)
                        {
                            debugInfo = $"via {name}[{layerIndexForIndexing}]={b}";
                            if (b) return true;
                        }
                        else
                        {
                            var bt = el?.GetType();
                            var bp = bt?.GetProperty(Elem_IsComplete[0]) ?? bt?.GetProperty(Elem_IsComplete[1]);
                            if (bp != null && bp.PropertyType == typeof(bool))
                            {
                                bool b2 = (bool)bp.GetValue(el);
                                debugInfo = $"via {name}[{layerIndexForIndexing}].{bp.Name}={b2}";
                                if (b2) return true;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        // 4) Conteo de capas completadas o capa actual
        foreach (var name in Prop_CompletedCount)
        {
            var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(int))
            {
                try
                {
                    int count = (int)p.GetValue(quadrant);
                    bool ok = count >= minCompletedThreshold;
                    debugInfo = $"via {name}={count} >= {minCompletedThreshold} -> {ok}";
                    if (ok) return true;
                }
                catch { }
            }
        }

        return false;
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        if (loadDelaySeconds > 0f)
            yield return new WaitForSeconds(loadDelaySeconds);

        SceneManager.LoadScene(nextSceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (refreshInterval < 0.05f) refreshInterval = 0.05f;
    }
#endif
}
