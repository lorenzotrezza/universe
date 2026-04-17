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

    [UnityTest]
    public IEnumerator GUID_04_WarehouseRegistersObjectAndAreaTargets()
    {
        yield return LoadWarehouseScene();

        var registry = FindOnlySceneComponent("LabGuideTargetRegistry");

        AssertResolve(registry, "postazione_dpi", "Object");
        AssertResolve(registry, "passaggio_operativo", "Area");
    }

    [UnityTest]
    public IEnumerator GUID_04_HelperOff_RobotOrientsWithoutStrongGlow()
    {
        yield return LoadWarehouseScene();

        var service = FindOnlySceneComponent("LabGuideTargetCueService");
        var presenter = FindOnlySceneComponent("LabGuideRobotPresenter");
        var sessionManager = FindFirstSceneComponent("LabSessionManager");
        Assert.IsNotNull(sessionManager, "Warehouse scene should expose a LabSessionManager for helper settings.");

        var settings = GetProperty(sessionManager, "Settings") as ScriptableObject;
        if (settings == null)
        {
            var settingsType = Type.GetType("LabSessionSettings, Assembly-CSharp");
            Assert.IsNotNull(settingsType, "LabSessionSettings type was not found.");
            settings = ScriptableObject.CreateInstance(settingsType);
            Invoke(sessionManager, "Initialize", settings);
        }

        SetField(settings.GetType(), settings, "HelpersEnabled", false);
        Invoke(service, "RefreshCurrentTarget");

        Assert.IsFalse((bool)GetProperty(service, "StrongCueActive"));
        Assert.IsNotNull(GetProperty(presenter, "FocusTarget"));
        Assert.IsTrue((bool)GetProperty(presenter, "OrientationOnlyFocus"));
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

    private static Component FindFirstSceneComponent(string typeName)
    {
        return FindSceneComponents(typeName).FirstOrDefault();
    }

    private static Component[] FindSceneComponents(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found in Assembly-CSharp.");

        return Resources
            .FindObjectsOfTypeAll<Component>()
            .Where(component => component != null)
            .Where(component => component.GetType() == type)
            .Where(component => component.gameObject.scene.IsValid())
            .Where(component => component.gameObject.activeInHierarchy)
            .ToArray();
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

    private static void AssertResolve(Component registry, string targetId, string expectedKind)
    {
        var args = new object[] { targetId, null };
        var resolved = (bool)registry.GetType().GetMethod("TryResolve").Invoke(registry, args);
        Assert.IsTrue(resolved, "Target '" + targetId + "' should resolve.");
        Assert.AreEqual(expectedKind, GetProperty(args[1], "Kind").ToString());
        Assert.IsNotNull(GetProperty(args[1], "FocusTransform"));
    }

    private static object Invoke(Component target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, "Expected public method '" + methodName + "' was not found.");
        return method.Invoke(target, args);
    }

    private static object GetProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected public property '" + propertyName + "' was not found.");
        return property.GetValue(target);
    }

    private static void SetField(Type type, object target, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, "Expected public field '" + fieldName + "' was not found.");
        field.SetValue(target, value);
    }
}
