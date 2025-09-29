using UnityEngine;

/// <summary>
/// Tutorial 3 Manager:
/// Detecta cuando cualquiera de los jugadores adquiere "material 2" (BridgeMaterialLayer1 -> layerIndex == 1)
/// y con ello apaga Text1 y enciende Text2.
/// 
/// Preferencia: escuchar evento del PlayerObjectHolder (si está disponible). Si no, hace polling.
/// </summary>
[DisallowMultipleComponent]
public class Tutorial3Manager : MonoBehaviour
{
    [Header("Referencias de Jugadores")]
    [SerializeField] private PlayerObjectHolder player1Holder;
    [SerializeField] private PlayerObjectHolder player2Holder;

    [Header("UI Textos Tutorial 3")]
    [SerializeField] private GameObject text1; // a desactivar
    [SerializeField] private GameObject text2; // a activar

    [Header("Canvas Opcional")]
    [Tooltip("Canvas a activar cuando se detecte el material 2.")]
    [SerializeField] private GameObject canvasToActivate;
    [Tooltip("Canvas a desactivar al activar el anterior (solo si está ACTIVO).")]
    [SerializeField] private GameObject canvasToDeactivate;
    [Tooltip("Si está activo, solo intentará cambiar los canvas una vez.")]
    [SerializeField] private bool activateCanvasOnlyOnce = true;

    [Header("Opciones")]
    [Tooltip("Si está activo, al completar el cambio de textos se deshabilitará este manager.")]
    [SerializeField] private bool oneShot = true;

    [Tooltip("Si no hay eventos disponibles, hacer polling del holder cada frame.")]
    [SerializeField] private bool enablePollingFallback = true;

    private bool switched;
    private bool canvasSwitched;

    private void Reset()
    {
        // Intentar autollenado de referencias comunes
        if (player1Holder == null)
        {
            var p1 = FindFirstObjectByType<Player>();
            if (p1 != null) player1Holder = p1.GetComponent<PlayerObjectHolder>();
        }
        if (player2Holder == null)
        {
            var p2 = FindFirstObjectByType<Player2>();
            if (p2 != null) player2Holder = p2.GetComponent<PlayerObjectHolder>();
        }
    }

    private void OnEnable()
    {
        TrySubscribe(player1Holder);
        TrySubscribe(player2Holder);
    }

    private void OnDisable()
    {
        TryUnsubscribe(player1Holder);
        TryUnsubscribe(player2Holder);
    }

    private void Update()
    {
        if (switched) return;
        if (!enablePollingFallback) return;

        // Fallback por polling: revisar si alguno sostiene layerIndex == 1
        if (IsHoldingLayer1(player1Holder) || IsHoldingLayer1(player2Holder))
        {
            ApplyTextSwitch();
        }
    }

    private void TrySubscribe(PlayerObjectHolder holder)
    {
        if (holder == null) return;
        // Usar reflexión para compatibilidad si el evento no existía antes
        var evtInfo = typeof(PlayerObjectHolder).GetEvent("OnPickedUp");
        if (evtInfo != null)
        {
            System.Action<GameObject> handler = OnHolderPickedUp;
            evtInfo.AddEventHandler(holder, handler);
        }
    }

    private void TryUnsubscribe(PlayerObjectHolder holder)
    {
        if (holder == null) return;
        var evtInfo = typeof(PlayerObjectHolder).GetEvent("OnPickedUp");
        if (evtInfo != null)
        {
            System.Action<GameObject> handler = OnHolderPickedUp;
            evtInfo.RemoveEventHandler(holder, handler);
        }
    }

    private void OnHolderPickedUp(GameObject picked)
    {
        if (switched || picked == null) return;
        var info = picked.GetComponent<BridgeMaterialInfo>();
        if (info != null && info.layerIndex == 1)
        {
            ApplyTextSwitch();
        }
    }

    private bool IsHoldingLayer1(PlayerObjectHolder holder)
    {
        if (holder == null || !holder.HasObjectInHand()) return false;
        var obj = holder.GetHeldObject();
        if (obj == null) return false;
        var info = obj.GetComponent<BridgeMaterialInfo>();
        return info != null && info.layerIndex == 1;
    }

    private void ApplyTextSwitch()
    {
        if (switched) return;
        switched = true;
        if (text1) text1.SetActive(false);
        if (text2) text2.SetActive(true);

        // Activación opcional de Canvas con guardas
        SwitchCanvasIfNeeded();

        if (oneShot)
        {
            enabled = false;
        }
    }

    private void SwitchCanvasIfNeeded()
    {
        if (canvasSwitched && activateCanvasOnlyOnce) return;

        // Guard: solo ejecutar si el Canvas a desactivar está activo (si existe)
        if (canvasToDeactivate != null && !canvasToDeactivate.activeInHierarchy)
        {
            return;
        }

        bool didSomething = false;

        if (canvasToDeactivate != null && canvasToDeactivate.activeInHierarchy)
        {
            canvasToDeactivate.SetActive(false);
            didSomething = true;
        }

        if (canvasToActivate != null && !canvasToActivate.activeInHierarchy)
        {
            canvasToActivate.SetActive(true);
            didSomething = true;
        }

        if (didSomething && activateCanvasOnlyOnce)
        {
            canvasSwitched = true;
        }
    }
}
