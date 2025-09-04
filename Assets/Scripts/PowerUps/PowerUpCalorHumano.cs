using System.Collections;
using UnityEngine;

public class PowerUpCalorHumano : PowerUpBase
{
    [Header("Referencias Específicas")]
    public int carbonesNecesarios = 3;
    private int carbonesActuales = 0;
    public GameObject[] activationButtons;
    private bool[] buttonsPressed;
    public float activationHoldTime = 2f;
    private float[] holdTimers;
    public float effectDuration = 20f;
    public MachineManager[] machinesInRange;

    private void Awake()
    {
        buttonsPressed = new bool[activationButtons.Length];
        holdTimers = new float[activationButtons.Length];
    }

    public void InsertarCarbon()
    {
        if (carbonesActuales < carbonesNecesarios)
        {
            carbonesActuales++;
            // Feedback visual/sonoro de carga
        }
    }

    public void PressButton(int buttonIndex)
    {
        if (carbonesActuales < carbonesNecesarios) return;
        buttonsPressed[buttonIndex] = true;
    }

    public void ReleaseButton(int buttonIndex)
    {
        buttonsPressed[buttonIndex] = false;
        holdTimers[buttonIndex] = 0f;
    }

    private void Update()
    {
        if (carbonesActuales < carbonesNecesarios) return;
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
        // Encender todas las máquinas en rango
        foreach (var machine in machinesInRange)
        {
            if (machine != null)
            {
                machine.Encender();
            }
        }
        // Feedback visual/sonoro de activación
        yield return new WaitForSeconds(effectDuration);
        foreach (var machine in machinesInRange)
        {
            if (machine != null)
            {
                machine.Apagar();
            }
        }
        Despawn();
    }
} 