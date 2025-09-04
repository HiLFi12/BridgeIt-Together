using UnityEngine;

/// <summary>
/// Contrato para objetos que requieren mantener presionado el botón de interacción.
/// El InteractionManager llamará a DetenerInteraccion cuando el jugador suelte la tecla.
/// </summary>
public interface IHoldInteractable
{
    void DetenerInteraccion();
}
