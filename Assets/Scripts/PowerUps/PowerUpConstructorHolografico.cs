using System.Collections;
using UnityEngine;

public class PowerUpConstructorHolografico : PowerUpBase
{
    [Header("Referencias Específicas")]
    public GameObject[] batterySlots;
    private bool[] batteryInserted;
    public GameObject[] activationButtons;
    private bool[] buttonsPressed;
    public float batteryLife = 5f;
    private float[] batteryTimers;
    public float buttonHoldTime = 1f;
    private float[] buttonTimers;
    public float effectDuration = 15f;
    public TechUpgradeManager techUpgradeManager;

    private void Awake()
    {
        batteryInserted = new bool[batterySlots.Length];
        batteryTimers = new float[batterySlots.Length];
        buttonsPressed = new bool[activationButtons.Length];
        buttonTimers = new float[activationButtons.Length];
    }

    public void InsertBattery(int slotIndex)
    {
        batteryInserted[slotIndex] = true;
        batteryTimers[slotIndex] = 0f;
        // Feedback visual/sonoro de inserción
    }

    public void PressButton(int buttonIndex)
    {
        if (!batteryInserted[buttonIndex]) return;
        buttonsPressed[buttonIndex] = true;
    }

    public void ReleaseButton(int buttonIndex)
    {
        buttonsPressed[buttonIndex] = false;
        buttonTimers[buttonIndex] = 0f;
    }

    private void Update()
    {
        for (int i = 0; i < batteryInserted.Length; i++)
        {
            if (batteryInserted[i])
            {
                batteryTimers[i] += Time.deltaTime;
                if (batteryTimers[i] > batteryLife)
                {
                    batteryInserted[i] = false;
                    // Feedback visual/sonoro de batería agotada
                }
            }
        }
        for (int i = 0; i < buttonsPressed.Length; i++)
        {
            if (buttonsPressed[i] && batteryInserted[i])
            {
                buttonTimers[i] += Time.deltaTime;
            }
            else
            {
                buttonTimers[i] = 0f;
            }
        }
        bool allReady = true;
        for (int i = 0; i < buttonTimers.Length; i++)
        {
            if (!(buttonsPressed[i] && batteryInserted[i] && buttonTimers[i] >= buttonHoldTime))
            {
                allReady = false;
                break;
            }
        }
        if (allReady && !isActive)
        {
            TryActivate(null);
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Activar mejora tecnológica
        if (techUpgradeManager != null)
        {
            techUpgradeManager.ActivarMejoraFuturista();
        }
        // Feedback visual/sonoro de activación
        yield return new WaitForSeconds(effectDuration);
        if (techUpgradeManager != null)
        {
            techUpgradeManager.DesactivarMejoraFuturista();
        }
        Despawn();
    }
} 