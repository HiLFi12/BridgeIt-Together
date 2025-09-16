using UnityEngine;

/// <summary>
/// Vincula un objeto de cuadrante físico con su ScriptableObject correspondiente.
/// Añádelo al GameObject del cuadrante (etiquetado como "BridgeQuadrant").
/// </summary>
[DisallowMultipleComponent]
public class BridgeQuadrantInstance : MonoBehaviour
{
    [Tooltip("Referencia al ScriptableObject que representa el estado lógico de este cuadrante.")]
    public BridgeQuadrantSO quadrantSO;
}
