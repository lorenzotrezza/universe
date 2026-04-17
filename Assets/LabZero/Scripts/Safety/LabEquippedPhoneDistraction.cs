using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(LabSafetyInteractable))]
public class LabEquippedPhoneDistraction : MonoBehaviour
{
    [SerializeField] private LabSafetyInteractable phoneInteractable;
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private LabHotbarInventory hotbarInventory;
    [SerializeField] private Transform equippedAnchor;
    [SerializeField] private Vector3 equippedLocalPosition = new(0.28f, -0.22f, 0.45f);
    [SerializeField] private Vector3 equippedLocalEulerAngles = new(70f, 0f, -8f);
    [SerializeField] private float minNotificationDelaySeconds = 45f;
    [SerializeField] private float maxNotificationDelaySeconds = 90f;
    [SerializeField] private string notificationFeedback = "Notifica sul telefono: prendilo solo se puoi fermarti in sicurezza.";
    [SerializeField] private string notificationLabel = "NOTIFICA";
    [SerializeField] private AudioSource audioSource;

    private Collider[] _colliders;
    private Coroutine _notificationRoutine;

    public bool IsEquipped => equippedAnchor != null && transform.parent == equippedAnchor;
    public bool IsStoredInHotbar => phoneInteractable != null && phoneInteractable.IsInHotbar;
    public bool IsNotificationPending { get; private set; }
    public bool HasNotificationTriggered { get; private set; }

    private void Awake()
    {
        phoneInteractable ??= GetComponent<LabSafetyInteractable>();
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        audioSource ??= GetComponent<AudioSource>();
        _colliders = GetComponentsInChildren<Collider>(true);
    }

    private void Start()
    {
        if (equippedAnchor != null)
        {
            EquipToAnchor();
        }

        if (hotbarInventory == null)
        {
            SetInteractionAvailable(false);
        }

        ArmRandomNotification();
    }

    private void OnEnable()
    {
        LabSafetyInteractable.Activated += OnSafetyInteractableActivated;
    }

    private void OnDisable()
    {
        LabSafetyInteractable.Activated -= OnSafetyInteractableActivated;

        if (_notificationRoutine != null)
        {
            StopCoroutine(_notificationRoutine);
            _notificationRoutine = null;
        }
    }

    public void Configure(Transform anchor, LabSessionManager manager)
    {
        equippedAnchor = anchor;
        sessionManager = manager;
        EquipToAnchor();
        SetInteractionAvailable(false);
    }

    public void ConfigureForHotbar(LabHotbarInventory inventory, LabSessionManager manager)
    {
        hotbarInventory = inventory;
        sessionManager = manager;
        phoneInteractable ??= GetComponent<LabSafetyInteractable>();
        SetInteractionAvailable(true);
        hotbarInventory?.TryEquip(phoneInteractable);
        ArmRandomNotification();
    }

    public void ArmRandomNotification()
    {
        if (_notificationRoutine != null)
        {
            StopCoroutine(_notificationRoutine);
        }

        IsNotificationPending = false;
        HasNotificationTriggered = false;
        var delay = Random.Range(
            Mathf.Max(0f, minNotificationDelaySeconds),
            Mathf.Max(minNotificationDelaySeconds, maxNotificationDelaySeconds));
        _notificationRoutine = StartCoroutine(NotifyAfterDelay(delay));
    }

    public void TriggerNotification()
    {
        if (HasNotificationTriggered)
        {
            return;
        }

        HasNotificationTriggered = true;
        IsNotificationPending = true;
        SetInteractionAvailable(true);
        SetLabelText(notificationLabel);

        if (audioSource != null)
        {
            audioSource.Play();
        }

        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        if (sessionManager != null)
        {
            var feedback = IsStoredInHotbar
                ? notificationFeedback + " Seleziona il telefono nella hotbar e premi A per usarlo."
                : notificationFeedback;
            sessionManager.RegisterSafetyFeedback(feedback, false);
        }
    }

    private IEnumerator NotifyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _notificationRoutine = null;
        TriggerNotification();
    }

    private void EquipToAnchor()
    {
        if (equippedAnchor == null)
        {
            return;
        }

        transform.SetParent(equippedAnchor, false);
        transform.localPosition = equippedLocalPosition;
        transform.localRotation = Quaternion.Euler(equippedLocalEulerAngles);
    }

    private void SetInteractionAvailable(bool available)
    {
        phoneInteractable ??= GetComponent<LabSafetyInteractable>();
        if (phoneInteractable != null)
        {
            phoneInteractable.enabled = available;
        }

        _colliders ??= GetComponentsInChildren<Collider>(true);
        foreach (var phoneCollider in _colliders)
        {
            if (phoneCollider != null)
            {
                phoneCollider.enabled = available;
            }
        }
    }

    private void SetLabelText(string text)
    {
        var label = GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = text;
        }
    }

    private void OnSafetyInteractableActivated(LabSafetyInteractionContext context)
    {
        if (context.Source != gameObject)
        {
            return;
        }

        IsNotificationPending = false;
    }
}
