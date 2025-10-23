using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[DisallowMultipleComponent]
public class Level0IndexManager : MonoBehaviour
{
    // Enum local para botones de gamepad
    private enum GamepadButton
    {
        South, East, West, North,
        Start, Select,
        LeftShoulder, RightShoulder,
        LeftStick, RightStick,
        DpadUp, DpadDown, DpadLeft, DpadRight
    }

    [System.Serializable]
    private struct Step
    {
        [Header("UI del Paso")]
        [Tooltip("Objetos de UI a encender en este índice.")]
        public GameObject[] uiRoots;

        [Tooltip("Textos TMP a encender en este índice.")]
        public TMP_Text[] texts;

        [Header("Inputs del Paso")]
        [Tooltip("Teclas de teclado que avanzan desde este índice (cualquiera).")]
        public Key[] keyboardKeys;

        [Tooltip("Botones de gamepad que avanzan desde este índice (cualquiera, en el gamepad del Player).")]
        public GamepadButton[] gamepadButtons;

        [Header("Detección de Sticks (opcional)")]
        [Tooltip("Si está activo, avanzar cuando se mueva el stick izquierdo por encima del umbral (flanco ascendente).")]
        public bool detectLeftStickMove;
        [Tooltip("Si está activo, avanzar cuando se mueva el stick derecho por encima del umbral (flanco ascendente).")]
        public bool detectRightStickMove;
        [Range(0.1f, 1f)]
        [Tooltip("Umbral de magnitud para considerar que el stick se movió.")]
        public float stickThreshold;
    }

    [Header("Pasos (UI + Teclas/Botones)")]
    [SerializeField] private Step[] steps;

    [Header("Referencia de Player (por jugador)")]
    [Tooltip("PlayerInput del jugador dueño de este manager. Si está vacío, se buscará en este GameObject o padres.")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Estado")]
    [Min(0)]
    [SerializeField] private int startIndex = 0;
    [SerializeField] private bool debugLogs = false;

    public int CurrentIndex => _currentIndex;

    private int _currentIndex = -1;
    private bool _initialized;
    // Estado previo de magnitud para detectar flanco ascendente en sticks
    private float _prevLeftStickMag = 0f;
    private float _prevRightStickMag = 0f;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>() ?? GetComponentInParent<PlayerInput>();
    }

    private void OnEnable() => Initialize();
    private void Start() => Initialize();

    private void Update()
    {
        if (_currentIndex < 0 || steps == null || _currentIndex >= steps.Length) return;

        if (WasStepTriggeredThisFrame(steps[_currentIndex]))
            AvanzarAlSiguientePaso();
    }

