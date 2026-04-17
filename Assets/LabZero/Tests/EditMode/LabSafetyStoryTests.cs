using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LabSafetyStoryTests
{
    [Test]
    public void SafetyItemTypeDefinesPpeAndDistractorAnchors()
    {
        var itemType = Type.GetType("LabSafetyItemType, Assembly-CSharp");
        Assert.IsNotNull(itemType, "LabSafetyItemType should define the warehouse story anchor item set.");

        var expectedNames = new[]
        {
            "Helmet",
            "SafetyGlasses",
            "HearingProtection",
            "Gloves",
            "HighVisibilityVest",
            "SafetyShoes",
            "Phone",
            "Tablet",
            "Ball",
            "HandheldGame",
            "Food",
            "Beer",
        };

        foreach (var expectedName in expectedNames)
        {
            Assert.IsTrue(Enum.IsDefined(itemType, expectedName), expectedName + " should be a defined safety story item.");
        }
    }

    [Test]
    public void RegisterSafetyFeedbackStoresNonBlockingMistakeCopy()
    {
        var manager = CreateStartedSessionManager(out var managerType);

        Invoke(managerType, manager, "RegisterSafetyFeedback", "Telefono riposto. Continua il controllo DPI.", false);
        Assert.AreEqual("Telefono riposto. Continua il controllo DPI.", GetProperty<string>(managerType, manager, "LastFeedbackText"));
        Assert.AreEqual(0, GetProperty<int>(managerType, manager, "MistakeCount"));

        Invoke(managerType, manager, "RegisterSafetyFeedback", "Attenzione: il telefono distrae in area operativa. Riponilo.", true);
        Assert.AreEqual("Attenzione: il telefono distrae in area operativa. Riponilo.", GetProperty<string>(managerType, manager, "LastFeedbackText"));
        Assert.AreEqual(1, GetProperty<int>(managerType, manager, "MistakeCount"));

        UnityEngine.Object.DestroyImmediate(manager.gameObject);
    }

    [Test]
    public void SafetyInteractableExposesActivateForMouseAndXrAdapters()
    {
        var interactableType = Type.GetType("LabSafetyInteractable, Assembly-CSharp");
        Assert.IsNotNull(interactableType, "LabSafetyInteractable should bridge scene objects into safety story feedback.");

        var activate = interactableType.GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(activate, "LabSafetyInteractable.Activate() should be callable from mouse and XR selection paths.");
    }

    private static Component CreateStartedSessionManager(out Type managerType)
    {
        managerType = Type.GetType("LabSessionManager, Assembly-CSharp");
        Assert.IsNotNull(managerType, "LabSessionManager type was not found.");

        var go = new GameObject("TestSessionManager");
        var manager = go.AddComponent(managerType);
        Invoke(managerType, manager, "ResetSession");
        Invoke(managerType, manager, "StartRun");
        return manager;
    }

    private static T GetProperty<T>(Type type, object target, string propertyName)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected property '" + propertyName + "' was not found.");
        return (T)property.GetValue(target);
    }

    private static void Invoke(Type type, object target, string methodName, params object[] args)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, "Expected method '" + methodName + "' was not found.");
        method.Invoke(target, args);
    }
}
