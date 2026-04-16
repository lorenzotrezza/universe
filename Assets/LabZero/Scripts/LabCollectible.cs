using UnityEngine;

public class LabCollectible : MonoBehaviour
{
    [SerializeField] private string displayName = "Item";
    [SerializeField] private LabTaskType taskType = LabTaskType.None;
    [SerializeField] private bool isCorrect = true;
    [SerializeField] private bool consumeOnValidDrop = true;

    public string DisplayName => displayName;
    public LabTaskType TaskType => taskType;
    public bool IsCorrect => isCorrect;
    public bool ConsumeOnValidDrop => consumeOnValidDrop;
    public bool IsAccepted { get; private set; }

    public bool CanBeAcceptedBy(LabTaskType acceptedTaskType)
    {
        return !IsAccepted && isCorrect && taskType == acceptedTaskType;
    }

    public void MarkAccepted()
    {
        IsAccepted = true;

        if (consumeOnValidDrop)
        {
            gameObject.SetActive(false);
        }
    }
}
