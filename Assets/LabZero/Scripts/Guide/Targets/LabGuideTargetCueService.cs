using UnityEngine;

public sealed class LabGuideTargetCueService : MonoBehaviour
{
    [SerializeField] private LabGuideDirector director;
    [SerializeField] private LabGuideTargetRegistry registry;
    [SerializeField] private LabGuideRobotPresenter robotPresenter;
    [SerializeField] private LabSessionManager sessionManager;

    private LabGuideTargetKind _currentKind;
    private string _currentTargetId;
    private LabGuideTargetHighlighter _activeHighlighter;
    private LabGuideAreaCue _activeAreaCue;
    private bool _directorSubscribed;
    private bool _sessionSubscribed;

    public bool StrongCueActive =>
        (_activeHighlighter != null && _activeHighlighter.IsHighlighted)
        || (_activeAreaCue != null && _activeAreaCue.IsPulseActive);

    public LabGuideTargetHighlighter ActiveHighlighter => _activeHighlighter;
    public LabGuideAreaCue ActiveAreaCue => _activeAreaCue;
    public Transform CurrentFocusTarget { get; private set; }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
        RefreshCurrentTarget();
    }

    private void OnDisable()
    {
        Unsubscribe();
        ClearStrongCue();
    }

    public void Bind(
        LabGuideDirector guideDirector,
        LabGuideTargetRegistry targetRegistry,
        LabGuideRobotPresenter presenter,
        LabSessionManager manager = null)
    {
        Unsubscribe();
        director = guideDirector;
        registry = targetRegistry;
        robotPresenter = presenter;
        sessionManager = manager;
        ResolveReferences();
        Subscribe();
        RefreshCurrentTarget();
    }

    public void RefreshCurrentTarget()
    {
        if (string.IsNullOrWhiteSpace(_currentTargetId) && director != null && director.TryGetActiveTarget(out var kind, out var targetId))
        {
            _currentKind = kind;
            _currentTargetId = targetId;
        }

        if (string.IsNullOrWhiteSpace(_currentTargetId))
        {
            return;
        }

        ApplyTarget(_currentKind, _currentTargetId);
    }

    public void ClearStrongCue()
    {
        if (_activeHighlighter != null)
        {
            _activeHighlighter.SetActivePulse(false);
            _activeHighlighter = null;
        }

        if (_activeAreaCue != null)
        {
            _activeAreaCue.SetActivePulse(false);
            _activeAreaCue = null;
        }
    }

    private void OnTargetChanged(LabGuideTargetKind kind, string targetId)
    {
        _currentKind = kind;
        _currentTargetId = targetId;
        ApplyTarget(kind, targetId);
    }

    private void ApplyTarget(LabGuideTargetKind kind, string targetId)
    {
        ClearStrongCue();
        CurrentFocusTarget = null;

        if (registry == null || !registry.TryResolve(targetId, out var target))
        {
            robotPresenter?.SetFocusTarget(null, true);
            return;
        }

        CurrentFocusTarget = target.FocusTransform;
        var helpersEnabled = HelpersEnabled();
        robotPresenter?.SetFocusTarget(CurrentFocusTarget, !helpersEnabled);

        if (!helpersEnabled)
        {
            return;
        }

        if (kind == LabGuideTargetKind.Object && target.Renderer != null)
        {
            _activeHighlighter = ResolveHighlighter(target.Renderer);
            _activeHighlighter.SetActivePulse(true);
            return;
        }

        if (kind == LabGuideTargetKind.Area && target.AreaCue != null)
        {
            _activeAreaCue = target.AreaCue;
            _activeAreaCue.SetActivePulse(true);
        }
    }

    private LabGuideTargetHighlighter ResolveHighlighter(Renderer targetRenderer)
    {
        var highlighter = targetRenderer.GetComponent<LabGuideTargetHighlighter>();
        if (highlighter == null)
        {
            highlighter = targetRenderer.gameObject.AddComponent<LabGuideTargetHighlighter>();
        }

        highlighter.Bind(targetRenderer);
        return highlighter;
    }

    private bool HelpersEnabled()
    {
        if (sessionManager != null && sessionManager.Settings != null)
        {
            return sessionManager.Settings.HelpersEnabled;
        }

        var taskManager = FindAnyObjectByType<LabTaskManager>();
        if (taskManager != null)
        {
            return taskManager.HelpersEnabled;
        }

        return true;
    }

    private void ResolveReferences()
    {
        director ??= FindAnyObjectByType<LabGuideDirector>();
        registry ??= FindAnyObjectByType<LabGuideTargetRegistry>();
        robotPresenter ??= FindAnyObjectByType<LabGuideRobotPresenter>();
        sessionManager ??= FindAnyObjectByType<LabSessionManager>();
    }

    private void Subscribe()
    {
        if (!_directorSubscribed && director != null)
        {
            director.TargetChanged += OnTargetChanged;
            _directorSubscribed = true;
        }

        if (!_sessionSubscribed && sessionManager != null)
        {
            sessionManager.StateChanged += RefreshCurrentTarget;
            _sessionSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (_directorSubscribed && director != null)
        {
            director.TargetChanged -= OnTargetChanged;
        }

        if (_sessionSubscribed && sessionManager != null)
        {
            sessionManager.StateChanged -= RefreshCurrentTarget;
        }

        _directorSubscribed = false;
        _sessionSubscribed = false;
    }
}
