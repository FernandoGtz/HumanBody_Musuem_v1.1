using UnityEngine;
using UnityEngine.Video;

public class RaycastInteractorVideo : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 5f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.F;

    [Header("UI Settings")]
    public GameObject interactionCanvas;
    public GameObject pausePlayCanvas;

    [Header("Video Settings")]
    public float interactionRange = 5f;

    private GameObject currentTarget;
    private VideoPlayer currentVideo;
    private bool hasInteracted = false;
    private bool isVideoPlaying = false;

    private GameObject objectLookedAt;

    // Variables para Canvas Group
    private CanvasGroup interactionCanvasGroup;
    private CanvasGroup pausePlayCanvasGroup;

    void Start()
    {
        InitializeCanvasGroup(ref interactionCanvasGroup, interactionCanvas);
        InitializeCanvasGroup(ref pausePlayCanvasGroup, pausePlayCanvas);

        SetCanvasAlpha(interactionCanvasGroup, 0f);
        SetCanvasAlpha(pausePlayCanvasGroup, 0f);
    }

    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, maxDistance, interactableLayer);
        objectLookedAt = hitSomething ? hit.collider.gameObject : null;

        if (objectLookedAt != null)
        {
            float distance = Vector3.Distance(transform.position, objectLookedAt.transform.position);

            if (distance <= interactionRange)
            {
                if (currentTarget != objectLookedAt)
                {
                    currentTarget = objectLookedAt;
                    currentVideo = currentTarget.GetComponent<VideoPlayer>();
                    isVideoPlaying = currentVideo != null && currentVideo.isPlaying;
                    hasInteracted = isVideoPlaying;
                }

                UpdateCanvas();

                if (Input.GetKeyDown(interactKey) && currentVideo != null)
                {
                    if (!hasInteracted)
                    {
                        currentVideo.Play();
                        isVideoPlaying = true;
                        hasInteracted = true;
                    }
                    else
                    {
                        if (currentVideo.isPlaying)
                        {
                            currentVideo.Pause();
                            isVideoPlaying = false;
                        }
                        else
                        {
                            currentVideo.Play();
                            isVideoPlaying = true;
                        }
                    }

                    UpdateCanvas();
                }
            }
            else
            {
                HandleExitRange();
            }
        }
        else
        {
            HandleNotLookingAtObject();
        }

        if (currentTarget != null && currentVideo != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance > interactionRange)
            {
                HandleExitRange();
            }
        }
    }

    private void InitializeCanvasGroup(ref CanvasGroup canvasGroup, GameObject canvas)
    {
        if (canvas != null)
        {
            canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.AddComponent<CanvasGroup>();
            }
        }
    }

    private void SetCanvasAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = alpha > 0.1f;
            canvasGroup.blocksRaycasts = alpha > 0.1f;
        }
    }

    private void HandleNotLookingAtObject()
    {
        if (currentVideo != null && (currentVideo.isPlaying || hasInteracted))
        {
            SetCanvasAlpha(pausePlayCanvasGroup, 1f);
            SetCanvasAlpha(interactionCanvasGroup, 0f);
        }
        else
        {
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(pausePlayCanvasGroup, 0f);
        }
    }

    private void HandleExitRange()
    {
        if (currentVideo != null)
        {
            currentVideo.Stop();
        }

        currentTarget = null;
        currentVideo = null;
        isVideoPlaying = false;
        hasInteracted = false;

        SetCanvasAlpha(interactionCanvasGroup, 0f);
        SetCanvasAlpha(pausePlayCanvasGroup, 0f);
    }

    private void UpdateCanvas()
    {
        if (currentVideo == null)
        {
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            return;
        }

        bool looking = objectLookedAt == currentTarget;

        if (looking)
        {
            if (isVideoPlaying || hasInteracted)
            {
                SetCanvasAlpha(pausePlayCanvasGroup, 1f);
                SetCanvasAlpha(interactionCanvasGroup, 0f);
            }
            else
            {
                SetCanvasAlpha(interactionCanvasGroup, 1f);
                SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            }
        }
        else
        {
            if (isVideoPlaying || hasInteracted)
            {
                SetCanvasAlpha(pausePlayCanvasGroup, 1f);
                SetCanvasAlpha(interactionCanvasGroup, 0f);
            }
            else
            {
                SetCanvasAlpha(interactionCanvasGroup, 0f);
                SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            }
        }
    }
}
