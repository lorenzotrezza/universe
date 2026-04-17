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

    [Test]
    public void CommandPadTopsDoNotOverlap()
    {
        BuildDesk();

        var padNames = new[]
        {
            "Pad Timer Minus Top",
            "Pad Timer Plus Top",
            "Pad Overlay Errori Top",
            "Pad Aiuti Top",
            "Pad Modalita Top",
            "Pad Start Training Top",
            "Pad Reset Lobby Top",
        };

        for (var i = 0; i < padNames.Length; i++)
        {
            var first = RequireComponent<Renderer>(RequireTransform(padNames[i])).bounds;
            for (var j = i + 1; j < padNames.Length; j++)
            {
                var second = RequireComponent<Renderer>(RequireTransform(padNames[j])).bounds;
                Assert.IsFalse(
                    OverlapsOnDeskSurface(first, second, 0.015f),
                    $"{padNames[i]} should not overlap {padNames[j]} on the desk surface.");
            }
        }
    }

    [Test]
    public void LobbyDesktopPreviewDefaultsToSeatedHeadLookOnly()
    {
        var debugType = Type.GetType("LabDebugHotkeys, Assembly-CSharp");
        Assert.That(debugType, Is.Not.Null, "LabDebugHotkeys type should be available in Assembly-CSharp.");

        var field = debugType.GetField("allowDesktopTranslationInLobby", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, "Lobby desktop translation should be explicitly guarded by a serialized field.");

        var harness = new GameObject("LabDebugHotkeysLayoutHarness");
        var component = harness.AddComponent(debugType);
        Assert.That(field.GetValue(component), Is.EqualTo(false), "Lobby should default to seated preview without WASD/vertical translation.");
    }

    [Test]
    public void WarehousePreviewDefaultsToStandingWithoutVerticalFlight()
    {
        var previewType = Type.GetType("LabWarehousePreview, Assembly-CSharp");
        Assert.That(previewType, Is.Not.Null, "LabWarehousePreview type should be available in Assembly-CSharp.");

        var cameraHeightField = previewType.GetField("cameraHeight", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(cameraHeightField, Is.Not.Null, "Warehouse preview should expose a serialized camera height.");

        var verticalFlightField = previewType.GetField("allowVerticalDebugMovement", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(verticalFlightField, Is.Not.Null, "Warehouse preview vertical movement should be explicitly opt-in debug behavior.");

        var harness = new GameObject("LabWarehousePreviewHarness");
        var component = harness.AddComponent(previewType);
        Assert.That((float)cameraHeightField.GetValue(component), Is.GreaterThanOrEqualTo(1.5f), "Warehouse should default to a standing human eye height.");
        Assert.That(verticalFlightField.GetValue(component), Is.EqualTo(false), "Warehouse walking preview should not fly vertically by default.");
        UnityEngine.Object.DestroyImmediate(harness);
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

    private static bool OverlapsOnDeskSurface(Bounds first, Bounds second, float minimumGap)
    {
        var xOverlaps = first.min.x < second.max.x + minimumGap && first.max.x + minimumGap > second.min.x;
        var zOverlaps = first.min.z < second.max.z + minimumGap && first.max.z + minimumGap > second.min.z;
        return xOverlaps && zOverlaps;
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
