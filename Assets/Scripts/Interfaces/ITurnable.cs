using UnityEngine;

public interface ITurnable
{
    bool isTurned { get; }
    void TurnOn();
    void TurnOff();
}