using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class IndicatorFloatingController : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerCamera; 

    [Header("Indicator Settings")]
    public float visibleDistance = 5f; 
    public float heightOffset = 0.5f; 
    public bool useHoverAnimation = true;

    [Header("3D Models To Hide")]
    [Tooltip("Modelos 3D que también deben ocultarse/mostrarse")]
    public GameObject[] models3D; // Ej. tu diamante 3D

    [Header("Hover Animation")]
    public float hoverSpeed = 2f;
    public float hoverAmplitude = 0.1f;

    private CanvasGroup canvasGroup;
    private Vector3 initialLocalPos;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (!playerCamera) return;

        HandleVisibility();
        FaceCamera();
        HoverEffect();
    }

    private void HandleVisibility()
    {
        float distance = Vector3.Distance(playerCamera.position, transform.position);
        bool shouldBeVisible = distance > visibleDistance;
        SetVisibility(shouldBeVisible);
    }

    private void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        Toggle3DModels(visible);
    }

    private void Toggle3DModels(bool state)
    {
        if (models3D == null) return;

        foreach (var model in models3D)
        {
            if (model != null)
            {
                model.SetActive(state);
            }
        }
    }

    private void FaceCamera()
    {
        transform.LookAt(playerCamera);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    private void HoverEffect()
    {
        if (!useHoverAnimation) return;
        float newY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.localPosition = initialLocalPos + new Vector3(0, heightOffset + newY, 0);
    }
}
