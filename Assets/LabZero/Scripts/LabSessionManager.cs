using System;
using System.Collections.Generic;
using UnityEngine;

public class LabSessionManager : MonoBehaviour
{
    [SerializeField] private LabSessionSettings settings;

    public event Action StateChanged;
    public event Action<LabPpeSlotType, LabPpeItemType, string> PpeSlotEquipped;

    private readonly HashSet<LabPpeSlotType> _equippedPpeSlots = new();

    public LabSessionState RunState { get; private set; }
    public float ElapsedTime { get; private set; }
    public int MistakeCount { get; private set; }
    public string LastFeedbackText { get; private set; } = "Sessione pronta. Completa il controllo DPI.";
    public string LastPpeEquipFeedbackText { get; private set; } = "DPI non ancora equipaggiati.";
    public float Score => CalculateScore();
    public LabSessionSettings Settings => settings;
    public IReadOnlyCollection<LabPpeSlotType> EquippedPpeSlots => _equippedPpeSlots;

    public void Initialize(LabSessionSettings sessionSettings)
    {
        settings = sessionSettings;
        ResetSession();
    }

    public void StartRun()
    {
        if (RunState != LabSessionState.NotStarted)
        {
            return;
        }

        RunState = LabSessionState.Running;
        StateChanged?.Invoke();
    }

    public void RegisterMistake()
    {
        RegisterSafetyFeedback("Errore registrato. Correggi l'azione e continua il percorso.", true);
    }

    public void RegisterSafetyFeedback(string feedbackText, bool countsAsMistake)
    {
        if (string.IsNullOrWhiteSpace(feedbackText))
        {
            return;
        }

        LastFeedbackText = feedbackText;
        if (countsAsMistake && RunState == LabSessionState.Running)
        {
            MistakeCount++;
        }

        StateChanged?.Invoke();
    }

    public bool TryEquipPpeSlot(LabPpeSlotType slot, LabPpeItemType itemType, string sourceId)
    {
        if (!IsValidPpeMapping(slot, itemType))
        {
            return false;
        }

        var feedback = BuildPpeFeedback(slot);
        LastPpeEquipFeedbackText = feedback;
        LastFeedbackText = feedback;

        if (!_equippedPpeSlots.Add(slot))
        {
            return true;
        }

        PpeSlotEquipped?.Invoke(slot, itemType, sourceId ?? string.Empty);
        StateChanged?.Invoke();
        return true;
    }

    public bool IsRequiredPpeComplete()
    {
        return _equippedPpeSlots.Contains(LabPpeSlotType.Head)
            && _equippedPpeSlots.Contains(LabPpeSlotType.Ears)
            && _equippedPpeSlots.Contains(LabPpeSlotType.Eyes)
            && _equippedPpeSlots.Contains(LabPpeSlotType.Hands);
    }

    public string GetPpeImmediateFeedbackText()
    {
        return LastPpeEquipFeedbackText;
    }

    public string GetPpeSummaryText()
    {
        return $"DPI: {_equippedPpeSlots.Count}/4";
    }

    public void CompleteRun()
    {
        if (RunState != LabSessionState.Running)
        {
            return;
        }

        RunState = LabSessionState.Complete;
        StateChanged?.Invoke();
    }

    public void ResetSession()
    {
        RunState = LabSessionState.NotStarted;
        ElapsedTime = 0f;
        MistakeCount = 0;
        _equippedPpeSlots.Clear();
        LastPpeEquipFeedbackText = "DPI non ancora equipaggiati.";
        LastFeedbackText = "Sessione pronta. Completa il controllo DPI.";
        StateChanged?.Invoke();
    }

    private void Update()
    {
        AdvanceElapsedTime(Time.deltaTime);
    }

    private void AdvanceElapsedTime(float deltaTime)
    {
        if (RunState != LabSessionState.Running)
        {
            return;
        }

        ElapsedTime += Mathf.Max(0f, deltaTime);
    }

    private float CalculateScore()
    {
        if (RunState == LabSessionState.NotStarted)
        {
            return 0f;
        }

        const float maxScore = 1000f;
        var timePenalty = ElapsedTime * 1.5f;
        var mistakePenalty = MistakeCount * 50f;

        return Mathf.Max(0f, maxScore - timePenalty - mistakePenalty);
    }

    private static bool IsValidPpeMapping(LabPpeSlotType slot, LabPpeItemType itemType)
    {
        return slot switch
        {
            LabPpeSlotType.Head => itemType == LabPpeItemType.HardHat,
            LabPpeSlotType.Ears => itemType == LabPpeItemType.Earmuffs,
            LabPpeSlotType.Eyes => itemType == LabPpeItemType.SafetyGlasses,
            LabPpeSlotType.Hands => itemType == LabPpeItemType.Gloves,
            _ => false,
        };
    }

    private static string BuildPpeFeedback(LabPpeSlotType slot)
    {
        return slot switch
        {
            LabPpeSlotType.Head => "Casco equipaggiato. Protegge da urti e cadute di materiale.",
            LabPpeSlotType.Ears => "Cuffie equipaggiate. Riduci l'esposizione al rumore dei macchinari.",
            LabPpeSlotType.Eyes => "Occhiali equipaggiati. Proteggono gli occhi da schegge e polvere.",
            LabPpeSlotType.Hands => "Guanti equipaggiati. Proteggono le mani durante movimentazione e presa.",
            _ => "DPI equipaggiato.",
        };
    }
}
