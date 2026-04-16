using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LabSessionManagerTests
{
    private Component _manager;
    private ScriptableObject _settings;
    private Type _managerType;
    private Type _settingsType;
    private Type _stateType;
    private Type _presentationModeType;
    private int _stateChangedCount;

    [SetUp]
    public void SetUp()
    {
        _managerType = RequireType("LabSessionManager");
        _settingsType = RequireType("LabSessionSettings");
        _stateType = RequireType("LabSessionState");
        _presentationModeType = RequireType("LabPresentationMode");

        var go = new GameObject("TestSessionManager");
        _manager = go.AddComponent(_managerType);
        _settings = ScriptableObject.CreateInstance(_settingsType);
        SetField(_settings, "TimerMinutes", 7);
        SetField(_settings, "HelpersEnabled", true);
        SetField(_settings, "PresentationMode", Enum.Parse(_presentationModeType, "Standard"));

        _stateChangedCount = 0;
        var eventInfo = _managerType.GetEvent("StateChanged");
        Assert.IsNotNull(eventInfo);
        eventInfo.AddEventHandler(_manager, new Action(() => _stateChangedCount++));

        Invoke("Initialize", _settings);
        _stateChangedCount = 0;
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_manager.gameObject);
        UnityEngine.Object.DestroyImmediate(_settings);
    }

    [Test]
    public void Initialize_SetsNotStartedState()
    {
        Assert.AreEqual("NotStarted", GetPropertyValue("RunState").ToString());
        Assert.AreEqual(0f, GetFloatProperty("ElapsedTime"));
        Assert.AreEqual(0, GetIntProperty("MistakeCount"));
    }

    [Test]
    public void StartRun_SetsRunningState()
    {
        Invoke("StartRun");

        Assert.AreEqual("Running", GetPropertyValue("RunState").ToString());
    }

    [Test]
    public void RegisterMistake_IncrementsMistakeCount()
    {
        Invoke("StartRun");
        Invoke("RegisterMistake");

        Assert.AreEqual(1, GetIntProperty("MistakeCount"));
        Assert.AreEqual("Running", GetPropertyValue("RunState").ToString());
    }

    [Test]
    public void RegisterMistake_DoesNotBlockRun()
    {
        Invoke("StartRun");
        for (var i = 0; i < 100; i++)
        {
            Invoke("RegisterMistake");
        }

        Assert.AreEqual("Running", GetPropertyValue("RunState").ToString());
        Assert.AreEqual(100, GetIntProperty("MistakeCount"));
    }

    [Test]
    public void CompleteRun_SetsCompleteState()
    {
        Invoke("StartRun");
        Invoke("CompleteRun");

        Assert.AreEqual("Complete", GetPropertyValue("RunState").ToString());
    }

    [Test]
    public void Score_DecreasesWithElapsedTime()
    {
        Invoke("StartRun");
        var initialScore = GetFloatProperty("Score");

        AdvanceElapsedTime(20f);

        Assert.Less(GetFloatProperty("Score"), initialScore);
    }

    [Test]
    public void Score_DecreasesWithMistakes()
    {
        Invoke("StartRun");
        var scoreNoMistakes = GetFloatProperty("Score");

        Invoke("RegisterMistake");

        Assert.Less(GetFloatProperty("Score"), scoreNoMistakes);
    }

    [Test]
    public void Score_NeverNegative()
    {
        Invoke("StartRun");
        for (var i = 0; i < 100; i++)
        {
            Invoke("RegisterMistake");
        }

        Assert.GreaterOrEqual(GetFloatProperty("Score"), 0f);
    }

    [Test]
    public void ResetSession_ClearsAllMutableState()
    {
        Invoke("StartRun");
        Invoke("RegisterMistake");
        AdvanceElapsedTime(12f);

        Invoke("ResetSession");

        Assert.AreEqual("NotStarted", GetPropertyValue("RunState").ToString());
        Assert.AreEqual(0f, GetFloatProperty("ElapsedTime"));
        Assert.AreEqual(0, GetIntProperty("MistakeCount"));
    }

    [Test]
    public void StateChanged_FiresOnMutations()
    {
        Invoke("StartRun");
        Assert.AreEqual(1, _stateChangedCount);

        Invoke("RegisterMistake");
        Assert.AreEqual(2, _stateChangedCount);

        Invoke("CompleteRun");
        Assert.AreEqual(3, _stateChangedCount);

        Invoke("ResetSession");
        Assert.AreEqual(4, _stateChangedCount);
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found in Assembly-CSharp.");
        return type;
    }

    private void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, "Expected field '" + fieldName + "' was not found.");
        field.SetValue(target, value);
    }

    private object GetPropertyValue(string propertyName)
    {
        var property = _managerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected property '" + propertyName + "' was not found.");
        return property.GetValue(_manager);
    }

    private float GetFloatProperty(string propertyName)
    {
        return (float)GetPropertyValue(propertyName);
    }

    private int GetIntProperty(string propertyName)
    {
        return (int)GetPropertyValue(propertyName);
    }

    private void Invoke(string methodName, params object[] args)
    {
        var method = _managerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, "Expected method '" + methodName + "' was not found.");
        method.Invoke(_manager, args);
    }

    private void AdvanceElapsedTime(float deltaTime)
    {
        var method = _managerType.GetMethod("AdvanceElapsedTime", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        method.Invoke(_manager, new object[] { deltaTime });
    }
}
