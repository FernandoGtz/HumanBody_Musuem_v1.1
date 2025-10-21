using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource currentAudio;          // Audio que se está reproduciendo actualmente
    private GameObject currentAudioObject;     // Objeto asociado al audio actual

    private void Awake()
    {
        // Singleton para que siempre haya solo un AudioManager
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

    /// <summary>
    /// Reproduce un audio interactuable. Si otro audio está sonando, lo pausa y reinicia.
    /// </summary>
    /// <param name="audioSource">AudioSource del objeto interactuable</param>
    /// <param name="audioObject">Objeto que posee el AudioSource</param>
    public void PlayAudio(AudioSource audioSource, GameObject audioObject)
    {
        if (audioSource == null) return;

        // Si el audio que queremos reproducir es diferente del actual
        if (currentAudio != null && currentAudio != audioSource)
        {
            // Detener y reiniciar el audio anterior
            currentAudio.Stop();
        }

        currentAudio = audioSource;
        currentAudioObject = audioObject;

        // Reproducir el nuevo audio
        if (!currentAudio.isPlaying)
        {
            currentAudio.Play();
        }
    }

    /// <summary>
    /// Pausa o reanuda el audio actualmente seleccionado
    /// </summary>
    public void ToggleAudio()
    {
        if (currentAudio == null) return;

        if (currentAudio.isPlaying)
        {
            currentAudio.Pause();
        }
        else
        {
            currentAudio.Play();
        }
    }

    /// <summary>
    /// Detiene el audio actual
    /// </summary>
    public void StopCurrentAudio()
    {
        if (currentAudio == null) return;

        currentAudio.Stop();
        currentAudio = null;
        currentAudioObject = null;
    }

    /// <summary>
    /// Determina si un audio específico está reproduciéndose
    /// </summary>
    public bool IsAudioPlaying(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }
}
