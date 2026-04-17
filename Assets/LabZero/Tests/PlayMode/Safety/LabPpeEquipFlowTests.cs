using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LabPpeEquipFlowTests
{
    [UnityTest]
    public IEnumerator PPE_01_ExplicitEquipMatchesSlotItem()
    {
        var manager = CreateSessionManager();
        var casco = CreateAdapter("PPE_HardHat_Adapter", manager, "Head", "HardHat", "casco");
        var cuffie = CreateAdapter("PPE_Earmuffs_Adapter", manager, "Ears", "Earmuffs", "cuffie");
        var occhiali = CreateAdapter("PPE_SafetyGlasses_Adapter", manager, "Eyes", "SafetyGlasses", "occhiali");

        try
        {
            Assert.IsTrue((bool)Invoke(casco, "TryEquip"));
            Assert.IsTrue((bool)Invoke(cuffie, "TryEquip"));
            Assert.IsTrue((bool)Invoke(occhiali, "TryEquip"));
            yield return null;

            Assert.IsTrue((bool)Invoke(manager, "IsRequiredPpeComplete"));
            Assert.That((string)Invoke(manager, "GetPpeSummaryText"), Does.Contain("3/3"));
            Assert.That((string)Invoke(manager, "GetPpeImmediateFeedbackText"), Does.Contain("Occhiali"));
        }
        finally
        {
            UnityEngine.Object.Destroy(casco.gameObject);
            UnityEngine.Object.Destroy(cuffie.gameObject);
            UnityEngine.Object.Destroy(occhiali.gameObject);
            UnityEngine.Object.Destroy(manager.gameObject);
        }
    }

    private static Component CreateSessionManager()
    {
        var managerType = RequireType("LabSessionManager");
        var settingsType = RequireType("LabSessionSettings");
        var settings = ScriptableObject.CreateInstance(settingsType);
        var go = new GameObject("PlayMode Session Manager");
        var manager = go.AddComponent(managerType);
        Invoke(manager, "Initialize", settings);
        Invoke(manager, "StartRun");
        return manager;
    }

    private static Component CreateAdapter(Component manager, string slotName, string itemName, string sourceId)
    {
        return CreateAdapter("PPE_" + itemName + "_Adapter", manager, slotName, itemName, sourceId);
    }

    private static Component CreateAdapter(string objectName, Component manager, string slotName, string itemName, string sourceId)
    {
        var adapterType = RequireType("LabPpeEquipAdapter");
        var slotType = RequireType("LabPpeSlotType");
        var itemType = RequireType("LabPpeItemType");
        var go = new GameObject(objectName);
        go.AddComponent<BoxCollider>().isTrigger = true;
        var adapter = go.AddComponent(adapterType);

        var sessionField = adapterType.GetField("sessionManager", BindingFlags.NonPublic | BindingFlags.Instance);
        var slotField = adapterType.GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance);
        var itemField = adapterType.GetField("itemType", BindingFlags.NonPublic | BindingFlags.Instance);
        var sourceField = adapterType.GetField("sourceId", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(sessionField);
        Assert.IsNotNull(slotField);
        Assert.IsNotNull(itemField);
        Assert.IsNotNull(sourceField);

        sessionField.SetValue(adapter, manager);
        slotField.SetValue(adapter, Enum.Parse(slotType, slotName));
        itemField.SetValue(adapter, Enum.Parse(itemType, itemName));
        sourceField.SetValue(adapter, sourceId);
        return adapter;
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
}
