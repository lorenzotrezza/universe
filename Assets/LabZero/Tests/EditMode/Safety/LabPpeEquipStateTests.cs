using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LabPpeEquipStateTests
{
    [Test]
    public void PPE_01_RequiredSlotsStartUnequipped()
    {
        var manager = CreateSessionManager();

        try
        {
            Assert.AreEqual(0, EquippedSlots(manager).Count);
            Assert.IsFalse((bool)Invoke(manager, "IsRequiredPpeComplete"));
            Assert.That((string)Invoke(manager, "GetPpeSummaryText"), Does.Contain("0/3"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void PPE_01_ExplicitEquipMatchesSlotItem()
    {
        var manager = CreateSessionManager();

        try
        {
            Assert.IsTrue(TryEquip(manager, "Head", "HardHat", "casco"));
            Assert.IsTrue(TryEquip(manager, "Ears", "Earmuffs", "cuffie"));
            Assert.IsTrue(TryEquip(manager, "Eyes", "SafetyGlasses", "occhiali"));

            Assert.IsFalse(TryEquip(manager, "Head", "SafetyGlasses", "wrong_head"));
            Assert.IsFalse(TryEquip(manager, "Eyes", "HardHat", "wrong_eyes"));
            Assert.IsTrue((bool)Invoke(manager, "IsRequiredPpeComplete"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void PPE_01_EquipPublishesImmediateFeedback()
    {
        var manager = CreateSessionManager();
        var equippedCount = 0;
        var eventInfo = manager.GetType().GetEvent("PpeSlotEquipped", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(eventInfo);
        var handler = CreatePpeSlotEquippedHandler(eventInfo.EventHandlerType, () => equippedCount++);
        eventInfo.AddEventHandler(manager, handler);

        try
        {
            Assert.IsTrue(TryEquip(manager, "Head", "HardHat", "casco"));

            Assert.AreEqual(1, equippedCount);
            Assert.That((string)Invoke(manager, "GetPpeImmediateFeedbackText"), Does.Contain("Casco"));
        }
        finally
        {
            eventInfo.RemoveEventHandler(manager, handler);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void PPE_01_DuplicateEquipDoesNotDoubleCount()
    {
        var manager = CreateSessionManager();
        var equippedCount = 0;
        var eventInfo = manager.GetType().GetEvent("PpeSlotEquipped", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(eventInfo);
        var handler = CreatePpeSlotEquippedHandler(eventInfo.EventHandlerType, () => equippedCount++);
        eventInfo.AddEventHandler(manager, handler);

        try
        {
            Assert.IsTrue(TryEquip(manager, "Head", "HardHat", "casco"));
            Assert.IsTrue(TryEquip(manager, "Head", "HardHat", "casco_again"));

            Assert.AreEqual(1, EquippedSlots(manager).Count);
            Assert.AreEqual(1, equippedCount);
            Assert.That((string)Invoke(manager, "GetPpeSummaryText"), Does.Contain("1/3"));
        }
        finally
        {
            eventInfo.RemoveEventHandler(manager, handler);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void PPE_01_NoZoneCounterDependency()
    {
        var manager = CreateSessionManager();

        try
        {
            Assert.IsTrue(TryEquip(manager, "Head", "HardHat", "casco"));
            Assert.IsTrue(TryEquip(manager, "Ears", "Earmuffs", "cuffie"));
            Assert.IsTrue(TryEquip(manager, "Eyes", "SafetyGlasses", "occhiali"));

            Assert.IsTrue((bool)Invoke(manager, "IsRequiredPpeComplete"));
            Assert.That((string)Invoke(manager, "GetPpeSummaryText"), Does.Contain("3/3"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
    }

    private static Component CreateSessionManager()
    {
        var managerType = RequireType("LabSessionManager");
        var settingsType = RequireType("LabSessionSettings");
        var settings = ScriptableObject.CreateInstance(settingsType);
        var go = new GameObject("Test Session Manager");
        var manager = go.AddComponent(managerType);
        Invoke(manager, "Initialize", settings);
        return manager;
    }

    private static bool TryEquip(Component manager, string slotName, string itemName, string sourceId)
    {
        var slotType = RequireType("LabPpeSlotType");
        var itemType = RequireType("LabPpeItemType");
        return (bool)Invoke(
            manager,
            "TryEquipPpeSlot",
            Enum.Parse(slotType, slotName),
            Enum.Parse(itemType, itemName),
            sourceId);
    }

    private static ICollection EquippedSlots(Component manager)
    {
        return (ICollection)GetProperty(manager, "EquippedPpeSlots");
    }

    private static Delegate CreatePpeSlotEquippedHandler(Type eventHandlerType, Action onEvent)
    {
        var method = typeof(LabPpeEquipStateTests)
            .GetMethod(nameof(HandlePpeSlotEquipped), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(eventHandlerType.GetMethod("Invoke").GetParameters()[0].ParameterType, eventHandlerType.GetMethod("Invoke").GetParameters()[1].ParameterType);
        return (Delegate)method.Invoke(null, new object[] { eventHandlerType, onEvent });
    }

    private static Delegate HandlePpeSlotEquipped<TSlot, TItem>(Type eventHandlerType, Action onEvent)
    {
        Action<TSlot, TItem, string> handler = (slot, item, sourceId) => onEvent();
        return Delegate.CreateDelegate(eventHandlerType, handler.Target, handler.Method);
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found.");
        return type;
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
}
