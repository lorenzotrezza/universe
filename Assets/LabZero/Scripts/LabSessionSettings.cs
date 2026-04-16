using UnityEngine;

[CreateAssetMenu(fileName = "SessionSettings", menuName = "LabZero/Session Settings")]
public class LabSessionSettings : ScriptableObject
{
    [Header("Timer")]
    public int TimerMinutes = 7;

    [Header("Display")]
    public bool ShowErrorOverlay;
    public bool HelpersEnabled = true;

    [Header("Mode")]
    public LabPresentationMode PresentationMode = LabPresentationMode.Standard;
}
