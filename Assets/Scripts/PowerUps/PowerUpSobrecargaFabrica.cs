using System.Collections;
using UnityEngine;

public class PowerUpSobrecargaFabrica : PowerUpBase
{
    [Header("Referencias Específicas")]
    public GameObject[] activationButtons;
    private bool[] buttonsPressed;
    public float activationHoldTime = 5f;
    private float[] holdTimers;
    public float effectDuration = 15f;
    public ConveyorBelt[] conveyorBelts;
    public float speedMultiplier = 2f;

    private void Awake()
    {
        buttonsPressed = new bool[activationButtons.Length];
        holdTimers = new float[activationButtons.Length];
    }

    public void PressButton(int buttonIndex)
    {
        buttonsPressed[buttonIndex] = true;
    }

    public void ReleaseButton(int buttonIndex)
    {
        buttonsPressed[buttonIndex] = false;
        holdTimers[buttonIndex] = 0f;
    }

    private void Update()
    {
        for (int i = 0; i < buttonsPressed.Length; i++)
        {
            if (buttonsPressed[i])
            {
                holdTimers[i] += Time.deltaTime;
            }
            else
            {
                holdTimers[i] = 0f;
            }
        }
        bool allHeld = true;
        for (int i = 0; i < holdTimers.Length; i++)
        {
            if (holdTimers[i] < activationHoldTime)
            {
                allHeld = false;
                break;
            }
        }
        if (allHeld && !isActive)
        {
            TryActivate(null);
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Acelerar todas las cintas transportadoras
        foreach (var belt in conveyorBelts)
        {
            if (belt != null)
            {
                belt.SetSpeedMultiplier(speedMultiplier);
            }
        }
        // Feedback visual/sonoro de activación
        yield return new WaitForSeconds(effectDuration);
        foreach (var belt in conveyorBelts)
        {
            if (belt != null)
            {
                belt.SetSpeedMultiplier(1f);
            }
        }
        Despawn();
    }
} 