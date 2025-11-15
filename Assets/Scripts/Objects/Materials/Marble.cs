using UnityEngine;

/// <summary>
/// Marble: material que se activa con calor (HeatSphere) en lugar de flechas.
/// Hereda de MaterialTipo2Ready para reutilizar el manejo de mallas y estado listo.
/// Implementa ITurnable para ser controlado por HeatSphere (On/Off al entrar/salir del radio).
/// </summary>
public class Marble : MaterialTipo2Ready, ITurnable
{
    [Header("Marble Settings")]
    [SerializeField, Tooltip("Si está activo, al salir del área de calor vuelve al estado 'no listo'.")]
    private bool revertWhenOutOfHeat = true;

    [Header("Audio - Cambio de Estado (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando Marble cambia de 'no listo' a 'ready'. -1 desactiva.")]
    [SerializeField] private int readySfxIndex = -1;

    public bool isTurned => isReady;

    protected override void Awake()
    {
        base.Awake();
        // Para mármol, por defecto queremos que solo el calor lo active, no flechas.
        // El Awake base ya hace auto-vinculación y aplica estado visual.
    }

    // Ocultamos la activación por flecha del padre para que solo el calor afecte.
    private new void OnCollisionEnter(Collision collision)
    {
        // Intencionalmente vacío: Marble no se activa con flechas.
    }

    // ITurnable: llamado por HeatSphere cuando entra en el radio de calor
    public void TurnOn()
    {
        if (isReady) return;
        isReady = true;
        AplicarEstadoVisual();
        PlayReadySfx();
    }

    // ITurnable: llamado por HeatSphere cuando sale del radio de calor
    public void TurnOff()
    {
        if (!revertWhenOutOfHeat) return;
        if (!isReady) return;
        isReady = false;
        AplicarEstadoVisual();
    }

    private void PlayReadySfx()
    {
        if (readySfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(readySfxIndex);
        }
    }
}
