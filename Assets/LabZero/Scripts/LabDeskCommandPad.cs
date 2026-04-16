using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class LabDeskCommandPad : MonoBehaviour
{
    private const float HoverDurationSeconds = 0.12f; // 120ms hover/aim response
    private const float PressDurationSeconds = 0.08f; // 80ms press response
    private const float CtaPressLockSeconds = 0.30f; // 300ms CTA press lock
    private const float DisabledBrightnessFactor = 0.40f; // 40% brightness at timer bounds
    private const float HoverLiftAmount = 0.01f;
    private const float PressDipAmount = 0.01f;

    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private LabDeskCommandType commandType;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text label;
    [SerializeField] private XRSimpleInteractable xrInteractable;
    [SerializeField] private Color idleColor = new(0.12f, 0.18f, 0.24f, 1f);
    [SerializeField] private Color activeColor = new(0.95f, 0.66f, 0.05f, 1f);

    private Material _runtimeMaterial;
    private Vector3 _baseLocalPosition;
    private Color _baseVisualColor;
    private float _hoverUntil;
    private float _pressUntil;
    private float _ctaLockedUntil;
    private bool _hovered;

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
        _baseLocalPosition = transform.localPosition;
        RefreshVisual();
    }

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        targetRenderer ??= GetComponent<Renderer>();
        xrInteractable ??= GetComponent<XRSimpleInteractable>();
        xrInteractable ??= gameObject.AddComponent<XRSimpleInteractable>();
        CacheRuntimeMaterial();
        _baseLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged += RefreshVisual;
        }

        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.AddListener(OnXrSelectEntered);
        }

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged -= RefreshVisual;
        }

        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnXrSelectEntered);
        }
    }

    private void OnMouseEnter()
    {
        if (IsDisabledCommand())
        {
            return;
        }

        _hovered = true;
        _hoverUntil = Time.unscaledTime + HoverDurationSeconds;
    }

    private void OnMouseExit()
    {
        _hovered = false;
    }

    private void OnMouseDown()
    {
        Activate();
    }

    private void OnXrSelectEntered(SelectEnterEventArgs args)
    {
        Activate();
    }

    private void Update()
    {
        UpdateFeedback();
    }

    public void Activate()
    {
        if (taskManager == null || IsDisabledCommand())
        {
            return;
        }

        var now = Time.unscaledTime;
        if (commandType == LabDeskCommandType.StartTraining && now < _ctaLockedUntil)
        {
            return;
        }

        ExecuteCommand();

        _pressUntil = now + PressDurationSeconds;
        if (commandType == LabDeskCommandType.StartTraining)
        {
            _ctaLockedUntil = now + CtaPressLockSeconds;
        }
    }

    private void ExecuteCommand()
    {
        switch (commandType)
        {
            case LabDeskCommandType.TimerDown:
                taskManager.AdjustTimer(-1);
                break;
            case LabDeskCommandType.TimerUp:
                taskManager.AdjustTimer(1);
                break;
            case LabDeskCommandType.ToggleErrorOverlay:
                taskManager.ToggleErrorOverlay();
                break;
            case LabDeskCommandType.ToggleHelpers:
                taskManager.ToggleHelpers();
                break;
            case LabDeskCommandType.ToggleMode:
                taskManager.ToggleRunMode();
                break;
            case LabDeskCommandType.StartTraining:
                taskManager.StartConfiguredRun();
                LabSceneTransition.LoadWarehouse();
                break;
            case LabDeskCommandType.ResetLobby:
                taskManager.ResetLobbyConfiguration();
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
        var baseColor = selected ? activeColor : idleColor;
        if (IsDisabledCommand())
        {
            baseColor *= DisabledBrightnessFactor;
            baseColor.a = 1f;
        }

        _baseVisualColor = baseColor;
        ApplyVisual(baseColor);
    }

    private bool IsCommandActive()
    {
        if (taskManager == null)
        {
            return false;
        }

        switch (commandType)
        {
            case LabDeskCommandType.ToggleErrorOverlay:
                return taskManager.ShowErrorOverlay;
            case LabDeskCommandType.ToggleHelpers:
                return taskManager.HelpersEnabled;
            case LabDeskCommandType.ToggleMode:
                return taskManager.RunMode == LabRunModeType.Live;
            case LabDeskCommandType.StartTraining:
                return taskManager.RunConfigured;
            default:
                return false;
        }
    }

    private bool IsDisabledCommand()
    {
        if (taskManager == null)
        {
            return false;
        }

        return commandType switch
        {
            LabDeskCommandType.TimerDown => taskManager.TimerMinutes <= LabTaskManager.MinimumTimerMinutes,
            LabDeskCommandType.TimerUp => taskManager.TimerMinutes >= LabTaskManager.MaximumTimerMinutes,
            _ => false,
        };
    }

    private void UpdateFeedback()
    {
        if (_runtimeMaterial == null)
        {
            return;
        }

        var now = Time.unscaledTime;
        var isPressed = now < _pressUntil;
        var canHover = !IsDisabledCommand() && (_hovered || now < _hoverUntil);
        var hoverFactor = canHover ? 1f : 0f;
        var pressFactor = isPressed ? 1f : 0f;

        var offset = Vector3.up * (hoverFactor * HoverLiftAmount - pressFactor * PressDipAmount);
        transform.localPosition = _baseLocalPosition + offset;

        var color = _baseVisualColor;
        if (canHover)
        {
            color = Color.Lerp(color, Color.white, 0.18f);
        }

        if (isPressed)
        {
            color = Color.Lerp(color, Color.black, 0.12f);
        }

        ApplyVisual(color);
    }

    private void ApplyVisual(Color color)
    {
        _runtimeMaterial.color = color;
        if (label != null)
        {
            label.color = IsCommandActive() ? Color.black : Color.white;
        }
    }

    private void CacheRuntimeMaterial()
    {
        if (targetRenderer != null)
        {
            _runtimeMaterial = Application.isPlaying ? targetRenderer.material : targetRenderer.sharedMaterial;
        }
    }
}

public enum LabDeskCommandType
{
    TimerDown = 0,
    TimerUp = 1,
    ToggleErrorOverlay = 2,
    ToggleHelpers = 3,
    ToggleMode = 4,
    StartTraining = 5,
    ResetLobby = 6,
}
