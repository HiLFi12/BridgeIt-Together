using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuración de volumen para SFX por índice.
/// Se espera que esté en el mismo GameObject que el AudioManager.
/// </summary>
[DisallowMultipleComponent]
public class SFXVolumeSettings : MonoBehaviour
{
    [Header("Volumen global SFX")]
    [Range(0f, 2f)]
    public float masterSfxVolume = 1f;

    [Header("Volumen por SFX (factor 0-1 por índice)")]
    [Tooltip("Factor de volumen por índice en AudioManager.soundEffects (1 = volumen completo).")]
    public List<float> perSfxVolume = new List<float>();

    /// <summary>
    /// Devuelve el volumen efectivo para un índice de SFX concreto (0-1).
    /// </summary>
    public float GetVolumeForIndex(int index)
    {
        if (index < 0)
            return 0f;

        float factor = 1f;
        if (index < perSfxVolume.Count)
        {
            factor = Mathf.Clamp(perSfxVolume[index], 0f, 2f);
        }

        float master = Mathf.Clamp(masterSfxVolume, 0f, 2f);
        // Permitimos hasta 2x del volumen original (puede saturar si el clip ya viene alto)
        return Mathf.Clamp(master * factor, 0f, 2f);
    }
}
