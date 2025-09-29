using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;
using BridgeItTogether.Gameplay.Spawning;

/// <summary>
/// Tutorial 1 manager: monitorea el BridgeConstructionGrid y, cuando TODOS los cuadrantes
/// están completos (todas las capas), desactiva el movimiento de los jugadores y activa
/// el CarSpawner (hijos VehicleGen y RoundController).
/// </summary>
[DisallowMultipleComponent]
public class Tutorial1Manager : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Players to freeze (opcional)")]
    [Tooltip("Si se deja vacío, se buscarán todos los PlayerController de la escena.")]
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
                int last = Mathf.Max(0, so.requiredLayers.Length - 1);
                if (!so.requiredLayers[last].isCompleted) return false;
            }
        }
        return true;
    }

    private void OnBridgeFullyCompleted()
    {
        if (triggered) return;
        triggered = true;
        if (showDebugLogs) Debug.Log("[Tutorial1Manager] Puente COMPLETO. Desactivando movimiento y activando CarSpawner.");

        // 1) Desactivar movimiento de jugadores
        foreach (var pc in playersToFreeze)
        {
            if (pc == null) continue;
            pc.enabled = false;
        }

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
        // VehicleSpawner
        var spawner = carSpawnerRoot != null
            ? carSpawnerRoot.GetComponentInChildren<VehicleSpawner>(true)
            : FindFirstObjectByType<VehicleSpawner>(FindObjectsInactive.Include);
        if (spawner != null && !spawner.gameObject.activeInHierarchy)
            spawner.gameObject.SetActive(true);

        // RoundController
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
