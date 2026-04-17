using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class LabSafetyInteractable : MonoBehaviour
{
    public static event Action<LabSafetyInteractionContext> Activated;

    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private LabSafetyItemType itemType;
    [SerializeField] private LabSafetyItemRole role;
    [SerializeField] private LabSafetyZoneType currentZone = LabSafetyZoneType.Operational;
    [SerializeField] private Transform safeReturnPoint;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text label;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private XRSimpleInteractable xrInteractable;
    [SerializeField] private bool moveToSafeReturnOnMistake = true;

    private Color _baseColor;
    private float _flashUntil;

    public LabSafetyItemType ItemType => itemType;
    public LabSafetyItemRole Role => role;
    public LabSafetyZoneType CurrentZone => currentZone;

    private void Awake()
    {
        sessionManager ??= UnityEngine.Object.FindAnyObjectByType<LabSessionManager>();
        targetRenderer ??= GetComponentInChildren<Renderer>();
        audioSource ??= GetComponent<AudioSource>();
        xrInteractable ??= GetComponent<XRSimpleInteractable>();
        xrInteractable ??= gameObject.AddComponent<XRSimpleInteractable>();

        if (targetRenderer != null)
        {
            _baseColor = targetRenderer.sharedMaterial != null ? targetRenderer.sharedMaterial.color : Color.white;
        }
    }

    private void OnEnable()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.AddListener(OnXrSelectEntered);
        }
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnXrSelectEntered);
        }
    }

    private void OnMouseDown()
    {
        Activate();
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
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (sessionManager != null)
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

    private void OnXrSelectEntered(SelectEnterEventArgs args)
    {
        Activate();
    }

    private bool ShouldCountAsMistake()
    {
        if (role == LabSafetyItemRole.Ppe)
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
