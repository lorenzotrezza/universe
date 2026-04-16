using TMPro;
using UnityEngine;

public class LabStatusPanel : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text ppeStatusText;
    [SerializeField] private TMP_Text toolStatusText;
    [SerializeField] private TMP_Text hazardStatusText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text debugHintText;

    private void OnEnable()
    {
        AutoWireIfNeeded();

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

    public void Refresh()
    {
        if (taskManager == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = taskManager.HasThemeSelection
                ? $"LabZero VR  |  {taskManager.GetThemeDisplayName()}"
                : "LabZero VR  |  Theme Hub";
        }

        if (instructionText != null)
        {
            instructionText.text = taskManager.GetInstructionText();
        }

        if (ppeStatusText != null)
        {
            ppeStatusText.text = $"PPE: {taskManager.PpeCount}/{taskManager.PpeRequired}";
        }

        if (toolStatusText != null)
        {
            toolStatusText.text = $"Bench Setup: {taskManager.ToolCount}/{taskManager.ToolRequired}";
        }

        if (hazardStatusText != null)
        {
            hazardStatusText.text = $"Hazard Check: {taskManager.HazardCount}/{taskManager.HazardRequired}";
        }

        if (summaryText != null)
        {
            summaryText.text = taskManager.GetSummaryText();
            summaryText.color = taskManager.GetSummaryColor();
        }

        if (debugHintText != null)
        {
            debugHintText.text = taskManager.GetDebugHintText();
        }
    }

    private void AutoWireIfNeeded()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();

        if (titleText != null && instructionText != null && ppeStatusText != null && toolStatusText != null && hazardStatusText != null && summaryText != null && debugHintText != null)
        {
            return;
        }

        titleText ??= FindChildText("Panel/Title");
        instructionText ??= FindChildText("Panel/Instruction");
        ppeStatusText ??= FindChildText("Panel/PPE Status");
        toolStatusText ??= FindChildText("Panel/Tool Status");
        hazardStatusText ??= FindChildText("Panel/Hazard Status");
        summaryText ??= FindChildText("Panel/Summary");
        debugHintText ??= FindChildText("Panel/Debug Hint");
    }

    private TMP_Text FindChildText(string path)
    {
        var child = transform.Find(path);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<TMP_Text>();
    }
}
