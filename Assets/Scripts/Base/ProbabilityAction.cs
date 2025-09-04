using UnityEngine;

/// <summary>
/// Clase base genérica para ejecutar una acción con una probabilidad.
/// Los objetos/vehículos pueden heredar de esta clase y sobreescribir Execute.
/// </summary>
[DisallowMultipleComponent]
public class ProbabilityAction : MonoBehaviour
{
    [Tooltip("Probabilidad de que la acción se ejecute (0..1)")]
    [Range(0f, 1f)]
    public float probability = 0.25f;

    /// <summary>
    /// Método que se debe sobreescribir en las subclases para ejecutar la acción deseada.
    /// Por defecto no hace nada.
    /// </summary>
    /// <param name="quadrantX">Coordenada X del cuadrante (puede ser -1 si no aplica)</param>
    /// <param name="quadrantZ">Coordenada Z del cuadrante (puede ser -1 si no aplica)</param>
    public virtual void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        // Implementado por subclases
    }

    /// <summary>
    /// Intenta ejecutar la acción basada en la probabilidad configurada.
    /// </summary>
    public void TryExecuteOnQuadrant(int x, int z)
    {
        if (RollChance()) Execute(x, z);
    }

    /// <summary>
    /// Intenta ejecutar la acción dado un punto en el mundo y una grilla de puente.
    /// Calcula el cuadrante y aplica la probabilidad.
    /// </summary>
    public void TryExecuteOnBridgePoint(Vector3 worldPoint, BridgeConstructionGrid grid)
    {
        if (grid == null) return;
        Vector3 localPos = worldPoint - grid.transform.position;
        int x = Mathf.FloorToInt(localPos.x / grid.quadrantSize);
        int z = Mathf.FloorToInt(localPos.z / grid.quadrantSize);

        if (x >= 0 && x < grid.gridWidth && z >= 0 && z < grid.gridLength)
        {
            if (RollChance()) Execute(x, z);
        }
    }

    /// <summary>
    /// Roll aleatorio simple.
    /// </summary>
    protected bool RollChance()
    {
        return Random.value <= probability;
    }
}
