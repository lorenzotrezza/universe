using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LabGuideConditionSignal : MonoBehaviour
{
    [SerializeField] private LabGuideDirector director;
    [SerializeField] private string signalId;
    [SerializeField] private string requiredTag;
    [SerializeField] private LayerMask acceptedLayers = ~0;

    private void Awake()
    {
        director ??= Object.FindAnyObjectByType<LabGuideDirector>();
    }

    private void Reset()
    {
        var trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Activate(other != null ? other.gameObject : null);
    }

    public void Configure(LabGuideDirector guideDirector, string guideSignalId)
    {
        director = guideDirector;
        signalId = guideSignalId;
    }

    public void Activate(GameObject source)
    {
        if (director == null || string.IsNullOrWhiteSpace(signalId) || !AcceptsSource(source))
        {
            return;
        }

        director.TryReportCondition(signalId, source);
    }

    private bool AcceptsSource(GameObject source)
    {
        if (source == null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(requiredTag) && !source.CompareTag(requiredTag))
        {
            return false;
        }

        return (acceptedLayers.value & (1 << source.layer)) != 0;
    }
}
