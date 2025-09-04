using UnityEngine;

namespace BridgeItTogether.Gameplay.Carriles
{
    /// <summary>
    /// Tipos de configuración de carril para el spawner
    /// </summary>
    public enum TipoCarril
    {
        DobleCarril,
        SoloIzquierda,
        SoloDerecha
    }

    /// <summary>
    /// Posición del carril para carriles de una sola dirección
    /// </summary>
    public enum PosicionCarril
    {
        Inferior,
        Superior,
        Random
    }
}
