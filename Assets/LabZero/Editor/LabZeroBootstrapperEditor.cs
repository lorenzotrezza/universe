#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class LabZeroBootstrapperEditor
{
    [MenuItem("LabZero/Cleanup Missing Scripts In Open Scene")]
    public static void CleanupMissingScripts()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("Open a scene before running cleanup.");
            return;
        }

        int removed = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            }
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"LabZero cleanup finished. Removed {removed} missing script components.");
    }

    [MenuItem("LabZero/Bootstrap Prototype In Open Scene")]
    public static void BootstrapPrototype()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("Open the copied LabZero scene before bootstrapping.");
            return;
        }

        var existingRoot = GameObject.Find("LabZero");
        if (existingRoot != null)
        {
            Selection.activeObject = existingRoot;
            Debug.LogWarning("LabZero root already exists. Delete it first if you want to rebuild the bootstrap layout.");
            return;
        }

        var root = CreateGameObject("LabZero", Vector3.zero, null);
        var managerObject = CreateGameObject("Lab Task Manager", Vector3.zero, root.transform);
        var manager = managerObject.AddComponent<LabTaskManager>();
        var debugObject = CreateGameObject("Lab Debug Hotkeys", Vector3.zero, root.transform);
        var debugHotkeys = debugObject.AddComponent<LabDebugHotkeys>();
        AssignObjectReference(debugHotkeys, "taskManager", manager);

        var panel = CreateStatusPanel(root.transform, out var title, out var instruction, out var ppe, out var tools, out var hazard, out var summary, out var debugHint);
        var panelBinder = panel.AddComponent<LabStatusPanel>();

        AssignObjectReference(panelBinder, "taskManager", manager);
        AssignObjectReference(panelBinder, "titleText", title);
        AssignObjectReference(panelBinder, "instructionText", instruction);
        AssignObjectReference(panelBinder, "ppeStatusText", ppe);
        AssignObjectReference(panelBinder, "toolStatusText", tools);
        AssignObjectReference(panelBinder, "hazardStatusText", hazard);
        AssignObjectReference(panelBinder, "summaryText", summary);

        AssignObjectReference(manager, "titleText", title);
        AssignObjectReference(manager, "instructionText", instruction);
        AssignObjectReference(manager, "ppeStatusText", ppe);
        AssignObjectReference(manager, "toolStatusText", tools);
        AssignObjectReference(manager, "hazardStatusText", hazard);
        AssignObjectReference(manager, "summaryText", summary);

        CreateZone("PPE Zone", new Vector3(-0.7f, 1.0f, 0.95f), LabTaskType.PPE, new Color(0.2f, 0.55f, 0.95f, 0.85f), root.transform, manager);
        CreateZone("Tool Zone", new Vector3(0.0f, 1.0f, 0.95f), LabTaskType.Tool, new Color(0.3f, 0.65f, 1f, 0.85f), root.transform, manager);
        CreateZone("Hazard Bin", new Vector3(0.7f, 1.0f, 0.95f), LabTaskType.Hazard, new Color(0.9f, 0.25f, 0.25f, 0.9f), root.transform, manager);

        CreateCollectible("Safety Goggles", PrimitiveType.Cylinder, new Vector3(-1.15f, 1.0f, 1.45f), new Vector3(0.16f, 0.05f, 0.16f), new Color(0.2f, 0.8f, 1f), LabTaskType.PPE, true, root.transform);
        CreateCollectible("Gloves", PrimitiveType.Capsule, new Vector3(-0.95f, 1.0f, 1.55f), new Vector3(0.1f, 0.08f, 0.1f), new Color(0.2f, 0.9f, 0.6f), LabTaskType.PPE, true, root.transform);
        CreateCollectible("Lab ID", PrimitiveType.Cube, new Vector3(-0.8f, 1.0f, 1.4f), new Vector3(0.09f, 0.14f, 0.02f), new Color(0.95f, 0.95f, 0.95f), LabTaskType.PPE, true, root.transform);

        CreateCollectible("Beaker", PrimitiveType.Cylinder, new Vector3(-0.15f, 1.0f, 1.55f), new Vector3(0.1f, 0.14f, 0.1f), new Color(0.7f, 0.9f, 1f), LabTaskType.Tool, true, root.transform);
        CreateCollectible("Pipette", PrimitiveType.Capsule, new Vector3(0.05f, 1.05f, 1.45f), new Vector3(0.04f, 0.18f, 0.04f), new Color(1f, 1f, 0.7f), LabTaskType.Tool, true, root.transform);
        CreateCollectible("Probe", PrimitiveType.Cube, new Vector3(0.25f, 1.0f, 1.35f), new Vector3(0.04f, 0.16f, 0.04f), new Color(0.75f, 0.75f, 0.85f), LabTaskType.Tool, true, root.transform);

        CreateCollectible("Open Drink", PrimitiveType.Cylinder, new Vector3(0.95f, 1.0f, 1.45f), new Vector3(0.09f, 0.15f, 0.09f), new Color(1f, 0.2f, 0.2f), LabTaskType.Hazard, true, root.transform);
        CreateCollectible("Headphones", PrimitiveType.Sphere, new Vector3(1.15f, 1.0f, 1.3f), new Vector3(0.1f, 0.1f, 0.1f), new Color(0.45f, 0.35f, 0.85f), LabTaskType.None, false, root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeObject = root;
        Debug.Log("LabZero bootstrap complete. Save the scene as Assets/LabZero/Scenes/LabZero_Prototype.unity if you have not already.");
    }

    private static GameObject CreateStatusPanel(Transform parent, out TMP_Text title, out TMP_Text instruction, out TMP_Text ppe, out TMP_Text tools, out TMP_Text hazard, out TMP_Text summary, out TMP_Text debugHint)
    {
        var canvasObject = CreateGameObject("Lab Status Canvas", new Vector3(-1.15f, 1.62f, 2.1f), parent);
        canvasObject.transform.rotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.0009f;

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(900f, 520f);

        var panel = CreateUiPanel("Panel", canvasObject.transform, new Vector2(900f, 520f), new Color(0.02f, 0.04f, 0.08f, 0.62f));
        title = CreateUiText("Title", panel.transform, "LabZero VR", 54, FontStyles.Bold, new Vector2(0f, -55f));
        instruction = CreateUiText("Instruction", panel.transform, "Choose theme: 1 Cell Biology  2 Quantum Lab  3 AI Safety", 26, FontStyles.Normal, new Vector2(0f, -135f));
        ppe = CreateUiText("PPE Status", panel.transform, "PPE: 0/3", 32, FontStyles.Normal, new Vector2(0f, -240f));
        tools = CreateUiText("Tool Status", panel.transform, "Bench Setup: 0/3", 32, FontStyles.Normal, new Vector2(0f, -295f));
        hazard = CreateUiText("Hazard Status", panel.transform, "Hazard Check: 0/1", 32, FontStyles.Normal, new Vector2(0f, -350f));
        summary = CreateUiText("Summary", panel.transform, "Theme Required", 38, FontStyles.Bold, new Vector2(0f, -435f));
        summary.color = new Color(0.45f, 0.8f, 1f);
        debugHint = CreateUiText("Debug Hint", panel.transform, "Move: WASD + Q/E | Tasks: Z/X/C | SPACE start lesson | R reset | T change theme", 20, FontStyles.Italic, new Vector2(0f, -490f));
        debugHint.color = new Color(0.7f, 0.85f, 1f);

        return canvasObject;
    }

    private static void CreateZone(string name, Vector3 position, LabTaskType type, Color color, Transform parent, LabTaskManager manager)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = name;
        zone.transform.SetParent(parent);
        zone.transform.position = position;
        zone.transform.localScale = new Vector3(0.32f, 0.04f, 0.32f);

        var collider = zone.GetComponent<Collider>();
        collider.isTrigger = true;

        var renderer = zone.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial($"{name} Material", color);

        var labelObject = new GameObject($"{name} Label");
        labelObject.transform.SetParent(zone.transform);
        labelObject.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        var label = labelObject.AddComponent<TextMeshPro>();
        label.text = name;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 4f;
        label.color = Color.white;
        label.rectTransform.sizeDelta = new Vector2(2.4f, 0.5f);

        var zoneComponent = zone.AddComponent<LabTaskZone>();
        AssignObjectReference(zoneComponent, "taskManager", manager);
        AssignEnum(zoneComponent, "acceptedTaskType", type);
        AssignObjectReference(zoneComponent, "zoneRenderer", renderer);
        AssignObjectReference(zoneComponent, "label", label);
        AssignObjectReference(zoneComponent, "idleColor", color);
    }

    private static void CreateCollectible(string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color color, LabTaskType taskType, bool isCorrect, Transform parent)
    {
        var item = GameObject.CreatePrimitive(primitiveType);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;

        var renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial($"{name} Material", color);

        var rigidbody = item.AddComponent<Rigidbody>();
        rigidbody.mass = 0.25f;
        rigidbody.linearDamping = 0.3f;
        rigidbody.angularDamping = 0.3f;

        item.AddComponent<XRGrabInteractable>();

        var collectible = item.AddComponent<LabCollectible>();
        AssignString(collectible, "displayName", name);
        AssignEnum(collectible, "taskType", taskType);
        AssignBool(collectible, "isCorrect", isCorrect);
        AssignBool(collectible, "consumeOnValidDrop", isCorrect && taskType != LabTaskType.None);

        var labelObject = new GameObject($"{name} Label");
        labelObject.transform.SetParent(item.transform);
        labelObject.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        var label = labelObject.AddComponent<TextMeshPro>();
        label.text = name;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 3f;
        label.color = Color.white;
        label.rectTransform.sizeDelta = new Vector2(2.2f, 0.4f);
    }

    private static GameObject CreateGameObject(string name, Vector3 localPosition, Transform parent)
    {
        var gameObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
        return gameObject;
    }

    private static GameObject CreateUiPanel(string name, Transform parent, Vector2 size, Color color)
    {
        var panelObject = CreateGameObject(name, Vector3.zero, parent);
        var image = panelObject.AddComponent<Image>();
        image.color = color;
        var rect = panelObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return panelObject;
    }

    private static TMP_Text CreateUiText(string name, Transform parent, string text, float fontSize, FontStyles fontStyle, Vector2 anchoredPosition)
    {
        var textObject = CreateGameObject(name, Vector3.zero, parent);
        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;

        var rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800f, 60f);
        rect.anchoredPosition = anchoredPosition;
        return textComponent;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var material = new Material(shader)
        {
            name = name,
            color = color,
        };
        return material;
    }

    private static void AssignObjectReference(Object target, string propertyName, Object value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignString(Object target, string propertyName, string value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignBool(Object target, string propertyName, bool value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignEnum(Object target, string propertyName, LabTaskType value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = (int)value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignObjectReference(Object target, string propertyName, Color value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.colorValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
