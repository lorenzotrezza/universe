using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
public class LabWarehouseInteractionTests
{
    [Test]
    public void HotbarInputBindings_UseRequestedXyAbMapping()
    {
        var routerType = RequireType("LabWarehouseInteractionInputRouter");

        Assert.AreEqual("<XRController>{LeftHand}/{PrimaryButton}", GetStaticString(routerType, "PreviousBinding"), "X should move hotbar selection backward.");
        Assert.AreEqual("<XRController>{LeftHand}/{SecondaryButton}", GetStaticString(routerType, "NextBinding"), "Y should move hotbar selection forward.");
        Assert.AreEqual("<XRController>{RightHand}/{PrimaryButton}", GetStaticString(routerType, "UseBinding"), "A should use the selected item.");
        Assert.AreEqual("<XRController>{RightHand}/{SecondaryButton}", GetStaticString(routerType, "DropBinding"), "B should drop the selected item.");
    }

    [Test]
    public void Hotbar_EquipUseAndDropItem()
    {
        var inventoryType = RequireType("LabHotbarInventory");
        var manager = CreateStartedSessionManager();
        var phone = CreateSafetyInteractable("Phone", "Distractor", "Operational", manager);
        var cameraGo = new GameObject("Hotbar Camera");
        cameraGo.AddComponent<Camera>();
        cameraGo.transform.position = Vector3.zero;
        cameraGo.transform.rotation = Quaternion.identity;
        var inventoryGo = new GameObject("Hotbar Inventory");
        var inventory = inventoryGo.AddComponent(inventoryType);

        try
        {
            Invoke(inventoryType, inventory, "Configure", manager.Component, cameraGo.transform);

            Assert.IsTrue((bool)Invoke(inventoryType, inventory, "TryEquip", phone));
            Assert.AreEqual(1, GetProperty<int>(inventoryType, inventory, "Count"));
            Assert.AreEqual(phone, GetProperty<Component>(inventoryType, inventory, "SelectedItem"));
            Assert.IsTrue(GetProperty<bool>(phone.GetType(), phone, "IsInHotbar"), "Equipped items should leave the world and live in the hotbar.");

            Invoke(inventoryType, inventory, "UseSelected");
            Assert.AreEqual(1, GetProperty<int>(manager.Type, manager.Component, "MistakeCount"), "Using the selected phone should run the safety interaction.");

            Invoke(inventoryType, inventory, "DropSelected");
            Assert.AreEqual(0, GetProperty<int>(inventoryType, inventory, "Count"));
            Assert.IsFalse(GetProperty<bool>(phone.GetType(), phone, "IsInHotbar"), "Dropped items should return to the world.");
            Assert.That(phone.transform.position.z, Is.GreaterThan(0.25f), "Dropped items should land in front of the viewer.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(inventoryGo);
            UnityEngine.Object.DestroyImmediate(cameraGo);
            UnityEngine.Object.DestroyImmediate(phone.gameObject);
            UnityEngine.Object.DestroyImmediate(manager.Component.gameObject);
        }
    }

    [Test]
    public void WarehouseHud_OverlayVisibilityFollowsSessionSettings()
    {
        var hudType = RequireType("LabWarehouseHud");
        var cameraGo = new GameObject("Hud Camera");
        cameraGo.AddComponent<Camera>();
        var hudGo = new GameObject("Warehouse HUD");
        var hud = hudGo.AddComponent(hudType);
        var manager = CreateStartedSessionManager();

        try
        {
            SetField(manager.SettingsType, manager.Settings, "ShowErrorOverlay", false);
            Invoke(hudType, hud, "Configure", manager.Component, cameraGo.transform);
            Invoke(hudType, hud, "Refresh");
            Assert.IsFalse(GetProperty<bool>(hudType, hud, "OverlayVisible"));

            SetField(manager.SettingsType, manager.Settings, "ShowErrorOverlay", true);
            Invoke(hudType, hud, "Refresh");
            Assert.IsTrue(GetProperty<bool>(hudType, hud, "OverlayVisible"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(hudGo);
            UnityEngine.Object.DestroyImmediate(cameraGo);
            UnityEngine.Object.DestroyImmediate(manager.Component.gameObject);
            UnityEngine.Object.DestroyImmediate(manager.Settings);
        }
    }

    [Test]
    public void SafetyInteractable_TriggerSelectionDoesNotRegisterMistake()
    {
        var manager = CreateStartedSessionManager();
        var phone = CreateSafetyInteractable("Phone", "Distractor", "Operational", manager);

        try
        {
            Invoke(phone.GetType(), phone, "SelectForAction");
            Assert.AreEqual(0, GetProperty<int>(manager.Type, manager.Component, "MistakeCount"), "Selecting with trigger should not use the object.");

            Invoke(phone.GetType(), phone, "Activate");
            Assert.AreEqual(1, GetProperty<int>(manager.Type, manager.Component, "MistakeCount"), "Using the selected object should still run safety feedback.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(phone.gameObject);
            UnityEngine.Object.DestroyImmediate(manager.Component.gameObject);
            UnityEngine.Object.DestroyImmediate(manager.Settings);
        }
    }

    [Test]
    public void LocomotionConfigurator_LimitsStructuralClimbing()
    {
        var rig = new GameObject("XR Rig");
        var controller = rig.AddComponent<CharacterController>();
        controller.stepOffset = 1f;
        controller.slopeLimit = 90f;
        var configuratorType = RequireType("LabXRLocomotionConfigurator");
        var configurator = new GameObject("Configurator").AddComponent(configuratorType);

        try
        {
            Invoke(configuratorType, configurator, "Apply");

            Assert.That(controller.stepOffset, Is.LessThanOrEqualTo(0.25f));
            Assert.That(controller.slopeLimit, Is.LessThanOrEqualTo(45f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(configurator.gameObject);
            UnityEngine.Object.DestroyImmediate(rig);
        }
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type should exist.");
        return type;
    }

    private static string GetStaticString(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(field, type.Name + "." + fieldName + " should be public static.");
        return (string)field.GetValue(null);
    }

    private static object Invoke(Type type, object target, string methodName, params object[] args)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, type.Name + "." + methodName + " should exist.");
        return method.Invoke(target, args);
    }

    private static T GetProperty<T>(Type type, object target, string propertyName)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, type.Name + "." + propertyName + " should exist.");
        return (T)property.GetValue(target);
    }

    private static SessionHarness CreateStartedSessionManager()
    {
        var settingsType = RequireType("LabSessionSettings");
        var managerType = RequireType("LabSessionManager");
        var settings = ScriptableObject.CreateInstance(settingsType);
        SetField(settingsType, settings, "TimerMinutes", 7);
        SetField(settingsType, settings, "HelpersEnabled", true);

        var go = new GameObject("Test Session Manager");
        var manager = go.AddComponent(managerType);
        Invoke(managerType, manager, "Initialize", settings);
        Invoke(managerType, manager, "StartRun");
        return new SessionHarness(managerType, manager, settingsType, settings);
    }

    private static Component CreateSafetyInteractable(
        string itemType,
        string role,
        string zone,
        SessionHarness manager)
    {
        var interactableType = RequireType("LabSafetyInteractable");
        var itemTypeType = RequireType("LabSafetyItemType");
        var roleType = RequireType("LabSafetyItemRole");
        var zoneType = RequireType("LabSafetyZoneType");
        var go = new GameObject("Test " + itemType);
        go.AddComponent<BoxCollider>();
        var interactable = go.AddComponent(interactableType);
        var serialized = new SerializedObject(interactable);
        serialized.FindProperty("sessionManager").objectReferenceValue = manager.Component;
        serialized.FindProperty("itemType").enumValueIndex = (int)Enum.Parse(itemTypeType, itemType);
        serialized.FindProperty("role").enumValueIndex = (int)Enum.Parse(roleType, role);
        serialized.FindProperty("currentZone").enumValueIndex = (int)Enum.Parse(zoneType, zone);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return interactable;
    }

    private static void SetField(Type type, object target, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, type.Name + "." + fieldName + " should exist.");
        field.SetValue(target, value);
    }

    private sealed class SessionHarness
    {
        public SessionHarness(Type type, Component component, Type settingsType, ScriptableObject settings)
        {
            Type = type;
            Component = component;
            SettingsType = settingsType;
            Settings = settings;
        }

        public Type Type { get; }
        public Component Component { get; }
        public Type SettingsType { get; }
        public ScriptableObject Settings { get; }
    }
}
