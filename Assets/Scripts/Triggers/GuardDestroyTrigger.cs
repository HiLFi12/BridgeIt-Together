using UnityEngine;

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

        if (debugLogs)
            Debug.Log($"[GuardDestroyTrigger] Destruyendo '{toDestroy.name}' (delay={destroyDelay}).", toDestroy);

        if (destroyDelay <= 0f)
            Destroy(toDestroy);
        else
            Destroy(toDestroy, destroyDelay);
    }
}
