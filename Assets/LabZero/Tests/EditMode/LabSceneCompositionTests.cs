using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

[TestFixture]
public class LabSceneCompositionTests
{
    private const float WarehouseFloorY = -4.38f;

    [Test]
    public void LobbyScene_HidesLegacyFloorAndStatusCanvasInEditMode()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabZero_Prototype.unity");

        AssertActive("LearningDesk_Root");
        AssertInactive("Floor");
        AssertInactive("Lab Status Canvas");
    }

    [Test]
    public void WarehouseScene_UsesOneUniformWarehouseShell()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        Assert.IsNotNull(GameObject.Find("WarehouseEnvironment"), "WarehouseEnvironment should be the single authored warehouse root.");
        Assert.IsNull(GameObject.Find("WarehouseTrainingRoute"), "WarehouseTrainingRoute should not exist as a second parallel environment.");

        var warehouseBay = GameObject.Find("WarehouseBay");
        Assert.IsNotNull(warehouseBay, "WarehouseBay should remain the one warehouse shell from the imported asset.");

        var scale = warehouseBay.transform.localScale;
        Assert.That(scale.x, Is.EqualTo(scale.y).Within(0.001f), "WarehouseBay scale must be uniform on X/Y.");
        Assert.That(scale.x, Is.EqualTo(scale.z).Within(0.001f), "WarehouseBay scale must be uniform on X/Z.");
    }

    [Test]
    public void WarehouseScene_UsesOnlyTheImportedHangarWithTheRealFloor()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        AssertActive("Object_32");
        AssertInactive("Object_30");

        var realFloor = FindSceneObjectIncludingInactive("Object_46");
        Assert.IsNotNull(realFloor, "Object_46 should remain the imported asset floor reference.");
        Assert.IsTrue(realFloor.activeInHierarchy, "The imported floor mesh should stay visible and active.");
        Assert.That(GetRendererBounds(realFloor).max.y, Is.EqualTo(WarehouseFloorY).Within(0.03f), "Object_46 top surface should be the warehouse floor height.");
    }

    [Test]
    public void WarehouseScene_DoesNotUseArtificialFloorOrBoundaryShell()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        AssertMissing("PlayableFloor");
        AssertMissing("Boundary_Left");
        AssertMissing("Boundary_Right");
        AssertMissing("Boundary_Back");
        AssertMissing("Entrance_GuideRail");
    }

    [Test]
    public void WarehouseScene_KeepsStoryAnchorsUnderTheWarehouseRoot()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        var environment = GameObject.Find("WarehouseEnvironment");
        Assert.IsNotNull(environment);

        var anchors = GameObject.Find("WarehouseStoryAnchors");
        Assert.IsNotNull(anchors, "WarehouseStoryAnchors should contain factory props without acting as a second warehouse shell.");
        Assert.AreEqual(environment.transform, anchors.transform.parent, "Story anchors should live under the single warehouse environment root.");

        AssertHasRenderer("Break Room Enclosed");
        AssertHasRenderer("Conveyor Sorting Line");
        AssertHasRenderer("PPE Station Rack");
        AssertHasRenderer("Packaging And Quality Check");
        AssertHasRenderer("Final Conveyor To Loading Door");
        Assert.IsNotNull(GameObject.Find("Loading Truck Placeholder"), "A loading truck placeholder should remain authored in the scene.");
    }

    [Test]
    public void WarehouseScene_KeyStationSupportsRemainAuthoredAndVisible()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        AssertHasRenderer("PPE Bench Support Left");
        AssertHasRenderer("PPE Bench Support Right");
        AssertHasRenderer("Work Bench Support A");
        AssertHasRenderer("Work Bench Support B");
        AssertHasRenderer("Conveyor Support A");
        AssertHasRenderer("Conveyor Support B");
        AssertHasRenderer("Quality Table Support A");
        AssertHasRenderer("Quality Table Support B");
        AssertHasRenderer("Final Conveyor Support A");
        AssertHasRenderer("Final Conveyor Support B");
    }

    private static void AssertActive(string objectName)
    {
        var go = FindSceneObjectIncludingInactive(objectName);
        Assert.IsNotNull(go, objectName + " should exist.");
        Assert.IsTrue(go.activeSelf, objectName + " should be active.");
    }

    private static void AssertInactive(string objectName)
    {
        var go = FindSceneObjectIncludingInactive(objectName);
        Assert.IsNotNull(go, objectName + " should exist.");
        Assert.IsFalse(go.activeSelf, objectName + " should be inactive.");
    }

    private static void AssertMissing(string objectName)
    {
        var go = FindSceneObjectIncludingInactive(objectName);
        Assert.IsNull(go, objectName + " should not exist in the rebuilt warehouse scene.");
    }

    private static void AssertHasRenderer(string objectName)
    {
        var go = FindSceneObjectIncludingInactive(objectName);
        Assert.IsNotNull(go, objectName + " should exist.");
        Assert.IsNotEmpty(go.GetComponentsInChildren<Renderer>(true), objectName + " should keep visible authored geometry.");
    }

    private static Bounds GetRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        Assert.IsNotEmpty(renderers, go.name + " should have renderer bounds.");

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static GameObject FindSceneObjectIncludingInactive(string objectName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go != null && go.name == objectName && go.scene.IsValid())
            {
                return go;
            }
        }

        return null;
    }
}
