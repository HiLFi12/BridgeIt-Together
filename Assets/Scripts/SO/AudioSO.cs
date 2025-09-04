using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioSO", menuName = "Audios/Audio", order = 1)]
public class AudioSO : ScriptableObject
{
    [Range(0, 1f)] public float volume = 1f;
    public AudioClip play;

    public void PlaySound(AudioSource audioSource)
    {
        audioSource.PlayOneShot(play, volume);
    }
}

