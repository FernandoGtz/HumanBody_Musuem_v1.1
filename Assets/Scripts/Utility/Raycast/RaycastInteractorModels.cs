using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RaycastInteractorModels : MonoBehaviour
{
    public static RaycastInteractorModels Instance;

    [Header("Raycast Settings")]
    public float maxDistance = 5f;
    public LayerMask interactableLayer;

    [Header("CanvasGroups (mantener activos en la jerarquía)")]
    public CanvasGroup promptGroup;
    public CanvasGroup infoModeGroup;
    public CanvasGroup objectInfoGroup;

    [Header("Object Info UI")]
    public TMP_Text objectNameText;
    public TMP_Text objectDescriptionText;
    public Image objectImage;

    [Header("Prompt Fade Settings")]
    public float promptDisplayTime = 3f;
    public float promptFadeDuration = 1f;

    [Header("Model Rotation Settings")]
    public KeyCode rotateLeftKey = KeyCode.R;
    public KeyCode rotateRightKey = KeyCode.T;
    public float rotationDuration = 0.5f;
    public Transform modelTransform;

    [Header("Muscle Isolation Settings")]
    public Vector3 isolationWorldOffset = new Vector3(1f, 0f, 0f);
    public float isolationScaleFactor = 1.3f;
    public float transitionDuration = 0.5f;
    public float returnWaitTime = 3f;

    [HideInInspector]
    public bool infoMode = false;
    private bool isPlayerInsideZone = false;
    private bool isPromptFading = false;
    private float promptTimer = 0f;
    private Coroutine currentFadeCoroutine;
    private InteractableInfo lastHit;

    // Nuevas variables para rotación y aislamiento
    private Quaternion modelOriginalRotation;
    private Vector3 modelOriginalScale;
    private int currentRotationStep = 0;
    private bool isRotating = false;
    private bool isMuscleIsolated = false;
    private Coroutine currentRotationCoroutine;
    private Coroutine currentIsolationCoroutine;
    private Coroutine returnTimerCoroutine;
    private Dictionary<Transform, Vector3> originalMusclePositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> originalMuscleScales = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        // Guardar estado original del modelo
        if (modelTransform != null)
        {
            modelOriginalRotation = modelTransform.rotation;
            modelOriginalScale = modelTransform.localScale;
        }

        ForceHideAll();
    }

    private void Update()
    {
        HandleRaycast();
        HandleInput();
        HandlePromptTimer();
        HandleRotationInput();
    }

    private void HandleRaycast()
    {
        if (!infoMode || isRotating || isMuscleIsolated)
        {
            if (lastHit != null && !isMuscleIsolated)
            {
                lastHit = null;
                ClearObjectInfo();
            }
            return;
        }

        if (!isPlayerInsideZone) return;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer))
        {
            InteractableInfo info = hit.collider.GetComponent<InteractableInfo>();

            if (info != null)
            {
                if (info != lastHit)
                {
                    lastHit = info;
                    ShowObjectInfo(info);
                    IsolateMuscle(info.transform);
                }
            }
            else
            {
                if (lastHit != null)
                {
                    lastHit = null;
                    ClearObjectInfo();
                }
            }
        }
        else
        {
            if (lastHit != null)
            {
                lastHit = null;
                ClearObjectInfo();
            }
        }
    }

    private void HandleInput()
    {
        if (!isPlayerInsideZone || isRotating || isMuscleIsolated) return;

        if (!infoMode && Input.GetKeyDown(KeyCode.F))
        {
            SetInfoMode(true);
        }
        else if (infoMode && Input.GetKeyDown(KeyCode.F))
        {
            SetInfoMode(false);
        }
    }

    private void HandleRotationInput()
    {
        if (!isPlayerInsideZone || infoMode || isRotating || isMuscleIsolated) return;

        if (Input.GetKeyDown(rotateLeftKey))
        {
            RotateModel(true); // Rotación izquierda
        }
        else if (Input.GetKeyDown(rotateRightKey))
        {
            RotateModel(false); // Rotación derecha
        }
    }

    private void HandlePromptTimer()
    {
        if (!isPlayerInsideZone) return;

        if (!infoMode && promptGroup.alpha > 0 && !isPromptFading)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptDisplayTime)
            {
                StartPromptFade();
            }
        }
    }

    private void RotateModel(bool rotateLeft)
    {
        if (isRotating || modelTransform == null) return;

        if (rotateLeft)
        {
            currentRotationStep = (currentRotationStep + 1) % 4;
        }
        else
        {
            currentRotationStep = (currentRotationStep - 1 + 4) % 4;
        }

        Quaternion targetRotation = modelOriginalRotation * Quaternion.Euler(0, currentRotationStep * 90, 0);
        currentRotationCoroutine = StartCoroutine(RotateModelCoroutine(targetRotation));
    }

    private IEnumerator RotateModelCoroutine(Quaternion targetRotation)
    {
        isRotating = true;
        Quaternion startRotation = modelTransform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            modelTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            yield return null;
        }

        modelTransform.rotation = targetRotation;
        isRotating = false;
        currentRotationCoroutine = null;
    }

    private void IsolateMuscle(Transform muscleTransform)
    {
        if (isMuscleIsolated || muscleTransform == null) return;

        // Guardar posición y escala original si es la primera vez
        if (!originalMusclePositions.ContainsKey(muscleTransform))
        {
            originalMusclePositions[muscleTransform] = muscleTransform.position;
            originalMuscleScales[muscleTransform] = muscleTransform.localScale;
        }

        Vector3 targetPosition = originalMusclePositions[muscleTransform] + isolationWorldOffset;
        Vector3 targetScale = originalMuscleScales[muscleTransform] * isolationScaleFactor;

        currentIsolationCoroutine = StartCoroutine(IsolateMuscleCoroutine(muscleTransform, targetPosition, targetScale));
    }

    private IEnumerator IsolateMuscleCoroutine(Transform muscleTransform, Vector3 targetPosition, Vector3 targetScale)
    {
        isMuscleIsolated = true;

        // Animación de ida (aislamiento)
        Vector3 startPosition = muscleTransform.position;
        Vector3 startScale = muscleTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            muscleTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            muscleTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        muscleTransform.position = targetPosition;
        muscleTransform.localScale = targetScale;

        // Esperar hasta que el usuario deje de apuntar
        while (lastHit != null && lastHit.transform == muscleTransform && isPlayerInsideZone && infoMode)
        {
            yield return null;
        }

        // Animación de vuelta (retorno)
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            muscleTransform.position = Vector3.Lerp(targetPosition, originalMusclePositions[muscleTransform], t);
            muscleTransform.localScale = Vector3.Lerp(targetScale, originalMuscleScales[muscleTransform], t);
            yield return null;
        }

        muscleTransform.position = originalMusclePositions[muscleTransform];
        muscleTransform.localScale = originalMuscleScales[muscleTransform];

        isMuscleIsolated = false;
        currentIsolationCoroutine = null;
    }

    private void StartReturnTimer()
    {
        if (returnTimerCoroutine != null)
            StopCoroutine(returnTimerCoroutine);

        returnTimerCoroutine = StartCoroutine(ReturnTimerCoroutine());
    }

    private IEnumerator ReturnTimerCoroutine()
    {
        yield return new WaitForSeconds(returnWaitTime);

        // Si después de 3 segundos no hay jugador, resetear rotación
        if (!isPlayerInsideZone && modelTransform != null)
        {
            currentRotationStep = 0;
            if (currentRotationCoroutine != null)
            {
                StopCoroutine(currentRotationCoroutine);
                currentRotationCoroutine = null;
            }
            modelTransform.rotation = modelOriginalRotation;
        }
    }

    public void PlayerEnteredZone()
    {
        isPlayerInsideZone = true;
        ShowPrompt(true);

        // Cancelar timer de retorno si existe
        if (returnTimerCoroutine != null)
        {
            StopCoroutine(returnTimerCoroutine);
            returnTimerCoroutine = null;
        }
    }

    public void PlayerExitedZone()
    {
        isPlayerInsideZone = false;
        ShowPrompt(false);
        SetInfoMode(false);

        // Iniciar timer para resetear rotación
        StartReturnTimer();
    }

    public void SetInfoMode(bool state)
    {
        infoMode = state;

        SetCanvasVisible(promptGroup, !state && isPlayerInsideZone);
        SetCanvasVisible(infoModeGroup, state);

        if (!state && isPlayerInsideZone)
        {
            ResetPromptTimer();
        }

        if (!state)
        {
            ClearObjectInfo();
            lastHit = null;
        }
    }

    // Los demás métodos permanecen igual...
    public void ShowPrompt(bool state)
    {
        if (state)
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            isPromptFading = false;
            SetCanvasVisible(promptGroup, true);
            ResetPromptTimer();
        }
        else
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            isPromptFading = false;
            promptTimer = 0f;
            SetCanvasVisible(promptGroup, false);
        }
    }

    public void ShowObjectInfo(InteractableInfo info)
    {
        if (info == null) return;

        SetCanvasVisible(objectInfoGroup, true);

        if (objectNameText != null) objectNameText.text = info.nombreObjeto ?? "";
        if (objectDescriptionText != null) objectDescriptionText.text = info.descripcion ?? "";
        if (objectImage != null)
        {
            objectImage.sprite = info.imagen;
            objectImage.enabled = info.imagen != null;
        }
    }

    public void ClearObjectInfo()
    {
        SetCanvasVisible(objectInfoGroup, false);

        if (objectNameText != null) objectNameText.text = "";
        if (objectDescriptionText != null) objectDescriptionText.text = "";
        if (objectImage != null) objectImage.sprite = null;
    }

    private void SetCanvasVisible(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        group.gameObject.SetActive(true);
    }

    private void ForceHideAll()
    {
        SetCanvasVisible(promptGroup, false);
        SetCanvasVisible(infoModeGroup, false);
        SetCanvasVisible(objectInfoGroup, false);
    }

    private void ResetPromptTimer()
    {
        promptTimer = 0f;
        isPromptFading = false;
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        
        if (promptGroup != null)
        {
            promptGroup.alpha = 1f;
        }
    }

    private void StartPromptFade()
    {
        if (!isPromptFading && promptGroup != null && isPlayerInsideZone && !infoMode)
        {
            isPromptFading = true;
            currentFadeCoroutine = StartCoroutine(FadePromptOut());
        }
    }

    private IEnumerator FadePromptOut()
    {
        float startAlpha = promptGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < promptFadeDuration && isPlayerInsideZone && !infoMode)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / promptFadeDuration);
            promptGroup.alpha = newAlpha;
            yield return null;
        }

        if (isPlayerInsideZone && !infoMode)
        {
            promptGroup.alpha = 0f;
        }
        
        isPromptFading = false;
        currentFadeCoroutine = null;
    }
}