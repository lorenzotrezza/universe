using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class LabLobbySceneSmokeTests
{
    private static readonly string[] ForbiddenCopy =
    {
        "English",
        "Math",
        "Digital",
        "Select a Course",
        "Video Source",
        "video-placeholder",
        "Course Hub",
        "Theme Hub",
        "PPE:",
        "Bench Setup",
        "Hazard Check",
        "Choose theme",
        "Cell Biology",
        "Quantum Lab",
        "AI Safety",
    };

    [UnityTest]
    public IEnumerator LobbyScene_BootsWithItalianBriefingAndSetupPads()
    {
        yield return LoadLobbyScene();

        Assert.IsNotNull(GameObject.Find("LearningDesk_Root"));
        AssertTextExists("Briefing Sicurezza Magazzino");
        AssertTextExists("Configura la sessione prima di entrare nell'area operativa.");

        Assert.IsNotNull(GameObject.Find("Pad Timer Minus"));
        Assert.IsNotNull(GameObject.Find("Pad Timer Plus"));
        Assert.IsNotNull(GameObject.Find("Pad Overlay Errori"));
        Assert.IsNotNull(GameObject.Find("Pad Aiuti"));
        Assert.IsNotNull(GameObject.Find("Pad Modalita"));
        Assert.IsNotNull(GameObject.Find("Pad Start Training"));
        AssertTextExists("Avvia Addestramento");
    }

    [UnityTest]
    public IEnumerator LobbyScene_HasNoLegacyPlayerFacingCopy()
    {
        yield return LoadLobbyScene();

        var allText = GetVisibleText();
        foreach (var forbidden in ForbiddenCopy)
        {
            Assert.IsFalse(
                allText.Any(text => text.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0),
                $"Found legacy copy '{forbidden}' in visible text.");
        }
    }

    [UnityTest]
    public IEnumerator LobbyControls_UpdateManagerAndVisibleSummaries()
    {
        yield return LoadLobbyScene();

        var manager = FindManager();
        Assert.IsNotNull(manager, "LabTaskManager component not found.");

        var initialTimer = GetIntProperty(manager, "TimerMinutes");
        var initialOverlay = GetBoolProperty(manager, "ShowErrorOverlay");
        var initialHelpers = GetBoolProperty(manager, "HelpersEnabled");
        var initialMode = GetPropertyValue(manager, "RunMode")?.ToString();
        var initialConfigured = GetBoolProperty(manager, "RunConfigured");

        ActivatePad("Pad Timer Plus");
        ActivatePad("Pad Overlay Errori");
        ActivatePad("Pad Aiuti");
        ActivatePad("Pad Modalita");
        ActivatePad("Pad Start Training");
        yield return null;

        Assert.AreEqual(initialTimer + 1, GetIntProperty(manager, "TimerMinutes"));
        Assert.AreNotEqual(initialOverlay, GetBoolProperty(manager, "ShowErrorOverlay"));
        Assert.AreNotEqual(initialHelpers, GetBoolProperty(manager, "HelpersEnabled"));
        Assert.AreNotEqual(initialMode, GetPropertyValue(manager, "RunMode")?.ToString());
        Assert.IsTrue(initialConfigured || GetBoolProperty(manager, "RunConfigured"));

        var text = GetVisibleText();
        Assert.IsTrue(text.Any(x => x.Contains("Timer", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(text.Any(x => x.Contains("Overlay errori", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(text.Any(x => x.Contains("Aiuti", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(text.Any(x => x.Contains("Modalita", StringComparison.OrdinalIgnoreCase)));
    }

    [UnityTest]
    public IEnumerator LobbyErrorOverlay_DefaultHiddenAndToggleShowsPanel()
    {
        yield return LoadLobbyScene();

        var manager = FindManager();
        Assert.IsNotNull(manager, "LabTaskManager component not found.");

        var panel = GameObject.Find("Error Overlay Panel");
        Assert.IsNotNull(panel, "Error Overlay Panel not found.");
        Assert.IsFalse(GetBoolProperty(manager, "ShowErrorOverlay"));
        Assert.IsFalse(panel.activeInHierarchy);

        ActivatePad("Pad Overlay Errori");
        yield return null;

        Assert.IsTrue(GetBoolProperty(manager, "ShowErrorOverlay"));
        Assert.IsTrue(panel.activeInHierarchy);
        AssertTextExists("Errori");
        AssertTextExists("Visibilita overlay");
    }

    private static IEnumerator LoadLobbyScene()
    {
        SceneManager.LoadScene("LabZero_Prototype");
        yield return null;
        yield return null;
    }

    private static Component FindManager()
    {
        var managerType = Type.GetType("LabTaskManager, Assembly-CSharp");
        Assert.IsNotNull(managerType, "LabTaskManager type was not found in Assembly-CSharp.");
        return UnityEngine.Object.FindAnyObjectByType(managerType) as Component;
    }

    private static void ActivatePad(string padName)
    {
        var pad = GameObject.Find(padName);
        Assert.IsNotNull(pad, $"Pad '{padName}' not found.");
        var component = pad.GetComponent("LabDeskCommandPad");
        Assert.IsNotNull(component, $"Pad '{padName}' is missing LabDeskCommandPad.");
        var activate = component.GetType().GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(activate, "LabDeskCommandPad.Activate() not found.");
        activate.Invoke(component, null);
    }

    private static object GetPropertyValue(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, $"Expected property '{propertyName}' was not found.");
        return property.GetValue(target);
    }

    private static bool GetBoolProperty(object target, string propertyName)
    {
        return (bool)GetPropertyValue(target, propertyName);
    }

    private static int GetIntProperty(object target, string propertyName)
    {
        return (int)GetPropertyValue(target, propertyName);
    }

    private static string[] GetVisibleText()
    {
        return UnityEngine.Object
            .FindObjectsByType<Component>(FindObjectsSortMode.None)
            .Where(component => component != null)
            .Where(component =>
            {
                var typeName = component.GetType().Name;
                return typeName == "TMP_Text" || typeName == "TextMeshPro" || typeName == "TextMeshProUGUI";
            })
            .Where(component =>
            {
                if (!component.gameObject.activeInHierarchy)
                {
                    return false;
                }

                if (component is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    return false;
                }

                return true;
            })
            .Select(component =>
            {
                var textProperty = component.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                return textProperty?.GetValue(component) as string;
            })
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim())
            .Distinct()
            .ToArray();
    }

    private static void AssertTextExists(string expected)
    {
        var found = GetVisibleText().Any(text => text.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0);
        Assert.IsTrue(found, $"Expected visible text containing '{expected}' was not found.");
    }
}
