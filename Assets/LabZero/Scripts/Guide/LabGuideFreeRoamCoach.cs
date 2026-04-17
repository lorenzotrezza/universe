using UnityEngine;

public class LabGuideFreeRoamCoach : MonoBehaviour
{
    [SerializeField] private LabGuideDirector director;

    private void Awake()
    {
        director ??= GetComponent<LabGuideDirector>();
        director ??= Object.FindAnyObjectByType<LabGuideDirector>();
    }

    private void OnEnable()
    {
        LabSafetyInteractable.Activated += OnSafetyInteractableActivated;
    }

    private void OnDisable()
    {
        LabSafetyInteractable.Activated -= OnSafetyInteractableActivated;
    }

    public void Bind(LabGuideDirector guideDirector)
    {
        director = guideDirector;
    }

    private void OnSafetyInteractableActivated(LabSafetyInteractionContext context)
    {
        director ??= GetComponent<LabGuideDirector>();
        director ??= Object.FindAnyObjectByType<LabGuideDirector>();

        if (director == null || director.Mode != LabGuideMode.FreeRoam)
        {
            return;
        }

        director.ReportFreeRoamSafetyEvent(context);
    }
}
