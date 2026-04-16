using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LabDebugHotkeysLayoutTests
{
    [SetUp]
    public void SetUp()
    {
        CleanupGeneratedDesk();
    }

    [TearDown]
    public void TearDown()
    {
        CleanupGeneratedDesk();
    }

    [Test]
    public void ScreenTextAnchorsStayInsideScreenPanel()
    {
        BuildDesk();

        var screenPanel = RequireTransform("Screen Panel");
        var panelBounds = RequireComponent<Renderer>(screenPanel).bounds;
        var textNames = new[]
        {
            "Screen Scenario Title",
            "Screen Objective",
            "Screen Settings Heading",
            "Screen Timer Row",
            "Screen Overlay Row",
            "Screen Helpers Row",
            "Screen Mode Row",
            "Screen Cta Line",
        };

        foreach (var textName in textNames)
        {
            var text = RequireTransform(textName);
            var position = text.position;
            Assert.That(position.x, Is.InRange(panelBounds.min.x + 0.05f, panelBounds.max.x - 0.05f), textName + " x anchor should stay inside screen panel.");
            Assert.That(position.y, Is.InRange(panelBounds.min.y + 0.05f, panelBounds.max.y - 0.05f), textName + " y anchor should stay inside screen panel.");
        }
    }

    [Test]
    public void CommandPadTopAndLabelUseReadableWorldLayout()
    {
        BuildDesk();

        var top = RequireTransform("Pad Timer Plus Top");
        var topBounds = RequireComponent<Renderer>(top).bounds;
        var label = RequireTransform("Pad Timer Plus_Label");
        var labelScale = label.lossyScale;

        Assert.That(topBounds.size.x, Is.GreaterThanOrEqualTo(0.24f), "Pad top width should match the visible button footprint, not a nested scaled sliver.");
        Assert.That(topBounds.size.z, Is.GreaterThanOrEqualTo(0.15f), "Pad top depth should match the visible button footprint, not a nested scaled sliver.");
        Assert.That(Mathf.Max(labelScale.x, labelScale.y), Is.GreaterThanOrEqualTo(0.04f), "Pad label should have readable world scale.");
        Assert.That(label.position.y, Is.GreaterThan(topBounds.max.y + 0.005f), "Pad label should sit above the button surface.");
    }

    private static void BuildDesk()
    {
        var debugType = Type.GetType("LabDebugHotkeys, Assembly-CSharp");
        Assert.That(debugType, Is.Not.Null, "LabDebugHotkeys type should be available in Assembly-CSharp.");

        var harness = new GameObject("LabDebugHotkeysLayoutHarness");
        var component = harness.AddComponent(debugType);
        var buildMethod = debugType.GetMethod("BuildLobbyDesk", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(buildMethod, Is.Not.Null, "BuildLobbyDesk should remain available for layout regression coverage.");

        buildMethod.Invoke(component, new object[] { true });
    }

    private static Transform RequireTransform(string name)
    {
        var go = GameObject.Find(name);
        Assert.That(go, Is.Not.Null, name + " should be generated.");
        return go.transform;
    }

    private static T RequireComponent<T>(Transform target) where T : Component
    {
        var component = target.GetComponent<T>();
        Assert.That(component, Is.Not.Null, target.name + " should have " + typeof(T).Name + ".");
        return component;
    }

    private static void CleanupGeneratedDesk()
    {
        DestroyIfFound("LearningDesk_Root");
        DestroyIfFound("LabDebugHotkeysLayoutHarness");
    }

    private static void DestroyIfFound(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
