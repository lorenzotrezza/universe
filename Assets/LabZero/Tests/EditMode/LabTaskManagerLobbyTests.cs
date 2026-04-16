using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LabTaskManagerLobbyTests
{
    private static readonly string[] ForbiddenBudgetNames =
    {
        "AllowedMistake",
        "MistakeBudget",
        "ErrorBudget",
        "FailThreshold",
        "MaxMistake",
    };

    [Test]
    public void DefaultLobbyConfiguration_IsSevenMinutesAndSimulation()
    {
        var manager = CreateManager(out var gameObject);
        try
        {
            Assert.AreEqual(7, GetIntProperty(manager, "TimerMinutes"));
            Assert.IsFalse(GetBoolProperty(manager, "ShowErrorOverlay"));
            Assert.IsTrue(GetBoolProperty(manager, "HelpersEnabled"));
            Assert.AreEqual("Simulation", GetPropertyValue(manager, "RunMode")?.ToString());
            Assert.IsFalse(GetBoolProperty(manager, "RunConfigured"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void AdjustTimer_ClampsBetweenFiveAndTen()
    {
        var manager = CreateManager(out var gameObject);
        try
        {
            InvokeMethod(manager, "AdjustTimer", -99);
            Assert.AreEqual(5, GetIntProperty(manager, "TimerMinutes"));

            InvokeMethod(manager, "AdjustTimer", 2);
            Assert.AreEqual(7, GetIntProperty(manager, "TimerMinutes"));

            InvokeMethod(manager, "AdjustTimer", 99);
            Assert.AreEqual(10, GetIntProperty(manager, "TimerMinutes"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void ErrorOverlay_IsVisibilityOnlyAndHasNoBudgetApi()
    {
        var manager = CreateManager(out var gameObject);
        try
        {
            var before = GetBoolProperty(manager, "ShowErrorOverlay");
            InvokeMethod(manager, "ToggleErrorOverlay");
            var after = GetBoolProperty(manager, "ShowErrorOverlay");

            Assert.AreNotEqual(before, after);

            var publicMemberNames = manager
                .GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(member => member.Name)
                .ToArray();

            foreach (var forbidden in ForbiddenBudgetNames)
            {
                Assert.IsFalse(
                    publicMemberNames.Any(name => name.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0),
                    $"Found forbidden budget API member: {forbidden}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void HelpersAndMode_ToggleThroughItalianStates()
    {
        var manager = CreateManager(out var gameObject);
        try
        {
            Assert.AreEqual("Attivi", InvokeStringMethod(manager, "GetHelpersStateText"));
            Assert.AreEqual("Simulazione", InvokeStringMethod(manager, "GetRunModeText"));

            InvokeMethod(manager, "ToggleHelpers");
            Assert.IsFalse(GetBoolProperty(manager, "HelpersEnabled"));
            Assert.AreEqual("Disattivati", InvokeStringMethod(manager, "GetHelpersStateText"));

            InvokeMethod(manager, "ToggleRunMode");
            Assert.AreEqual("Live", GetPropertyValue(manager, "RunMode")?.ToString());
            Assert.AreEqual("Live", InvokeStringMethod(manager, "GetRunModeText"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void StartConfiguredRun_SetsConfirmationWithoutSceneLoad()
    {
        var manager = CreateManager(out var gameObject);
        try
        {
            InvokeMethod(manager, "StartConfiguredRun");

            Assert.IsTrue(GetBoolProperty(manager, "RunConfigured"));
            Assert.AreEqual("Configurazione confermata", InvokeStringMethod(manager, "GetLobbyStatusText"));
            Assert.AreEqual("Preparazione sessione...", InvokeStringMethod(manager, "GetStartPromptText"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static Component CreateManager(out GameObject gameObject)
    {
        gameObject = new GameObject("LabTaskManagerLobbyTests");
        var managerType = Type.GetType("LabTaskManager, Assembly-CSharp");
        Assert.IsNotNull(managerType, "LabTaskManager type was not found in Assembly-CSharp.");
        return gameObject.AddComponent(managerType);
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

    private static void InvokeMethod(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, $"Expected method '{methodName}' was not found.");
        method.Invoke(target, args);
    }

    private static string InvokeStringMethod(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, $"Expected method '{methodName}' was not found.");
        return (string)method.Invoke(target, null);
    }
}
