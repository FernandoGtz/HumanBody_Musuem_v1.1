using UnityEngine;
using System.Collections;

public class InteractionAudioTrigger : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pressFCanvas;      // Canvas que muestra "Presiona F para escuchar"
    public GameObject exitCanvas;        // Canvas que muestra "Presiona F para pausar audio"

    [Header("Audio")]
    public AudioSource audioSource;      // Audio a reproducir

    [Header("Ajustes de interacción")]
    [Tooltip("Tiempo de retardo al cambiar de canvas (por estética visual)")]
    public float canvasSwitchDelay = 0.2f;  // retardo al cambiar de canvas

    private bool isPlayerInside = false;
    private bool audioStarted = false;

    void Start()
    {
        // Asegurarnos de que ambos canvas estén ocultos al inicio
        if (pressFCanvas != null) pressFCanvas.SetActive(false);
        if (exitCanvas != null) exitCanvas.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            if (!audioStarted)
            {
                StartCoroutine(StartAudioInteraction());
            }
            else
            {
                ToggleAudio();
            }
        }
    }

    private IEnumerator StartAudioInteraction()
    {
        audioStarted = true;

        // Ocultar el primer canvas con un pequeño retardo antes de mostrar el segundo
        if (pressFCanvas != null) pressFCanvas.SetActive(false);
        yield return new WaitForSeconds(canvasSwitchDelay);
        if (exitCanvas != null) exitCanvas.SetActive(true);

        // Reproducir audio desde el inicio
        if (audioSource != null)
        {
            audioSource.time = 0f;
            audioSource.Play();
        }
    }

    private void ToggleAudio()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.Play();
        }
    }

    private void StopAndResetAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }

        audioStarted = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (pressFCanvas != null) pressFCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Ocultar ambos Canvas
            if (pressFCanvas != null) pressFCanvas.SetActive(false);
            if (exitCanvas != null) exitCanvas.SetActive(false);

            // Reiniciar audio si estaba reproduciéndose
            StopAndResetAudio();
        }
    }
}
