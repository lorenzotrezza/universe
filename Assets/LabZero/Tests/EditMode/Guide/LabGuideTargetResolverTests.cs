using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LabGuideTargetResolverTests
{
    [Test]
    public void GUID_04_ObjectAndAreaSupport()
    {
        var registryType = RequireType("LabGuideTargetRegistry");
        var areaCueType = RequireType("LabGuideAreaCue");

        var registry = CreateComponent(registryType, "Target Registry");
        var objectGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var areaGo = new GameObject("Area Cue");
        var areaCue = areaGo.AddComponent(areaCueType);

        try
        {
            Invoke(registry, "RegisterObjectTarget", "postazione_dpi", objectGo.GetComponent<Renderer>());
            Invoke(registry, "RegisterAreaTarget", "passaggio_operativo", areaCue);

            AssertResolve(registry, "postazione_dpi", "Object", objectGo.GetComponent<Renderer>(), null);
            AssertResolve(registry, "passaggio_operativo", "Area", null, areaCue);
        }
        finally
        {
            DestroyAll(registry.gameObject, objectGo, areaGo);
        }
    }

    [Test]
    public void GUID_04_HelperOn_OnlyActiveTargetHighlighted()
    {
        var fixture = CreateCueFixture(helpersEnabled: true);

        try
        {
            LabGuideTestFixtures.BeginLesson(fixture.Director);

            Assert.IsTrue((bool)GetProperty(fixture.AreaCueA, "IsPulseActive"));
            Assert.IsFalse((bool)GetProperty(fixture.AreaCueB, "IsPulseActive"));

            LabGuideTestFixtures.TryReportCondition(fixture.Director, "guide_intro");

            Assert.IsFalse((bool)GetProperty(fixture.AreaCueA, "IsPulseActive"));
            Assert.IsTrue((bool)GetProperty(fixture.AreaCueB, "IsPulseActive"));
            Assert.IsTrue((bool)GetProperty(fixture.Service, "StrongCueActive"));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Test]
    public void GUID_04_HelperOff_NoStrongHighlight()
    {
        var fixture = CreateCueFixture(helpersEnabled: false);

        try
        {
            LabGuideTestFixtures.BeginLesson(fixture.Director);

            Assert.IsFalse((bool)GetProperty(fixture.AreaCueA, "IsPulseActive"));
            Assert.IsFalse((bool)GetProperty(fixture.AreaCueB, "IsPulseActive"));
            Assert.IsFalse((bool)GetProperty(fixture.Service, "StrongCueActive"));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Test]
    public void GUID_04_HelperOff_RobotStillOrients()
    {
        var fixture = CreateCueFixture(helpersEnabled: false);

        try
        {
            LabGuideTestFixtures.BeginLesson(fixture.Director);

            Assert.AreEqual(fixture.AreaA.transform, GetProperty(fixture.Presenter, "FocusTarget"));
            Assert.IsTrue((bool)GetProperty(fixture.Presenter, "OrientationOnlyFocus"));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Test]
    public void GUID_04_SubtlePulse()
    {
        var highlighterType = RequireType("LabGuideTargetHighlighter");
        var areaCueType = RequireType("LabGuideAreaCue");
        var highlighter = CreateComponent(highlighterType, "Highlighter");
        var areaCue = CreateComponent(areaCueType, "Area Cue");

        try
        {
            Assert.That((float)GetProperty(highlighter, "PulseFrequency"), Is.InRange(0.4f, 0.8f));
            Assert.That((float)GetProperty(areaCue, "PulseFrequency"), Is.InRange(0.4f, 0.8f));
            Assert.AreEqual(0.6f, (float)GetProperty(highlighter, "PulseFrequency"));
        }
        finally
        {
            DestroyAll(highlighter.gameObject, areaCue.gameObject);
        }
    }

    private static CueFixture CreateCueFixture(bool helpersEnabled)
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);
        var registryType = RequireType("LabGuideTargetRegistry");
        var areaCueType = RequireType("LabGuideAreaCue");
        var presenterType = RequireType("LabGuideRobotPresenter");
        var serviceType = RequireType("LabGuideTargetCueService");

        var registry = CreateComponent(registryType, "Target Registry");
        var presenter = CreateComponent(presenterType, "Guide Presenter");
        var service = CreateComponent(serviceType, "Target Cue Service");
        var sessionManager = CreateSessionManager(helpersEnabled);

        var areaA = new GameObject("Intro Area");
        var areaCueA = areaA.AddComponent(areaCueType);
        var areaB = new GameObject("Control Area");
        var areaCueB = areaB.AddComponent(areaCueType);

        Invoke(registry, "RegisterAreaTarget", "guide_intro_target", areaCueA);
        Invoke(registry, "RegisterAreaTarget", "raggiungi_area_controllo_target", areaCueB);
        Invoke(service, "Bind", director, registry, presenter, sessionManager);

        return new CueFixture(
            lesson,
            director,
            registry,
            presenter,
            service,
            sessionManager,
            areaA,
            areaCueA,
            areaB,
            areaCueB);
    }

    private static Component CreateSessionManager(bool helpersEnabled)
    {
        var settingsType = RequireType("LabSessionSettings");
        var managerType = RequireType("LabSessionManager");
        var settings = ScriptableObject.CreateInstance(settingsType);
        SetField(settingsType, settings, "HelpersEnabled", helpersEnabled);

        var go = new GameObject("Session Manager");
        var manager = go.AddComponent(managerType);
        Invoke(manager, "Initialize", settings);
        return manager;
    }

    private static void AssertResolve(Component registry, string targetId, string expectedKind, Renderer expectedRenderer, Component expectedAreaCue)
    {
        var args = new object[] { targetId, null };
        var resolved = (bool)registry.GetType().GetMethod("TryResolve").Invoke(registry, args);
        Assert.IsTrue(resolved, "Target '" + targetId + "' should resolve.");

        var resolution = args[1];
        Assert.AreEqual(expectedKind, GetProperty(resolution, "Kind").ToString());
        Assert.AreEqual(expectedRenderer, GetProperty(resolution, "Renderer"));
        Assert.AreEqual(expectedAreaCue, GetProperty(resolution, "AreaCue"));
        Assert.IsNotNull(GetProperty(resolution, "FocusTransform"));
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found.");
        return type;
    }

    private static Component CreateComponent(Type type, string name)
    {
        var go = new GameObject(name);
        return go.AddComponent(type);
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

    private static void DestroyAll(params UnityEngine.Object[] objects)
    {
        foreach (var item in objects)
        {
            if (item != null)
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
        }
    }

    private sealed class CueFixture : IDisposable
    {
        public CueFixture(
            ScriptableObject lesson,
            Component director,
            Component registry,
            Component presenter,
            Component service,
            Component sessionManager,
            GameObject areaA,
            Component areaCueA,
            GameObject areaB,
            Component areaCueB)
        {
            Lesson = lesson;
            Director = director;
            Registry = registry;
            Presenter = presenter;
            Service = service;
            SessionManager = sessionManager;
            AreaA = areaA;
            AreaCueA = areaCueA;
            AreaB = areaB;
            AreaCueB = areaCueB;
        }

        public ScriptableObject Lesson { get; }
        public Component Director { get; }
        public Component Registry { get; }
        public Component Presenter { get; }
        public Component Service { get; }
        public Component SessionManager { get; }
        public GameObject AreaA { get; }
        public Component AreaCueA { get; }
        public GameObject AreaB { get; }
        public Component AreaCueB { get; }

        public void Dispose()
        {
            DestroyAll(
                Lesson,
                Director != null ? Director.gameObject : null,
                Registry != null ? Registry.gameObject : null,
                Presenter != null ? Presenter.gameObject : null,
                Service != null ? Service.gameObject : null,
                SessionManager != null ? SessionManager.gameObject : null,
                AreaA,
                AreaB);
        }
    }
}
