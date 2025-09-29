using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger que, al detectar a un jugador dentro y al presionar su tecla de interacción
/// (se lee del componente Player/Player2), desactiva un Canvas (referencia) y activa
/// otro Canvas (referencia).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CanvasSwitchTrigger : MonoBehaviour
{
    [Header("Canvas")]
    [Tooltip("Canvas que se activará cuando el jugador presione su tecla de interacción dentro del trigger.")]
    [SerializeField] private GameObject canvasToActivate;

    [Tooltip("Canvas que se desactivará cuando el jugador presione su tecla de interacción dentro del trigger.")]
    [SerializeField] private GameObject canvasToDeactivate;

    [Header("Opciones")]
    [Tooltip("Si está activo, solo se ejecutará una vez y luego se deshabilitará este componente.")]
    [SerializeField] private bool activateOnlyOnce = false;

    // Jugadores dentro del trigger (guardamos el transform raíz del jugador)
    private readonly HashSet<Transform> playersInTrigger = new HashSet<Transform>();

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        // Asegurar que el collider sea trigger
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var root = GetPlayerRoot(other);
        if (root != null)
        {
            playersInTrigger.Add(root);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var root = GetPlayerRoot(other);
        if (root != null)
        {
            playersInTrigger.Remove(root);
        }
    }

    private void Update()
    {
        if (playersInTrigger.Count == 0) return;

        foreach (var root in playersInTrigger)
        {
            if (root == null) continue;

            // Determinar qué jugador es y obtener su tecla de interacción real
            var p1 = root.GetComponent<Player>();
            var p2 = root.GetComponent<Player2>();
            if (p1 == null && p2 == null) continue;

            var comp = (Component)p1 ?? (Component)p2;
            var key = GetInteractKeyFromComponent(comp, p1 != null ? KeyCode.E : KeyCode.P);

            if (Input.GetKeyDown(key))
            {
                // Desactivar el canvas referenciado
                if (canvasToDeactivate != null)
                {
                    canvasToDeactivate.SetActive(false);
                }

                // Activar el canvas de referencia
                if (canvasToActivate != null)
                {
                    canvasToActivate.SetActive(true);
                }

                if (activateOnlyOnce)
                {
                    // Opcional: deshabilitar para no repetir
                    enabled = false;
                }

                // Como ya procesamos el input para uno, salimos del foreach este frame
                break;
            }
        }
    }

    private static Transform GetPlayerRoot(Collider col)
    {
        if (col == null) return null;

        // Buscar componente Player o Player2 en la jerarquía
        var t = col.transform;
        var p1 = t.GetComponentInParent<Player>();
        if (p1 != null) return p1.transform;

        var p2 = t.GetComponentInParent<Player2>();
        if (p2 != null) return p2.transform;

        return null;
    }

    // Intenta leer por reflexión la KeyCode 'interactKey' del componente Player/Player2.
    // Si falla, devuelve el valor por defecto provisto.
    private static KeyCode GetInteractKeyFromComponent(Component comp, KeyCode defaultKey)
    {
        if (comp == null) return defaultKey;
        var t = comp.GetType();
        var f = t.GetField("interactKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(KeyCode))
        {
            try
            {
                return (KeyCode)f.GetValue(comp);
            }
            catch { /* ignorar y devolver default */ }
        }
        return defaultKey;
    }
}
