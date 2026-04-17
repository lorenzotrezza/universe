using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
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

    [Test]
    public void ObsoleteWarehouseCompositionEditorScriptIsRemoved()
    {
        Assert.IsFalse(
            File.Exists("Assets/LabZero/Editor/LabWarehouseCompositionEditor.cs"),
            "The generated warehouse composition editor should be removed because it can overwrite hand-authored warehouse placement.");
    }

    [Test]
    public void EquippedPhoneNotificationInvitesBeforeActivationAndActivationPublishesSafetyEvent()
    {
        var equippedPhoneType = Type.GetType("LabEquippedPhoneDistraction, Assembly-CSharp");
        Assert.IsNotNull(equippedPhoneType, "A runtime component should manage the user's equipped phone notification.");

        var contextType = Type.GetType("LabSafetyInteractionContext, Assembly-CSharp");
        Assert.IsNotNull(contextType, "Safety interaction events should publish structured context for the next robot phase.");

        var interactableType = Type.GetType("LabSafetyInteractable, Assembly-CSharp");
        Assert.IsNotNull(interactableType, "LabSafetyInteractable should exist before subscribing to safety events.");

        var activatedEvent = interactableType.GetEvent("Activated", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(activatedEvent, "LabSafetyInteractable should expose a static Activated event for future guide/robot listeners.");

        var manager = CreateStartedSessionManager(out var managerType);
        var phone = new GameObject("Equipped Phone");
        phone.AddComponent<BoxCollider>();
        var interactable = phone.AddComponent(interactableType);
        ConfigureSafetyInteractable(interactable, manager, "Phone", "Distractor", "Operational");

        var anchor = new GameObject("Equipped Phone Anchor").transform;
        var equippedPhone = phone.AddComponent(equippedPhoneType);
        Invoke(equippedPhoneType, equippedPhone, "Configure", anchor, manager);

        Assert.AreEqual(anchor, phone.transform.parent, "The phone should be equipped by parenting it to the provided runtime anchor.");

        var eventCount = 0;
        object lastContext = null;
        var handler = SubscribeToActivated(activatedEvent, contextType, context =>
        {
            eventCount++;
            lastContext = context;
        });

        try
        {
            Invoke(equippedPhoneType, equippedPhone, "TriggerNotification");

            Assert.IsTrue(GetProperty<bool>(equippedPhoneType, equippedPhone, "IsNotificationPending"));
            Assert.AreEqual(0, GetProperty<int>(managerType, manager, "MistakeCount"), "The notification should invite interaction before counting a mistake.");
            Assert.That(GetProperty<string>(managerType, manager, "LastFeedbackText"), Does.Contain("telefono"));

            Invoke(interactableType, interactable, "Activate");

            Assert.AreEqual(1, GetProperty<int>(managerType, manager, "MistakeCount"), "Taking the phone after the notification should register the distraction mistake.");
            Assert.AreEqual(1, eventCount, "Activation should publish one safety event for future guide listeners.");
            Assert.AreEqual("Phone", GetProperty<object>(contextType, lastContext, "ItemType").ToString());
            Assert.AreEqual(true, GetProperty<bool>(contextType, lastContext, "CountsAsMistake"));
            Assert.That(GetProperty<string>(contextType, lastContext, "FeedbackText"), Does.Contain("telefono"));
        }
        finally
        {
            activatedEvent.RemoveEventHandler(null, handler);
            UnityEngine.Object.DestroyImmediate(anchor.gameObject);
            UnityEngine.Object.DestroyImmediate(phone);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
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

    private static void ConfigureSafetyInteractable(
        Component interactable,
        Component manager,
        string itemType,
        string role,
        string zone)
    {
        var itemTypeType = Type.GetType("LabSafetyItemType, Assembly-CSharp");
        var roleType = Type.GetType("LabSafetyItemRole, Assembly-CSharp");
        var zoneType = Type.GetType("LabSafetyZoneType, Assembly-CSharp");
        Assert.IsNotNull(itemTypeType);
        Assert.IsNotNull(roleType);
        Assert.IsNotNull(zoneType);

        var serialized = new SerializedObject(interactable);
        serialized.FindProperty("sessionManager").objectReferenceValue = manager;
        serialized.FindProperty("itemType").enumValueIndex = (int)Enum.Parse(itemTypeType, itemType);
        serialized.FindProperty("role").enumValueIndex = (int)Enum.Parse(roleType, role);
        serialized.FindProperty("currentZone").enumValueIndex = (int)Enum.Parse(zoneType, zone);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Delegate SubscribeToActivated(EventInfo activatedEvent, Type contextType, Action<object> onActivated)
    {
        var method = typeof(LabSafetyStoryTests)
            .GetMethod(nameof(SubscribeToActivatedGeneric), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(contextType);
        return (Delegate)method.Invoke(null, new object[] { activatedEvent, onActivated });
    }

    private static Delegate SubscribeToActivatedGeneric<TContext>(EventInfo activatedEvent, Action<object> onActivated)
    {
        Action<TContext> handler = context => onActivated(context);
        activatedEvent.AddEventHandler(null, handler);
        return handler;
    }
}
