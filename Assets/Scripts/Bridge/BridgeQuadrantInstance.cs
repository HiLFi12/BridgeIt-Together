using UnityEngine;

/// <summary>
/// Vincula un objeto de cuadrante físico con su ScriptableObject correspondiente.
/// Añádelo al GameObject del cuadrante (etiquetado como "BridgeQuadrant").
/// </summary>
[DisallowMultipleComponent]
public class BridgeQuadrantInstance : MonoBehaviour, ITurnable
{
    [Tooltip("Referencia al ScriptableObject que representa el estado lógico de este cuadrante.")]
    public BridgeQuadrantSO quadrantSO;

    // ITurnable delega al SO para que HeatSphere pueda activar/desactivar el calor
    public bool isTurned => quadrantSO != null && quadrantSO.isTurned;
    public void TurnOn()
    {
        if (quadrantSO != null) quadrantSO.TurnOn();
    }
    public void TurnOff()
    {
        if (quadrantSO != null) quadrantSO.TurnOff();
    }
}
