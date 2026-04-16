using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LabTaskZone : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private LabTaskType acceptedTaskType = LabTaskType.None;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private TMP_Text label;

    [Header("Colors")]
    [SerializeField] private Color idleColor = new(0.1f, 0.5f, 0.9f, 0.8f);
    [SerializeField] private Color completeColor = new(0.2f, 0.85f, 0.35f, 0.9f);

    private int _acceptedCount;

    private void Reset()
    {
        var zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void Start()
    {
        RefreshVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        var collectible = other.GetComponentInParent<LabCollectible>();
        if (collectible == null || taskManager == null)
        {
            return;
        }

        if (!collectible.CanBeAcceptedBy(acceptedTaskType))
        {
            return;
        }

        collectible.MarkAccepted();
        _acceptedCount++;
        taskManager.RegisterValidDrop(acceptedTaskType);
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (zoneRenderer != null && zoneRenderer.sharedMaterial != null)
        {
            zoneRenderer.sharedMaterial.color = _acceptedCount > 0 ? completeColor : idleColor;
        }

        if (label != null)
        {
            label.text = acceptedTaskType switch
            {
                LabTaskType.PPE => "PPE Zone",
                LabTaskType.Tool => "Tool Zone",
                LabTaskType.Hazard => "Hazard Bin",
                _ => "Task Zone",
            };
        }
    }
}
