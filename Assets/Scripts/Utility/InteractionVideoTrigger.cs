using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class InteractionVideoTrigger : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pressFCanvas;      // Canvas que muestra "Presiona F para reproducir video"
    public GameObject exitCanvas;        // Canvas que muestra "Presiona F para pausar video"

    [Header("Video")]
    public VideoPlayer videoPlayer;      // VideoPlayer que se reproducirá

    [Header("Ajustes de interacción")]
    [Tooltip("Tiempo de retardo al cambiar de canvas (por estética visual)")]
    public float canvasSwitchDelay = 0.2f;

    private bool isPlayerInside = false;
    private bool videoStarted = false;

    void Start()
    {
        // Asegurarnos de que ambos canvas estén ocultos al inicio
        if (pressFCanvas != null) pressFCanvas.SetActive(false);
        if (exitCanvas != null) exitCanvas.SetActive(false);

        // Asegurarnos de que el VideoPlayer no reproduzca automáticamente
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Pause();
        }
    }

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            if (!videoStarted)
            {
                StartCoroutine(StartVideoInteraction());
            }
            else
            {
                ToggleVideo();
            }
        }
    }

    private IEnumerator StartVideoInteraction()
    {
        videoStarted = true;

        // Ocultar el primer canvas con retardo y mostrar el segundo
        if (pressFCanvas != null) pressFCanvas.SetActive(false);
        yield return new WaitForSeconds(canvasSwitchDelay);
        if (exitCanvas != null) exitCanvas.SetActive(true);

        // Reproducir video desde el inicio
        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    private void ToggleVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }

    private void StopAndResetVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        videoStarted = false;
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

            // Reiniciar video
            StopAndResetVideo();
        }
    }
}
