using TMPro;
using UnityEngine;

public class LabDeskScreenPresenter : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private TMP_Text scenarioTitleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text settingsHeadingText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text overlayText;
    [SerializeField] private TMP_Text helpersText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text ctaText;
    [SerializeField] private GameObject errorOverlayPanel;
    [SerializeField] private TMP_Text errorOverlayTitleText;
    [SerializeField] private TMP_Text errorOverlayStateText;

    public void Configure(LabTaskManager manager)
    {
        taskManager = manager;
        AutoBindTexts();
        Refresh();
    }

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        AutoBindTexts();
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (taskManager == null)
        {
            return;
        }

        if (scenarioTitleText != null)
        {
            scenarioTitleText.text = taskManager.GetScenarioTitle();
        }

        if (objectiveText != null)
        {
            objectiveText.text = taskManager.GetObjectiveText();
        }

        if (settingsHeadingText != null)
        {
            settingsHeadingText.text = "Impostazioni sessione";
        }

        if (timerText != null)
        {
            timerText.text = $"Timer: {taskManager.GetTimerSummaryText()}";
        }

        if (overlayText != null)
        {
            overlayText.text = $"Overlay errori: {taskManager.GetErrorOverlayStateText()}";
        }

        if (helpersText != null)
        {
            helpersText.text = $"Aiuti: {taskManager.GetHelpersStateText()}";
        }

        if (modeText != null)
        {
            modeText.text = $"Modalita: {taskManager.GetRunModeText()}";
        }

        if (ctaText != null)
        {
            ctaText.text = taskManager.GetStartPromptText();
        }

        if (errorOverlayTitleText != null)
        {
            errorOverlayTitleText.text = "Errori";
        }

        if (errorOverlayStateText != null)
        {
            errorOverlayStateText.text = $"Visibilita overlay: {taskManager.GetErrorOverlayStateText()}";
        }

        if (errorOverlayPanel != null)
        {
            errorOverlayPanel.SetActive(taskManager.ShowErrorOverlay);
        }
    }

    private void AutoBindTexts()
    {
        scenarioTitleText ??= FindText("Screen Scenario Title");
        objectiveText ??= FindText("Screen Objective");
        settingsHeadingText ??= FindText("Screen Settings Heading");
        timerText ??= FindText("Screen Timer Row");
        overlayText ??= FindText("Screen Overlay Row");
        helpersText ??= FindText("Screen Helpers Row");
        modeText ??= FindText("Screen Mode Row");
        ctaText ??= FindText("Screen Cta Line");
        errorOverlayPanel ??= FindGameObject("Error Overlay Panel");
        errorOverlayTitleText ??= FindText("Error Overlay Title");
        errorOverlayStateText ??= FindText("Error Overlay State");
    }

    private TMP_Text FindText(string name)
    {
        var child = transform.Find(name);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<TMP_Text>();
    }

    private GameObject FindGameObject(string name)
    {
        var child = transform.Find(name);
        return child != null ? child.gameObject : null;
    }
}
