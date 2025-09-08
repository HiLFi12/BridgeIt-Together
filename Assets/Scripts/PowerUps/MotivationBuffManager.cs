using UnityEngine;

/// <summary>
/// Gestor estático simple del buff de "motivación".
/// Mientras está activo, los jugadores construyen una columna completa (todas las capas) con un solo material.
/// </summary>
public static class MotivationBuffManager
{
    private static float endTime = -1f;

    /// <summary>Activo si el tiempo actual es menor que endTime.</summary>
    public static bool Active => Time.time < endTime;

    /// <summary>Activa / refresca el buff por la duración indicada.</summary>
    public static void Activate(float durationSeconds)
    {
        if (durationSeconds <= 0f) return;
        endTime = Mathf.Max(endTime, Time.time) + durationSeconds; // acumulativo simple
    }

    /// <summary>Tiempo restante aproximado.</summary>
    public static float Remaining => Mathf.Max(0f, endTime - Time.time);
}