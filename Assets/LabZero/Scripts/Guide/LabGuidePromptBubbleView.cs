using TMPro;
using UnityEngine;

public class LabGuidePromptBubbleView : MonoBehaviour
{
    public const string StartupGreeting = "Benvenuto. Ti guido in sicurezza, passo dopo passo.";

    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string defaultPrompt = StartupGreeting;
    [SerializeField] private bool faceLearnerCamera = true;

    public string CurrentText { get; private set; }

    private void Awake()
    {
        EnsureText();
        ShowPrompt(string.IsNullOrWhiteSpace(CurrentText) ? defaultPrompt : CurrentText);
    }

    public void Bind(TMP_Text text)
    {
        promptText = text;
        EnsureText();
        ShowPrompt(string.IsNullOrWhiteSpace(CurrentText) ? defaultPrompt : CurrentText);
    }

    public void ShowPrompt(string text, LabGuidePromptSeverity severity = LabGuidePromptSeverity.Info)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CurrentText = text.Trim();
        EnsureText();

        if (promptText != null)
        {
            promptText.text = CurrentText;
            promptText.color = severity == LabGuidePromptSeverity.Warning
                ? new Color(1f, 0.72f, 0.28f, 1f)
                : Color.white;
        }
    }

    private void LateUpdate()
    {
        if (!faceLearnerCamera)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindAnyObjectByType<Camera>();
        }

        if (camera == null)
        {
            return;
        }

        var direction = transform.position - camera.transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }

    private void EnsureText()
    {
        if (promptText != null)
        {
            return;
        }

        promptText = GetComponentInChildren<TMP_Text>(true);
    }
}
