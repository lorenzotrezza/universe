using TMPro;
using UnityEngine;

public class LabGuideStatusLineView : MonoBehaviour
{
    private const int MaxVisibleCharacters = 70;

    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string defaultStatus = "Obiettivo: segui la guida iniziale in sicurezza.";

    public string CurrentText { get; private set; }

    private void Awake()
    {
        EnsureText();
        SetStatus(string.IsNullOrWhiteSpace(CurrentText) ? defaultStatus : CurrentText);
    }

    public void Bind(TMP_Text text)
    {
        statusText = text;
        EnsureText();
        SetStatus(string.IsNullOrWhiteSpace(CurrentText) ? defaultStatus : CurrentText);
    }

    public void SetStatus(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CurrentText = Compact(text);
        EnsureText();

        if (statusText != null)
        {
            statusText.text = CurrentText;
        }
    }

    private static string Compact(string text)
    {
        var singleLine = text.Replace("\r", " ").Replace("\n", " ").Trim();
        if (singleLine.Length <= MaxVisibleCharacters)
        {
            return singleLine;
        }

        return singleLine.Substring(0, MaxVisibleCharacters - 3).TrimEnd() + "...";
    }

    private void EnsureText()
    {
        if (statusText != null)
        {
            return;
        }

        statusText = GetComponentInChildren<TMP_Text>(true);
    }
}
