using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LabLiveModeTests
{
    [Test]
    public void Initialize_IdenticalState_AcrossModes()
    {
        var standard = CreateManager("Standard");
        var live = CreateManager("Live");

        try
        {
            Assert.AreEqual(GetProperty(standard.Manager, "RunState"), GetProperty(live.Manager, "RunState"));
            Assert.AreEqual(GetProperty(standard.Manager, "ElapsedTime"), GetProperty(live.Manager, "ElapsedTime"));
            Assert.AreEqual(GetProperty(standard.Manager, "MistakeCount"), GetProperty(live.Manager, "MistakeCount"));
            Assert.AreEqual(GetProperty(standard.Manager, "Score"), GetProperty(live.Manager, "Score"));
        }
        finally
        {
            Cleanup(standard);
            Cleanup(live);
        }
    }

    [Test]
    public void RunLifecycle_IdenticalBehavior_AcrossModes()
    {
        var standard = CreateManager("Standard");
        var live = CreateManager("Live");

        try
        {
            Invoke(standard.Manager, "StartRun");
            Invoke(live.Manager, "StartRun");
            Assert.AreEqual(GetProperty(standard.Manager, "RunState"), GetProperty(live.Manager, "RunState"));

            Invoke(standard.Manager, "RegisterMistake");
            Invoke(live.Manager, "RegisterMistake");
            Assert.AreEqual(GetProperty(standard.Manager, "MistakeCount"), GetProperty(live.Manager, "MistakeCount"));
            Assert.AreEqual(GetProperty(standard.Manager, "RunState"), GetProperty(live.Manager, "RunState"));

            Invoke(standard.Manager, "CompleteRun");
            Invoke(live.Manager, "CompleteRun");
            Assert.AreEqual(GetProperty(standard.Manager, "RunState"), GetProperty(live.Manager, "RunState"));
        }
        finally
        {
            Cleanup(standard);
            Cleanup(live);
        }
    }

    [Test]
    public void Score_IdenticalFormula_AcrossModes()
    {
        var standard = CreateManager("Standard");
        var live = CreateManager("Live");

        try
        {
            Invoke(standard.Manager, "StartRun");
            Invoke(live.Manager, "StartRun");

            for (var i = 0; i < 3; i++)
            {
                Invoke(standard.Manager, "RegisterMistake");
                Invoke(live.Manager, "RegisterMistake");
            }

            Assert.AreEqual(GetProperty(standard.Manager, "Score"), GetProperty(live.Manager, "Score"));
        }
        finally
        {
            Cleanup(standard);
            Cleanup(live);
        }
    }

    private static Harness CreateManager(string presentationMode)
    {
        var managerType = RequireType("LabSessionManager");
        var settingsType = RequireType("LabSessionSettings");
        var presentationModeType = RequireType("LabPresentationMode");

        var go = new GameObject("TestManager_" + presentationMode);
        var manager = go.AddComponent(managerType);
        var settings = ScriptableObject.CreateInstance(settingsType);
        SetField(settings, "TimerMinutes", 7);
        SetField(settings, "HelpersEnabled", true);
        SetField(settings, "PresentationMode", Enum.Parse(presentationModeType, presentationMode));
        Invoke(manager, "Initialize", settings);

        return new Harness(manager, settings);
    }

    private static void Cleanup(Harness harness)
    {
        UnityEngine.Object.DestroyImmediate(harness.Manager.gameObject);
        UnityEngine.Object.DestroyImmediate(harness.Settings);
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found in Assembly-CSharp.");
        return type;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, "Expected field '" + fieldName + "' was not found.");
        field.SetValue(target, value);
    }

    private static object GetProperty(Component manager, string propertyName)
    {
        var property = manager.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected property '" + propertyName + "' was not found.");
        return property.GetValue(manager);
    }

    private static void Invoke(Component manager, string methodName, params object[] args)
    {
        var method = manager.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, "Expected method '" + methodName + "' was not found.");
        method.Invoke(manager, args);
    }

    private readonly struct Harness
    {
        public Harness(Component manager, ScriptableObject settings)
        {
            Manager = manager;
            Settings = settings;
        }

        public Component Manager { get; }
        public ScriptableObject Settings { get; }
    }
}
