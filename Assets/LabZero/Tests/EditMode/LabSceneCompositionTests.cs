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
    public void WarehouseScene_PlacesStoryAnchorsOnTheImportedFloor()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        var environment = GameObject.Find("WarehouseEnvironment");
        Assert.IsNotNull(environment);

        var anchors = GameObject.Find("WarehouseStoryAnchors");
        Assert.IsNotNull(anchors, "WarehouseStoryAnchors should contain factory props without acting as a second warehouse shell.");
        Assert.AreEqual(environment.transform, anchors.transform.parent, "Story anchors should live under the single warehouse environment root.");
        Assert.That(anchors.transform.position.y, Is.EqualTo(WarehouseFloorY).Within(0.03f), "Story anchors should be grounded on the imported asset floor, not the old Y=0 floor.");

        Assert.IsNotNull(GameObject.Find("Break Room Enclosed"), "Break area should be an enclosed room in one corner.");
        Assert.IsNotNull(GameObject.Find("Conveyor Sorting Line"), "Conveyor sorting should exist inside the warehouse floor.");
        Assert.IsNotNull(GameObject.Find("PPE Station Rack"), "PPE station should exist inside the warehouse floor.");
        Assert.IsNotNull(GameObject.Find("Final Conveyor To Loading Door"), "Final conveyor should carry packed goods through the loading door.");
        Assert.IsNotNull(GameObject.Find("Loading Truck Placeholder"), "A loading truck placeholder should sit outside the simulated open shutter.");

        AssertContainedByImportedFloorFootprint(GameObject.Find("Break Room Enclosed"), "Break Room Enclosed");
        AssertContainedByImportedFloorFootprint(GameObject.Find("Conveyor Sorting Line"), "Conveyor Sorting Line");
        AssertContainedByImportedFloorFootprint(GameObject.Find("PPE Station Rack"), "PPE Station Rack");
        AssertContainedByImportedFloorFootprint(GameObject.Find("Packaging And Quality Check"), "Packaging And Quality Check");
        AssertOutsideImportedFloorFootprint(GameObject.Find("Loading Truck Placeholder"), "Loading Truck Placeholder");
    }

    [Test]
    public void WarehouseScene_KeyStationSupportsTouchTheImportedFloor()
    {
        EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity");

        AssertBottomTouchesImportedFloor("PPE Bench Support Left");
        AssertBottomTouchesImportedFloor("PPE Bench Support Right");
        AssertBottomTouchesImportedFloor("Work Bench Support A");
        AssertBottomTouchesImportedFloor("Work Bench Support B");
        AssertBottomTouchesImportedFloor("Conveyor Support A");
        AssertBottomTouchesImportedFloor("Conveyor Support B");
        AssertBottomTouchesImportedFloor("Quality Table Support A");
        AssertBottomTouchesImportedFloor("Quality Table Support B");
        AssertBottomTouchesImportedFloor("Final Conveyor Support A");
        AssertBottomTouchesImportedFloor("Final Conveyor Support B");
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

    private static void AssertContainedByImportedFloorFootprint(GameObject go, string label)
    {
        Assert.IsNotNull(go, label + " should exist.");
        var floor = FindSceneObjectIncludingInactive("Object_46");
        Assert.IsNotNull(floor, "Object_46 floor should exist before checking " + label + ".");

        var floorBounds = GetRendererBounds(floor);
        var objectBounds = GetRendererBounds(go);
        Assert.That(objectBounds.min.x, Is.GreaterThanOrEqualTo(floorBounds.min.x - 0.15f), label + " should stay inside the imported floor footprint on X.");
        Assert.That(objectBounds.max.x, Is.LessThanOrEqualTo(floorBounds.max.x + 0.15f), label + " should stay inside the imported floor footprint on X.");
        Assert.That(objectBounds.min.z, Is.GreaterThanOrEqualTo(floorBounds.min.z - 0.15f), label + " should stay inside the imported floor footprint on Z.");
        Assert.That(objectBounds.max.z, Is.LessThanOrEqualTo(floorBounds.max.z + 0.15f), label + " should stay inside the imported floor footprint on Z.");
    }

    private static void AssertOutsideImportedFloorFootprint(GameObject go, string label)
    {
        Assert.IsNotNull(go, label + " should exist.");
        var floor = FindSceneObjectIncludingInactive("Object_46");
        Assert.IsNotNull(floor, "Object_46 floor should exist before checking " + label + ".");

        var floorBounds = GetRendererBounds(floor);
        var objectBounds = GetRendererBounds(go);
        Assert.That(objectBounds.min.z, Is.GreaterThan(floorBounds.max.z), label + " should sit beyond the loading opening, outside the warehouse floor.");
    }

    private static void AssertBottomTouchesImportedFloor(string objectName)
    {
        var go = FindSceneObjectIncludingInactive(objectName);
        Assert.IsNotNull(go, objectName + " should exist.");
        Assert.That(GetRendererBounds(go).min.y, Is.EqualTo(WarehouseFloorY).Within(0.04f), objectName + " should visibly touch the imported floor.");
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
