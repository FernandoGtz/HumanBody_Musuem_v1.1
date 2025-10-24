using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("CanvasGroups (mantener activos en la jerarquía)")]
    public CanvasGroup promptGroup;
    public CanvasGroup infoModeGroup;
    public CanvasGroup objectInfoGroup;

    [Header("Object Info UI")]
    public TMP_Text objectNameText;
    public TMP_Text objectDescriptionText;
    public Image objectImage;

    [Header("Prompt Fade Settings")]
    public float promptDisplayTime = 3f; // Tiempo antes de empezar el fade
    public float promptFadeDuration = 1f; // Duración del efecto fade

    [HideInInspector]
    public bool infoMode = false;
    private bool isPlayerInsideZone = false;
    private bool isPromptFading = false;
    private float promptTimer = 0f;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        // Forzar invisibilidad al inicio
        ForceHideAll();
    }

    private void Update()
    {
        if (!isPlayerInsideZone) return;

        // Actualizar temporizador del prompt si está visible y no en modo info
        if (!infoMode && promptGroup.alpha > 0 && !isPromptFading)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptDisplayTime)
            {
                StartPromptFade();
            }
        }

        if (!infoMode && Input.GetKeyDown(KeyCode.F))
        {
            SetInfoMode(true);
        }
        else if (infoMode && Input.GetKeyDown(KeyCode.F))
        {
            SetInfoMode(false);
        }
    }

    public void PlayerEnteredZone()
    {
        isPlayerInsideZone = true;
        ShowPrompt(true);
        Debug.Log("[UIManager] Jugador entró al área — prompt visible");
    }

    public void PlayerExitedZone()
    {
        isPlayerInsideZone = false;
        ShowPrompt(false); // Esto debe forzar la ocultación inmediata
        SetInfoMode(false);
        Debug.Log("[UIManager] Jugador salió del área — UI limpia");
    }

    public void SetInfoMode(bool state)
    {
        infoMode = state;

        // Prompt visible si no está en modo info
        SetCanvasVisible(promptGroup, !state);
        // InfoMode visible si está activo
        SetCanvasVisible(infoModeGroup, state);

        // Reiniciar el fade del prompt cuando se activa/desactiva el modo info
        if (!state && isPlayerInsideZone)
        {
            ResetPromptTimer();
        }

        if (!state)
            ClearObjectInfo();

        Debug.Log("[UIManager] Modo información: " + state);
    }

    public void ShowPrompt(bool state)
    {
        if (state)
        {
            // Detener cualquier fade en progreso
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            isPromptFading = false;
            SetCanvasVisible(promptGroup, true);
            ResetPromptTimer();
            Debug.Log("[UIManager] Canvas Prompt mostrado");
        }
        else
        {
            // Detener cualquier fade en progreso y ocultar inmediatamente
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            isPromptFading = false;
            promptTimer = 0f;
            SetCanvasVisible(promptGroup, false);
            Debug.Log("[UIManager] Canvas Prompt oculto inmediatamente");
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

        Debug.Log("[UIManager] Mostrando info de objeto: " + info.nombreObjeto);
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
        group.gameObject.SetActive(true); // 🔥 Asegura que el GO esté activo visualmente
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
        
        // Detener fade actual si existe
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        
        // Asegurarse de que el prompt esté completamente visible
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

        // Solo completar el fade si todavía estamos en la zona y no en modo info
        if (isPlayerInsideZone && !infoMode)
        {
            promptGroup.alpha = 0f;
            Debug.Log("[UIManager] Prompt faded out completamente");
        }
        
        isPromptFading = false;
        currentFadeCoroutine = null;
    }
}