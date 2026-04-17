using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabGuideDirector : MonoBehaviour
{
    [SerializeField] private LabGuideLessonDefinition lessonDefinition;
    [SerializeField] private LabGuidePromptBubbleView promptBubble;
    [SerializeField] private LabGuideStatusLineView statusLine;
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private bool beginOnStart = true;

    private readonly List<LabGuideMistakeRecord> _mistakeLog = new();
    private Coroutine _reminderRoutine;
    private bool _lessonStarted;

    public int ActiveStepIndex { get; private set; } = -1;
    public LabGuideMode Mode { get; private set; } = LabGuideMode.Guided;
    public LabGuidePromptSeverity LastPromptSeverity { get; private set; } = LabGuidePromptSeverity.Info;
    public string LastPromptText { get; private set; } = string.Empty;
    public string LastStatusText { get; private set; } = string.Empty;

    public event Action<int, LabGuideStepDefinition> StepStarted;
    public event Action<LabGuidePromptSeverity, string, string> PromptChanged;
    public event Action<LabGuideMistakeRecord> MistakeRecorded;
    public event Action LessonCompleted;
    public event Action<LabGuideTargetKind, string> TargetChanged;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        if (beginOnStart && Application.isPlaying)
        {
            BeginLesson();
        }
    }

    private void OnDisable()
    {
        StopReminderRoutine();
    }

    public void BindPresentation(LabGuidePromptBubbleView bubble, LabGuideStatusLineView line)
    {
        promptBubble = bubble;
        statusLine = line;
    }

    public void BeginLesson()
    {
        ResolveReferences();
        EnsureLessonDefinition();

        if (lessonDefinition == null || lessonDefinition.Steps == null || lessonDefinition.Steps.Count == 0)
        {
            return;
        }

        _lessonStarted = true;
        Mode = LabGuideMode.Guided;
        _mistakeLog.Clear();
        StartStep(0);
    }

    public bool TryReportCondition(string signalId, GameObject source, bool forcedFailure = false)
    {
        if (!_lessonStarted || Mode != LabGuideMode.Guided || !HasActiveStep())
        {
            return false;
        }

        var step = lessonDefinition.Steps[ActiveStepIndex];
        if (forcedFailure || !string.Equals(signalId, step.RequiredSignalId, StringComparison.Ordinal))
        {
            var record = RecordMistake(
                step.StepId,
                string.IsNullOrWhiteSpace(signalId) ? "missing_signal" : signalId,
                "azione non coerente con il passaggio attivo",
                step.ObjectiveItalian,
                "torna al passaggio indicato dalla guida");
            ShowPrompt(
                LabGuidePromptSeverity.Warning,
                "Attenzione: " + record.RiskItalian + ". Correzione: " + record.RecoveryItalian + ".",
                "Obiettivo: " + step.ObjectiveItalian);
            return false;
        }

        StopReminderRoutine();
        ShowPrompt(LabGuidePromptSeverity.Success, "Corretto. " + step.SafetyReasonItalian, "Obiettivo completato.");

        if (ActiveStepIndex >= lessonDefinition.Steps.Count - 1)
        {
            CompleteGuidedLesson();
            return true;
        }

        StartStep(ActiveStepIndex + 1);
        return true;
    }

    public IReadOnlyList<LabGuideMistakeRecord> GetMistakeLog()
    {
        return _mistakeLog;
    }

    public void AdvanceReminderForTests(float elapsedSeconds)
    {
        if (Mode != LabGuideMode.Guided || !HasActiveStep())
        {
            return;
        }

        var step = lessonDefinition.Steps[ActiveStepIndex];
        var hintDelay = ResolveHintDelay(step);
        var warningDelay = ResolveWarningDelay(step, hintDelay);

        if (elapsedSeconds >= hintDelay)
        {
            ShowPrompt(LabGuidePromptSeverity.Hint, "Suggerimento: " + step.ObjectiveItalian, "Obiettivo: " + step.ObjectiveItalian);
        }

        if (elapsedSeconds >= warningDelay)
        {
            ShowPrompt(LabGuidePromptSeverity.Warning, "Attenzione: completa questo passaggio prima di continuare. " + step.SafetyReasonItalian, "Obiettivo: " + step.ObjectiveItalian);
        }
    }

    public void ReportFreeRoamSafetyEvent(LabSafetyInteractionContext context)
    {
        if (context == null || Mode != LabGuideMode.FreeRoam)
        {
            return;
        }

        if (!context.CountsAsMistake)
        {
            ShowPrompt(LabGuidePromptSeverity.Info, context.FeedbackText, "Esplorazione libera: continua in sicurezza.");
            return;
        }

        var risk = BuildFreeRoamRisk(context);
        var expected = BuildFreeRoamExpectedAction(context);
        var recovery = BuildFreeRoamRecovery(context);
        var record = RecordMistake("free_roam", "free_roam_" + context.ItemType, risk, expected, recovery);
        ShowPrompt(
            LabGuidePromptSeverity.Warning,
            "Attenzione: " + record.RiskItalian + ". Correzione: " + record.RecoveryItalian + ".",
            "Esplorazione libera: correggi e continua.");
    }

    private void StartStep(int index)
    {
        StopReminderRoutine();
        ActiveStepIndex = Mathf.Clamp(index, 0, lessonDefinition.Steps.Count - 1);
        var step = lessonDefinition.Steps[ActiveStepIndex];

        StepStarted?.Invoke(ActiveStepIndex, step);
        TargetChanged?.Invoke(step.TargetKind, step.TargetId);
        var prompt = step.ObjectiveItalian + " " + step.SafetyReasonItalian;
        if (ActiveStepIndex == 0)
        {
            prompt = LabGuidePromptBubbleView.StartupGreeting + " " + prompt;
        }

        ShowPrompt(LabGuidePromptSeverity.Info, prompt, "Obiettivo: " + step.ObjectiveItalian);
        _reminderRoutine = StartCoroutine(ReminderRoutine(step, ActiveStepIndex));
    }

    private IEnumerator ReminderRoutine(LabGuideStepDefinition step, int stepIndex)
    {
        var hintDelay = ResolveHintDelay(step);
        var warningDelay = ResolveWarningDelay(step, hintDelay);

        yield return new WaitForSecondsRealtime(hintDelay);
        if (Mode != LabGuideMode.Guided || stepIndex != ActiveStepIndex)
        {
            yield break;
        }

        ShowPrompt(LabGuidePromptSeverity.Hint, "Suggerimento: " + step.ObjectiveItalian, "Obiettivo: " + step.ObjectiveItalian);

        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, warningDelay - hintDelay));
        if (Mode != LabGuideMode.Guided || stepIndex != ActiveStepIndex)
        {
            yield break;
        }

        ShowPrompt(LabGuidePromptSeverity.Warning, "Attenzione: completa questo passaggio prima di continuare. " + step.SafetyReasonItalian, "Obiettivo: " + step.ObjectiveItalian);
    }

    private void CompleteGuidedLesson()
    {
        StopReminderRoutine();
        Mode = LabGuideMode.FreeRoam;
        ActiveStepIndex = Mathf.Max(0, lessonDefinition.Steps.Count - 1);
        ShowPrompt(
            LabGuidePromptSeverity.Success,
            "Percorso guidato completato. Ora puoi muoverti nello stabilimento, ma resta attento agli oggetti e alle distrazioni.",
            "Obiettivo: esplorazione libera in sicurezza.");
        LessonCompleted?.Invoke();
    }

    private LabGuideMistakeRecord RecordMistake(
        string stepId,
        string mistakeCode,
        string riskItalian,
        string expectedActionItalian,
        string recoveryItalian)
    {
        var record = new LabGuideMistakeRecord
        {
            StepId = stepId,
            MistakeCode = mistakeCode,
            RiskItalian = riskItalian,
            ExpectedActionItalian = expectedActionItalian,
            RecoveryItalian = recoveryItalian,
            TimestampSeconds = Time.realtimeSinceStartup,
        };

        _mistakeLog.Add(record);
        MistakeRecorded?.Invoke(record);
        return record;
    }

    private void ShowPrompt(LabGuidePromptSeverity severity, string prompt, string status)
    {
        LastPromptSeverity = severity;
        LastPromptText = prompt;
        LastStatusText = status;

        if (promptBubble != null)
        {
            promptBubble.ShowPrompt(prompt, severity);
        }

        if (statusLine != null)
        {
            statusLine.SetStatus(status);
        }

        PromptChanged?.Invoke(severity, prompt, status);
    }

    private bool HasActiveStep()
    {
        return lessonDefinition != null
            && lessonDefinition.Steps != null
            && ActiveStepIndex >= 0
            && ActiveStepIndex < lessonDefinition.Steps.Count;
    }

    private void ResolveReferences()
    {
        promptBubble ??= FindAnyObjectByType<LabGuidePromptBubbleView>();
        statusLine ??= FindAnyObjectByType<LabGuideStatusLineView>();
        sessionManager ??= FindAnyObjectByType<LabSessionManager>();
    }

    private void EnsureLessonDefinition()
    {
        if (lessonDefinition == null)
        {
            lessonDefinition = LabGuideLessonDefinition.CreateDefaultRuntimeLesson();
        }
    }

    private void StopReminderRoutine()
    {
        if (_reminderRoutine != null)
        {
            StopCoroutine(_reminderRoutine);
            _reminderRoutine = null;
        }
    }

    private static float ResolveHintDelay(LabGuideStepDefinition step)
    {
        return step.HintDelaySeconds > 0f ? step.HintDelaySeconds : 20f;
    }

    private static float ResolveWarningDelay(LabGuideStepDefinition step, float hintDelay)
    {
        if (step.WarningDelaySeconds > hintDelay)
        {
            return step.WarningDelaySeconds;
        }

        return Mathf.Max(hintDelay + 0.01f, 45f);
    }

    private static string BuildFreeRoamRisk(LabSafetyInteractionContext context)
    {
        return context.ItemType == LabSafetyItemType.Phone
            ? "il telefono distrae in area operativa"
            : context.FeedbackText;
    }

    private static string BuildFreeRoamExpectedAction(LabSafetyInteractionContext context)
    {
        return context.ItemType == LabSafetyItemType.Phone
            ? "non usare il telefono mentre lavori"
            : "usa gli oggetti solo quando sono coerenti con l'area";
    }

    private static string BuildFreeRoamRecovery(LabSafetyInteractionContext context)
    {
        return context.ItemType == LabSafetyItemType.Phone
            ? "riponi il telefono e continua solo quando sei fermo in sicurezza"
            : "riponi l'oggetto e torna all'attivita in sicurezza";
    }
}