    private void OnValidate()
    {
        if (steps == null) return;
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, steps.Length - 1));
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        HideAllUIAndTexts();

        if (steps == null || steps.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[Level0IndexManager] No hay pasos configurados.", this);
            return;
        }

        SetIndex(startIndex);
    }

    private void HideAllUIAndTexts()
    {
        if (steps == null) return;
        for (int i = 0; i < steps.Length; i++)
        {
            ToggleUIArray(steps[i].uiRoots, false);
            ToggleTextArray(steps[i].texts, false);
        }
    }

    private void SetIndex(int index)
    {
        if (steps == null || steps.Length == 0) return;
        if (index < 0 || index >= steps.Length) return;

        HideAllUIAndTexts();
        _currentIndex = index;

        if (debugLogs) Debug.Log($"[Level0IndexManager] Índice actual = {_currentIndex}", this);

        var step = steps[_currentIndex];
        ToggleUIArray(step.uiRoots, true);
        ToggleTextArray(step.texts, true);

        // Resetear magnitudes previas de sticks al valor actual para evitar disparo inmediato
        ResetStickMagnitudesToCurrent();
    }

    private bool WasStepTriggeredThisFrame(Step step)
    {
        // Preferir dispositivos del PlayerInput
        if (playerInput != null && playerInput.devices.Count > 0)
        {
            float currLeftMax = 0f, currRightMax = 0f;

            for (int d = 0; d < playerInput.devices.Count; d++)
            {
                var dev = playerInput.devices[d];

                // Teclado del jugador
                if (dev is Keyboard kb && step.keyboardKeys != null)
                {
                    for (int i = 0; i < step.keyboardKeys.Length; i++)
                        if (kb[step.keyboardKeys[i]].wasPressedThisFrame) return true;
                }

                // Gamepad del jugador
                if (dev is Gamepad pad)
                {
                    // Botones
                    if (step.gamepadButtons != null)
                    {
                        for (int i = 0; i < step.gamepadButtons.Length; i++)
                            if (GamepadButtonWasPressedThisFrame(pad, step.gamepadButtons[i])) return true;
                    }
                    // Sticks (acumular máximos entre pads asignados)
                    var l = pad.leftStick.ReadValue();   currLeftMax = Mathf.Max(currLeftMax, l.magnitude);
                    var r = pad.rightStick.ReadValue();  currRightMax = Mathf.Max(currRightMax, r.magnitude);
                }
            }

            // Flanco ascendente de sticks
            float thr = step.stickThreshold > 0f ? step.stickThreshold : 0.4f;
            bool leftRise  = step.detectLeftStickMove  && (_prevLeftStickMag  < thr && currLeftMax  >= thr);
            bool rightRise = step.detectRightStickMove && (_prevRightStickMag < thr && currRightMax >= thr);

            // Actualizar previos
            _prevLeftStickMag = currLeftMax;
            _prevRightStickMag = currRightMax;

            return leftRise || rightRise;
        }

        // Fallback global (Editor)
        if (step.keyboardKeys != null && Keyboard.current != null)
        {
            for (int i = 0; i < step.keyboardKeys.Length; i++)
            {
                var key = step.keyboardKeys[i];
                if (Keyboard.current[key].wasPressedThisFrame) return true;
            }
        }

        if (step.gamepadButtons != null && Gamepad.all.Count > 0)
        {
            for (int p = 0; p < Gamepad.all.Count; p++)
            {
                var pad = Gamepad.all[p];
                for (int i = 0; i < step.gamepadButtons.Length; i++)
                {
                    if (GamepadButtonWasPressedThisFrame(pad, step.gamepadButtons[i])) return true;
                }
            }
        }

        // Fallback: sticks globales
        if (Gamepad.all.Count > 0 && (step.detectLeftStickMove || step.detectRightStickMove))
        {
            float currLeftMax = 0f, currRightMax = 0f;
            for (int p = 0; p < Gamepad.all.Count; p++)
            {
                var pad = Gamepad.all[p];
                currLeftMax  = Mathf.Max(currLeftMax,  pad.leftStick.ReadValue().magnitude);
                currRightMax = Mathf.Max(currRightMax, pad.rightStick.ReadValue().magnitude);
            }

            float thr = step.stickThreshold > 0f ? step.stickThreshold : 0.4f;
            bool leftRise  = step.detectLeftStickMove  && (_prevLeftStickMag  < thr && currLeftMax  >= thr);
            bool rightRise = step.detectRightStickMove && (_prevRightStickMag < thr && currRightMax >= thr);

            _prevLeftStickMag = currLeftMax;
            _prevRightStickMag = currRightMax;

            if (leftRise || rightRise) return true;
        }

        return false;
    }

    private static bool GamepadButtonWasPressedThisFrame(Gamepad pad, GamepadButton btn)
    {
        if (pad == null) return false;

        switch (btn)
        {
            case GamepadButton.South:         return pad.buttonSouth.wasPressedThisFrame;   // A / Cross
            case GamepadButton.East:          return pad.buttonEast.wasPressedThisFrame;    // B / Circle
            case GamepadButton.West:          return pad.buttonWest.wasPressedThisFrame;    // X / Square
            case GamepadButton.North:         return pad.buttonNorth.wasPressedThisFrame;   // Y / Triangle
            case GamepadButton.Start:         return pad.startButton.wasPressedThisFrame;
            case GamepadButton.Select:        return pad.selectButton.wasPressedThisFrame;
            case GamepadButton.LeftShoulder:  return pad.leftShoulder.wasPressedThisFrame;
            case GamepadButton.RightShoulder: return pad.rightShoulder.wasPressedThisFrame;
            case GamepadButton.LeftStick:     return pad.leftStickButton.wasPressedThisFrame;
            case GamepadButton.RightStick:    return pad.rightStickButton.wasPressedThisFrame;
            case GamepadButton.DpadUp:        return pad.dpad.up.wasPressedThisFrame;
            case GamepadButton.DpadDown:      return pad.dpad.down.wasPressedThisFrame;
            case GamepadButton.DpadLeft:      return pad.dpad.left.wasPressedThisFrame;
            case GamepadButton.DpadRight:     return pad.dpad.right.wasPressedThisFrame;
            default: return false;
        }
    }

    private static void ToggleUIArray(GameObject[] arr, bool on)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != null) arr[i].SetActive(on);
        }
    }

    private static void ToggleTextArray(TMP_Text[] arr, bool on)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != null) arr[i].gameObject.SetActive(on);
        }
    }

    // API pública

    public void AvanzarAlSiguientePaso()
    {
        int next = _currentIndex + 1;
        if (steps == null || next >= steps.Length)
        {
            if (debugLogs) Debug.Log("[Level0IndexManager] Último paso completado.", this);
            HideAllUIAndTexts();
            _currentIndex = -1;
            return;
        }

        SetIndex(next);
    }

    public void CumplirInputActual() => AvanzarAlSiguientePaso();

    public void ForzarIndice(int index) => SetIndex(index);

    private void ResetStickMagnitudesToCurrent()
    {
        float currLeftMax = 0f, currRightMax = 0f;

        if (playerInput != null && playerInput.devices.Count > 0)
        {
            for (int d = 0; d < playerInput.devices.Count; d++)
            {
                if (playerInput.devices[d] is Gamepad pad)
                {
                    currLeftMax  = Mathf.Max(currLeftMax,  pad.leftStick.ReadValue().magnitude);
                    currRightMax = Mathf.Max(currRightMax, pad.rightStick.ReadValue().magnitude);
                }
            }
        }
        else
        {
            for (int p = 0; p < Gamepad.all.Count; p++)
            {
                var pad = Gamepad.all[p];
                currLeftMax  = Mathf.Max(currLeftMax,  pad.leftStick.ReadValue().magnitude);
                currRightMax = Mathf.Max(currRightMax, pad.rightStick.ReadValue().magnitude);
            }
        }

        _prevLeftStickMag = currLeftMax;
        _prevRightStickMag = currRightMax;
    }
}