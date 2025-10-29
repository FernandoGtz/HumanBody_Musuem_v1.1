using UnityEngine;
using UnityEngine.UI;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    [Header("HUD icons (checks) - assign 4 RawImages in same order that prefabs/spawner use")]
    public RawImage[] checkIcons; // 4 images: activarlas cuando se recolecte

    [Header("Initial states (default false = not collected)")]
    public bool[] initialCollected; // size recommended 4

    private bool[] collectedStates;

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

        // init
        for (int i = 0; i < n; i++)
        {
            bool val = (initialCollected != null && i < initialCollected.Length) ? initialCollected[i] : false;
            collectedStates[i] = val;

            if (checkIcons != null && i < checkIcons.Length && checkIcons[i] != null)
            {
                checkIcons[i].gameObject.SetActive(val);
            }
        }
    }

    /// <summary>
    /// Marca el objeto con id como recolectado (true) y actualiza el HUD.
    /// </summary>
    public void MarkCollected(int id)
    {
        if (id < 0 || id >= collectedStates.Length) return;

        collectedStates[id] = true;

        if (checkIcons != null && id < checkIcons.Length && checkIcons[id] != null)
        {
            checkIcons[id].gameObject.SetActive(true);
        }
    }

    public bool IsCollected(int id)
    {
        if (id < 0 || id >= collectedStates.Length) return false;
        return collectedStates[id];
    }

    // opcional: reset desde inspector / debug
    [ContextMenu("Reset All")]
    public void ResetAll()
    {
        for (int i = 0; i < collectedStates.Length; i++)
        {
            collectedStates[i] = false;
            if (checkIcons != null && i < checkIcons.Length && checkIcons[i] != null)
                checkIcons[i].gameObject.SetActive(false);
        }
    }
}
