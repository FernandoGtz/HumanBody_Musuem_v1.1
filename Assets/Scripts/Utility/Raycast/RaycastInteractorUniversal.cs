using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class RaycastInteractorUniversal : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 5f;
    public LayerMask interactableLayer;
    public LayerMask obstacleLayer;

    public KeyCode interactKey = KeyCode.F;

    [Header("UI Settings")]
    public GameObject interactionCanvas;
    public GameObject pausePlayCanvas;
    public GameObject radialProgressCanvas;

    [Header("Icons (Play/Pause)")]
    public RawImage playIcon;
    public RawImage pauseIcon;

    [Header("Interaction Audio")]
    public AudioClip interactionClip;
    private AudioSource interactionAudioSource;

    [Header("Interaction Settings")]
    public float interactionRange = 5f;

    [Header("Radial Image Settings - Unity 6")]
    public RawImage radialProgressRawImage;

    private GameObject currentTarget;
    private VideoPlayer currentVideo;
    private AudioSource currentAudio;
    private bool hasInteracted = false;
    private bool isMediaPlaying = false;
    private enum MediaType { None, Video, Audio }
    private MediaType currentMediaType;
    private GameObject objectLookedAt;

    private CanvasGroup interactionCanvasGroup;
    private CanvasGroup pausePlayCanvasGroup;
    private CanvasGroup radialProgressCanvasGroup;

    private float currentFillAmount = 0f;
    private bool isRadialActive = false;
    private float mediaDuration = 0f;
    private float mediaPlayTime = 0f;
    private Material radialMaterial;

    void Start()
    {
        // CanvasGroups
        InitializeCanvasGroup(ref interactionCanvasGroup, interactionCanvas);
        InitializeCanvasGroup(ref pausePlayCanvasGroup, pausePlayCanvas);
        InitializeCanvasGroup(ref radialProgressCanvasGroup, radialProgressCanvas);

        SetCanvasAlpha(interactionCanvasGroup, 0f);
        SetCanvasAlpha(pausePlayCanvasGroup, 0f);
        SetCanvasAlpha(radialProgressCanvasGroup, 0f);

        // Radial Material
        if (radialProgressRawImage != null)
        {
            radialMaterial = new Material(Shader.Find("UI/RadialFill"));
            radialProgressRawImage.material = radialMaterial;
            SetRadialFill(0f);
        }

        if (playIcon != null) playIcon.gameObject.SetActive(false);
        if (pauseIcon != null) pauseIcon.gameObject.SetActive(false);

        // AudioSource de interacción
        interactionAudioSource = gameObject.AddComponent<AudioSource>();
        interactionAudioSource.playOnAwake = false;
        interactionAudioSource.spatialBlend = 0f;
        interactionAudioSource.volume = 1f;
    }

    void Update()
    {
        // Raycast
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, interactableLayer | obstacleLayer);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        GameObject detectedObject = null;
        bool blocked = false;

        foreach (var h in hits)
        {
            int hitLayer = h.collider.gameObject.layer;
            if (((1 << hitLayer) & obstacleLayer) != 0) { blocked = true; break; }
            if (((1 << hitLayer) & interactableLayer) != 0) { detectedObject = h.collider.gameObject; break; }
        }

        objectLookedAt = (!blocked) ? detectedObject : null;

        if (objectLookedAt != null)
        {
            float distance = Vector3.Distance(transform.position, objectLookedAt.transform.position);
            if (distance <= interactionRange)
            {
                if (currentTarget != objectLookedAt)
                {
                    currentTarget = objectLookedAt;
                    currentVideo = currentTarget.GetComponent<VideoPlayer>();
                    currentAudio = currentTarget.GetComponent<AudioSource>();

                    if (currentVideo != null)
                    {
                        currentMediaType = MediaType.Video;
                        isMediaPlaying = currentVideo.isPlaying;
                        mediaDuration = (float)currentVideo.length;
                    }
                    else if (currentAudio != null)
                    {
                        currentMediaType = MediaType.Audio;
                        isMediaPlaying = currentAudio.isPlaying;
                        mediaDuration = currentAudio.clip != null ? currentAudio.clip.length : 0f;
                    }
                    else currentMediaType = MediaType.None;

                    hasInteracted = isMediaPlaying;
                }

                UpdateCanvas();

                if (Input.GetKeyDown(interactKey) && currentMediaType != MediaType.None)
                {
                    // 🔹 Bloqueo si NPC está hablando
                    if (MediaManager.Instance.IsNPCTalking) return;

                    // 🔹 Audio de interacción
                    if (interactionClip != null)
                        interactionAudioSource.PlayOneShot(interactionClip);

                    if (!hasInteracted)
                    {
                        PlayMedia();
                        isMediaPlaying = true;
                        hasInteracted = true;
                        StartRadialProgress();
                        ShowPauseIcon();
                    }
                    else
                    {
                        ToggleMedia();
                        UpdateCanvas();
                    }
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

        if (currentTarget != null && currentMediaType != MediaType.None)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance > interactionRange)
                HandleExitRange();
        }

        UpdateRadialProgress();
    }

    private void PlayMedia()
    {
        switch (currentMediaType)
        {
            case MediaType.Video:
                MediaManager.Instance.PlayVideo(currentVideo, currentTarget);
                break;
            case MediaType.Audio:
                MediaManager.Instance.PlayAudio(currentAudio, currentTarget);
                break;
        }
    }

    private void ToggleMedia()
    {
        switch (currentMediaType)
        {
            case MediaType.Video:
                MediaManager.Instance.ToggleVideo();
                isMediaPlaying = MediaManager.Instance.IsVideoPlaying(currentVideo);
                if (isMediaPlaying) ShowPauseIcon(); else ShowPlayIcon();
                break;
            case MediaType.Audio:
                MediaManager.Instance.ToggleAudio();
                isMediaPlaying = MediaManager.Instance.IsAudioPlaying(currentAudio);
                if (isMediaPlaying) ShowPauseIcon(); else ShowPlayIcon();
                break;
        }
    }

    private void StopMedia()
    {
        switch (currentMediaType)
        {
            case MediaType.Video:
                if (currentVideo != null) currentVideo.Stop();
                break;
            case MediaType.Audio:
                if (currentAudio != null) MediaManager.Instance.StopCurrentAudio();
                break;
        }
        HideIcons();
    }

    private void ShowPlayIcon()
    {
        if (pauseIcon != null) pauseIcon.gameObject.SetActive(false);
        if (playIcon != null) playIcon.gameObject.SetActive(true);
    }

    private void ShowPauseIcon()
    {
        if (playIcon != null) playIcon.gameObject.SetActive(false);
        if (pauseIcon != null) pauseIcon.gameObject.SetActive(true);
    }

    private void HideIcons()
    {
        if (playIcon != null) playIcon.gameObject.SetActive(false);
        if (pauseIcon != null) pauseIcon.gameObject.SetActive(false);
    }

    private void InitializeCanvasGroup(ref CanvasGroup canvasGroup, GameObject canvas)
    {
        if (canvas != null)
        {
            canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvas.AddComponent<CanvasGroup>();
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
        if (currentMediaType != MediaType.None && (isMediaPlaying || hasInteracted))
        {
            SetCanvasAlpha(pausePlayCanvasGroup, 1f);
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(radialProgressCanvasGroup, 1f);
        }
        else
        {
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            SetCanvasAlpha(radialProgressCanvasGroup, 0f);
        }
    }

    private void HandleExitRange()
    {
        StopMedia();
        StopRadialProgress();

        currentTarget = null;
        currentVideo = null;
        currentAudio = null;
        currentMediaType = MediaType.None;
        isMediaPlaying = false;
        hasInteracted = false;

        SetCanvasAlpha(interactionCanvasGroup, 0f);
        SetCanvasAlpha(pausePlayCanvasGroup, 0f);
        SetCanvasAlpha(radialProgressCanvasGroup, 0f);
    }

    private void UpdateCanvas()
    {
        if (MediaManager.Instance.IsNPCTalking)
        {
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            SetCanvasAlpha(radialProgressCanvasGroup, 0f);
            return;
        }

        bool looking = objectLookedAt == currentTarget;

        if (looking)
        {
            if (isMediaPlaying || hasInteracted)
            {
                SetCanvasAlpha(pausePlayCanvasGroup, 1f);
                SetCanvasAlpha(interactionCanvasGroup, 0f);
                SetCanvasAlpha(radialProgressCanvasGroup, 1f);
            }
            else
            {
                SetCanvasAlpha(interactionCanvasGroup, 1f);
                SetCanvasAlpha(pausePlayCanvasGroup, 0f);
                SetCanvasAlpha(radialProgressCanvasGroup, 0f);
            }
        }
        else
        {
            SetCanvasAlpha(interactionCanvasGroup, 0f);
            SetCanvasAlpha(pausePlayCanvasGroup, 0f);
            SetCanvasAlpha(radialProgressCanvasGroup, 0f);
        }
    }

    // ---------------- Radial Methods ----------------
    private void SetRadialFill(float amount)
    {
        if (radialMaterial != null)
            radialMaterial.SetFloat("_FillAmount", amount);
    }

    private void StartRadialProgress()
    {
        if (radialProgressRawImage != null && mediaDuration > 0f)
        {
            isRadialActive = true;
            currentFillAmount = 0f;
            mediaPlayTime = 0f;
            SetRadialFill(0f);
            SetCanvasAlpha(radialProgressCanvasGroup, 1f);
        }
    }

    private void StopRadialProgress()
    {
        isRadialActive = false;
        currentFillAmount = 0f;
        mediaPlayTime = 0f;
        SetRadialFill(0f);
        SetCanvasAlpha(radialProgressCanvasGroup, 0f);
    }

    private void PauseRadialProgress() => isRadialActive = false;
    private void ResumeRadialProgress()
    {
        if (currentMediaType != MediaType.None && isMediaPlaying && currentFillAmount < 1f)
            isRadialActive = true;
    }

    private void UpdateRadialProgress()
    {
        if (isRadialActive && radialProgressRawImage != null && currentMediaType != MediaType.None)
        {
            if (isMediaPlaying)
            {
                switch (currentMediaType)
                {
                    case MediaType.Video:
                        if (currentVideo.frameCount > 0) mediaPlayTime = (float)currentVideo.time;
                        break;
                    case MediaType.Audio:
                        mediaPlayTime += Time.deltaTime;
                        break;
                }

                if (mediaDuration > 0f)
                {
                    currentFillAmount = mediaPlayTime / mediaDuration;
                    currentFillAmount = Mathf.Clamp01(currentFillAmount);
                    SetRadialFill(currentFillAmount);

                    if (currentFillAmount >= 1f)
                    {
                        isRadialActive = false;
                        SetRadialFill(1f);
                    }
                }
            }
        }
    }

    public void ResetRadialProgress()
    {
        currentFillAmount = 0f;
        mediaPlayTime = 0f;
        SetRadialFill(0f);
        isRadialActive = false;
    }

    private void OnDestroy()
    {
        if (radialMaterial != null)
            DestroyImmediate(radialMaterial);
    }
}
