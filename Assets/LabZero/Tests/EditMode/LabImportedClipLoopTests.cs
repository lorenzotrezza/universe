using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LabImportedClipLoopTests
{
    [Test]
    public void ImportedClipLoopSamplesAssignedClipCurves()
    {
        var loopType = Type.GetType("LabImportedClipLoop, Assembly-CSharp");
        Assert.That(loopType, Is.Not.Null, "LabImportedClipLoop type should be available in Assembly-CSharp.");

        var root = new GameObject("ImportedClipLoopHarness");
        var roller = new GameObject("Roller");
        roller.transform.SetParent(root.transform, false);
        root.transform.localPosition = new Vector3(2f, 3f, 4f);
        root.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        root.transform.localScale = new Vector3(1f, 2f, 3f);

        var clip = new AnimationClip();
        clip.legacy = false;
        clip.SetCurve(
            string.Empty,
            typeof(Transform),
            "localPosition.x",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 10f)));
        clip.SetCurve(
            string.Empty,
            typeof(Transform),
            "localEulerAnglesRaw.y",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 90f)));
        clip.SetCurve(
            string.Empty,
            typeof(Transform),
            "localScale.x",
            new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(1f, 0.5f)));
        clip.SetCurve(
            "Roller",
            typeof(Transform),
            "localEulerAnglesRaw.x",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 90f)));

        var component = root.AddComponent(loopType);
        var clipsField = loopType.GetField("clips", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(clipsField, Is.Not.Null, "Loop component should keep assigned imported clips serialized.");
        clipsField.SetValue(component, new[] { clip });

        loopType.GetMethod("Step", BindingFlags.Instance | BindingFlags.Public)?.Invoke(component, new object[] { 0.5f });

        Assert.That(Quaternion.Angle(Quaternion.identity, roller.transform.localRotation), Is.GreaterThan(1f), "The assigned clip should drive the child transform.");
        Assert.That(Vector3.Distance(new Vector3(2f, 3f, 4f), root.transform.localPosition), Is.LessThan(0.001f));
        Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 45f, 0f), root.transform.localRotation), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(new Vector3(1f, 2f, 3f), root.transform.localScale), Is.LessThan(0.001f));

        UnityEngine.Object.DestroyImmediate(root);
    }
}
