using UnityEngine;

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
        if (!fakeguard) return; // No es un guard

        GameObject toDestroy = destroyRoot ? fakeguard.transform.root.gameObject : fakeguard.gameObject;

        if (debugLogs)
            Debug.Log($"[FakeGuardDestroyTrigger] Destruyendo '{toDestroy.name}' (delay={destroyDelay}).", toDestroy);

        if (destroyDelay <= 0f)
            Destroy(toDestroy);
        else
            Destroy(toDestroy, destroyDelay);
    }
}
