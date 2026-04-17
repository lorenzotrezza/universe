using UnityEngine;

public sealed class LabSafetyInteractionContext
{
    public LabSafetyInteractionContext(
        GameObject source,
        LabSafetyItemType itemType,
        LabSafetyItemRole role,
        LabSafetyZoneType zone,
        bool countsAsMistake,
        string feedbackText)
    {
        Source = source;
        ItemType = itemType;
        Role = role;
        Zone = zone;
        CountsAsMistake = countsAsMistake;
        FeedbackText = feedbackText;
    }

    public GameObject Source { get; }
    public LabSafetyItemType ItemType { get; }
    public LabSafetyItemRole Role { get; }
    public LabSafetyZoneType Zone { get; }
    public bool CountsAsMistake { get; }
    public string FeedbackText { get; }
}
