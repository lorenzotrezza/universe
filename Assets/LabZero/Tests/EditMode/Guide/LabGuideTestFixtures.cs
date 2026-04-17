using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public static class LabGuideTestFixtures
{
    public static readonly string[] SignalIds =
    {
        "guide_intro",
        "raggiungi_area_controllo",
        "interagisci_postazione_dpi",
        "metti_in_sicurezza_passaggio",
        "chiusura_guidata",
    };

    public static ScriptableObject CreateLesson()
    {
        var lessonType = RequireType("LabGuideLessonDefinition");
        var stepType = RequireType("LabGuideStepDefinition");
        var targetKindType = RequireType("LabGuideTargetKind");
        var listType = typeof(List<>).MakeGenericType(stepType);
        var steps = (IList)Activator.CreateInstance(listType);

        steps.Add(CreateStep(stepType, targetKindType, "guide_intro", "Segui la guida e resta nell'area sicura.", "Ti orienta prima di entrare.", "guide_intro", "Area", 0.02f, 0.04f));
        steps.Add(CreateStep(stepType, targetKindType, "raggiungi_area_controllo", "Raggiungi il punto di controllo prima di operare.", "Eviti il traffico interno.", "raggiungi_area_controllo", "Area", 0.02f, 0.04f));
        steps.Add(CreateStep(stepType, targetKindType, "interagisci_postazione_dpi", "Controlla la postazione DPI prima di entrare.", "Riduci i rischi vicino ai macchinari.", "interagisci_postazione_dpi", "Object", 0.02f, 0.04f));
        steps.Add(CreateStep(stepType, targetKindType, "metti_in_sicurezza_passaggio", "Rimuovi l'ostacolo dal passaggio operativo.", "Eviti inciampi e collisioni.", "metti_in_sicurezza_passaggio", "Area", 0.02f, 0.04f));
        steps.Add(CreateStep(stepType, targetKindType, "chiusura_guidata", "Conferma il percorso sicuro completato.", "Fissi le azioni corrette.", "chiusura_guidata", "Area", 0.02f, 0.04f));

        var lesson = ScriptableObject.CreateInstance(lessonType);
        lessonType.GetField("Steps", BindingFlags.Public | BindingFlags.Instance).SetValue(lesson, steps);
        return lesson;
    }

    public static Component CreateDirectorForTests(ScriptableObject lesson)
    {
        var directorType = RequireType("LabGuideDirector");
        var go = new GameObject("Test Guide Director");
        var director = go.AddComponent(directorType);
        var serialized = new SerializedObject(director);
        serialized.FindProperty("lessonDefinition").objectReferenceValue = lesson;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return director;
    }

    public static void AddFreeRoamCoach(Component director)
    {
        var coachType = RequireType("LabGuideFreeRoamCoach");
        var coach = director.gameObject.AddComponent(coachType);
        var serialized = new SerializedObject(coach);
        serialized.FindProperty("director").objectReferenceValue = director;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static PromptCapture SubscribePromptChanged(Component director)
    {
        var promptEvent = director.GetType().GetEvent("PromptChanged", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(promptEvent, "PromptChanged event was not found.");

        var eventType = promptEvent.EventHandlerType;
        var severityType = eventType.GetMethod("Invoke").GetParameters()[0].ParameterType;
        var capture = new PromptCapture();
        var handle = typeof(PromptCapture)
            .GetMethod(nameof(PromptCapture.Handle), BindingFlags.Public | BindingFlags.Instance)
            .MakeGenericMethod(severityType);
        var handler = Delegate.CreateDelegate(eventType, capture, handle);
        promptEvent.AddEventHandler(director, handler);
        return capture;
    }

    public static void DestroyDirector(Component director)
    {
        if (director != null)
        {
            UnityEngine.Object.DestroyImmediate(director.gameObject);
        }
    }

    public static void BeginLesson(Component director)
    {
        Invoke(director, "BeginLesson");
    }

    public static bool TryReportCondition(Component director, string signalId)
    {
        return (bool)Invoke(director, "TryReportCondition", signalId, null, false);
    }

    public static int ActiveStepIndex(Component director)
    {
        return GetProperty<int>(director, "ActiveStepIndex");
    }

    public static string Mode(Component director)
    {
        return GetProperty<object>(director, "Mode").ToString();
    }

    public static IList MistakeLog(Component director)
    {
        return (IList)Invoke(director, "GetMistakeLog");
    }

    public static void CompleteLesson(Component director)
    {
        foreach (var signalId in SignalIds)
        {
            TryReportCondition(director, signalId);
        }
    }

    public static T GetProperty<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected property '" + propertyName + "' was not found.");
        return (T)property.GetValue(target);
    }

    public static object GetObjectProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, "Expected property '" + propertyName + "' was not found.");
        return property.GetValue(target);
    }

    private static Type RequireType(string typeName)
    {
        var type = Type.GetType(typeName + ", Assembly-CSharp");
        Assert.IsNotNull(type, typeName + " type was not found.");
        return type;
    }

    private static object CreateStep(
        Type stepType,
        Type targetKindType,
        string stepId,
        string objective,
        string reason,
        string signal,
        string kind,
        float hintDelay,
        float warningDelay)
    {
        var step = Activator.CreateInstance(stepType);
        SetField(stepType, step, "StepId", stepId);
        SetField(stepType, step, "ObjectiveItalian", objective);
        SetField(stepType, step, "SafetyReasonItalian", reason);
        SetField(stepType, step, "RequiredSignalId", signal);
        SetField(stepType, step, "TargetId", stepId + "_target");
        SetField(stepType, step, "TargetKind", Enum.Parse(targetKindType, kind));
        SetField(stepType, step, "HintDelaySeconds", hintDelay);
        SetField(stepType, step, "WarningDelaySeconds", warningDelay);
        SetField(stepType, step, "ExpectedDurationSeconds", 30f);
        return step;
    }

    private static void SetField(Type type, object target, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, "Expected field '" + fieldName + "' was not found.");
        field.SetValue(target, value);
    }

    private static object Invoke(Component director, string methodName, params object[] args)
    {
        var method = director.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, "Expected method '" + methodName + "' was not found.");
        return method.Invoke(director, args);
    }

    public sealed class PromptCapture
    {
        public readonly List<string> Severities = new();
        public readonly List<string> Prompts = new();
        public readonly List<string> Statuses = new();

        public void Handle<TSeverity>(TSeverity severity, string prompt, string status)
        {
            Severities.Add(severity.ToString());
            Prompts.Add(prompt);
            Statuses.Add(status);
        }
    }
}
