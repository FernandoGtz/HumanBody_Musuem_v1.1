using UnityEngine;
using UnityEngine.Video;

public class MediaManager : MonoBehaviour
{
    public static MediaManager Instance;

    private AudioSource currentAudio;
    private GameObject currentAudioObject;

    private VideoPlayer currentVideo;
    private GameObject currentVideoObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -----------------------------
    // 🎧 CONTROL DE AUDIO
    // -----------------------------
    public void PlayAudio(AudioSource audioSource, GameObject audioObject)
    {
        if (audioSource == null) return;

        // Si hay un video reproduciéndose, lo detenemos
        if (currentVideo != null && currentVideo.isPlaying)
        {
            currentVideo.Stop();
            currentVideo = null;
            currentVideoObject = null;
        }

        // Si otro audio está sonando, detenerlo
        if (currentAudio != null && currentAudio != audioSource)
        {
            currentAudio.Stop();
        }

        currentAudio = audioSource;
        currentAudioObject = audioObject;

        if (!currentAudio.isPlaying)
        {
            currentAudio.Play();
        }
    }

    public void ToggleAudio()
    {
        if (currentAudio == null) return;

        if (currentAudio.isPlaying)
            currentAudio.Pause();
        else
            currentAudio.Play();
    }

    public void StopCurrentAudio()
    {
        if (currentAudio == null) return;

        currentAudio.Stop();
        currentAudio = null;
        currentAudioObject = null;
    }

    public bool IsAudioPlaying(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // -----------------------------
    // 🎬 CONTROL DE VIDEO
    // -----------------------------
    public void PlayVideo(VideoPlayer videoPlayer, GameObject videoObject)
    {
        if (videoPlayer == null) return;

        // Si hay un audio en reproducción, lo detenemos
        if (currentAudio != null && currentAudio.isPlaying)
        {
            currentAudio.Stop();
            currentAudio = null;
            currentAudioObject = null;
        }

        // Si hay otro video sonando, lo detenemos
        if (currentVideo != null && currentVideo != videoPlayer)
        {
            currentVideo.Stop();
        }

        currentVideo = videoPlayer;
        currentVideoObject = videoObject;

        if (!currentVideo.isPlaying)
        {
            currentVideo.Play();
        }
    }

    public void ToggleVideo()
    {
        if (currentVideo == null) return;

        if (currentVideo.isPlaying)
            currentVideo.Pause();
        else
            currentVideo.Play();
    }

    public void StopCurrentVideo()
    {
        if (currentVideo == null) return;

        currentVideo.Stop();
        currentVideo = null;
        currentVideoObject = null;
    }

    public bool IsVideoPlaying(VideoPlayer videoPlayer)
    {
        return videoPlayer != null && videoPlayer.isPlaying;
    }

    // -----------------------------
    // 🧹 MÉTODO GENERAL
    // -----------------------------
    public void StopAllMedia()
    {
        if (currentAudio != null)
        {
            currentAudio.Stop();
            currentAudio = null;
            currentAudioObject = null;
        }

        if (currentVideo != null)
        {
            currentVideo.Stop();
            currentVideo = null;
            currentVideoObject = null;
        }
    }
}
