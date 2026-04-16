using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LabDeskCommandPad : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private LabDeskCommandType commandType;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Color idleColor = new(0.3f, 0.5f, 0.8f, 1f);
    [SerializeField] private Color activeColor = Color.white;

    private Material _runtimeMaterial;

    public void Configure(
        LabTaskManager manager,
        LabDeskCommandType command,
        Renderer renderer,
        TMP_Text textLabel,
        Color baseColor,
        Color selectedColor)
    {
        taskManager = manager;
        commandType = command;
        targetRenderer = renderer;
        label = textLabel;
        idleColor = baseColor;
        activeColor = selectedColor;
        CacheRuntimeMaterial();
        RefreshVisual();
    }

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        targetRenderer ??= GetComponent<Renderer>();
        CacheRuntimeMaterial();
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged += RefreshVisual;
        }

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged -= RefreshVisual;
        }
    }

    private void OnMouseDown()
    {
        ExecuteCommand();
    }

    private void ExecuteCommand()
    {
        if (taskManager == null)
        {
            return;
        }

        switch (commandType)
        {
            case LabDeskCommandType.SelectEnglish:
                taskManager.SelectTheme(LabThemeType.EnglishCommunication);
                break;
            case LabDeskCommandType.SelectMath:
                taskManager.SelectTheme(LabThemeType.BasicMathematics);
                break;
            case LabDeskCommandType.SelectDigital:
                taskManager.SelectTheme(LabThemeType.DigitalSkills);
                break;
            case LabDeskCommandType.TogglePlayPause:
                taskManager.TogglePlayPause();
                break;
            case LabDeskCommandType.NextModule:
                taskManager.AdvanceLessonChunk();
                break;
            case LabDeskCommandType.PreviousModule:
                taskManager.RewindLessonChunk();
                break;
            case LabDeskCommandType.ResetCourse:
                taskManager.ClearThemeSelection();
                break;
        }
    }

    private void RefreshVisual()
    {
        if (_runtimeMaterial == null)
        {
            return;
        }

        var selected = IsCommandActive();
        _runtimeMaterial.color = selected ? activeColor : idleColor;
        if (label != null)
        {
            label.color = selected ? Color.black : Color.white;
        }
    }

    private bool IsCommandActive()
    {
        if (taskManager == null)
        {
            return false;
        }

        switch (commandType)
        {
            case LabDeskCommandType.SelectEnglish:
                return taskManager.SelectedTheme == LabThemeType.EnglishCommunication;
            case LabDeskCommandType.SelectMath:
                return taskManager.SelectedTheme == LabThemeType.BasicMathematics;
            case LabDeskCommandType.SelectDigital:
                return taskManager.SelectedTheme == LabThemeType.DigitalSkills;
            case LabDeskCommandType.TogglePlayPause:
                return taskManager.IsPlaying;
            default:
                return false;
        }
    }

    private void CacheRuntimeMaterial()
    {
        if (targetRenderer != null)
        {
            _runtimeMaterial = targetRenderer.material;
        }
    }
}

public enum LabDeskCommandType
{
    SelectEnglish = 0,
    SelectMath = 1,
    SelectDigital = 2,
    TogglePlayPause = 3,
    NextModule = 4,
    PreviousModule = 5,
    ResetCourse = 6,
}
