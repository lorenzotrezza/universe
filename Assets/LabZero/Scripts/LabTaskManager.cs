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

    [Header("Lobby Configuration")]
    [SerializeField] private int timerMinutes = 7;
    [SerializeField] private bool showErrorOverlay;
    [SerializeField] private bool helpersEnabled = true;
    [SerializeField] private LabRunModeType runMode = LabRunModeType.Simulation;

    private bool _runConfigured;

    public const int MinimumTimerMinutes = 5;
    public const int MaximumTimerMinutes = 10;

    public event Action StateChanged;

    public int TimerMinutes => timerMinutes;
    public bool ShowErrorOverlay => showErrorOverlay;
    public bool HelpersEnabled => helpersEnabled;
    public LabRunModeType RunMode => runMode;
    public bool RunConfigured => _runConfigured;

    private void Start()
    {
        RefreshUi();
    }

    public void AdjustTimer(int deltaMinutes)
    {
        var next = timerMinutes + deltaMinutes;
        var clamped = next < MinimumTimerMinutes
            ? MinimumTimerMinutes
            : (next > MaximumTimerMinutes ? MaximumTimerMinutes : next);
        if (clamped == timerMinutes)
        {
            return;
        }

        timerMinutes = clamped;
        NotifyStateChanged();
    }

    public void ToggleErrorOverlay()
    {
        showErrorOverlay = !showErrorOverlay;
        NotifyStateChanged();
    }

    public void ToggleHelpers()
    {
        helpersEnabled = !helpersEnabled;
        NotifyStateChanged();
    }

    public void ToggleRunMode()
    {
        runMode = runMode == LabRunModeType.Simulation
            ? LabRunModeType.Live
            : LabRunModeType.Simulation;
        NotifyStateChanged();
    }

    public void StartConfiguredRun()
    {
        if (_runConfigured)
        {
            return;
        }

        _runConfigured = true;
        NotifyStateChanged();
    }

    public void ResetLobbyConfiguration()
    {
        var changed = false;

        if (timerMinutes != 7)
        {
            timerMinutes = 7;
            changed = true;
        }

        if (showErrorOverlay)
        {
            showErrorOverlay = false;
            changed = true;
        }

        if (!helpersEnabled)
        {
            helpersEnabled = true;
            changed = true;
        }

        if (runMode != LabRunModeType.Simulation)
        {
            runMode = LabRunModeType.Simulation;
            changed = true;
        }

        if (_runConfigured)
        {
            _runConfigured = false;
            changed = true;
        }

        if (changed)
        {
            NotifyStateChanged();
        }
    }

    public void RegisterValidDrop(LabTaskType taskType)
    {
        if (taskType != LabTaskType.None)
        {
            StartConfiguredRun();
        }
    }

    public void CompleteTaskForDebug(LabTaskType taskType)
    {
        RegisterValidDrop(taskType);
    }

    public string GetScenarioTitle()
    {
        return "Briefing Sicurezza Magazzino";
    }

    public string GetObjectiveText()
    {
        return "Configura la sessione prima di entrare nell'area operativa.";
    }

    public string GetTimerSummaryText()
    {
        return $"{TimerMinutes} min";
    }

    public string GetErrorOverlayStateText()
    {
        return ShowErrorOverlay ? "Visibile" : "Nascosta";
    }

    public string GetHelpersStateText()
    {
        return HelpersEnabled ? "Attivi" : "Disattivati";
    }

    public string GetRunModeText()
    {
        return RunMode == LabRunModeType.Simulation ? "Simulazione" : "Live";
    }

    public string GetLobbyStatusText()
    {
        return RunConfigured ? "Configurazione confermata" : "Lobby pronta";
    }

    public string GetStartPromptText()
    {
        return RunConfigured
            ? "Preparazione sessione..."
            : "Quando sei pronto, premi Avvia Addestramento.";
    }

    public string GetSummaryText()
    {
        return GetLobbyStatusText();
    }

    public Color GetSummaryColor()
    {
        return RunConfigured
            ? new Color(0.50f, 0.90f, 0.65f)
            : new Color(0.95f, 0.68f, 0.10f);
    }

    public string GetDebugHintText()
    {
        return "Timer: -/+ | Overlay: O | Aiuti: H | Modalita: M | Avvia: INVIO | Reset: R | Movimento: WASD, Q/E, I/K";
    }

    private void NotifyStateChanged()
    {
        RefreshUi();
        StateChanged?.Invoke();
    }

    private void RefreshUi()
    {
        if (titleText != null)
        {
            titleText.text = GetScenarioTitle();
        }

        if (instructionText != null)
        {
            instructionText.text = GetObjectiveText();
        }

        if (ppeStatusText != null)
        {
            ppeStatusText.text = $"Timer: {GetTimerSummaryText()}";
        }

        if (toolStatusText != null)
        {
            toolStatusText.text = $"Overlay errori: {GetErrorOverlayStateText()}";
        }

        if (hazardStatusText != null)
        {
            hazardStatusText.text = $"Aiuti: {GetHelpersStateText()}";
        }

        if (summaryText != null)
        {
            summaryText.text = $"Modalita: {GetRunModeText()} | Stato lobby: {GetLobbyStatusText()}";
            summaryText.color = GetSummaryColor();
        }
    }
}
