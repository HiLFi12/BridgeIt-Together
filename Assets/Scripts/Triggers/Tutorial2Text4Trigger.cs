using UnityEngine;

/// <summary>
/// Al entrar un jugador (Player o Player2) en este trigger, se apagan Text1/2/3 y se enciende solo Text4.
/// Solo se ejecuta si Text3 está activo. Opcionalmente deshabilita los UITextSwitchTrigger para impedir
/// que Text1/2/3 vuelvan a activarse mientras Text4 esté activo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Tutorial2Text4Trigger : MonoBehaviour
{
    [Header("UI Texts")]
    [SerializeField] private GameObject text1;
    [SerializeField] private GameObject text2;
    [SerializeField] private GameObject text3;
    [SerializeField] private GameObject text4;

    [Header("Options")]
    [Tooltip("Si está activo, solo ejecuta una vez y luego se deshabilita.")]
    [SerializeField] private bool onlyOnce = true;

    [Header("Lock controllers while Text4 is active")] 
    [Tooltip("Deshabilitar los UITextSwitchTrigger para que no vuelvan a encender Text1/2/3 mientras Text4 esté activo.")]
    [SerializeField] private bool disableTextSwitchTriggersOnActivate = true;

    [Tooltip("Triggers a deshabilitar. Si se deja vacío y 'disableTextSwitchTriggersOnActivate' está activo, se buscarán en escena.")]
    [SerializeField] private UITextSwitchTrigger[] textSwitchTriggers;

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
        if (triggered && onlyOnce) return;
        if (!IsPlayer(other)) return;

        // Solo actuar si Text3 está activo
        if (text3 == null || !text3.activeInHierarchy) return;

        ApplyText4State();
        if (onlyOnce) { triggered = true; enabled = false; }
    }

    private void ApplyText4State()
    {
        if (text1) text1.SetActive(false);
        if (text2) text2.SetActive(false);
        if (text3) text3.SetActive(false);
        if (text4) text4.SetActive(true);

        if (disableTextSwitchTriggersOnActivate)
        {
            if (textSwitchTriggers == null || textSwitchTriggers.Length == 0)
            {
                textSwitchTriggers = FindObjectsOfType<UITextSwitchTrigger>(true);
            }
            foreach (var trig in textSwitchTriggers)
            {
                if (trig != null) trig.enabled = false;
            }
        }
    }

    private static bool IsPlayer(Collider col)
    {
        if (!col) return false;
        return col.GetComponentInParent<Player>() != null || col.GetComponentInParent<Player2>() != null;
    }
}
