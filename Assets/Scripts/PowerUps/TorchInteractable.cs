using UnityEngine;

/// <summary>
/// Componente que maneja la interacción con las antorchas del tótem ritual.
/// Requiere que el jugador tenga un PaloIgnifugo encendido para activar la antorcha.
/// Una vez activada, la antorcha del jugador se destruye y la antorcha del tótem se enciende.
/// </summary>
public class TorchInteractable : MonoBehaviour, IInteractable
{
    public enum TorchSide { Left, Right }
    
    [SerializeField] private TorchSide torchSide;
    [SerializeField] private PowerUpRitualGranFuego ritualPowerUp;
    
    public InteractPriority InteractPriority => InteractPriority.High;

    /// <summary>
    /// Configura la antorcha con su lado y referencia al power up
    /// </summary>
    /// <param name="side">Lado de la antorcha (izquierda o derecha)</param>
    /// <param name="powerUp">Referencia al power up ritual</param>
    public void SetupTorch(TorchSide side, PowerUpRitualGranFuego powerUp)
    {
        torchSide = side;
        ritualPowerUp = powerUp;
    }

    public void Interact(GameObject interactor)
    {
        if (ritualPowerUp == null)
        {
            Debug.LogWarning("TorchInteractable: No hay referencia al PowerUpRitualGranFuego");
            return;
        }

        // Verificar que el jugador tenga un PlayerObjectHolder
        PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
        if (playerObjectHolder == null || !playerObjectHolder.HasObjectInHand())
        {
            Debug.Log("Necesitas un palo ignífugo encendido para encender la antorcha del tótem.");
            return;
        }

        // Verificar que tenga un PaloIgnifugo encendido
        GameObject heldObject = playerObjectHolder.GetHeldObject();
        PaloIgnifugo paloIgnifugo = heldObject?.GetComponent<PaloIgnifugo>();
        
        if (paloIgnifugo == null)
        {
            Debug.Log("Necesitas un palo ignífugo para encender la antorcha.");
            return;
        }

        if (!paloIgnifugo.EstaEncendido())
        {
            Debug.Log("El palo ignífugo debe estar encendido para poder encender la antorcha del tótem.");
            return;
        }

        // El jugador tiene un palo ignífugo encendido, proceder con el ritual
        // Destruir la antorcha del jugador (como indica el spec)
        playerObjectHolder.UseHeldObject();

        // Encender la antorcha correspondiente del tótem
        if (torchSide == TorchSide.Left)
        {
            ritualPowerUp.LightLeftTorch();
            Debug.Log("¡Antorcha izquierda del tótem encendida!");
        }
        else
        {
            ritualPowerUp.LightRightTorch();
            Debug.Log("¡Antorcha derecha del tótem encendida!");
        }

        // Aquí se pueden añadir efectos visuales y sonoros adicionales
        // TODO: Añadir feedback audiovisual
    }
} 