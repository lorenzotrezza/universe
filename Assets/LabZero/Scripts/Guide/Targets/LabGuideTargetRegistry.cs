using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LabGuideTargetRegistry : MonoBehaviour
{
    [SerializeField] private List<LabGuideTargetMapping> targets = new();

    public void RegisterObjectTarget(string targetId, Renderer renderer)
    {
        if (string.IsNullOrWhiteSpace(targetId) || renderer == null)
        {
            return;
        }

        Upsert(new LabGuideTargetMapping
        {
            TargetId = targetId,
            Kind = LabGuideTargetKind.Object,
            Renderer = renderer,
        });
    }

    public void RegisterAreaTarget(string targetId, LabGuideAreaCue areaCue)
    {
        if (string.IsNullOrWhiteSpace(targetId) || areaCue == null)
        {
            return;
        }

        Upsert(new LabGuideTargetMapping
        {
            TargetId = targetId,
            Kind = LabGuideTargetKind.Area,
            AreaCue = areaCue,
        });
    }

    public bool TryResolve(string targetId, out LabGuideTargetResolution resolution)
    {
        resolution = null;
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return false;
        }

        foreach (var target in targets)
        {
            if (target == null || !string.Equals(target.TargetId, targetId, StringComparison.Ordinal))
            {
                continue;
            }

            resolution = target.ToResolution();
            return resolution != null;
        }

        return false;
    }

    public void EnsureDefaultWarehouseTargets(Transform guideRoot)
    {
        var objectRenderer = ResolvePpeRenderer();
        if (objectRenderer != null)
        {
            RegisterObjectTarget("postazione_dpi", objectRenderer);
        }

        EnsureAreaTarget(guideRoot, "area_sicura_intro", new Vector3(-0.65f, -1.25f, 1.20f));
        EnsureAreaTarget(guideRoot, "punto_controllo", new Vector3(0.00f, -1.25f, 2.20f));
        EnsureAreaTarget(guideRoot, "passaggio_operativo", new Vector3(1.15f, -1.25f, 3.10f));
        EnsureAreaTarget(guideRoot, "uscita_sicura", new Vector3(-1.10f, -1.25f, 3.70f));
    }

    private void Upsert(LabGuideTargetMapping mapping)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && string.Equals(targets[i].TargetId, mapping.TargetId, StringComparison.Ordinal))
            {
                targets[i] = mapping;
                return;
            }
        }

        targets.Add(mapping);
    }

    private void EnsureAreaTarget(Transform guideRoot, string targetId, Vector3 learnerRelativeOffset)
    {
        if (TryResolve(targetId, out _))
        {
            return;
        }

        var areaGo = new GameObject("GuideAreaCue_" + targetId);
        if (guideRoot != null)
        {
            areaGo.transform.SetParent(guideRoot, false);
        }

        areaGo.transform.position = ResolveAreaPosition(learnerRelativeOffset);
        var areaCue = areaGo.AddComponent<LabGuideAreaCue>();
        RegisterAreaTarget(targetId, areaCue);
    }

    private static Vector3 ResolveAreaPosition(Vector3 learnerRelativeOffset)
    {
        var learner = ResolveLearner();
        if (learner == null)
        {
            return new Vector3(learnerRelativeOffset.x, 1.0f + learnerRelativeOffset.y, learnerRelativeOffset.z);
        }

        return learner.TransformPoint(learnerRelativeOffset);
    }

    private static Transform ResolveLearner()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        var previewCamera = GameObject.Find("WarehousePreviewCamera");
        return previewCamera != null ? previewCamera.transform : null;
    }

    private static Renderer ResolvePpeRenderer()
    {
        var interactables = UnityEngine.Object.FindObjectsByType<LabSafetyInteractable>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var interactable in interactables)
        {
            if (interactable == null || interactable.Role != LabSafetyItemRole.Ppe)
            {
                continue;
            }

            var renderer = interactable.GetComponentInChildren<Renderer>(true);
            if (renderer != null)
            {
                return renderer;
            }
        }

        foreach (var name in new[] { "Casco DPI", "Occhiali DPI", "Cuffie DPI" })
        {
            var item = GameObject.Find(name);
            var renderer = item != null ? item.GetComponentInChildren<Renderer>(true) : null;
            if (renderer != null)
            {
                return renderer;
            }
        }

        return null;
    }
}

[Serializable]
public sealed class LabGuideTargetMapping
{
    public string TargetId;
    public LabGuideTargetKind Kind;
    public Renderer Renderer;
    public LabGuideAreaCue AreaCue;

    public LabGuideTargetResolution ToResolution()
    {
        return Kind switch
        {
            LabGuideTargetKind.Object when Renderer != null => new LabGuideTargetResolution(TargetId, Kind, Renderer, null, Renderer.transform),
            LabGuideTargetKind.Area when AreaCue != null => new LabGuideTargetResolution(TargetId, Kind, null, AreaCue, AreaCue.transform),
            _ => null,
        };
    }
}

public sealed class LabGuideTargetResolution
{
    public LabGuideTargetResolution(
        string targetId,
        LabGuideTargetKind kind,
        Renderer renderer,
        LabGuideAreaCue areaCue,
        Transform focusTransform)
    {
        TargetId = targetId;
        Kind = kind;
        Renderer = renderer;
        AreaCue = areaCue;
        FocusTransform = focusTransform;
    }

    public string TargetId { get; }
    public LabGuideTargetKind Kind { get; }
    public Renderer Renderer { get; }
    public LabGuideAreaCue AreaCue { get; }
    public Transform FocusTransform { get; }
}
