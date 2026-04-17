using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LabGuideConditionBridgeXR : MonoBehaviour
{
    [SerializeField] private LabGuideDirector director;
    [SerializeField] private XRSimpleInteractable xrInteractable;
    [SerializeField] private string signalId;

    private void Awake()
    {
        director ??= Object.FindAnyObjectByType<LabGuideDirector>();
        xrInteractable ??= GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    public void Configure(LabGuideDirector guideDirector, string guideSignalId)
    {
        director = guideDirector;
        signalId = guideSignalId;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var source = args.interactorObject != null
            ? args.interactorObject.transform.gameObject
            : null;

        if (director == null || string.IsNullOrWhiteSpace(signalId))
        {
            return;
        }

        director.TryReportCondition(signalId, source);
    }
}
