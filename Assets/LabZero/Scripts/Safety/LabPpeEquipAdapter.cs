using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class LabPpeEquipAdapter : MonoBehaviour
{
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private LabPpeSlotType slot;
    [SerializeField] private LabPpeItemType itemType;
    [SerializeField] private string sourceId;
    [SerializeField] private XRSimpleInteractable xrInteractable;
    [SerializeField] private string requiredTag;
    [SerializeField] private float equipCooldownSeconds = 0.25f;

    private float _lastEquipTime = -999f;

    private void Awake()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        xrInteractable ??= GetComponent<XRSimpleInteractable>();
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            sourceId = gameObject.name;
        }
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

    public void Configure(
        LabSessionManager manager,
        LabPpeSlotType ppeSlot,
        LabPpeItemType ppeItemType,
        string equipSourceId)
    {
        sessionManager = manager;
        slot = ppeSlot;
        itemType = ppeItemType;
        sourceId = equipSourceId;
    }

    public bool TryEquip()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        if (sessionManager == null || Time.unscaledTime - _lastEquipTime < equipCooldownSeconds)
        {
            return false;
        }

        var equipped = sessionManager.TryEquipPpeSlot(slot, itemType, sourceId);
        if (equipped)
        {
            _lastEquipTime = Time.unscaledTime;
            var presenter = Object.FindAnyObjectByType<LabGuideRobotPresenter>();
            presenter?.ShowImmediateFeedback(sessionManager.GetPpeImmediateFeedbackText());
        }

        return equipped;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        TryEquip();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowedInteractor(other))
        {
            return;
        }

        TryEquip();
    }

    private bool IsAllowedInteractor(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(requiredTag))
        {
            return true;
        }

        return other.CompareTag(requiredTag);
    }
}
