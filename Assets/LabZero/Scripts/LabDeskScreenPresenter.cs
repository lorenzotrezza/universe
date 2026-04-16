using TMPro;
using UnityEngine;

public class LabDeskScreenPresenter : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private TMP_Text courseTitleText;
    [SerializeField] private TMP_Text moduleTitleText;
    [SerializeField] private TMP_Text videoPlaceholderText;
    [SerializeField] private TMP_Text controlsHintText;

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

        if (courseTitleText != null)
        {
            courseTitleText.text = taskManager.HasThemeSelection
                ? taskManager.GetThemeDisplayName()
                : "Select a Course";
        }

        if (moduleTitleText != null)
        {
            moduleTitleText.text = taskManager.GetCurrentModuleTitle();
        }

        if (videoPlaceholderText != null)
        {
            videoPlaceholderText.text = "Video Source: " + taskManager.GetCurrentVideoPlaceholderUrl();
        }

        if (controlsHintText != null)
        {
            controlsHintText.text = taskManager.GetInstructionText();
        }
    }

    private void AutoBindTexts()
    {
        courseTitleText ??= FindText("Screen Course Title");
        moduleTitleText ??= FindText("Screen Module Title");
        videoPlaceholderText ??= FindText("Screen Video Url");
        controlsHintText ??= FindText("Screen Controls Hint");
    }

    private TMP_Text FindText(string name)
    {
        var t = transform.Find(name);
        if (t == null)
        {
            return null;
        }

        return t.GetComponent<TMP_Text>();
    }
}
