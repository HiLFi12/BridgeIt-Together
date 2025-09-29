using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cambia entre textos según el estado del jugador respecto a un trigger:
/// - Fuera del trigger: Text1 activo (Text2 y Text3 inactivos)
/// - Dentro del trigger: Text2 activo (Text1 y Text3 inactivos)
/// - Si estando dentro presiona su tecla de interacción (E o P): Text3 activo y se ocultan Text1 y Text2 (opcionalmente finaliza).
/// </summary>
[RequireComponent(typeof(Collider))]
public class UITextSwitchTrigger : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject text1;
    [SerializeField] private GameObject text2;
    [SerializeField] private GameObject text3;

    [Header("Options")]
    [Tooltip("Forzar estado inicial al iniciar: Text1 ON, Text2 OFF, Text3 OFF")] 
    [SerializeField] private bool setInitialStateOnStart = true;

    [Tooltip("Si está activo, al activar Text3 no se volverá a cambiar el estado (queda finalizado).")]
    [SerializeField] private bool finalizeOnInteract = true;

    [Tooltip("Si está activo, al finalizar deshabilita este componente.")]
    [SerializeField] private bool disableComponentAfterFinalize = true;

    [Tooltip("Evita activar Text1 si ninguno de los 3 textos está activo actualmente.")]
    [SerializeField] private bool preventActivatingText1WhenAllOff = true;

    private readonly HashSet<Transform> playersInTrigger = new HashSet<Transform>();
    private bool finalized;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    private void Start()
    {
        if (setInitialStateOnStart)
        {
            // Si se solicita prevenir activación cuando todos están OFF, no forzar Text1
            if (!(preventActivatingText1WhenAllOff && !AnyTextActive()))
            {
                SetState_Text1();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finalized) return;
        var root = GetPlayerRoot(other);
        if (root == null) return;

        // Solo ejecutar si Text1 (el que se va a desactivar) está activo
        if (!IsText1Active()) return;

        playersInTrigger.Add(root);
        // Al menos un jugador dentro -> mostrar Text2
        SetState_Text2();
    }

    private void OnTriggerExit(Collider other)
    {
        if (finalized) return;
        var root = GetPlayerRoot(other);
        if (root == null) return;

        playersInTrigger.Remove(root);
        if (playersInTrigger.Count == 0)
        {
            // Ningún jugador dentro -> volver a Text1 solo si no está el caso "todos OFF" bloqueado
            if (!(preventActivatingText1WhenAllOff && !AnyTextActive()))
            {
                SetState_Text1();
            }
        }
    }

    private void Update()
    {
        if (finalized) return;
        if (playersInTrigger.Count == 0) return;

        // Si cualquier jugador dentro presiona su tecla de interactuar, pasar a Text3 y finalizar
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
                SetState_Text3();
                if (finalizeOnInteract)
                {
                    finalized = true;
                    if (disableComponentAfterFinalize) enabled = false;
                }
                break;
            }
        }
    }

    // ----- State helpers -----
    private void SetState_Text1()
    {
        if (text1 != null) text1.SetActive(true);
        if (text2 != null) text2.SetActive(false);
        if (text3 != null) text3.SetActive(false);
    }
    private void SetState_Text2()
    {
        if (text1 != null) text1.SetActive(false);
        if (text2 != null) text2.SetActive(true);
        if (text3 != null) text3.SetActive(false);
    }
    private void SetState_Text3()
    {
        if (text1 != null) text1.SetActive(false);
        if (text2 != null) text2.SetActive(false);
        if (text3 != null) text3.SetActive(true);
    }

    // ----- Utilities -----
    private static Transform GetPlayerRoot(Collider col)
    {
        if (col == null) return null;
        var p1 = col.GetComponentInParent<Player>();
        if (p1 != null) return p1.transform;
        var p2 = col.GetComponentInParent<Player2>();
        if (p2 != null) return p2.transform;
        return null;
    }

    private static KeyCode GetInteractKeyFromComponent(Component comp, KeyCode fallback)
    {
        if (comp == null) return fallback;
        var t = comp.GetType();
        var f = t.GetField("interactKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(KeyCode))
        {
            try { return (KeyCode)f.GetValue(comp); } catch { }
        }
        return fallback;
    }

    private bool IsText1Active()
    {
        return text1 != null && text1.activeInHierarchy;
    }

    private bool AnyTextActive()
    {
        return (text1 != null && text1.activeInHierarchy) ||
               (text2 != null && text2.activeInHierarchy) ||
               (text3 != null && text3.activeInHierarchy);
    }
}
