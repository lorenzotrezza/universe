using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

[TestFixture]
public class LabXRLocomotionConfiguratorTests
{
    private const string LobbyScene = "Assets/LabZero/Scenes/LabZero_Prototype.unity";
    private const string WarehouseScene = "Assets/LabZero/Scenes/LabWarehouse.unity";

    [TestCase(LobbyScene)]
    [TestCase(WarehouseScene)]
    public void Scene_ContainsXrLocomotionConfigurator(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath);

        var configuratorType = RequireType("LabXRLocomotionConfigurator", "Assembly-CSharp");
        Assert.IsNotEmpty(FindSceneComponents(configuratorType), scenePath + " should configure XR locomotion at startup.");
    }

    [Test]
    public void WarehouseScene_UsesHeadRelativeMovementAndSmoothRightTurn()
    {
        EditorSceneManager.OpenScene(WarehouseScene);

        var configuratorType = RequireType("LabXRLocomotionConfigurator", "Assembly-CSharp");
        var configurator = RequireFirstSceneComponent(configuratorType, "XR locomotion configurator should exist in the warehouse scene.");
        configuratorType.GetMethod("Apply", BindingFlags.Instance | BindingFlags.Public)?.Invoke(configurator, Array.Empty<object>());

        var controllerType = RequireType(
            "UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.ControllerInputActionManager",
            "Unity.XR.Interaction.Toolkit.Samples.StarterAssets");
        var leftController = RequireController(controllerType, "left");
        var rightController = RequireController(controllerType, "right");

        AssertBoolProperty(leftController, "smoothMotionEnabled", true);
        AssertBoolProperty(rightController, "smoothMotionEnabled", false);
        AssertBoolProperty(rightController, "smoothTurnEnabled", true);

        var moveProviderType = RequireType(
            "UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.DynamicMoveProvider",
            "Unity.XR.Interaction.Toolkit.Samples.StarterAssets");
        var movementDirectionType = moveProviderType.GetNestedType("MovementDirection");
        var headRelative = Enum.Parse(movementDirectionType, "HeadRelative");
        var moveProviders = FindSceneComponents(moveProviderType);

        Assert.IsNotEmpty(moveProviders, "The XRI rig should contain a DynamicMoveProvider.");
        foreach (var moveProvider in moveProviders)
        {
            Assert.AreEqual(headRelative, GetProperty(moveProvider, "leftHandMovementDirection"));
            Assert.AreEqual(headRelative, GetProperty(moveProvider, "rightHandMovementDirection"));
        }
    }

    private static Type RequireType(string typeName, string assemblyName)
    {
        var type = Type.GetType(typeName + ", " + assemblyName);
        Assert.IsNotNull(type, typeName + " type should be available.");
        return type;
    }

    private static Component RequireFirstSceneComponent(Type type, string message)
    {
        var components = FindSceneComponents(type);
        Assert.IsNotEmpty(components, message);
        return components[0];
    }

    private static Component RequireController(Type controllerType, string controllerName)
    {
        foreach (var controller in FindSceneComponents(controllerType))
        {
            if (controller.gameObject.name.IndexOf(controllerName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return controller;
            }
        }

        Assert.Fail("Expected to find " + controllerName + " controller input manager.");
        return null;
    }

    private static List<Component> FindSceneComponents(Type type)
    {
        var results = new List<Component>();
        foreach (var obj in Resources.FindObjectsOfTypeAll(type))
        {
            if (obj is Component component && component.gameObject.scene.IsValid())
            {
                results.Add(component);
            }
        }

        return results;
    }

    private static void AssertBoolProperty(Component component, string propertyName, bool expected)
    {
        Assert.AreEqual(expected, GetProperty(component, propertyName), component.gameObject.name + "." + propertyName);
    }

    private static object GetProperty(Component component, string propertyName)
    {
        var property = component.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, component.GetType().Name + "." + propertyName + " should exist.");
        return property.GetValue(component);
    }
}
