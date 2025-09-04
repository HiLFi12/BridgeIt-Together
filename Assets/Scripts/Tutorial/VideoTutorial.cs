using UnityEngine;
using UnityEngine.Video;

public class VideoTutorial : MonoBehaviour
{
    private VideoPlayer video;

    void Awake()
    {
        video = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (video != null)
            video.Play();
    }

    void OnDisable()
    {
        if (video != null)
            video.Stop();
    }
}

