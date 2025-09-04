using UnityEngine;

public class PowerUpButtonInteractable : MonoBehaviour, IInteractable
{
    public enum PowerUpButtonType { Industrial, Contemporary, Futuristic }
    public PowerUpButtonType buttonType;
    public PowerUpCalorHumano powerUpIndustrial;
    public PowerUpSobrecargaFabrica powerUpContemporary;
    public PowerUpConstructorHolografico powerUpFuturistic;
    public int buttonIndex;

    public InteractPriority InteractPriority => InteractPriority.High;

    public void Interact(GameObject interactor)
    {
        switch (buttonType)
        {
            case PowerUpButtonType.Industrial:
                powerUpIndustrial.PressButton(buttonIndex);
                break;
            case PowerUpButtonType.Contemporary:
                powerUpContemporary.PressButton(buttonIndex);
                break;
            case PowerUpButtonType.Futuristic:
                powerUpFuturistic.PressButton(buttonIndex);
                break;
        }
        // Feedback visual/sonoro de presionar
    }

    private void OnTriggerExit(Collider other)
    {
        // Suelta el botón cuando el jugador se aleja
        switch (buttonType)
        {
            case PowerUpButtonType.Industrial:
                powerUpIndustrial.ReleaseButton(buttonIndex);
                break;
            case PowerUpButtonType.Contemporary:
                powerUpContemporary.ReleaseButton(buttonIndex);
                break;
            case PowerUpButtonType.Futuristic:
                powerUpFuturistic.ReleaseButton(buttonIndex);
                break;
        }
        // Feedback visual/sonoro de soltar
    }
} 