using System.Collections.Generic;
using UnityEngine;

public class InspectorCanvasManager : MonoBehaviour
{
    public static InspectorCanvasManager Instance;

    [Header("Canvases Compartidos")]
    [SerializeField] private CanvasGroup canvasC_PieceDetail; // Canvas C compartido

    [Header("Referencias a Canvas A y B (opcionales)")]
    [SerializeField] private CanvasGroup canvasA_EnterPrompt;
    [SerializeField] private CanvasGroup canvasB_InspectModel;

    private List<ModelInspectorController> activeInspectors = new List<ModelInspectorController>();
    private ModelInspectorController currentInspectingModel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterInspector(ModelInspectorController inspector)
    {
        if (!activeInspectors.Contains(inspector))
        {
            activeInspectors.Add(inspector);
        }
    }

    public void UnregisterInspector(ModelInspectorController inspector)
    {
        activeInspectors.Remove(inspector);
        if (currentInspectingModel == inspector)
        {
            currentInspectingModel = null;
        }
    }

    public void SetCurrentInspectingModel(ModelInspectorController inspector)
    {
        // Si otro modelo estaba inspeccionando, lo sacamos del modo inspección
        if (currentInspectingModel != null && currentInspectingModel != inspector)
        {
            currentInspectingModel.ForceExitInspection();
        }

        currentInspectingModel = inspector;
        UpdateCanvasStates();
    }

    public void ClearCurrentInspectingModel(ModelInspectorController inspector)
    {
        if (currentInspectingModel == inspector)
        {
            currentInspectingModel = null;
            UpdateCanvasStates();
        }
    }

    public void UpdateCanvasStates()
    {
        // Oculta todos los Canvas A y B primero
        HideAllModelCanvases();

        // Si hay un modelo en inspección, muestra SUS Canvas B
        if (currentInspectingModel != null)
        {
            currentInspectingModel.SetLocalCanvasB(true);
        }

        // Actualiza Canvas A para todos los modelos que están siendo mirados
        foreach (var inspector in activeInspectors)
        {
            if (inspector != currentInspectingModel && inspector.IsLookingAtModel)
            {
                inspector.SetLocalCanvasA(true);
            }
        }
    }

    private void HideAllModelCanvases()
    {
        foreach (var inspector in activeInspectors)
        {
            inspector.SetLocalCanvasA(false);
            inspector.SetLocalCanvasB(false);
        }
    }

    // Métodos para acceder al Canvas C compartido
    public void SetCanvasC(bool visible)
    {
        if (canvasC_PieceDetail != null)
        {
            canvasC_PieceDetail.alpha = visible ? 1f : 0f;
            canvasC_PieceDetail.blocksRaycasts = visible;
            canvasC_PieceDetail.interactable = visible;
        }
    }

    public CanvasGroup GetCanvasC()
    {
        return canvasC_PieceDetail;
    }
}