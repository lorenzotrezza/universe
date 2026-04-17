using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[TestFixture]
public class LabWarehouseAssetReplacementTests
{
    private const string PropsFolder = "Assets/LabZero/Resources/ImportedProps";

    [Test]
    public void RequestedGlbAssetsAreImportedIntoOwnedFolder()
    {
        var expectedAssets = new[]
        {
            "robot.glb",
            "construction_helmet.glb",
            "laser_protection_glasses.glb",
            "safety_headphones.glb",
            "phone.glb",
            "tablet.glb",
            "coffee_machine.glb",
            "potato_chips.glb",
        };

        foreach (var assetName in expectedAssets)
        {
            var path = PropsFolder + "/" + assetName;
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(path), path + " should be imported as an owned LabZero asset.");
        }
    }

    [Test]
    public void WarehouseSceneUsesRequestedAssetsAndRemovesUnprovidedPlaceholders()
    {
        var scene = EditorSceneManager.OpenScene("Assets/LabZero/Scenes/LabWarehouse.unity", OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid());

        AssertHasImportedVisual("Casco DPI", "construction_helmet");
        AssertHasImportedVisual("Occhiali DPI", "laser_protection_glasses");
        AssertHasImportedVisual("Cuffie DPI", "safety_headphones");
        AssertHasImportedVisual("Telefono con Notifica", "phone");
        AssertHasImportedVisual("Tablet Distrattore", "tablet");
        AssertHasImportedVisual("Coffee Machine Ristoro", "coffee_machine");
        AssertHasImportedVisual("Patatine Ristoro 1", "potato_chips");
        AssertHasImportedVisual("Patatine Ristoro 2", "potato_chips");
        AssertHasImportedVisual("Patatine Ristoro 3", "potato_chips");

        Assert.IsNull(GameObject.Find("Guanti DPI"));
        Assert.IsNull(GameObject.Find("Pallina Distrattore"));
        Assert.IsNull(GameObject.Find("Gameboy Distrattore"));
        Assert.IsNull(GameObject.Find("Birra Distrattore"));
        Assert.IsNull(GameObject.Find("Cibo Zona Ristoro"));
    }

    private static void AssertHasImportedVisual(string rootName, string expectedChildName)
    {
        var root = GameObject.Find(rootName);
        Assert.IsNotNull(root, rootName + " should exist in LabWarehouse.");
        var hasImportedChild = root
            .GetComponentsInChildren<Transform>(true)
            .Any(child => child != root.transform && child.name.Contains(expectedChildName));
        Assert.IsTrue(hasImportedChild, rootName + " should contain imported visual child " + expectedChildName + ".");
    }
}
