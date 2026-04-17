using System;

public enum LabGuideMode
{
    Guided = 0,
    FreeRoam = 1,
}

public enum LabGuidePromptSeverity
{
    Info = 0,
    Hint = 1,
    Warning = 2,
    Success = 3,
}

public enum LabGuideTargetKind
{
    Object = 0,
    Area = 1,
}

[Serializable]
public class LabGuideStepDefinition
{
    public string StepId;
    public string ObjectiveItalian;
    public string SafetyReasonItalian;
    public string RequiredSignalId;
    public string TargetId;
    public LabGuideTargetKind TargetKind;
    public float HintDelaySeconds;
    public float WarningDelaySeconds;
    public float ExpectedDurationSeconds;
}

[Serializable]
public class LabGuideMistakeRecord
{
    public string StepId;
    public string MistakeCode;
    public string RiskItalian;
    public string ExpectedActionItalian;
    public string RecoveryItalian;
    public float TimestampSeconds;
}
