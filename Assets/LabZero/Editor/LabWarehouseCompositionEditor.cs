#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LabWarehouseCompositionEditor
{
    private const string LobbyScenePath = "Assets/LabZero/Scenes/LabZero_Prototype.unity";
    private const string WarehouseScenePath = "Assets/LabZero/Scenes/LabWarehouse.unity";

    private static Material _darkMetal;
    private static Material _belt;
    private static Material _yellow;
    private static Material _cardboard;
    private static Material _blue;
    private static Material _green;
    private static Material _orange;
    private static Material _red;
    private static Material _white;
    private static Material _black;
    private static Material _glass;
    private static AudioClip _notificationClip;

    [MenuItem("LabZero/Rebuild Warehouse Composition")]
    public static void RebuildWarehouseComposition()
    {
        CleanupLobbyScene();
        RebuildWarehouseScene();
    }

    private static void CleanupLobbyScene()
    {
        EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        SetInactiveIfPresent("Floor");
        SetInactiveIfPresent("Lab Status Canvas");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void RebuildWarehouseScene()
    {
        EditorSceneManager.OpenScene(WarehouseScenePath, OpenSceneMode.Single);
        LoadMaterials();

        var environment = FindSceneObject("WarehouseEnvironment");
        if (environment == null)
        {
            environment = new GameObject("WarehouseEnvironment");
        }

        DestroyIfPresent("WarehouseTrainingRoute");
        DestroyIfPresent("WarehouseStoryAnchors");

        RestoreSingleWarehouseShell(environment);
        RepositionPlayerStart(environment);

        var anchors = MakeGroup("WarehouseStoryAnchors", environment.transform, Vector3.zero);
        BuildPpeStation(anchors.transform);
        BuildBreakCorner(anchors.transform);
        BuildConveyorLine(anchors.transform);
        BuildPackagingArea(anchors.transform);
        BuildMachineCell(anchors.transform);
        BuildReceivingArea(anchors.transform);
        BuildDistractionObjects(anchors.transform);
        BuildRouteMarkers(anchors.transform);

        EditorUtility.SetDirty(anchors);
        EditorUtility.SetDirty(environment);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void LoadMaterials()
    {
        _darkMetal = LoadMaterial("Assets/LabZero/Materials/Safety_Dark_Metal.mat");
        _belt = LoadMaterial("Assets/LabZero/Materials/Safety_Rubber_Belt.mat");
        _yellow = LoadMaterial("Assets/LabZero/Materials/Safety_FloorMark_Yellow.mat");
        _cardboard = LoadMaterial("Assets/LabZero/Materials/Safety_Cardboard.mat");
        _blue = LoadMaterial("Assets/LabZero/Materials/Safety_Action_Blue.mat");
        _green = LoadMaterial("Assets/LabZero/Materials/Safety_Green.mat");
        _orange = LoadMaterial("Assets/LabZero/Materials/Safety_Orange.mat");
        _red = LoadMaterial("Assets/LabZero/Materials/Safety_Red.mat");
        _white = LoadMaterial("Assets/LabZero/Materials/Safety_White.mat");
        _black = LoadMaterial("Assets/LabZero/Materials/Safety_Hazard_Black.mat");
        _glass = LoadMaterial("Assets/LabZero/Materials/Safety_Glass_Blue.mat");
        _notificationClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LabZero/Audio/GenericNotification.wav");
    }

    private static Material LoadMaterial(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    private static void RestoreSingleWarehouseShell(GameObject environment)
    {
        var warehouseBay = FindSceneObject("WarehouseBay");
        if (warehouseBay == null)
        {
            return;
        }

        warehouseBay.transform.SetParent(environment.transform, false);
        warehouseBay.transform.localPosition = Vector3.zero;
        warehouseBay.transform.localRotation = Quaternion.identity;
        warehouseBay.transform.localScale = Vector3.one;
        EditorUtility.SetDirty(warehouseBay);
    }

    private static void RepositionPlayerStart(GameObject environment)
    {
        var spawnPosition = new Vector3(-31f, 0f, -1.75f);
        var spawnRotation = Quaternion.Euler(0f, 86f, 0f);

        var spawnPoint = FindSceneObject("SpawnPoint");
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("SpawnPoint");
        }

        spawnPoint.transform.SetParent(environment.transform, false);
        spawnPoint.transform.localPosition = spawnPosition;
        spawnPoint.transform.localRotation = spawnRotation;
        EditorUtility.SetDirty(spawnPoint);

        var xrRig = FindSceneObject("XRRig (Controllers + Hands)");
        if (xrRig != null)
        {
            xrRig.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            EditorUtility.SetDirty(xrRig);
        }

        var preview = FindSceneObject("WarehousePreview");
        if (preview != null)
        {
            preview.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            ConfigureWarehousePreview(preview, spawnPoint.transform, environment);
            EditorUtility.SetDirty(preview);
        }

        var bootstrap = FindSceneComponent<LabWarehouseBootstrap>();
        if (bootstrap != null)
        {
            var serialized = new SerializedObject(bootstrap);
            var environmentRoot = serialized.FindProperty("warehouseEnvironmentRoot");
            if (environmentRoot != null)
            {
                environmentRoot.objectReferenceValue = environment;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureWarehousePreview(GameObject preview, Transform spawnPoint, GameObject environment)
    {
        var previewComponent = preview.GetComponent<LabWarehousePreview>();
        if (previewComponent == null)
        {
            return;
        }

        var serialized = new SerializedObject(previewComponent);
        serialized.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
        serialized.FindProperty("cameraHeight").floatValue = 1.62f;
        serialized.FindProperty("allowVerticalDebugMovement").boolValue = false;
        serialized.FindProperty("snapSpawnToFloor").boolValue = true;
        serialized.FindProperty("warehouseEnvironmentRoot").objectReferenceValue = environment;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildPpeStation(Transform anchors)
    {
        var ppe = MakeGroup("PPE Station Rack", anchors, new Vector3(-27f, 0f, -9.5f));
        MakeCube("PPE Rack Back", ppe.transform, new Vector3(0f, 1.25f, 0.25f), new Vector3(3.2f, 2.1f, 0.12f), _darkMetal);
        MakeCube("PPE Bench", ppe.transform, new Vector3(0f, 0.32f, -0.45f), new Vector3(3.3f, 0.18f, 0.7f), _cardboard);
        MakeCylinder("PPE Left Post", ppe.transform, new Vector3(-1.45f, 1.15f, -0.05f), new Vector3(0.06f, 1.1f, 0.06f), Vector3.zero, _darkMetal);
        MakeCylinder("PPE Right Post", ppe.transform, new Vector3(1.45f, 1.15f, -0.05f), new Vector3(0.06f, 1.1f, 0.06f), Vector3.zero, _darkMetal);
        MakeLabel("PPE Station Label", ppe.transform, "DPI: casco, occhiali, cuffie, guanti, gilet, scarpe", new Vector3(0f, 2.45f, -0.15f), new Vector3(65f, 0f, 0f), 0.22f, 4.8f);

        ConfigureInteractable(MakeSphere("Casco DPI", ppe.transform, new Vector3(-1.1f, 0.72f, -0.45f), new Vector3(0.45f, 0.24f, 0.36f), _yellow), LabSafetyItemType.Helmet, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Occhiali DPI", ppe.transform, new Vector3(-0.45f, 0.72f, -0.45f), new Vector3(0.55f, 0.08f, 0.12f), _glass), LabSafetyItemType.SafetyGlasses, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeSphere("Cuffie DPI", ppe.transform, new Vector3(0.25f, 0.72f, -0.45f), new Vector3(0.42f, 0.28f, 0.28f), _blue), LabSafetyItemType.HearingProtection, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeSphere("Guanti DPI", ppe.transform, new Vector3(0.9f, 0.72f, -0.45f), new Vector3(0.34f, 0.16f, 0.22f), _red), LabSafetyItemType.Gloves, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Gilet Alta Visibilita", ppe.transform, new Vector3(-0.75f, 1.45f, -0.05f), new Vector3(0.45f, 0.7f, 0.08f), _green), LabSafetyItemType.HighVisibilityVest, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Scarpe Antinfortunistiche", ppe.transform, new Vector3(0.75f, 1.05f, -0.05f), new Vector3(0.75f, 0.22f, 0.22f), _black), LabSafetyItemType.SafetyShoes, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
    }

    private static void BuildBreakCorner(Transform anchors)
    {
        var breakRoom = MakeGroup("Break Corner Open Room", anchors, new Vector3(-25.5f, 0f, 15.5f));
        MakeCube("Break Room Divider Wall", breakRoom.transform, new Vector3(3.3f, 1.1f, 0f), new Vector3(0.18f, 2.2f, 5.6f), _darkMetal);
        MakeCube("Break Room Floor Marker", breakRoom.transform, new Vector3(0f, 0.015f, -2.25f), new Vector3(5.6f, 0.03f, 0.14f), _yellow);
        MakeCube("Break Room Floor Marker Left", breakRoom.transform, new Vector3(-2.75f, 0.015f, 0f), new Vector3(0.14f, 0.03f, 4.5f), _yellow);
        MakeCube("Break Room Floor Marker Right", breakRoom.transform, new Vector3(2.75f, 0.015f, 0f), new Vector3(0.14f, 0.03f, 4.5f), _yellow);
        MakeCube("Break Counter Long", breakRoom.transform, new Vector3(-0.9f, 0.48f, 1.75f), new Vector3(4.4f, 0.18f, 0.62f), _cardboard);
        MakeCube("Break Counter Side", breakRoom.transform, new Vector3(-2.35f, 0.48f, 0.2f), new Vector3(0.62f, 0.18f, 2.5f), _cardboard);
        MakeCube("Coffee Machine Placeholder", breakRoom.transform, new Vector3(-1.6f, 0.92f, 1.75f), new Vector3(0.45f, 0.65f, 0.42f), _darkMetal);
        MakeCylinder("Coffee Cup", breakRoom.transform, new Vector3(-0.95f, 0.82f, 1.65f), new Vector3(0.11f, 0.12f, 0.11f), Vector3.zero, _white);
        ConfigureInteractable(MakeCube("Cibo Zona Ristoro", breakRoom.transform, new Vector3(-0.25f, 0.82f, 1.75f), new Vector3(0.52f, 0.14f, 0.36f), _orange), LabSafetyItemType.Food, LabSafetyItemRole.BreakAreaOnly, LabSafetyZoneType.BreakCorner, null);
        ConfigureInteractable(MakeCylinder("Birra Vietata", breakRoom.transform, new Vector3(0.55f, 0.88f, 1.72f), new Vector3(0.1f, 0.28f, 0.1f), Vector3.zero, _red), LabSafetyItemType.Beer, LabSafetyItemRole.Prohibited, LabSafetyZoneType.BreakCorner, null);
        MakeCylinder("Break Stool A", breakRoom.transform, new Vector3(0.9f, 0.32f, -0.95f), new Vector3(0.35f, 0.18f, 0.35f), Vector3.zero, _darkMetal);
        MakeCylinder("Break Stool B", breakRoom.transform, new Vector3(-0.2f, 0.32f, -1.1f), new Vector3(0.35f, 0.18f, 0.35f), Vector3.zero, _darkMetal);
        MakeLabel("Break Room Label", breakRoom.transform, "Zona ristoro: cibo solo qui, alcol vietato", new Vector3(0f, 2.25f, 2.1f), new Vector3(65f, 180f, 0f), 0.22f, 4.8f);
    }

    private static void BuildConveyorLine(Transform anchors)
    {
        var conveyor = MakeGroup("Conveyor Sorting Line", anchors, new Vector3(-7f, 0f, -1.5f));
        MakeCube("Conveyor Base", conveyor.transform, new Vector3(0f, 0.55f, 0f), new Vector3(12f, 0.45f, 2.2f), _darkMetal);
        MakeCube("Conveyor Belt", conveyor.transform, new Vector3(0f, 0.83f, 0f), new Vector3(11.6f, 0.12f, 1.75f), _belt);

        for (var i = 0; i < 8; i++)
        {
            MakeCylinder("Conveyor Roller " + i, conveyor.transform, new Vector3(-5.1f + i * 1.45f, 1.02f, 0f), new Vector3(0.08f, 0.9f, 0.08f), new Vector3(90f, 0f, 0f), _darkMetal);
        }

        MakeCube("Sorting Sensor Gate", conveyor.transform, new Vector3(2.5f, 1.7f, 0f), new Vector3(0.16f, 1.6f, 2.4f), _blue);
        MakeCube("Sorting Box A", conveyor.transform, new Vector3(-2.2f, 1.25f, -0.15f), new Vector3(0.95f, 0.55f, 0.72f), _cardboard);
        MakeCube("Sorting Box B", conveyor.transform, new Vector3(1.1f, 1.25f, 0.25f), new Vector3(0.75f, 0.5f, 0.62f), _cardboard);
        MakeLabel("Conveyor Label", conveyor.transform, "Smistamento merci: resta nel percorso segnato", new Vector3(0f, 2.35f, -1.35f), new Vector3(65f, 0f, 0f), 0.22f, 5.5f);

        for (var i = 0; i < 6; i++)
        {
            MakeCube("Sorting Floor Stripe " + i, conveyor.transform, new Vector3(-5.4f + i * 2.1f, 0.025f, -1.85f), new Vector3(1.35f, 0.035f, 0.12f), _yellow);
        }
    }

    private static void BuildPackagingArea(Transform anchors)
    {
        var packaging = MakeGroup("Packaging And Pallet Area", anchors, new Vector3(10f, 0f, -9.5f));
        MakeCube("Packaging Table", packaging.transform, new Vector3(0f, 0.78f, 0f), new Vector3(4.4f, 0.18f, 1.5f), _cardboard);
        MakeCube("Packaging Tape Roll", packaging.transform, new Vector3(-1.35f, 1.02f, 0.15f), new Vector3(0.45f, 0.12f, 0.45f), _yellow);
        MakeCube("Packaging Scanner", packaging.transform, new Vector3(1.3f, 1.05f, -0.1f), new Vector3(0.45f, 0.22f, 0.32f), _blue);
        MakePallet(packaging.transform, new Vector3(0.1f, 0f, 2.25f), _cardboard);
        MakeCube("Packaging Box Stack 1", packaging.transform, new Vector3(0.1f, 0.82f, 2.25f), new Vector3(0.9f, 0.7f, 0.8f), _cardboard);
        MakeCube("Packaging Box Stack 2", packaging.transform, new Vector3(1.05f, 0.62f, 2.22f), new Vector3(0.72f, 0.48f, 0.7f), _cardboard);
        MakeLabel("Packaging Label", packaging.transform, "Imballaggio: mani libere, niente distrazioni", new Vector3(0f, 2.15f, 0f), new Vector3(65f, 0f, 0f), 0.22f, 4.8f);
    }

    private static void BuildMachineCell(Transform anchors)
    {
        var machine = MakeGroup("Machine Hazard Cell", anchors, new Vector3(11.5f, 0f, 9.5f));
        MakeCube("Machine Body", machine.transform, new Vector3(0f, 1.0f, 0f), new Vector3(3.2f, 2.0f, 1.8f), _darkMetal);
        MakeCube("Machine Infeed", machine.transform, new Vector3(-2.5f, 0.75f, 0f), new Vector3(1.8f, 0.35f, 1.0f), _belt);
        MakeCube("Machine Control Panel", machine.transform, new Vector3(2.0f, 1.15f, -0.65f), new Vector3(0.55f, 0.9f, 0.18f), _blue);
        MakeSphere("Emergency Stop Button", machine.transform, new Vector3(2.02f, 1.28f, -0.78f), new Vector3(0.22f, 0.22f, 0.08f), _red);
        MakeCube("Machine Safety Rail Front", machine.transform, new Vector3(0f, 0.68f, -1.45f), new Vector3(4.8f, 0.12f, 0.12f), _yellow);
        MakeCube("Machine Safety Rail Back", machine.transform, new Vector3(0f, 0.68f, 1.45f), new Vector3(4.8f, 0.12f, 0.12f), _yellow);
        MakeLabel("Machine Label", machine.transform, "Area macchine: casco, cuffie e distanza", new Vector3(0f, 2.55f, 0f), new Vector3(65f, 0f, 0f), 0.22f, 4.8f);
    }

    private static void BuildReceivingArea(Transform anchors)
    {
        var receiving = MakeGroup("Receiving Freight Area", anchors, new Vector3(21f, 0f, -4f));
        MakeCube("Container Placeholder Blue", receiving.transform, new Vector3(0f, 1.2f, -4.4f), new Vector3(6.6f, 2.4f, 2.2f), _blue);
        MakeCube("Container Placeholder Green", receiving.transform, new Vector3(4.2f, 1.2f, -1.7f), new Vector3(6.0f, 2.4f, 2.2f), _green);

        for (var i = 0; i < 6; i++)
        {
            MakeCube("Container Rib Blue " + i, receiving.transform, new Vector3(-3f + i * 1.2f, 1.22f, -5.54f), new Vector3(0.08f, 2.25f, 0.08f), _darkMetal);
        }

        MakePallet(receiving.transform, new Vector3(-2.8f, 0f, 1.0f), _cardboard);
        MakeCube("Received Crate", receiving.transform, new Vector3(-2.8f, 0.7f, 1.0f), new Vector3(1.25f, 0.95f, 1.05f), _cardboard);
        MakeLabel("Receiving Label", receiving.transform, "Ricevimento merci e container", new Vector3(0f, 2.8f, -3.4f), new Vector3(65f, 0f, 0f), 0.24f, 4.0f);
    }

    private static void BuildDistractionObjects(Transform anchors)
    {
        var distractions = MakeGroup("Distraction Objects", anchors, Vector3.zero);
        ConfigureInteractable(MakeCube("Telefono con Notifica", distractions.transform, new Vector3(-3f, 1.25f, -2.15f), new Vector3(0.18f, 0.035f, 0.36f), _black), LabSafetyItemType.Phone, LabSafetyItemRole.Distractor, LabSafetyZoneType.Sorting, _notificationClip);
        ConfigureInteractable(MakeCube("Tablet Distrattore", distractions.transform, new Vector3(8.6f, 1.04f, -9.45f), new Vector3(0.55f, 0.04f, 0.38f), _glass), LabSafetyItemType.Tablet, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeSphere("Pallina Distrattore", distractions.transform, new Vector3(8.1f, 1.02f, -9.0f), new Vector3(0.34f, 0.34f, 0.34f), _red), LabSafetyItemType.Ball, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeCube("Gameboy Distrattore", distractions.transform, new Vector3(10.8f, 1.04f, -9.4f), new Vector3(0.42f, 0.08f, 0.55f), _green), LabSafetyItemType.HandheldGame, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeCube("Cibo Fuori Area", distractions.transform, new Vector3(12.4f, 1.04f, -9.1f), new Vector3(0.42f, 0.12f, 0.3f), _orange), LabSafetyItemType.Food, LabSafetyItemRole.BreakAreaOnly, LabSafetyZoneType.Packaging, null);
    }

    private static void BuildRouteMarkers(Transform anchors)
    {
        var route = MakeGroup("Ground Floor Training Route", anchors, Vector3.zero);
        MakeCube("Route Marker From Entrance", route.transform, new Vector3(-25f, 0.035f, -5.4f), new Vector3(4.5f, 0.04f, 0.16f), _yellow);
        MakeCube("Route Marker To Conveyor", route.transform, new Vector3(-16f, 0.035f, -4.3f), new Vector3(8.5f, 0.04f, 0.16f), _yellow);
        MakeCube("Route Marker To Packaging", route.transform, new Vector3(4.5f, 0.035f, -6.2f), new Vector3(8f, 0.04f, 0.16f), _yellow);
        MakeCube("Route Marker To Machine", route.transform, new Vector3(8.8f, 0.035f, 3.2f), new Vector3(0.16f, 0.04f, 7.6f), _yellow);
    }

    private static GameObject MakeGroup(string name, Transform parent, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static GameObject MakeCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        SetMaterial(go, material);
        return go;
    }

    private static GameObject MakeCylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Vector3 euler, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = localScale;
        SetMaterial(go, material);
        return go;
    }

    private static GameObject MakeSphere(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        SetMaterial(go, material);
        return go;
    }

    private static TextMeshPro MakeLabel(string name, Transform parent, string text, Vector3 localPosition, Vector3 euler, float fontSize, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = Vector3.one;

        var label = go.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.rectTransform.sizeDelta = new Vector2(width, 1f);
        return label;
    }

    private static void MakePallet(Transform parent, Vector3 origin, Material wood)
    {
        for (var i = 0; i < 4; i++)
        {
            MakeCube("Pallet Slat " + i, parent, origin + new Vector3(0f, 0.18f + i * 0.08f, -0.75f + i * 0.5f), new Vector3(2.2f, 0.06f, 0.18f), wood);
        }

        for (var i = 0; i < 3; i++)
        {
            MakeCube("Pallet Runner " + i, parent, origin + new Vector3(-0.8f + i * 0.8f, 0.08f, 0f), new Vector3(0.16f, 0.12f, 1.8f), wood);
        }
    }

    private static void ConfigureInteractable(GameObject go, LabSafetyItemType itemType, LabSafetyItemRole role, LabSafetyZoneType zone, AudioClip clip)
    {
        if (go.GetComponent<Collider>() == null)
        {
            go.AddComponent<BoxCollider>();
        }

        var interactable = go.GetComponent<LabSafetyInteractable>();
        if (interactable == null)
        {
            interactable = go.AddComponent<LabSafetyInteractable>();
        }

        var label = go.transform.Find("Item Label") != null
            ? go.transform.Find("Item Label").GetComponent<TextMeshPro>()
            : MakeLabel("Item Label", go.transform, go.name, new Vector3(0f, 0.45f, 0f), new Vector3(70f, 0f, 0f), 0.18f, 1.3f);

        AudioSource source = null;
        if (clip != null)
        {
            source = go.GetComponent<AudioSource>();
            if (source == null)
            {
                source = go.AddComponent<AudioSource>();
            }

            source.clip = clip;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.volume = 0.65f;
        }

        var renderer = go.GetComponentInChildren<Renderer>();
        var serialized = new SerializedObject(interactable);
        serialized.FindProperty("sessionManager").objectReferenceValue = FindSceneComponent<LabSessionManager>();
        serialized.FindProperty("itemType").enumValueIndex = (int)itemType;
        serialized.FindProperty("role").enumValueIndex = (int)role;
        serialized.FindProperty("currentZone").enumValueIndex = (int)zone;
        serialized.FindProperty("targetRenderer").objectReferenceValue = renderer;
        serialized.FindProperty("label").objectReferenceValue = label;
        serialized.FindProperty("audioSource").objectReferenceValue = source;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetMaterial(GameObject go, Material material)
    {
        if (go == null || material == null)
        {
            return;
        }

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void SetInactiveIfPresent(string name)
    {
        var go = FindSceneObject(name);
        if (go != null)
        {
            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }
    }

    private static void DestroyIfPresent(string name)
    {
        var go = FindSceneObject(name);
        if (go != null)
        {
            Object.DestroyImmediate(go);
        }
    }

    private static GameObject FindSceneObject(string name)
    {
        var activeScene = SceneManager.GetActiveScene();
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go != null && go.name == name && go.scene.IsValid() && go.scene == activeScene)
            {
                return go;
            }
        }

        return null;
    }

    private static T FindSceneComponent<T>() where T : Component
    {
        var activeScene = SceneManager.GetActiveScene();
        foreach (var component in Resources.FindObjectsOfTypeAll<T>())
        {
            if (component != null && component.gameObject.scene.IsValid() && component.gameObject.scene == activeScene)
            {
                return component;
            }
        }

        return null;
    }
}
#endif
