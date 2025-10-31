using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelInspectorController : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Camera playerCamera;

    [Header("Canvases LOCALES de este modelo")]
    [SerializeField] private CanvasGroup localCanvasA_EnterPrompt;  // Canvas A LOCAL
    [SerializeField] private CanvasGroup localCanvasB_InspectModel; // Canvas B LOCAL

    [Header("UI de detalles de pieza")]
    [SerializeField] private TextMeshProUGUI tmpName;
    [SerializeField] private TextMeshProUGUI tmpDescription;
    [SerializeField] private Image pieceImage;

    [Header("Raycast / Layers")]
    [SerializeField] private LayerMask piecesLayer;
    [SerializeField] private float raycastDistance = 10f;

    [Header("Rotación del modelo")]
    [SerializeField] private float rotationAnimTime = 1f;

    [Header("Audio Rotación")]
    [SerializeField] private AudioSource rotationAudioSource;
    [SerializeField] private AudioClip rotationClip;
    [SerializeField, Range(0f, 1f)] private float rotationVolume = 1f;

    // --- Estado interno ---
    private bool isInspectingModel = false;
    private bool isPieceSelected = false;
    private bool isLookingAtModel = false;

    private Transform selectedPiece = null;
    private Coroutine rotationCoroutine = null;
    private Coroutine returnToInitialCoroutine = null;
    private Quaternion initialRotation;

    // Propiedades públicas para el manager
    public bool IsInspecting => isInspectingModel;
    public bool IsLookingAtModel => isLookingAtModel;

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main;

        if (modelRoot != null)
            initialRotation = modelRoot.rotation;

        // Registrar este inspector en el manager
        if (InspectorCanvasManager.Instance != null)
        {
            InspectorCanvasManager.Instance.RegisterInspector(this);
        }

        // Inicializar canvases locales como ocultos
        SetLocalCanvasA(false);
        SetLocalCanvasB(false);
    }

    private void OnDestroy()
    {
        // Desregistrar cuando se destruye el objeto
        if (InspectorCanvasManager.Instance != null)
        {
            InspectorCanvasManager.Instance.UnregisterInspector(this);
        }
    }

    private void Update()
    {
        DetectModelAim();
        HandleInspectToggle();
        HandleRotationInput();

        if (isInspectingModel)
            ProcessRaycast();
    }

    private void DetectModelAim()
    {
        if (playerCamera == null || modelRoot == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        bool hitModel = false;

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == modelRoot || hit.transform.IsChildOf(modelRoot))
                hitModel = true;
        }

        bool previouslyLooking = isLookingAtModel;
        isLookingAtModel = hitModel;

        if (previouslyLooking && !isLookingAtModel && isInspectingModel)
        {
            isInspectingModel = false;
            DeselectPieceImmediate();
            InspectorCanvasManager.Instance?.ClearCurrentInspectingModel(this);
            InspectorCanvasManager.Instance?.SetCanvasC(false);
        }

        // Actualizar estados a través del manager
        if (InspectorCanvasManager.Instance != null)
        {
            InspectorCanvasManager.Instance.UpdateCanvasStates();
        }
    }

    private void HandleInspectToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isInspectingModel && !isLookingAtModel) return;

            isInspectingModel = !isInspectingModel;

            if (!isInspectingModel)
            {
                DeselectPieceImmediate();
                InspectorCanvasManager.Instance?.ClearCurrentInspectingModel(this);
                InspectorCanvasManager.Instance?.SetCanvasC(false);

                ScheduleReturnToInitialIfNeeded();
            }
            else
            {
                CancelScheduledReturnToInitial();
                DeselectPieceImmediate();
                
                // Notificar al manager que este modelo está en modo inspección
                InspectorCanvasManager.Instance?.SetCurrentInspectingModel(this);
            }
        }
    }

    // Métodos públicos para que el manager controle los canvases locales
    public void SetLocalCanvasA(bool visible)
    {
        if (localCanvasA_EnterPrompt == null) return;

        localCanvasA_EnterPrompt.alpha = visible ? 1f : 0f;
        localCanvasA_EnterPrompt.blocksRaycasts = visible;
        localCanvasA_EnterPrompt.interactable = visible;
    }

    public void SetLocalCanvasB(bool visible)
    {
        if (localCanvasB_InspectModel == null) return;

        localCanvasB_InspectModel.alpha = visible ? 1f : 0f;
        localCanvasB_InspectModel.blocksRaycasts = visible;
        localCanvasB_InspectModel.interactable = visible;
    }

    // Método para forzar salida de inspección (usado por el manager)
    public void ForceExitInspection()
    {
        if (isInspectingModel)
        {
            isInspectingModel = false;
            DeselectPieceImmediate();
            SetLocalCanvasB(false);
            ScheduleReturnToInitialIfNeeded();
        }
    }

    // El resto de los métodos se mantienen igual...
    private void HandleRotationInput()
    {
        if (!isInspectingModel || rotationCoroutine != null) return;

        if (Input.GetKeyDown(KeyCode.Q))
            RotateModel(+90f);
        else if (Input.GetKeyDown(KeyCode.E))
            RotateModel(-90f);
    }

    private void RotateModel(float deltaDegrees)
    {
        if (modelRoot == null || rotationCoroutine != null) return;

        if (rotationAudioSource != null && rotationClip != null)
            rotationAudioSource.PlayOneShot(rotationClip, rotationVolume);

        Quaternion start = modelRoot.rotation;
        Quaternion end = Quaternion.Euler(modelRoot.eulerAngles + Vector3.up * deltaDegrees);

        rotationCoroutine = StartCoroutine(RotateOverTime(start, end, rotationAnimTime));
    }

    private IEnumerator RotateOverTime(Quaternion start, Quaternion end, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            modelRoot.rotation = Quaternion.Slerp(start, end, p);
            yield return null;
        }

        modelRoot.rotation = end;
        rotationCoroutine = null;

        if (!isInspectingModel)
            ScheduleReturnToInitialIfNeeded();
    }

    private void ScheduleReturnToInitialIfNeeded()
    {
        if (returnToInitialCoroutine != null) return;
        if (modelRoot == null) return;
        if (Quaternion.Angle(modelRoot.rotation, initialRotation) < 0.01f) return;

        returnToInitialCoroutine = StartCoroutine(ReturnToInitialAfterDelayCoroutine(3f));
    }

    private void CancelScheduledReturnToInitial()
    {
        if (returnToInitialCoroutine != null)
        {
            StopCoroutine(returnToInitialCoroutine);
            returnToInitialCoroutine = null;
        }
    }

    private IEnumerator ReturnToInitialAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isInspectingModel)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        while (rotationCoroutine != null)
            yield return null;

        if (isInspectingModel)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        if (Quaternion.Angle(modelRoot.rotation, initialRotation) < 0.01f)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        rotationCoroutine = StartCoroutine(RotateOverTime(modelRoot.rotation, initialRotation, rotationAnimTime));

        while (rotationCoroutine != null)
            yield return null;

        returnToInitialCoroutine = null;
    }

    private void ProcessRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, piecesLayer, QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            if (isPieceSelected)
            {
                InspectorCanvasManager.Instance?.SetCanvasC(false);
                DeselectPieceImmediate();
            }
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Transform firstValidPiece = null;

        foreach (var hit in hits)
        {
            if (hit.transform.GetComponent<InteractableInfo>() != null)
            {
                firstValidPiece = hit.transform;
                break;
            }
        }

        if (firstValidPiece != null)
        {
            if (isPieceSelected && firstValidPiece == selectedPiece) return;
            DeselectPieceImmediate();
            SelectPiece(firstValidPiece);
        }
        else
        {
            if (isPieceSelected)
            {
                InspectorCanvasManager.Instance?.SetCanvasC(false);
                DeselectPieceImmediate();
            }
        }
    }

    private void SelectPiece(Transform piece)
    {
        if (piece == null) return;

        isPieceSelected = true;
        selectedPiece = piece;

        var info = piece.GetComponent<InteractableInfo>();

        if (info != null)
        {
            tmpName.text = info.nombreObjeto ?? "";
            tmpDescription.text = info.descripcion ?? "";

            if (pieceImage != null)
            {
                if (info.imagen != null)
                {
                    pieceImage.gameObject.SetActive(true);
                    pieceImage.sprite = info.imagen;
                }
                else
                    pieceImage.gameObject.SetActive(false);
            }
        }
        else
        {
            tmpName.text = "";
            tmpDescription.text = "";
            if (pieceImage != null) pieceImage.gameObject.SetActive(false);
        }

        InspectorCanvasManager.Instance?.SetCanvasC(true);
    }

    private void DeselectPieceImmediate()
    {
        selectedPiece = null;
        isPieceSelected = false;
    }
}