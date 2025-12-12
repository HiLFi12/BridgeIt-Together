using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    public AudioSource music;
    public List<AudioClip> bgTracks;

    [Tooltip("Índice del track de BGM a reproducir automáticamente al iniciar (-1 para ninguno)")]
    [SerializeField] private int initialBgmIndex = -1;

    [Header("SFX")]
    public AudioSource sfx;
    public List<AudioClip> soundEffects;

    private SFXVolumeSettings sfxVolumeSettings;

    private void Awake()
    {
        if (music == null) music = GetComponent<AudioSource>();
        if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
        sfxVolumeSettings = GetComponent<SFXVolumeSettings>();
        ServiceLocator.Instance.SetService(this);
    }

    private void Start()
    {
        if (initialBgmIndex >= 0)
        {
            PlayBGM(initialBgmIndex);
        }
    }

    public void PlayBGM(int bgmIndex)
    {
        if (bgmIndex < 0 || bgmIndex >= bgTracks.Count) return;
        
        // Solo cambiar si no está reproduciendo el clip correcto
        if (music.clip != bgTracks[bgmIndex] || !music.isPlaying)
        {
            music.clip = bgTracks[bgmIndex];
            music.Play();
        }
    }

    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex < 0 || sfxIndex >= soundEffects.Count) return;
        var clip = soundEffects[sfxIndex];
        if (clip == null) return;
        float volume = 1f;
        if (sfxVolumeSettings != null)
        {
            volume = sfxVolumeSettings.GetVolumeForIndex(sfxIndex);
        }
        sfx.PlayOneShot(clip, volume);
    }

    public void PauseMusic()
    {
        if (music != null && music.isPlaying)
        {
            music.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (music != null && !music.isPlaying)
        {
            music.UnPause();
        }
    }
}
