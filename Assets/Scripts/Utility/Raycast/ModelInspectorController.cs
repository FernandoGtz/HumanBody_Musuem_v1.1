using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelInspectorController : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Camera playerCamera;

    [Header("Canvases (CanvasGroup)")]
    [SerializeField] private CanvasGroup canvasA_EnterPrompt;
    [SerializeField] private CanvasGroup canvasB_InspectModel;
    [SerializeField] private CanvasGroup canvasC_PieceDetail;

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
    [SerializeField, Range(0f,1f)] private float rotationVolume = 1f;


    // --- Estado interno ---
    private bool isInspectingModel = false;
    private bool isPieceSelected = false;
    private bool isLookingAtModel = false;

    private Transform selectedPiece = null;
    private Coroutine rotationCoroutine = null;
    private Coroutine returnToInitialCoroutine = null;
    private Quaternion initialRotation;

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main;

        if (modelRoot != null)
            initialRotation = modelRoot.rotation;

        SetCanvas(canvasB_InspectModel, false);
        SetCanvas(canvasC_PieceDetail, false);
        // canvasA se deja según inspector
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

        // Actualizamos estado de mirada
        bool previouslyLooking = isLookingAtModel;
        isLookingAtModel = hitModel;

        // Si dejamos de mirar el modelo mientras estamos en inspección, salimos de inspección inmediatamente.
        if (previouslyLooking && !isLookingAtModel && isInspectingModel)
        {
            // Salir del modo inspección por alejarse
            isInspectingModel = false;
            DeselectPieceImmediate();
            SetCanvas(canvasB_InspectModel, false);
            SetCanvas(canvasC_PieceDetail, false);
            // canvasA no aparece porque no estamos mirando al modelo
            // Programar regreso a rotacion inicial (si corresponde)
            ScheduleReturnToInitialIfNeeded();
        }

        // Lógica de qué canvas mostrar (si no acabamos de forzar la salida)
        if (isLookingAtModel)
        {
            if (isInspectingModel)
            {
                // Inspeccionando: solo canvas B
                SetCanvas(canvasA_EnterPrompt, false);
                SetCanvas(canvasB_InspectModel, true);
            }
            else
            {
                // Mirando pero no inspeccionando: solo canvas A
                SetCanvas(canvasB_InspectModel, false);
                SetCanvas(canvasA_EnterPrompt, true);
            }
        }
        else
        {
            // No mira al modelo: ocultar ambos
            SetCanvas(canvasA_EnterPrompt, false);
            // Nota: si ya forzamos la salida de inspección arriba, canvasB ya fue ocultado.
            SetCanvas(canvasB_InspectModel, false);
        }
    }

    private void HandleInspectToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isInspectingModel && !isLookingAtModel)
                return; // solo se puede entrar a inspección si mira al modelo

            isInspectingModel = !isInspectingModel;

            if (!isInspectingModel)
            {
                // Salimos de inspección (toggle o por tecla)
                DeselectPieceImmediate();
                SetCanvas(canvasB_InspectModel, false);
                SetCanvas(canvasC_PieceDetail, false);
                if (isLookingAtModel)
                    SetCanvas(canvasA_EnterPrompt, true);

                // Programar regreso a rotación inicial si corresponde
                ScheduleReturnToInitialIfNeeded();
            }
            else
            {
                // Entramos a inspección
                // Cancelamos cualquier regreso planificado
                CancelScheduledReturnToInitial();

                DeselectPieceImmediate();
                SetCanvas(canvasA_EnterPrompt, false);
                if (isLookingAtModel)
                    SetCanvas(canvasB_InspectModel, true);
            }
        }
    }

    private void HandleRotationInput()
    {
        if (!isInspectingModel || rotationCoroutine != null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            RotateModel(+90f);
        else if (Input.GetKeyDown(KeyCode.E))
            RotateModel(-90f);
    }

    private void RotateModel(float deltaDegrees)
    {
        if (modelRoot == null || rotationCoroutine != null) return;

        // Reproducir audio de rotación
        if (rotationAudioSource != null && rotationClip != null)
        {
            rotationAudioSource.PlayOneShot(rotationClip, rotationVolume);
        }

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

        // Si ya no estamos inspeccionando, programamos (si procede) el regreso a rotación inicial.
        if (!isInspectingModel)
            ScheduleReturnToInitialIfNeeded();
    }

    private void ScheduleReturnToInitialIfNeeded()
    {
        // Si ya hay un regreso programado, no hacemos nada.
        if (returnToInitialCoroutine != null) return;

        // Solo programamos si la rotación inicial está definida y el modelo no está en la rotación inicial ya
        if (modelRoot == null) return;

        // Si la rotación actual es prácticamente igual a la inicial, no volver.
        if (Quaternion.Angle(modelRoot.rotation, initialRotation) < 0.01f) return;

        // Lanzamos la corrutina que esperará 3s y luego intentará volver al initialRotation
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

        // Si el jugador volvió a inspeccionar, cancelamos el regreso
        if (isInspectingModel)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        // Esperar a que termine cualquier rotación en curso
        while (rotationCoroutine != null)
            yield return null;

        // Si el jugador volvió a inspeccionar mientras esperábamos, abortar
        if (isInspectingModel)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        // Si ya estamos en la rotación inicial, no hacemos nada
        if (Quaternion.Angle(modelRoot.rotation, initialRotation) < 0.01f)
        {
            returnToInitialCoroutine = null;
            yield break;
        }

        // Iniciar la rotación de regreso
        rotationCoroutine = StartCoroutine(RotateOverTime(modelRoot.rotation, initialRotation, rotationAnimTime));

        // Esperar a que termine la rotación de regreso
        while (rotationCoroutine != null)
            yield return null;

        returnToInitialCoroutine = null;
    }

    private void ProcessRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, piecesLayer, QueryTriggerInteraction.Ignore))
        {
            Transform hitTransform = hit.transform;

            if (isPieceSelected && hitTransform == selectedPiece)
                return;

            DeselectPieceImmediate();
            SelectPiece(hitTransform);
        }
        else if (isPieceSelected)
        {
            SetCanvas(canvasC_PieceDetail, false);
            DeselectPieceImmediate();
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
                else pieceImage.gameObject.SetActive(false);
            }
        }
        else
        {
            tmpName.text = "";
            tmpDescription.text = "";
            if (pieceImage != null) pieceImage.gameObject.SetActive(false);
        }

        SetCanvas(canvasC_PieceDetail, true);
    }

    private void DeselectPieceImmediate()
    {
        selectedPiece = null;
        isPieceSelected = false;
        SetCanvas(canvasC_PieceDetail, false);
    }

    private void SetCanvas(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable = visible;
    }
}
