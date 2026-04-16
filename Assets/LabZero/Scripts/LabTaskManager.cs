using System;
using TMPro;
using UnityEngine;

public class LabTaskManager : MonoBehaviour
{
    [Header("Optional UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text ppeStatusText;
    [SerializeField] private TMP_Text toolStatusText;
    [SerializeField] private TMP_Text hazardStatusText;
    [SerializeField] private TMP_Text summaryText;

    public event Action StateChanged;

    // Kept for compatibility with previous scene/UI bindings.
    public int PpeCount { get; private set; }
    public int ToolCount { get; private set; }
    public int HazardCount { get; private set; }
    public int PpeRequired => 1;
    public int ToolRequired => 1;
    public int HazardRequired => 1;

    public bool HasThemeSelection => SelectedTheme != LabThemeType.None;
    public bool PpeComplete => HasThemeSelection;
    public bool ToolComplete => HasThemeSelection;
    public bool HazardComplete => HasThemeSelection;
    public bool AllTasksComplete => HasThemeSelection;

    public LabThemeType SelectedTheme { get; private set; } = LabThemeType.None;
    public bool LessonStarted { get; private set; }
    public bool LessonContentComplete { get; private set; }
    public bool IsPlaying { get; private set; }
    public int LessonChunkIndex { get; private set; }
    public int LessonChunkCount => GetModules(SelectedTheme).Length;

    private void Start()
    {
        RefreshUi();
    }

    public void SelectTheme(LabThemeType themeType)
    {
        if (themeType == LabThemeType.None)
        {
            return;
        }

        SelectedTheme = themeType;
        LessonChunkIndex = 0;
        LessonStarted = true;
        LessonContentComplete = false;
        IsPlaying = false;
        RefreshUi();
        StateChanged?.Invoke();
    }

    public void ClearThemeSelection()
    {
        SelectedTheme = LabThemeType.None;
        LessonChunkIndex = 0;
        LessonStarted = false;
        LessonContentComplete = false;
        IsPlaying = false;
        RefreshUi();
        StateChanged?.Invoke();
    }

    public void TryStartLesson()
    {
        if (!HasThemeSelection)
        {
            return;
        }

        LessonStarted = true;
        IsPlaying = true;
        RefreshUi();
        StateChanged?.Invoke();
    }

    public void TogglePlayPause()
    {
        if (!HasThemeSelection)
        {
            return;
        }

        LessonStarted = true;
        IsPlaying = !IsPlaying;
        RefreshUi();
        StateChanged?.Invoke();
    }

    public void AdvanceLessonChunk()
    {
        if (!HasThemeSelection)
        {
            return;
        }

        var modules = GetModules(SelectedTheme);
        if (modules.Length == 0)
        {
            return;
        }

        LessonStarted = true;
        if (LessonChunkIndex < modules.Length - 1)
        {
            LessonChunkIndex++;
            LessonContentComplete = false;
        }
        else
        {
            LessonContentComplete = true;
        }

        RefreshUi();
        StateChanged?.Invoke();
    }

    public void RewindLessonChunk()
    {
        if (!HasThemeSelection)
        {
            return;
        }

        LessonChunkIndex = Mathf.Max(0, LessonChunkIndex - 1);
        LessonContentComplete = false;
        RefreshUi();
        StateChanged?.Invoke();
    }

    // Compatibility methods for old bindings.
    public void RegisterValidDrop(LabTaskType taskType)
    {
        CompleteTaskForDebug(taskType);
    }

    public void CompleteTaskForDebug(LabTaskType taskType)
    {
        switch (taskType)
        {
            case LabTaskType.PPE:
                SelectTheme(LabThemeType.EnglishCommunication);
                break;
            case LabTaskType.Tool:
                SelectTheme(LabThemeType.BasicMathematics);
                break;
            case LabTaskType.Hazard:
                SelectTheme(LabThemeType.DigitalSkills);
                break;
            default:
                break;
        }
    }

    public void ResetProgress()
    {
        ClearThemeSelection();
        PpeCount = 0;
        ToolCount = 0;
        HazardCount = 0;
        RefreshUi();
        StateChanged?.Invoke();
    }

    public string GetThemeDisplayName()
    {
        switch (SelectedTheme)
        {
            case LabThemeType.EnglishCommunication:
                return "English Communication";
            case LabThemeType.BasicMathematics:
                return "Basic Mathematics";
            case LabThemeType.DigitalSkills:
                return "Digital Skills";
            default:
                return "No Course";
        }
    }

    public string GetCurrentModuleTitle()
    {
        var modules = GetModules(SelectedTheme);
        if (modules.Length == 0)
        {
            return "Select a course to start";
        }

        return modules[Mathf.Clamp(LessonChunkIndex, 0, modules.Length - 1)];
    }

    public string GetCurrentVideoPlaceholderUrl()
    {
        var ids = GetVideoPlaceholderIds(SelectedTheme);
        if (ids.Length == 0)
        {
            return "https://video-placeholder.local/not-selected";
        }

        var id = ids[Mathf.Clamp(LessonChunkIndex, 0, ids.Length - 1)];
        return "https://video-placeholder.local/" + id;
    }

    public string GetInstructionText()
    {
        if (!HasThemeSelection)
        {
            return "Sit at the desk and choose a course: 1 English, 2 Math, 3 Digital Skills.";
        }

        var modules = GetModules(SelectedTheme);
        if (modules.Length == 0)
        {
            return "No modules available for this course.";
        }

        if (LessonContentComplete)
        {
            return "Course module complete. Press B to review previous or T to switch course.";
        }

        var playState = IsPlaying ? "Playing" : "Paused";
        return $"{playState} [{LessonChunkIndex + 1}/{modules.Length}] {modules[LessonChunkIndex]}";
    }

    public string GetSummaryText()
    {
        if (!HasThemeSelection)
        {
            return "Course Hub Ready";
        }

        if (LessonContentComplete)
        {
            return "Module Complete";
        }

        return IsPlaying ? "Video Running" : "Video Paused";
    }

    public Color GetSummaryColor()
    {
        if (!HasThemeSelection)
        {
            return new Color(0.45f, 0.8f, 1f);
        }

        if (LessonContentComplete)
        {
            return new Color(0.5f, 1f, 0.8f);
        }

        return IsPlaying ? new Color(0.5f, 1f, 0.9f) : new Color(1f, 0.8f, 0.25f);
    }

    public string GetDebugHintText()
    {
        return "1/2/3 select course | SPACE play/pause | N next | B back | T reset course | WASD move | Q/E turn | I/K up/down";
    }

    private string[] GetModules(LabThemeType theme)
    {
        switch (theme)
        {
            case LabThemeType.EnglishCommunication:
                return new[]
                {
                    "Everyday Conversation Basics",
                    "Workplace Communication",
                    "Presentation and Speaking Practice",
                };
            case LabThemeType.BasicMathematics:
                return new[]
                {
                    "Percentages and Proportions",
                    "Budgeting with Real Numbers",
                    "Problem Solving with Equations",
                };
            case LabThemeType.DigitalSkills:
                return new[]
                {
                    "Email and Calendar Mastery",
                    "Spreadsheet Fundamentals",
                    "Online Safety and Password Hygiene",
                };
            default:
                return Array.Empty<string>();
        }
    }

    private string[] GetVideoPlaceholderIds(LabThemeType theme)
    {
        switch (theme)
        {
            case LabThemeType.EnglishCommunication:
                return new[] { "eng-01", "eng-02", "eng-03" };
            case LabThemeType.BasicMathematics:
                return new[] { "math-01", "math-02", "math-03" };
            case LabThemeType.DigitalSkills:
                return new[] { "dig-01", "dig-02", "dig-03" };
            default:
                return Array.Empty<string>();
        }
    }

    private void RefreshUi()
    {
        if (titleText != null)
        {
            titleText.text = HasThemeSelection
                ? $"Learning Desk  |  {GetThemeDisplayName()}"
                : "Learning Desk  |  Course Selection";
        }

        if (instructionText != null)
        {
            instructionText.text = GetInstructionText();
        }

        if (ppeStatusText != null)
        {
            ppeStatusText.text = "Course: " + GetThemeDisplayName();
        }

        if (toolStatusText != null)
        {
            toolStatusText.text = "Module: " + GetCurrentModuleTitle();
        }

        if (hazardStatusText != null)
        {
            hazardStatusText.text = "Video: " + GetCurrentVideoPlaceholderUrl();
        }

        if (summaryText != null)
        {
            summaryText.text = GetSummaryText();
            summaryText.color = GetSummaryColor();
        }
    }
}
