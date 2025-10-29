using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    public AudioSource music;
    public List<AudioClip> bgTracks;

    [Header("SFX")]
    public AudioSource sfx;
    public List<AudioClip> soundEffects;

    private void Awake()
    {
        if (music == null) music = GetComponent<AudioSource>();
        if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
        ServiceLocator.Instance.SetService(this);
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
        sfx.PlayOneShot(clip);
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
