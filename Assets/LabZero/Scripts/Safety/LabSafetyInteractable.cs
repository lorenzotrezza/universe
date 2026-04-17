using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class LabSafetyInteractable : MonoBehaviour
{
    public static event Action<LabSafetyInteractionContext> Activated;
    public static event Action<LabSafetyInteractionContext> Selected;

    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private LabSafetyItemType itemType;
    [SerializeField] private LabSafetyItemRole role;
    [SerializeField] private LabSafetyZoneType currentZone = LabSafetyZoneType.Operational;
    [SerializeField] private Transform safeReturnPoint;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text label;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private XRSimpleInteractable xrInteractable;
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody itemRigidbody;
    [SerializeField] private bool moveToSafeReturnOnMistake = true;
    [SerializeField] private bool grabbable = true;
    [SerializeField] private bool equippable = true;

    private Collider[] _colliders;
    private Renderer[] _renderers;
    private Color _baseColor;
    private float _flashUntil;

    public LabSafetyItemType ItemType => itemType;
    public LabSafetyItemRole Role => role;
    public LabSafetyZoneType CurrentZone => currentZone;
    public bool CanGrab => grabbable;
    public bool IsEquippable => equippable;
    public bool IsInHotbar { get; private set; }

    public void Configure(
        LabSessionManager manager,
        LabSafetyItemType type,
        LabSafetyItemRole itemRole,
        LabSafetyZoneType zone)
    {
        sessionManager = manager;
        itemType = type;
        role = itemRole;
        currentZone = zone;
    }

    private void Awake()
    {
        sessionManager ??= UnityEngine.Object.FindAnyObjectByType<LabSessionManager>();
        targetRenderer ??= GetComponentInChildren<Renderer>();
        audioSource ??= GetComponent<AudioSource>();
        if (itemRigidbody == null)
        {
            itemRigidbody = GetComponent<Rigidbody>();
        }
        xrInteractable ??= GetComponent<XRSimpleInteractable>();
        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);

        ConfigurePhysicalGrab();

        if (targetRenderer != null)
        {
            _baseColor = targetRenderer.sharedMaterial != null ? targetRenderer.sharedMaterial.color : Color.white;
        }
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void OnMouseDown()
    {
        SelectForAction();
    }

    private void Update()
    {
        if (targetRenderer == null || targetRenderer.material == null)
        {
            return;
        }

        var activeFlash = Time.unscaledTime < _flashUntil;
        targetRenderer.material.color = activeFlash
            ? Color.Lerp(_baseColor, Color.white, 0.45f)
            : _baseColor;
    }

    public void Activate()
    {
        sessionManager ??= UnityEngine.Object.FindAnyObjectByType<LabSessionManager>();
        var mistake = ShouldCountAsMistake();
        var feedback = BuildFeedback(mistake);
        var ppeAdapter = role == LabSafetyItemRole.Ppe ? GetComponent<LabPpeEquipAdapter>() : null;
        var ppeEquipped = ppeAdapter != null && ppeAdapter.TryEquip();
        if (ppeEquipped && sessionManager != null)
        {
            feedback = sessionManager.GetPpeImmediateFeedbackText();
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (sessionManager != null && !ppeEquipped)
        {
            sessionManager.RegisterSafetyFeedback(feedback, mistake);
        }

        if (label != null)
        {
            label.text = mistake ? "Riponi" : "OK";
        }

        _flashUntil = Time.unscaledTime + 0.35f;

        if (mistake && moveToSafeReturnOnMistake && safeReturnPoint != null)
        {
            transform.SetPositionAndRotation(safeReturnPoint.position, safeReturnPoint.rotation);
        }

        Activated?.Invoke(new LabSafetyInteractionContext(gameObject, itemType, role, currentZone, mistake, feedback));
    }

    public void SelectForAction()
    {
        sessionManager ??= UnityEngine.Object.FindAnyObjectByType<LabSessionManager>();
        if (label != null)
        {
            label.text = IsEquippable ? "Equipaggia" : "Selezionato";
        }

        _flashUntil = Time.unscaledTime + 0.20f;
        Selected?.Invoke(new LabSafetyInteractionContext(gameObject, itemType, role, currentZone, false, "Oggetto selezionato."));
    }

    public void SetHotbarState(bool inHotbar)
    {
        IsInHotbar = inHotbar;
        _colliders ??= GetComponentsInChildren<Collider>(true);
        _renderers ??= GetComponentsInChildren<Renderer>(true);

        foreach (var itemCollider in _colliders)
        {
            if (itemCollider != null)
            {
                itemCollider.enabled = !inHotbar;
            }
        }

        foreach (var itemRenderer in _renderers)
        {
            if (itemRenderer != null)
            {
                itemRenderer.enabled = !inHotbar;
            }
        }

        if (itemRigidbody == null)
        {
            itemRigidbody = GetComponent<Rigidbody>();
        }
        if (itemRigidbody != null)
        {
            if (inHotbar)
            {
                itemRigidbody.linearVelocity = Vector3.zero;
                itemRigidbody.angularVelocity = Vector3.zero;
            }

            itemRigidbody.isKinematic = inHotbar;
            itemRigidbody.useGravity = !inHotbar;
        }
    }

    private void ConfigurePhysicalGrab()
    {
        if (xrInteractable != null)
        {
            xrInteractable.enabled = false;
        }

        if (!grabbable)
        {
            return;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (itemRigidbody == null)
        {
            itemRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        itemRigidbody.useGravity = true;
        itemRigidbody.isKinematic = false;
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        var interactionManager = ResolveInteractionManager();
        if (interactionManager != null)
        {
            grabInteractable.interactionManager = interactionManager;
        }
    }

    private static XRInteractionManager ResolveInteractionManager()
    {
        var managers = UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsInactive.Exclude);
        foreach (var manager in managers)
        {
            if (manager != null && manager.gameObject.scene.IsValid())
            {
                return manager;
            }
        }

        return managers.Length > 0 ? managers[0] : null;
    }

    private bool ShouldCountAsMistake()
    {
        if (role == LabSafetyItemRole.Ppe)
        {
            return false;
        }

        if (role == LabSafetyItemRole.Neutral)
        {
            return false;
        }

        if (role == LabSafetyItemRole.Prohibited)
        {
            return true;
        }

        if (role == LabSafetyItemRole.BreakAreaOnly)
        {
            return currentZone != LabSafetyZoneType.BreakCorner;
        }

        return currentZone == LabSafetyZoneType.Operational
            || currentZone == LabSafetyZoneType.Sorting
            || currentZone == LabSafetyZoneType.Packaging
            || currentZone == LabSafetyZoneType.MachineHazard;
    }

    private string BuildFeedback(bool mistake)
    {
        if (role == LabSafetyItemRole.Ppe)
        {
            return itemType switch
            {
                LabSafetyItemType.Helmet => "Casco preso. Protegge da urti e cadute di materiale.",
                LabSafetyItemType.SafetyGlasses => "Occhiali indossati. Proteggono gli occhi da schegge e polvere.",
                LabSafetyItemType.HearingProtection => "Cuffie pronte. Riduci l'esposizione al rumore dei macchinari.",
                LabSafetyItemType.Gloves => "Guanti presi. Proteggono le mani durante movimentazione e imballaggio.",
                LabSafetyItemType.HighVisibilityVest => "Gilet alta visibilita pronto. Devi essere visibile nelle aree operative.",
                LabSafetyItemType.SafetyShoes => "Scarpe antinfortunistiche controllate. Proteggono piedi e stabilita.",
                _ => "DPI confermato. Continua il controllo sicurezza.",
            };
        }

        if (!mistake)
        {
            return itemType switch
            {
                LabSafetyItemType.Package => "Pacco movimentato. Mantieni libero il passaggio e non bloccare il nastro.",
                LabSafetyItemType.Food => "Cibo lasciato nella zona ristoro. Non portarlo nell'area di lavoro.",
                _ => "Oggetto riposto correttamente. Continua il percorso.",
            };
        }

        return itemType switch
        {
            LabSafetyItemType.Phone => "Attenzione: il telefono distrae in area operativa. Riponilo prima di lavorare.",
            LabSafetyItemType.Tablet => "Attenzione: il tablet non va usato durante la movimentazione. Riponilo.",
            LabSafetyItemType.Ball => "Attenzione: non si gioca nell'area lavoro. Riponi la pallina.",
            LabSafetyItemType.HandheldGame => "Attenzione: il gioco portatile distrae dal lavoro. Riponilo subito.",
            LabSafetyItemType.Food => "Attenzione: il cibo resta nella zona ristoro. Non portarlo tra macchine e nastri.",
            LabSafetyItemType.Beer => "Errore grave: alcol vietato durante il turno. Lascia la birra fuori dal percorso.",
            _ => "Attenzione: distrazione in area operativa. Riponi l'oggetto e continua.",
        };
    }
}
