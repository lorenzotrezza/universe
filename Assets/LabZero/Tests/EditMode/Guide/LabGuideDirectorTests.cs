using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class LabGuideDirectorTests
{
    [Test]
    public void GUID_02_DeterministicScriptOrder()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);
        var capture = LabGuideTestFixtures.SubscribePromptChanged(director);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);

            Assert.AreEqual(0, LabGuideTestFixtures.ActiveStepIndex(director));
            Assert.AreEqual("Guided", LabGuideTestFixtures.Mode(director));
            Assert.That(capture.Prompts[0], Does.Contain("Segui la guida"));

            for (var i = 0; i < LabGuideTestFixtures.SignalIds.Length - 1; i++)
            {
                Assert.IsTrue(LabGuideTestFixtures.TryReportCondition(director, LabGuideTestFixtures.SignalIds[i]));
                Assert.AreEqual(i + 1, LabGuideTestFixtures.ActiveStepIndex(director));
                Assert.That(capture.Prompts[capture.Prompts.Count - 1], Does.Contain(LabGuideTestFixtures.SignalIds[i + 1].Replace("_", " ").Split(' ')[0]));
            }

            Assert.IsTrue(LabGuideTestFixtures.TryReportCondition(director, LabGuideTestFixtures.SignalIds[^1]));
            Assert.AreEqual("FreeRoam", LabGuideTestFixtures.Mode(director));
            Assert.That(capture.Prompts[capture.Prompts.Count - 1], Does.Contain("muoverti"));
            Assert.IsTrue(capture.Severities.Contains("Success"));
            Assert.IsTrue(capture.Statuses.Exists(status => status.Contains("Obiettivo")));
        }
        finally
        {
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    [Test]
    public void GUID_03_ConditionDrivenAdvanceOnly()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);

            Assert.IsFalse(LabGuideTestFixtures.TryReportCondition(director, "wrong_signal"));
            Assert.AreEqual(0, LabGuideTestFixtures.ActiveStepIndex(director));
            Assert.AreEqual(1, LabGuideTestFixtures.MistakeLog(director).Count);

            Assert.IsTrue(LabGuideTestFixtures.TryReportCondition(director, "guide_intro"));
            Assert.AreEqual(1, LabGuideTestFixtures.ActiveStepIndex(director));
        }
        finally
        {
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    [UnityTest]
    public IEnumerator GUID_03_ReminderEscalation()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);
        var capture = LabGuideTestFixtures.SubscribePromptChanged(director);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);

            yield return new WaitForSecondsRealtime(0.07f);

            Assert.That(capture.Severities, Does.Contain("Hint"));
            Assert.That(capture.Severities, Does.Contain("Warning"));
            Assert.Less(capture.Severities.IndexOf("Hint"), capture.Severities.IndexOf("Warning"));
            Assert.That(capture.Prompts.Exists(prompt => prompt.Contains("Suggerimento")), Is.True);
            Assert.That(capture.Prompts.Exists(prompt => prompt.Contains("Attenzione")), Is.True);
        }
        finally
        {
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    [Test]
    public void GUID_03_RecoveryAfterMistake()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);

            Assert.IsFalse(LabGuideTestFixtures.TryReportCondition(director, "phone_used"));
            Assert.AreEqual(0, LabGuideTestFixtures.ActiveStepIndex(director));

            Assert.IsTrue(LabGuideTestFixtures.TryReportCondition(director, "guide_intro"));
            Assert.AreEqual(1, LabGuideTestFixtures.ActiveStepIndex(director));
        }
        finally
        {
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    [Test]
    public void GUID_02_MistakeRecordShape()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);
            LabGuideTestFixtures.TryReportCondition(director, "wrong_signal");

            var mistake = LabGuideTestFixtures.MistakeLog(director)[0];
            Assert.AreEqual("guide_intro", LabGuideTestFixtures.GetObjectProperty(mistake, "StepId"));
            Assert.IsFalse(string.IsNullOrWhiteSpace((string)LabGuideTestFixtures.GetObjectProperty(mistake, "MistakeCode")));
            Assert.IsFalse(string.IsNullOrWhiteSpace((string)LabGuideTestFixtures.GetObjectProperty(mistake, "RiskItalian")));
            Assert.IsFalse(string.IsNullOrWhiteSpace((string)LabGuideTestFixtures.GetObjectProperty(mistake, "ExpectedActionItalian")));
            Assert.IsFalse(string.IsNullOrWhiteSpace((string)LabGuideTestFixtures.GetObjectProperty(mistake, "RecoveryItalian")));
            Assert.GreaterOrEqual((float)LabGuideTestFixtures.GetObjectProperty(mistake, "TimestampSeconds"), 0f);
        }
        finally
        {
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    [Test]
    public void GUID_03_FreeRoamSafetyEventWarnsWithoutBlockingExploration()
    {
        var lesson = LabGuideTestFixtures.CreateLesson();
        var director = LabGuideTestFixtures.CreateDirectorForTests(lesson);
        LabGuideTestFixtures.AddFreeRoamCoach(director);
        var capture = LabGuideTestFixtures.SubscribePromptChanged(director);

        var manager = CreateStartedSessionManager(out var managerType);
        var phone = CreatePhoneInteractable(manager);

        try
        {
            LabGuideTestFixtures.BeginLesson(director);
            LabGuideTestFixtures.CompleteLesson(director);

            Assert.AreEqual("FreeRoam", LabGuideTestFixtures.Mode(director));
            var interactable = phone.GetComponent(Type.GetType("LabSafetyInteractable, Assembly-CSharp"));
            interactable.GetType().GetMethod("Activate").Invoke(interactable, null);

            Assert.AreEqual("FreeRoam", LabGuideTestFixtures.Mode(director));
            Assert.That(capture.Severities, Does.Contain("Warning"));
            Assert.That(capture.Prompts[capture.Prompts.Count - 1], Does.Contain("telefono"));
            Assert.That(LabGuideTestFixtures.MistakeLog(director).Count, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(1, GetProperty<int>(managerType, manager, "MistakeCount"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(phone);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
            LabGuideTestFixtures.DestroyDirector(director);
            UnityEngine.Object.DestroyImmediate(lesson);
        }
    }

    private static Component CreateStartedSessionManager(out Type managerType)
    {
        managerType = Type.GetType("LabSessionManager, Assembly-CSharp");
        Assert.IsNotNull(managerType);

        var go = new GameObject("Test Session Manager");
        var manager = go.AddComponent(managerType);
        managerType.GetMethod("ResetSession").Invoke(manager, null);
        managerType.GetMethod("StartRun").Invoke(manager, null);
        return manager;
    }

    private static GameObject CreatePhoneInteractable(Component manager)
    {
        var phone = new GameObject("Free Roam Phone");
        phone.AddComponent<BoxCollider>();
        var interactableType = Type.GetType("LabSafetyInteractable, Assembly-CSharp");
        var itemTypeType = Type.GetType("LabSafetyItemType, Assembly-CSharp");
        var roleType = Type.GetType("LabSafetyItemRole, Assembly-CSharp");
        var zoneType = Type.GetType("LabSafetyZoneType, Assembly-CSharp");
        Assert.IsNotNull(interactableType);
        Assert.IsNotNull(itemTypeType);
        Assert.IsNotNull(roleType);
        Assert.IsNotNull(zoneType);

        var interactable = phone.AddComponent(interactableType);

        var serialized = new SerializedObject(interactable);
        serialized.FindProperty("sessionManager").objectReferenceValue = manager;
        serialized.FindProperty("itemType").enumValueIndex = (int)Enum.Parse(itemTypeType, "Phone");
        serialized.FindProperty("role").enumValueIndex = (int)Enum.Parse(roleType, "Distractor");
        serialized.FindProperty("currentZone").enumValueIndex = (int)Enum.Parse(zoneType, "Operational");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return phone;
    }

    private static T GetProperty<T>(Type type, object target, string propertyName)
    {
        var property = type.GetProperty(propertyName);
        Assert.IsNotNull(property);
        return (T)property.GetValue(target);
    }
}
