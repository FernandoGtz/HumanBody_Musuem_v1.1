using UnityEngine;

public class RaycastInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 5f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.F;

    [Header("UI Settings")]
    public GameObject interactionCanvas;
    public GameObject pausePlayCanvas;

    [Header("Audio Settings")]
    public float interactionRange = 5f;

    private GameObject currentTarget;
    private AudioSource currentAudio;
    private bool hasInteracted = false;
    private bool isAudioPlaying = false;

    private GameObject objectLookedAt; // objeto actualmente apuntado por raycast

    void Update()
    {
        // Lanzar raycast en cada frame
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, maxDistance, interactableLayer);
        objectLookedAt = hitSomething ? hit.collider.gameObject : null;

        if (objectLookedAt != null)
        {
            float distance = Vector3.Distance(transform.position, objectLookedAt.transform.position);

            // Si está dentro del rango de interacción
            if (distance <= interactionRange)
            {
                // Cambiamos de objetivo si es necesario
                if (currentTarget != objectLookedAt)
                {
                    currentTarget = objectLookedAt;
                    currentAudio = currentTarget.GetComponent<AudioSource>();
                    isAudioPlaying = currentAudio != null && AudioManager.Instance.IsAudioPlaying(currentAudio);
                    hasInteracted = isAudioPlaying;
                }

                // Actualizamos canvas mientras apuntamos
                UpdateCanvas();

                // Detectar interacción con tecla F
                if (Input.GetKeyDown(interactKey) && currentAudio != null)
                {
                    if (!hasInteracted)
                    {
                        AudioManager.Instance.PlayAudio(currentAudio, currentTarget);
                        isAudioPlaying = true;
                        hasInteracted = true;
                    }
                    else
                    {
                        AudioManager.Instance.ToggleAudio();
                        isAudioPlaying = AudioManager.Instance.IsAudioPlaying(currentAudio);
                    }

                    UpdateCanvas();
                }
            }
            else
            {
                HandleExitRange(); // si salimos del rango, reiniciamos audio y ocultamos canvas
            }
        }
        else
        {
            // Si dejamos de apuntar
            HandleNotLookingAtObject();
        }

        // Revisión constante por si el jugador se aleja sin raycast apuntando
        if (currentTarget != null && currentAudio != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance > interactionRange)
            {
                HandleExitRange(); // <— aquí también reiniciamos
            }
        }
    }

    private void HandleNotLookingAtObject()
    {
        // Si no apuntamos pero el audio está sonando o pausado, mostrar solo pausePlayCanvas
        if (currentAudio != null && (AudioManager.Instance.IsAudioPlaying(currentAudio) || hasInteracted))
        {
            if (pausePlayCanvas != null) pausePlayCanvas.SetActive(true);
            if (interactionCanvas != null) interactionCanvas.SetActive(false);
        }
        else
        {
            // Ocultar solo si fue activado por raycast
            if (interactionCanvas != null) interactionCanvas.SetActive(false);
            if (pausePlayCanvas != null) pausePlayCanvas.SetActive(false);
        }
    }

    private void HandleExitRange()
    {
        // Reiniciar audio completamente al salir del rango
        if (currentAudio != null)
        {
            AudioManager.Instance.StopCurrentAudio(); // ya reinicia desde el AudioManager
        }

        currentTarget = null;
        currentAudio = null;
        isAudioPlaying = false;
        hasInteracted = false;

        if (interactionCanvas != null) interactionCanvas.SetActive(false);
        if (pausePlayCanvas != null) pausePlayCanvas.SetActive(false);
    }

    private void UpdateCanvas()
    {
        if (currentAudio == null) return;

        bool looking = objectLookedAt == currentTarget;

        // Si apuntamos al objeto actual
        if (looking)
        {
            if (isAudioPlaying || hasInteracted)
            {
                if (pausePlayCanvas != null) pausePlayCanvas.SetActive(true);
                if (interactionCanvas != null) interactionCanvas.SetActive(false);
            }
            else
            {
                if (interactionCanvas != null) interactionCanvas.SetActive(true);
                if (pausePlayCanvas != null) pausePlayCanvas.SetActive(false);
            }
        }
        else
        {
            // Si dejamos de apuntar y hay audio sonando o pausado, mantener solo pausePlayCanvas
            if (isAudioPlaying || hasInteracted)
            {
                if (pausePlayCanvas != null) pausePlayCanvas.SetActive(true);
                if (interactionCanvas != null) interactionCanvas.SetActive(false);
            }
            else
            {
                if (interactionCanvas != null) interactionCanvas.SetActive(false);
                if (pausePlayCanvas != null) pausePlayCanvas.SetActive(false);
            }
        }
    }
}
