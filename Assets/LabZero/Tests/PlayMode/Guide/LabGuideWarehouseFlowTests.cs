using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class LabGuideWarehouseFlowTests
{
    [UnityTest]
    public IEnumerator GUID_01_RobotWelcomesLearner()
    {
        yield return LoadWarehouseScene();

        var presenter = FindOnlySceneComponent("LabGuideRobotPresenter");
        Assert.AreEqual("LabGuideDrone", presenter.gameObject.name);
        Assert.IsTrue(presenter.gameObject.activeInHierarchy);

        var bubble = FindOnlySceneComponent("LabGuidePromptBubbleView");
        Assert.IsTrue(bubble.gameObject.activeInHierarchy);
        AssertVisibleTextContains("Benvenuto. Ti guido in sicurezza, passo dopo passo.");
    }

    [UnityTest]
    public IEnumerator GUID_01_RobotCompanionDistance()
    {
        yield return LoadWarehouseScene();

        var presenter = FindOnlySceneComponent("LabGuideRobotPresenter");
        var learner = ResolveLearnerAnchor();
        Assert.IsNotNull(learner, "Learner anchor was not found.");

        for (var i = 0; i < 120; i++)
        {
            yield return null;
        }

        var distance = Vector3.Distance(presenter.transform.position, learner.position);
        Assert.That(distance, Is.InRange(0.8f, 1.8f), "Guide drone should stay at companion distance from the learner.");
    }

    [UnityTest]
    public IEnumerator GUID_01_StatusLineCompact()
    {
        yield return LoadWarehouseScene();

        var statusLine = FindOnlySceneComponent("LabGuideStatusLineView");
        Assert.IsTrue(statusLine.gameObject.activeInHierarchy);

        var statusText = ReadTextFromChildren(statusLine.gameObject);
        Assert.IsFalse(string.IsNullOrWhiteSpace(statusText), "Status line should show the active objective.");
        Assert.LessOrEqual(statusText.Length, 70);
        Assert.IsFalse(statusText.Contains("\n"), "Status line should stay on one compact line.");

        var bubble = FindOnlySceneComponent("LabGuidePromptBubbleView");
        Assert.AreNotEqual(bubble.gameObject, statusLine.gameObject, "Status line must not replace or hide the guide bubble.");
    }

    private static IEnumerator LoadWarehouseScene()
    {
        SceneManager.LoadScene("LabWarehouse", LoadSceneMode.Single);
        for (var i = 0; i < 60; i++)
        {
            yield return null;
            if (SceneManager.GetActiveScene().name == "LabWarehouse")
            {
                yield break;
            }
        }
    }

    private static Component FindOnlySceneComponent(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found in Assembly-CSharp.");

        var components = Resources
            .FindObjectsOfTypeAll<Component>()
            .Where(component => component != null)
            .Where(component => component.GetType() == type)
            .Where(component => component.gameObject.scene.IsValid())
            .Where(component => component.gameObject.activeInHierarchy)
            .ToArray();

        Assert.AreEqual(1, components.Length, "Expected exactly one active scene component of type " + typeName + ".");
        return components[0];
    }

    private static Transform ResolveLearnerAnchor()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return GameObject.Find("WarehousePreviewCamera")?.transform;
    }

    private static void AssertVisibleTextContains(string expected)
    {
        var found = GetVisibleText().Any(text => text.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0);
        Assert.IsTrue(found, "Expected visible text containing '" + expected + "' was not found.");
    }

    private static string ReadTextFromChildren(GameObject root)
    {
        return root
            .GetComponentsInChildren<Component>(true)
            .Where(component => component != null)
            .Select(ReadText)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
    }

    private static string[] GetVisibleText()
    {
        return Resources
            .FindObjectsOfTypeAll<Component>()
            .Where(component => component != null)
            .Where(component => component.gameObject.scene.IsValid())
            .Where(component => component.gameObject.activeInHierarchy)
            .Where(component =>
            {
                var typeName = component.GetType().Name;
                return typeName == "TMP_Text" || typeName == "TextMeshPro" || typeName == "TextMeshProUGUI";
            })
            .Select(ReadText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim())
            .Distinct()
            .ToArray();
    }

    private static string ReadText(Component component)
    {
        var textProperty = component.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
        return textProperty?.GetValue(component) as string;
    }
}
