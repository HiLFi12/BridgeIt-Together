using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;          // + RoundController
using BridgeItTogether.Gameplay.Spawning;        // + VehicleReturnNotifier

[RequireComponent(typeof(Collider))]
public class FakeGuardDestroyTrigger : MonoBehaviour
{
    [SerializeField] private bool destroyRoot = false;
    [SerializeField, Min(0f)] private float destroyDelay = 0f;
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other) return;

        // Buscar componente FakeGuard en el objeto o en un padre
        FakeGuard fakeguard = other.GetComponent<FakeGuard>();
        if (!fakeguard) fakeguard = other.GetComponentInParent<FakeGuard>();
        if (!fakeguard) return; // No es un fake guard

        GameObject toDestroy = destroyRoot ? fakeguard.transform.root.gameObject : fakeguard.gameObject;

        // 1) Si tiene VehicleReturnNotifier, desactivar para que OnDisable notifique
        if (TryReturnViaNotifier(toDestroy)) return;

        // 2) Fallback: notificar manualmente y destruir
        NotifyRoundControllerManual(toDestroy);

        if (debugLogs)
            Debug.Log($"[FakeGuardDestroyTrigger] Destruyendo '{toDestroy.name}' (delay={destroyDelay}).", toDestroy);

        if (destroyDelay <= 0f)
            Destroy(toDestroy);
        else
            Destroy(toDestroy, destroyDelay);
    }

    private bool TryReturnViaNotifier(GameObject go)
    {
        if (!go) return false;

        var notifier = go.GetComponentInParent<VehicleReturnNotifier>();
        if (notifier != null)
        {
            if (debugLogs)
                Debug.Log($"[FakeGuardDestroyTrigger] Desactivando '{go.name}' para retorno vía VehicleReturnNotifier.", go);

            go.SetActive(false);
            return true;
        }
        return false;
    }

    private void NotifyRoundControllerManual(GameObject go)
    {
#if UNITY_2023_1_OR_NEWER
        var rc = Object.FindFirstObjectByType<RoundController>();
#else
        var rc = Object.FindObjectOfType<RoundController>();
#endif
        if (rc != null)
        {
            if (debugLogs)
                Debug.Log($"[FakeGuardDestroyTrigger] Notificando retorno manual de '{go.name}' al RoundController.", go);
            rc.NotificarAutoDevueltoAlPool(go);
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[FakeGuardDestroyTrigger] RoundController no encontrado para notificar retorno manual.");
        }
    }
}
