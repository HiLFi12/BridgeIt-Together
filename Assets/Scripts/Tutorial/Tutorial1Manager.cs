using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;
using BridgeItTogether.Gameplay.Spawning;

/// <summary>
/// Tutorial 1 manager: monitorea el BridgeConstructionGrid y, cuando TODOS los cuadrantes
/// estén en estado Complete (no Damaged/Destroyed), activa el CarSpawner (VehicleGen/RoundController).
/// Ya NO congela a los jugadores al completar.
/// </summary>
[DisallowMultipleComponent]
public class Tutorial1Manager : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Players to freeze (opcional - NO usado)")]
    [Tooltip("Se mantiene por compatibilidad, ya no se congela a los jugadores.")]
    [SerializeField] private List<PlayerController> playersToFreeze = new List<PlayerController>();

    [Header("Car Spawner Root")]
    [Tooltip("Raíz del CarSpawner. Se activará al completar el puente.")]
    [SerializeField] private GameObject carSpawnerRoot;

    [Tooltip("Nombre del hijo que contiene el VehicleSpawner (opcional, solo para activar por nombre).")]
    [SerializeField] private string vehicleGenChildName = "VehicleGen";

    [Tooltip("Nombre del hijo que contiene el RoundController (opcional, solo para activar por nombre).")]
    [SerializeField] private string roundControllerChildName = "RoundController";

    [Header("Tutorial Texts (opcional)")]
    [Tooltip("Asigná los textos del tutorial. Al completar todos los cuadrantes: se enciende Text4 y se apagan Text1/2/3.")]
    [SerializeField] private GameObject text1;
    [SerializeField] private GameObject text2;
    [SerializeField] private GameObject text3;
    [SerializeField] private GameObject text4;

    [Tooltip("Si está activo, al completar el puente se manejarán los textos (Text4 ON, Text1/2/3 OFF).")]
    [SerializeField] private bool manageTextsOnComplete = true;

    [Tooltip("Si está activo, al completar el puente se deshabilitan los UITextSwitchTrigger para que no vuelvan a cambiar Text1/2/3.")]
    [SerializeField] private bool disableTextSwitchTriggersOnComplete = true;

    [Tooltip("Triggers a deshabilitar al completar. Si se deja vacío, se buscarán en la escena.")]
    [SerializeField] private List<UITextSwitchTrigger> textSwitchTriggers = new List<UITextSwitchTrigger>();

    private bool triggered;

    private void Start()
    {
        if (bridgeGrid == null)
            bridgeGrid = FindFirstObjectByType<BridgeConstructionGrid>();

        if (playersToFreeze == null || playersToFreeze.Count == 0)
        {
            var found = FindObjectsOfType<PlayerController>(true);
            playersToFreeze = new List<PlayerController>(found);
        }

        StartCoroutine(MonitorBridgeCompletion());
    }

    private IEnumerator MonitorBridgeCompletion()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.05f, checkInterval));
        while (!triggered)
        {
            if (bridgeGrid != null && AreAllQuadrantsComplete())
            {
                OnBridgeFullyCompleted();
                yield break;
            }
            yield return wait;
        }
    }

    // Condición de victoria: cada cuadrante debe estar en estado "Complete" (no alcanza con estar "built" si está "Damaged").
    private bool AreAllQuadrantsComplete()
    {
        if (bridgeGrid == null) return false;
        int w = bridgeGrid.gridWidth;
        int l = bridgeGrid.gridLength;
        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < l; z++)
            {
                var so = bridgeGrid.GetQuadrantSO(x, z);
                if (so == null) return false;
                if (!IsQuadrantCompleteAndRepaired(so)) return false;
            }
        }
        return true;
    }

    // Usa lastLayerState == Complete si está disponible; si no, cae a verificar que la última capa esté marcada como completada.
    private bool IsQuadrantCompleteAndRepaired(object quadrantSO)
    {
        if (quadrantSO == null) return false;
        var t = quadrantSO.GetType();

        // 1) Intentar leer lastLayerState (campo o propiedad) y exigir "Complete"
        var stateProp = t.GetProperty("lastLayerState") ?? t.GetProperty("LastLayerState");
        var stateField = t.GetField("lastLayerState") ?? t.GetField("LastLayerState");
        object stateObj = null;
        if (stateProp != null) stateObj = stateProp.GetValue(quadrantSO);
        else if (stateField != null) stateObj = stateField.GetValue(quadrantSO);

        if (stateObj != null)
        {
            string stateName = stateObj.ToString(); // Enum.ToString() -> "Complete"/"Damaged"/"Destroyed"
            if (!string.Equals(stateName, "Complete", System.StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        // 2) Fallback: requiredLayers[last].isCompleted (menos estricto si no hay estado)
        var reqLayersPi = t.GetProperty("requiredLayers");
        var reqLayersFi = t.GetField("requiredLayers");
        object arrObj = reqLayersPi != null ? reqLayersPi.GetValue(quadrantSO) : reqLayersFi?.GetValue(quadrantSO);
        if (arrObj is System.Array arr && arr.Length > 0)
        {
            var last = arr.GetValue(arr.Length - 1);
            if (last != null)
            {
                var lt = last.GetType();
                var donePi = lt.GetProperty("isCompleted") ?? lt.GetProperty("IsCompleted");
                var doneFi = lt.GetField("isCompleted") ?? lt.GetField("IsCompleted");
                bool isCompleted = false;
                if (donePi != null) isCompleted = (bool)donePi.GetValue(last);
                else if (doneFi != null) isCompleted = (bool)doneFi.GetValue(last);
                return isCompleted;
            }
        }
        return false;
    }

    private void OnBridgeFullyCompleted()
    {
        if (triggered) return;
        triggered = true;
        if (showDebugLogs) Debug.Log("[Tutorial1Manager] Puente COMPLETO (estado Complete en todos los cuadrantes). Activando CarSpawner.");

        // 1) YA NO se desactiva el movimiento de los jugadores.

        // 2) Activar CarSpawner (root y componentes clave)
        ActivateCarSpawner();

        // 3) Manejo de textos del tutorial (opcional)
        if (manageTextsOnComplete)
        {
            if (disableTextSwitchTriggersOnComplete)
            {
                if (textSwitchTriggers == null || textSwitchTriggers.Count == 0)
                {
                    var foundTriggers = FindObjectsOfType<UITextSwitchTrigger>(true);
                    textSwitchTriggers = new List<UITextSwitchTrigger>(foundTriggers);
                }
                foreach (var trig in textSwitchTriggers)
                {
                    if (trig != null) trig.enabled = false;
                }
            }
            SetText4ActiveAndOthersOff();
        }
    }

    private void ActivateCarSpawner()
    {
        if (carSpawnerRoot != null)
        {
            if (!carSpawnerRoot.activeInHierarchy)
                carSpawnerRoot.SetActive(true);

            // Activar hijos por nombre (opcional)
            if (!string.IsNullOrWhiteSpace(vehicleGenChildName))
            {
                var vg = FindChildByName(carSpawnerRoot.transform, vehicleGenChildName);
                if (vg != null) vg.gameObject.SetActive(true);
            }
            if (!string.IsNullOrWhiteSpace(roundControllerChildName))
            {
                var rc = FindChildByName(carSpawnerRoot.transform, roundControllerChildName);
                if (rc != null) rc.gameObject.SetActive(true);
            }
        }

        // Asegurar componentes habilitados
        var spawner = carSpawnerRoot != null
            ? carSpawnerRoot.GetComponentInChildren<VehicleSpawner>(true)
            : FindFirstObjectByType<VehicleSpawner>(FindObjectsInactive.Include);
        if (spawner != null && !spawner.gameObject.activeInHierarchy)
            spawner.gameObject.SetActive(true);

        var round = carSpawnerRoot != null
            ? carSpawnerRoot.GetComponentInChildren<RoundController>(true)
            : FindFirstObjectByType<RoundController>(FindObjectsInactive.Include);
        if (round != null && !round.gameObject.activeInHierarchy)
            round.gameObject.SetActive(true);
    }

    private void SetText4ActiveAndOthersOff()
    {
        if (text1 != null) text1.SetActive(false);
        if (text2 != null) text2.SetActive(false);
        if (text3 != null) text3.SetActive(false);
        if (text4 != null) text4.SetActive(true);
        if (showDebugLogs) Debug.Log("[Tutorial1Manager] Text4 ON, Text1/2/3 OFF.");
    }

    private static Transform FindChildByName(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t;
        }
        return null;
    }
}
