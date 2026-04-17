#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LabImportedEquipmentInstaller
{
    private const string ConveyorAssetPath = "Assets/LabZero/Models/ImportedEquipment/rullo strasportatore.fbx";
    private const string PackagingMachineAssetPath = "Assets/LabZero/Models/ImportedEquipment/packagingMachine.glb";
    private const string ConveyorLoopScriptPath = "Assets/LabZero/Scripts/LabImportedClipLoop.cs";

    private const string SortingAnchorName = "Conveyor Sorting Line";
    private const string FinalAnchorName = "Final Conveyor To Loading Door";
    private const string PackagingAnchorName = "Packaging And Quality Check";

    private const string SortingInstanceName = "Animated Conveyor Sorting Line";
    private const string FinalInstanceName = "Animated Final Conveyor To Loading";
    private const string PackagingInstanceName = "Packaging Machine Imported";

    [MenuItem("LabZero/Install Imported Warehouse Equipment")]
    public static void InstallImportedWarehouseEquipment()
    {
        ConfigureConveyorImport();

        InstallConveyor(
            SortingAnchorName,
            SortingInstanceName,
            new Vector3(0f, 0f, 0f),
            IsSortingLegacyConveyorVisual);

        InstallConveyor(
            FinalAnchorName,
            FinalInstanceName,
            new Vector3(0f, 0f, 12.9f),
            IsFinalLegacyConveyorVisual);

        InstallPackagingMachine();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Imported warehouse equipment installed.");
    }

    private static void ConfigureConveyorImport()
    {
        var importer = GetConveyorImporter();
        if (importer == null)
        {
            Debug.LogWarning($"Conveyor model not found at {ConveyorAssetPath}.");
            return;
        }

        var importSettingsChanged = !importer.importAnimation
            || importer.animationType != ModelImporterAnimationType.Generic
            || importer.animationWrapMode != WrapMode.Loop;

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.animationWrapMode = WrapMode.Loop;

        if (importSettingsChanged)
        {
            importer.SaveAndReimport();
            importer = GetConveyorImporter();
            if (importer == null)
            {
                Debug.LogWarning($"Conveyor model not found after reimport: {ConveyorAssetPath}.");
                return;
            }
        }

        var clips = SelectOneLoopingClipPerRoller(importer.defaultClipAnimations);
        if (clips.Length > 0)
        {
            importer.clipAnimations = clips;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            Debug.Log($"Configured {clips.Length} looping imported conveyor animation clips.");
        }
    }

    private static ModelImporter GetConveyorImporter()
    {
        return AssetImporter.GetAtPath(ConveyorAssetPath) as ModelImporter;
    }

    private static void InstallConveyor(
        string anchorName,
        string instanceName,
        Vector3 localPosition,
        System.Func<string, bool> shouldHideLegacyVisual)
    {
        var anchor = FindSceneObject(anchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"Conveyor anchor not found: {anchorName}.");
            return;
        }

        HideLegacyChildren(anchor.transform, shouldHideLegacyVisual);
        DestroyImportedConveyorChildren(anchor.transform);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ConveyorAssetPath);
        if (model == null)
        {
            Debug.LogWarning($"Conveyor model not found at {ConveyorAssetPath}.");
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(model, anchor.scene) as GameObject;
        if (instance == null)
        {
            return;
        }

        instance.name = instanceName;
        instance.transform.SetParent(anchor.transform, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        GroundToAnchorFloor(instance, anchor.transform.position.y);

        var loopScript = AssetDatabase.LoadAssetAtPath<MonoScript>(ConveyorLoopScriptPath);
        var loopType = loopScript != null ? loopScript.GetClass() : null;
        if (loopType == null)
        {
            Debug.LogWarning("LabImportedClipLoop is not compiled yet; conveyor imported animation loop was not attached.");
            return;
        }

        var loop = instance.GetComponent(loopType);
        if (loop == null)
        {
            loop = instance.AddComponent(loopType);
        }

        AssignImportedClips(loop);

        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        GroundToAnchorFloor(instance, anchor.transform.position.y);
    }

    private static ModelImporterClipAnimation[] SelectOneLoopingClipPerRoller(ModelImporterClipAnimation[] sourceClips)
    {
        var selected = new List<ModelImporterClipAnimation>();
        foreach (var group in sourceClips
            .Where(clip => clip.lastFrame > clip.firstFrame)
            .GroupBy(clip => GetAnimatedObjectName(clip.takeName))
            .OrderBy(group => group.Key))
        {
            var clip = group
                .OrderBy(candidate => candidate.takeName, System.StringComparer.Ordinal)
                .First();

            clip.loopTime = true;
            clip.wrapMode = WrapMode.Loop;
            selected.Add(clip);
        }

        return selected.ToArray();
    }

    private static string GetAnimatedObjectName(string takeName)
    {
        var separator = takeName.IndexOf('|');
        return separator >= 0 ? takeName.Substring(0, separator) : takeName;
    }

    private static void AssignImportedClips(Component loop)
    {
        var importedClips = AssetDatabase.LoadAllAssetsAtPath(ConveyorAssetPath)
            .OfType<AnimationClip>()
            .Where(clip => clip != null && clip.length > 0f && !clip.name.StartsWith("__preview", System.StringComparison.Ordinal))
            .OrderBy(clip => clip.name, System.StringComparer.Ordinal)
            .ToArray();

        var serialized = new SerializedObject(loop);
        var clipsProperty = serialized.FindProperty("clips");
        clipsProperty.arraySize = importedClips.Length;
        for (var i = 0; i < importedClips.Length; i++)
        {
            clipsProperty.GetArrayElementAtIndex(i).objectReferenceValue = importedClips[i];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void InstallPackagingMachine()
    {
        var anchor = FindSceneObject(PackagingAnchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"Packaging anchor not found: {PackagingAnchorName}.");
            return;
        }

        DestroyExistingChild(anchor.transform, PackagingInstanceName);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(PackagingMachineAssetPath);
        if (model == null)
        {
            Debug.LogWarning($"Packaging machine model not found at {PackagingMachineAssetPath}.");
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(model, anchor.scene) as GameObject;
        if (instance == null)
        {
            return;
        }

        instance.name = PackagingInstanceName;
        instance.transform.SetParent(anchor.transform, false);
        instance.transform.localPosition = new Vector3(0f, 0f, -2.4f);
        instance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        instance.transform.localScale = Vector3.one;
        GroundToAnchorFloor(instance, anchor.transform.position.y);
    }

    private static bool IsSortingLegacyConveyorVisual(string childName)
    {
        return childName.StartsWith("Conveyor Base", System.StringComparison.Ordinal)
            || childName.StartsWith("Conveyor Belt", System.StringComparison.Ordinal)
            || childName.StartsWith("Conveyor Support", System.StringComparison.Ordinal)
            || childName.StartsWith("Conveyor Roller", System.StringComparison.Ordinal);
    }

    private static bool IsFinalLegacyConveyorVisual(string childName)
    {
        return childName.StartsWith("Final Conveyor Base", System.StringComparison.Ordinal)
            || childName.StartsWith("Final Conveyor Belt", System.StringComparison.Ordinal)
            || childName.StartsWith("Final Conveyor Support", System.StringComparison.Ordinal);
    }

    private static void HideLegacyChildren(Transform anchor, System.Func<string, bool> shouldHide)
    {
        for (var i = 0; i < anchor.childCount; i++)
        {
            var child = anchor.GetChild(i);
            if (shouldHide(child.name))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static void DestroyExistingChild(Transform anchor, string childName)
    {
        for (var i = anchor.childCount - 1; i >= 0; i--)
        {
            var child = anchor.GetChild(i);
            if (child.name == childName)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void DestroyImportedConveyorChildren(Transform anchor)
    {
        for (var i = anchor.childCount - 1; i >= 0; i--)
        {
            var child = anchor.GetChild(i);
            if (IsImportedConveyorInstance(child.name))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static bool IsImportedConveyorInstance(string childName)
    {
        return childName == SortingInstanceName
            || childName == FinalInstanceName
            || childName.StartsWith($"{SortingInstanceName} (", System.StringComparison.Ordinal)
            || childName.StartsWith($"{FinalInstanceName} (", System.StringComparison.Ordinal);
    }

    private static void GroundToAnchorFloor(GameObject instance, float floorY)
    {
        var bounds = CalculateBounds(instance);
        if (bounds.size == Vector3.zero)
        {
            return;
        }

        instance.transform.position += Vector3.up * (floorY - bounds.min.y);
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.zero);
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go != null && go.scene.IsValid() && go.name == objectName)
            {
                return go;
            }
        }

        return null;
    }
}
#endif
