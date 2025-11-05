using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    [Header("HUD icons (checks) - assign 4 RawImages in same order that prefabs/spawner use")]
    public RawImage[] checkIcons; // Activar cuando se recolecte

    [Header("Initial states (default false = not collected)")]
    public bool[] initialCollected; // size recomendado 4

    private bool[] collectedStates;

    [Header("Canvas que aparece al recolectar TODO")]
    public CanvasGroup completionCanvas;      // Asigna un CanvasGroup en el inspector
    public float fadeDuration = 1f;           // Tiempo de fade in/out
    public float displayTime = 5f;            // Tiempo visible antes de desaparecer

    private bool allCollectedTriggered = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        int n = Mathf.Max(4, checkIcons?.Length ?? 0);
        collectedStates = new bool[n];

        // Init collected states e íconos
        for (int i = 0; i < n; i++)
        {
            bool val = (initialCollected != null && i < initialCollected.Length) ? initialCollected[i] : false;
            collectedStates[i] = val;

            if (checkIcons != null && i < checkIcons.Length && checkIcons[i] != null)
                checkIcons[i].gameObject.SetActive(val);
        }

        // Init del Canvas de finalización
        if (completionCanvas != null)
        {
            completionCanvas.alpha = 0;
            completionCanvas.interactable = false;
            completionCanvas.blocksRaycasts = false;
        }
    }

    public void MarkCollected(int id)
    {
        if (id < 0 || id >= collectedStates.Length) return;
        collectedStates[id] = true;

        // ACTIVAR icono HUD
        if (checkIcons != null && id < checkIcons.Length && checkIcons[id] != null)
            checkIcons[id].gameObject.SetActive(true);

        // Revisar si todos los objetos están recolectados
        if (!allCollectedTriggered && AllCollected())
        {
            allCollectedTriggered = true;
            if (completionCanvas != null)
                StartCoroutine(ShowCompletionCanvas());
        }
    }

    public bool IsCollected(int id)
    {
        if (id < 0 || id >= collectedStates.Length) return false;
        return collectedStates[id];
    }

    [ContextMenu("Reset All")]
    public void ResetAll()
    {
        allCollectedTriggered = false;

        for (int i = 0; i < collectedStates.Length; i++)
        {
            collectedStates[i] = false;
            if (checkIcons != null && i < checkIcons.Length && checkIcons[i] != null)
                checkIcons[i].gameObject.SetActive(false);
        }

        if (completionCanvas != null)
        {
            completionCanvas.alpha = 0;
            completionCanvas.interactable = false;
            completionCanvas.blocksRaycasts = false;
        }
    }

    // --- Función para verificar si todos están recolectados ---
    private bool AllCollected()
    {
        foreach (bool collected in collectedStates)
        {
            if (!collected) return false;
        }
        return true;
    }

    // --- Corrutina para mostrar el Canvas con Fade y luego ocultarlo ---
    private IEnumerator ShowCompletionCanvas()
    {
        // Fade In
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            completionCanvas.alpha = alpha;
            yield return null;
        }

        completionCanvas.alpha = 1;
        completionCanvas.interactable = true;
        completionCanvas.blocksRaycasts = true;

        // Mantener visible
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0f;
        completionCanvas.interactable = false;
        completionCanvas.blocksRaycasts = false;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            completionCanvas.alpha = alpha;
            yield return null;
        }
        completionCanvas.alpha = 0;
    }
}
