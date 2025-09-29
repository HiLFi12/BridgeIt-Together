using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cambia de un Canvas a otro automáticamente al entrar un jugador en el trigger.
/// No requiere presionar tecla de interacción.
/// Solo funciona si el Canvas a desactivar está ACTIVO.
/// Opcionalmente puede revertir el cambio cuando el último jugador sale del trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CanvasProximitySwitchTrigger : MonoBehaviour
{
    [Header("Canvas")]
    [Tooltip("Canvas que se activará al entrar un jugador en el trigger.")]
    [SerializeField] private GameObject canvasToActivate;

    [Tooltip("Canvas que se desactivará al entrar un jugador en el trigger (debe estar ACTIVO para que esto funcione).")]
    [SerializeField] private GameObject canvasToDeactivate;

    [Header("Options")]
    [Tooltip("Cuando está activo, el cambio se realiza una sola vez y este componente se deshabilita.")]
    [SerializeField] private bool activateOnlyOnce = false;

    [Tooltip("Si está activo y 'activateOnlyOnce' es falso, al salir el último jugador se revierte el cambio.")]
    [SerializeField] private bool revertOnExit = false;

    private readonly HashSet<Transform> playersInTrigger = new HashSet<Transform>();
    private bool triggered;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col && !col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var root = GetPlayerRoot(other);
        if (root == null) return;

        playersInTrigger.Add(root);

        if (activateOnlyOnce && triggered) return;

        // No ejecutar si el canvas a desactivar no está activo
        if (!IsCanvasToDeactivateActive()) return;

        if (SwitchToActive())
        {
            triggered = true;
            if (activateOnlyOnce) enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var root = GetPlayerRoot(other);
        if (root == null) return;
        playersInTrigger.Remove(root);

        if (!activateOnlyOnce && revertOnExit && playersInTrigger.Count == 0)
        {
            SwitchToDeactivated();
        }
    }

    private bool SwitchToActive()
    {
        // Ejecutar solo si el canvas a desactivar está activo en jerarquía
        if (!IsCanvasToDeactivateActive()) return false;
        if (canvasToDeactivate) canvasToDeactivate.SetActive(false);
        if (canvasToActivate) canvasToActivate.SetActive(true);
        return true;
    }

    private void SwitchToDeactivated()
    {
        if (canvasToActivate) canvasToActivate.SetActive(false);
        if (canvasToDeactivate) canvasToDeactivate.SetActive(true);
    }

    private bool IsCanvasToDeactivateActive()
    {
        return canvasToDeactivate != null && canvasToDeactivate.activeInHierarchy;
    }

    private static Transform GetPlayerRoot(Collider col)
    {
        if (!col) return null;
        var p1 = col.GetComponentInParent<Player>();
        if (p1) return p1.transform;
        var p2 = col.GetComponentInParent<Player2>();
        if (p2) return p2.transform;
        return null;
    }
}
