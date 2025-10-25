using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ModelInspectorController : MonoBehaviour
{
    [Header("Referencias principales")]
    [Tooltip("Transform del modelo que rotaremos (padre del modelo completo).")]
    [SerializeField] private Transform modelRoot;
    [Tooltip("Transform del jugador (compara con OnTriggerEnter/Exit).")]
    [SerializeField] private Transform player;
    [Tooltip("Cámara usada para el raycast (normalmente la cámara del player).")]
    [SerializeField] private Camera playerCamera;

    [Header("Canvases (CanvasGroup)")]
    [SerializeField] private CanvasGroup canvasA_EnterPrompt; // "Presiona F..."
    [SerializeField] private CanvasGroup canvasB_InspectModel; // Inspección modelo (rotación)
    [SerializeField] private CanvasGroup canvasC_PieceDetail; // Detalles pieza (2 TMP + imagen)

    [Header("UI de detalles de pieza")]
    [SerializeField] private TextMeshProUGUI tmpName;
    [SerializeField] private TextMeshProUGUI tmpDescription;
    [SerializeField] private Image pieceImage; // opcional; si es null o sprite null se oculta

    [Header("Raycast / Layers")]
    [Tooltip("Layer(s) de las piezas interactuables.")]
    [SerializeField] private LayerMask piecesLayer;
    [Tooltip("Distancia máxima del raycast para detectar piezas.")]
    [SerializeField] private float raycastDistance = 10f;

    [Header("Escalado de piezas")]
    [Tooltip("Factor al que se escalará la pieza al apuntar (ej. 1.5).")]
    [SerializeField] private float pieceScaleFactor = 1.5f;
    [Tooltip("Tiempo de animación para escalar/desescalar pieza.")]
    [SerializeField] private float pieceScaleTime = 0.2f;

    [Header("Rotación del modelo")]
    [Tooltip("Duración (segundos) de cada rotación en 90° (Q/E).")]
    [SerializeField] private float rotationAnimTime = 1f;

    [Header("Temporizadores de salida")]
    [Tooltip("Segundos que esperamos si el player sale del collider mientras una pieza está escalada.")]
    [SerializeField] private float exitWaitSeconds = 3f; // Nota: 1s adicional antes de restaurar rotación implementado internamente.

    // --- Estado interno ---
    private bool playerInsideTrigger = false;
    private bool isInspectingModel = false;
    private bool isPieceSelected = false;

    private Transform selectedPiece = null;
    private Vector3 selectedPieceOriginalScale = Vector3.one;
    private Quaternion modelOriginalRotation;
    private bool modelRotatedFromOriginal = false;

    // Coroutines references
    private Coroutine pieceScaleCoroutine = null;
    private Coroutine rotationCoroutine = null;
    private Coroutine exitCoroutine = null;

    // Input lock for rotation when piece selected
    private bool allowModelRotation => playerInsideTrigger && !isPieceSelected;

    private void Reset()
    {
        // Asegura que el collider sea trigger para detectar player enter/exit.
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main;

        modelOriginalRotation = modelRoot != null ? modelRoot.rotation : Quaternion.identity;

        // Inicialmente: Canvas A visible solo cuando player dentro; por ahora ocultamos todos.
        SetCanvas(canvasA_EnterPrompt, false);
        SetCanvas(canvasB_InspectModel, false);
        SetCanvas(canvasC_PieceDetail, false);
    }

    private void Update()
    {
        HandleRotationInput();

        if (!playerInsideTrigger)
            return;

        // Toggle inspección con F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleInspectMode();
            return; // evitamos procesar raycast el mismo frame en que cambiamos modo
        }

        // Rotación del modelo con Q/E si se permite
        if (allowModelRotation && rotationCoroutine == null)
        {
            if (Input.GetKeyDown(KeyCode.Q)) RotateModel(+90f);
            if (Input.GetKeyDown(KeyCode.E)) RotateModel(-90f);
        }

        // Raycast solo si estamos inspeccionando
        if (isInspectingModel)
            ProcessRaycast();

        if (isPieceSelected)
        {
            Debug.DrawLine(selectedPiece.position, selectedPiece.position + Vector3.up, Color.red);
            Debug.Log(selectedPiece.localPosition);
        }
    }

    private void ToggleInspectMode()
    {
        isInspectingModel = !isInspectingModel;

        if (isInspectingModel && isPieceSelected)
            DeselectPieceImmediate();

        if (!isInspectingModel)
        {
            SetCanvas(canvasB_InspectModel, false);
            SetCanvas(canvasA_EnterPrompt, true);
            if (isPieceSelected) DeselectPieceImmediate();
            if (exitCoroutine != null) StopCoroutine(exitCoroutine);
            return;
        }

        SetCanvas(canvasA_EnterPrompt, false);
        SetCanvas(canvasB_InspectModel, true);
    }

    private void ProcessRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, piecesLayer))
        {
            Transform hitTransform = hit.transform;
            if (isPieceSelected && hitTransform == selectedPiece) return;
            DeselectPieceImmediate();
            SelectPiece(hitTransform);
        }
        else
        {
            if (isPieceSelected)
            {
                SetCanvas(canvasC_PieceDetail, false);
                DeselectPieceSmooth();
            }
        }
    }

    #region Selection / Deselection
    private void SelectPiece(Transform piece)
    {
        if (piece == null) return;

        isPieceSelected = true;
        selectedPiece = piece;
        selectedPieceOriginalScale = piece.localScale;

        if (exitCoroutine != null) { StopCoroutine(exitCoroutine); exitCoroutine = null; }

        if (pieceScaleCoroutine != null) StopCoroutine(pieceScaleCoroutine);
        pieceScaleCoroutine = StartCoroutine(ScaleOverTime(piece, selectedPieceOriginalScale * pieceScaleFactor, pieceScaleTime));

        // Mostrar UI de detalles
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
        if (selectedPiece != null)
        {
            if (pieceScaleCoroutine != null) StopCoroutine(pieceScaleCoroutine);
            selectedPiece.localScale = selectedPieceOriginalScale;
        }
        selectedPiece = null;
        isPieceSelected = false;
        SetCanvas(canvasC_PieceDetail, false);
    }

    private void DeselectPieceSmooth()
    {
        if (selectedPiece != null)
        {
            if (pieceScaleCoroutine != null) StopCoroutine(pieceScaleCoroutine);
            pieceScaleCoroutine = StartCoroutine(ScaleBackAndClear(selectedPiece, selectedPieceOriginalScale, pieceScaleTime));
        }
    }

    private IEnumerator ScaleBackAndClear(Transform piece, Vector3 targetScale, float time)
    {
        yield return ScaleOverTime(piece, targetScale, time);
        selectedPiece = null;
        isPieceSelected = false;
        SetCanvas(canvasC_PieceDetail, false);
    }

    private IEnumerator ScaleOverTime(Transform piece, Vector3 targetScale, float duration)
    {
        if (piece == null) yield break;
        Vector3 startScale = piece.localScale;
        Vector3 originalLocalPos = piece.localPosition;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            piece.localScale = Vector3.Lerp(startScale, targetScale, p);
            piece.localPosition = originalLocalPos;
            yield return null;
        }

        piece.localScale = targetScale;
        piece.localPosition = originalLocalPos;
    }
    #endregion

    #region Rotation logic
    private void HandleRotationInput()
    {
        if (!allowModelRotation || rotationCoroutine != null) return;

        if (Input.GetKeyDown(KeyCode.Q)) RotateModel(90f);
        else if (Input.GetKeyDown(KeyCode.E)) RotateModel(-90f);
    }

    private void RotateModel(float deltaDegrees)
    {
        if (modelRoot == null || rotationCoroutine != null) return;
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
    }

    private void RestoreModelRotation()
    {
        if (modelRoot == null) return;
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RestoreRotationCoroutine());
    }

    private IEnumerator RestoreRotationCoroutine()
    {
        Quaternion start = modelRoot.rotation;
        Quaternion end = modelOriginalRotation;
        float t = 0f;
        while (t < rotationAnimTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / rotationAnimTime);
            modelRoot.rotation = Quaternion.Slerp(start, end, p);
            yield return null;
        }
        modelRoot.rotation = end;
        modelRotatedFromOriginal = false;
    }
    #endregion

    #region Trigger detection
    private void OnTriggerEnter(Collider other)
    {
        if (player == null || other.transform != player) return;

        playerInsideTrigger = true;
        SetCanvas(canvasA_EnterPrompt, true);

        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null || other.transform != player) return;

        playerInsideTrigger = false;
        SetCanvas(canvasA_EnterPrompt, false);
        SetCanvas(canvasB_InspectModel, false);
        isInspectingModel = false;

        if (isPieceSelected && selectedPiece != null)
        {
            if (exitCoroutine != null) StopCoroutine(exitCoroutine);
            exitCoroutine = StartCoroutine(ExitWaitAndRestore(selectedPiece, selectedPieceOriginalScale));
        }
        else
        {
            SetCanvas(canvasC_PieceDetail, false);
        }
    }

    private IEnumerator ExitWaitAndRestore(Transform pieceAtExit, Vector3 originalScale)
    {
        float waited = 0f;
        while (waited < exitWaitSeconds)
        {
            if (playerInsideTrigger)
            {
                exitCoroutine = null;
                yield break;
            }
            waited += Time.deltaTime;
            yield return null;
        }

        if (pieceAtExit != null)
        {
            if (pieceScaleCoroutine != null) StopCoroutine(pieceScaleCoroutine);
            pieceScaleCoroutine = StartCoroutine(ScaleOverTime(pieceAtExit, originalScale, pieceScaleTime));
        }

        SetCanvas(canvasC_PieceDetail, false);
        isPieceSelected = false;
        selectedPiece = null;

        if (modelRotatedFromOriginal)
        {
            yield return new WaitForSeconds(1f);
            RestoreModelRotation();
        }

        exitCoroutine = null;
    }
    #endregion

    #region Utilities
    private void SetCanvas(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable = visible;
    }
    #endregion
}
