using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cambia de Text5 a Text6 al presionar la tecla de interacción real del jugador (E para Player, P para Player2).
/// Solo funciona si Text5 está activo y mientras el jugador esté dentro de este trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Text5ToText6KeySwitch : MonoBehaviour
{
    [Header("UI Texts")]
    [SerializeField] private GameObject text5;
    [SerializeField] private GameObject text6;

    [Header("Options")]
    [Tooltip("Si está activo, solo ejecuta una vez y luego se deshabilita.")]
    [SerializeField] private bool onlyOnce = true;

    private bool triggered;
    private readonly HashSet<Transform> playersInTrigger = new HashSet<Transform>();

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
        if (root != null) playersInTrigger.Add(root);
    }

    private void OnTriggerExit(Collider other)
    {
        var root = GetPlayerRoot(other);
        if (root != null) playersInTrigger.Remove(root);
    }

    private void Update()
    {
        if (onlyOnce && triggered) return;
        if (text5 == null || text6 == null) return;
        if (!text5.activeInHierarchy) return; // Solo cuando Text5 está activo
        if (playersInTrigger.Count == 0) return; // Requiere estar dentro del trigger

        foreach (var root in playersInTrigger)
        {
            if (root == null) continue;
            var p1 = root.GetComponent<Player>();
            var p2 = root.GetComponent<Player2>();
            if (p1 == null && p2 == null) continue;

            var comp = (Component)p1 ?? (Component)p2;
            var key = GetInteractKeyFromComponent(comp, p1 != null ? KeyCode.E : KeyCode.P);
            if (Input.GetKeyDown(key))
            {
                text5.SetActive(false);
                text6.SetActive(true);
                triggered = true;
                if (onlyOnce) enabled = false;
                break;
            }
        }
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

    private static KeyCode GetInteractKeyFromComponent(Component comp, KeyCode fallback)
    {
        if (!comp) return fallback;
        var t = comp.GetType();
        var f = t.GetField("interactKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(KeyCode))
        {
            try { return (KeyCode)f.GetValue(comp); } catch { }
        }
        return fallback;
    }
}
