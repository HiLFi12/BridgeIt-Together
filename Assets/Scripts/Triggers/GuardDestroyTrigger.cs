using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;          // + RoundController
using BridgeItTogether.Gameplay.Spawning;        // + VehicleReturnNotifier

/// <summary>
/// Coloca este script en un GameObject con un Collider marcado como Trigger.
/// Destruye cualquier objeto que entre al trigger y que tenga un componente Guard (en él o en un padre).
/// Opciones:
/// - destroyRoot: si true destruye la raíz del objeto que contiene el Guard.
/// - destroyDelay: tiempo en segundos antes de destruir (0 = inmediato).
/// </summary>
[RequireComponent(typeof(Collider))]
public class GuardDestroyTrigger : MonoBehaviour
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

        // Buscar componente Guard en el objeto o en un padre
        Guard guard = other.GetComponent<Guard>();
        if (!guard) guard = other.GetComponentInParent<Guard>();
        if (!guard) return; // No es un guard

        GameObject toDestroy = destroyRoot ? guard.transform.root.gameObject : guard.gameObject;

        // 1) Si tiene VehicleReturnNotifier en él o en su raíz, desactivar para que OnDisable notifique al RoundController
        if (TryReturnViaNotifier(toDestroy)) return;

        // 2) Fallback: notificar manualmente al RoundController y destruir
        NotifyRoundControllerManual(toDestroy);

        if (debugLogs)
            Debug.Log($"[GuardDestroyTrigger] Destruyendo '{toDestroy.name}' (delay={destroyDelay}).", toDestroy);

        if (destroyDelay <= 0f)
            Destroy(toDestroy);
        else
            Destroy(toDestroy, destroyDelay);
    }

    private bool TryReturnViaNotifier(GameObject go)
    {
        if (!go) return false;

        // Buscar un VehicleReturnNotifier en el objeto o en su raíz
        var notifier = go.GetComponentInParent<VehicleReturnNotifier>();
        if (notifier != null)
        {
            if (debugLogs)
                Debug.Log($"[GuardDestroyTrigger] Desactivando '{go.name}' para retorno vía VehicleReturnNotifier.", go);

            // Desactivar para disparar OnDisable en el notifier (notifica al RoundController)
            go.SetActive(false);
            return true;
        }
        return false;
    }

    private void NotifyRoundControllerManual(GameObject go)
    {
        // Intentar encontrar el RoundController en la escena
#if UNITY_2023_1_OR_NEWER
        var rc = Object.FindFirstObjectByType<RoundController>();
#else
        var rc = Object.FindObjectOfType<RoundController>();
#endif
        if (rc != null)
        {
            if (debugLogs)
                Debug.Log($"[GuardDestroyTrigger] Notificando retorno manual de '{go.name}' al RoundController.", go);
            rc.NotificarAutoDevueltoAlPool(go);
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[GuardDestroyTrigger] RoundController no encontrado para notificar retorno manual.");
        }
    }
}
