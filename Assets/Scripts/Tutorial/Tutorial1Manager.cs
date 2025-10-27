using System.Collections;
using UnityEngine;


[DisallowMultipleComponent]
public class Tutorial1Manager : MonoBehaviour
{
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [SerializeField] private float checkInterval = 0.25f;

    [Header("Target to activate when bridge is complete")]
    [SerializeField] private GameObject activateOnComplete;

    private bool _activated;

    private void Start()
    {
        if (bridgeGrid == null)
            bridgeGrid = FindFirstObjectByType<BridgeConstructionGrid>();

        StartCoroutine(MonitorBridgeCompletion());
    }

    private IEnumerator MonitorBridgeCompletion()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.05f, checkInterval));
        while (!_activated)
        {
            if (bridgeGrid != null && AreAllQuadrantsComplete())
            {
                if (activateOnComplete != null)
                    activateOnComplete.SetActive(true);
                _activated = true;
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
                if (!IsQuadrantComplete(so)) return false;
            }
        }
        return true;
    }

    // Minimal check: prefer a lastLayerState == Complete AND last layer completed; otherwise fall back to requiredLayers[last].isCompleted
    private bool IsQuadrantComplete(object quadrantSo)
    {
        if (quadrantSo == null) return false;
        var t = quadrantSo.GetType();

        // Try to get lastLayerState
        var stateProp = t.GetProperty("lastLayerState") ?? t.GetProperty("LastLayerState");
        var stateField = t.GetField("lastLayerState") ?? t.GetField("LastLayerState");
        object stateObj = null;
        if (stateProp != null) stateObj = stateProp.GetValue(quadrantSo);
        else if (stateField != null) stateObj = stateField.GetValue(quadrantSo);

        // Try to get requiredLayers and last layer's isCompleted
        var reqLayersPi = t.GetProperty("requiredLayers");
        var reqLayersFi = t.GetField("requiredLayers");
        object arrObj = reqLayersPi != null ? reqLayersPi.GetValue(quadrantSo) : reqLayersFi?.GetValue(quadrantSo);
        bool lastLayerCompleted = false;
        if (arrObj is System.Array arr && arr.Length > 0)
        {
            var last = arr.GetValue(arr.Length - 1);
            if (last != null)
            {
                var lt = last.GetType();
                var donePi = lt.GetProperty("isCompleted") ?? lt.GetProperty("IsCompleted");
                var doneFi = lt.GetField("isCompleted") ?? lt.GetField("IsCompleted");
                if (donePi != null) lastLayerCompleted = (bool)donePi.GetValue(last);
                else if (doneFi != null) lastLayerCompleted = (bool)doneFi.GetValue(last);
            }
        }

        // If we have a state, require BOTH last layer completed and state == Complete
        if (stateObj != null)
        {
            string stateName = stateObj.ToString();
            bool isStateComplete = string.Equals(stateName, "Complete", System.StringComparison.OrdinalIgnoreCase);
            return lastLayerCompleted && isStateComplete;
        }

        // Fallback: require last layer completed (no state available)
        return lastLayerCompleted;
    }
}
