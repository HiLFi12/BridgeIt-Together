using UnityEngine;

public class MaterialTipo2HeatActivated : MaterialTipo2Ready, ITurnable
{
    private bool _isTurned = false;

    public bool isTurned => _isTurned;

    protected override void Awake()
    {
        base.Awake();
        isReady = false;
        AplicarEstadoVisual();
    }

    public void TurnOn()
    {
        if (isReady) return;

        _isTurned = true;
        isReady = true;
        AplicarEstadoVisual();
    }

    public void TurnOff()
    {
        _isTurned = false;
    }
    
}