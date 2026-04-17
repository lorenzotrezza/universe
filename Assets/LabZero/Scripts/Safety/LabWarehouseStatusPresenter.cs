using TMPro;
using UnityEngine;

public class LabWarehouseStatusPresenter : MonoBehaviour
{
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text statsText;

    private void OnEnable()
    {
        AutoWireIfNeeded();

        if (sessionManager != null)
        {
            sessionManager.StateChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (sessionManager != null)
        {
            sessionManager.StateChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (sessionManager == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = "Percorso sicurezza magazzino";
        }

        if (feedbackText != null)
        {
            feedbackText.text = sessionManager.LastFeedbackText;
        }

        if (statsText != null)
        {
            statsText.text = $"Errori: {sessionManager.MistakeCount} | Punteggio: {Mathf.RoundToInt(sessionManager.Score)}";
        }
    }

    private void AutoWireIfNeeded()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        titleText ??= FindText("Warehouse Status Title");
        feedbackText ??= FindText("Warehouse Status Feedback");
        statsText ??= FindText("Warehouse Status Stats");
    }

    private TMP_Text FindText(string childName)
    {
        var texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (var text in texts)
        {
            if (text != null && text.name == childName)
            {
                return text;
            }
        }

        return null;
    }
}
