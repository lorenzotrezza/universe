using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LabCoursePortal : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;
    [SerializeField] private LabThemeType theme = LabThemeType.None;
    [SerializeField] private Renderer portalRenderer;
    [SerializeField] private TMP_Text portalLabel;
    [SerializeField] private Color baseColor = new(0.2f, 0.75f, 1f, 1f);
    [SerializeField] private Color selectedColor = new(0.45f, 1f, 1f, 1f);

    private Material _runtimeMaterial;

    public void Configure(
        LabTaskManager manager,
        LabThemeType selectedTheme,
        Renderer visual,
        TMP_Text label,
        Color idle,
        Color active)
    {
        taskManager = manager;
        theme = selectedTheme;
        portalRenderer = visual;
        portalLabel = label;
        baseColor = idle;
        selectedColor = active;

        if (portalLabel != null)
        {
            portalLabel.text = GetThemeTitle();
        }

        CacheMaterial();
        RefreshVisual();
    }

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        portalRenderer ??= GetComponent<Renderer>();
        CacheMaterial();
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged += RefreshVisual;
        }

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.StateChanged -= RefreshVisual;
        }
    }

    private void Update()
    {
        if (_runtimeMaterial == null)
        {
            return;
        }

        if (taskManager == null || taskManager.SelectedTheme != theme)
        {
            return;
        }

        var pulse = 0.85f + Mathf.PingPong(Time.time * 0.45f, 0.35f);
        _runtimeMaterial.color = selectedColor * pulse;
    }

    private void OnMouseDown()
    {
        SelectTheme();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            SelectTheme();
        }
    }

    private void SelectTheme()
    {
        if (taskManager == null || theme == LabThemeType.None)
        {
            return;
        }

        taskManager.SelectTheme(theme);
    }

    private void RefreshVisual()
    {
        if (_runtimeMaterial == null)
        {
            return;
        }

        var isSelected = taskManager != null && taskManager.SelectedTheme == theme;
        _runtimeMaterial.color = isSelected ? selectedColor : baseColor;
    }

    private void CacheMaterial()
    {
        if (portalRenderer == null)
        {
            return;
        }

        _runtimeMaterial = portalRenderer.material;
    }

    private string GetThemeTitle()
    {
        switch (theme)
        {
            case LabThemeType.EnglishCommunication:
                return "ENGLISH";
            case LabThemeType.BasicMathematics:
                return "MATH";
            case LabThemeType.DigitalSkills:
                return "DIGITAL";
            default:
                return "COURSE";
        }
    }
}
