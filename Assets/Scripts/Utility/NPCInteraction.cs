using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCInteraction : MonoBehaviour
{
    [Header("Player Settings")]
    public Camera playerCamera;
    public Transform player;
    public float interactionRange = 3f;

    [Header("Raycast Settings")]
    public LayerMask npcLayer;
    public LayerMask obstacleLayer;
    public float raycastDistance = 10f;

    [Header("Canvas References")]
    public CanvasGroup canvasA; // "Presiona F para hablar"
    public CanvasGroup canvasB; // "Hablando..."

    [Header("NPC Components")]
    public Animator npcAnimator;
    public AudioSource npcAudioSource;
    public AudioClip[] talkClips;

    private bool isLookingAtNPC = false;
    private bool isTalking = false;
    private bool isInRange = false;

    private AudioClip currentClip;

    void Start()
    {
        SetCanvasInstant(canvasA, 0f, false);
        SetCanvasInstant(canvasB, 0f, false);
    }

    void Update()
    {
        CheckDistance();
        CheckRaycast();
        HandleInput();
        CheckAudioEnd();
    }

    void CheckDistance()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRange;

        if (wasInRange && !isInRange)
        {
            if (isTalking)
                StopConversationDueToLeaveRange();
            else
                SetCanvasInstant(canvasA, 0f, false);
        }
    }

    void CheckRaycast()
    {
        if (!isInRange)
        {
            isLookingAtNPC = false;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, npcLayer | obstacleLayer);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool blocked = false;
        bool hitNPC = false;

        foreach (var h in hits)
        {
            int hitLayer = h.collider.gameObject.layer;
            if (((1 << hitLayer) & obstacleLayer) != 0)
            {
                blocked = true;
                break;
            }
            if (((1 << hitLayer) & npcLayer) != 0 && h.collider.gameObject == gameObject)
            {
                hitNPC = true;
                break;
            }
        }

        isLookingAtNPC = (!blocked && hitNPC);

        if (isLookingAtNPC && !isTalking && !MediaManager.Instance.IsMediaPlayingOrNPCTalking)
            SetCanvasInstant(canvasA, 1f, true);
        else if (!isTalking)
            SetCanvasInstant(canvasA, 0f, false);
    }

    void HandleInput()
    {
        if (!isLookingAtNPC || !isInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isTalking)
                StartConversation();
            else
                StopConversationByPlayer();
        }
    }

    void StartConversation()
    {
        if (talkClips == null || talkClips.Length == 0 || npcAudioSource == null || npcAnimator == null)
            return;

        MediaManager.Instance.SetNPCTalking(true);

        currentClip = talkClips[Random.Range(0, talkClips.Length)];
        npcAudioSource.clip = currentClip;
        npcAudioSource.Play();
        npcAnimator.SetTrigger("Talk");

        isTalking = true;
        SetCanvasInstant(canvasA, 0f, false);
        SetCanvasInstant(canvasB, 1f, true);
    }

    void StopConversationByPlayer()
    {
        if (npcAudioSource.isPlaying)
            npcAudioSource.Stop();

        npcAnimator.SetTrigger("Idle");
        isTalking = false;

        MediaManager.Instance.SetNPCTalking(false);

        SetCanvasInstant(canvasB, 0f, false);
        if (isLookingAtNPC && isInRange)
            SetCanvasInstant(canvasA, 1f, true);
        else
            SetCanvasInstant(canvasA, 0f, false);
    }

    void StopConversationDueToLeaveRange()
    {
        if (npcAudioSource.isPlaying)
            npcAudioSource.Stop();

        npcAnimator.SetTrigger("Idle");
        isTalking = false;

        MediaManager.Instance.SetNPCTalking(false);

        SetCanvasInstant(canvasB, 0f, false);
        SetCanvasInstant(canvasA, 0f, false);
    }

    void CheckAudioEnd()
    {
        if (isTalking && !npcAudioSource.isPlaying)
        {
            npcAnimator.SetTrigger("Idle");
            isTalking = false;

            MediaManager.Instance.SetNPCTalking(false);

            SetCanvasInstant(canvasB, 0f, false);
            if (isLookingAtNPC && isInRange)
                SetCanvasInstant(canvasA, 1f, true);
            else
                SetCanvasInstant(canvasA, 0f, false);
        }
    }

    void SetCanvasInstant(CanvasGroup cg, float alpha, bool interactable)
    {
        if (cg == null) return;
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }
}
