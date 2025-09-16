using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden encenderse/apagarse por una fuente de calor.
/// </summary>
public interface ITurnable
{
    bool IsTurn { get; }
    void TurnOn(GameObject source);
    void TurnOff(GameObject source);
}