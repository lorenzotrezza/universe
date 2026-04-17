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
    private const float ImportedFloorY = -4.38f;
    private const float ImportedFloorMinX = -29.31f;
    private const float ImportedFloorMaxX = -13.85f;
    private const float ImportedFloorMinZ = -15.78f;
    private const float ImportedFloorMaxZ = 15.70f;

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

        var anchors = MakeGroup("WarehouseStoryAnchors", environment.transform, new Vector3(0f, ImportedFloorY, 0f));
        BuildPpeStation(anchors.transform);
        BuildWorkCell(anchors.transform);
        BuildConveyorLine(anchors.transform);
        BuildPackagingArea(anchors.transform);
        BuildBreakRoom(anchors.transform);
        BuildFinalLoadingArea(anchors.transform);
        BuildInboundPalletArea(anchors.transform);
        BuildDistractionObjects(anchors.transform);
        BuildWorkflowFloorCues(anchors.transform);

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
        DestroyIfPresent("PlayableFloor");
        DestroyIfPresent("Boundary_Left");
        DestroyIfPresent("Boundary_Right");
        DestroyIfPresent("Boundary_Back");
        DestroyIfPresent("Entrance_GuideRail");
        SetActiveIfPresent("Object_32", true);
        SetActiveIfPresent("Object_30", false);
        SetActiveIfPresent("Object_46", true);
        EditorUtility.SetDirty(warehouseBay);
    }

    private static void RepositionPlayerStart(GameObject environment)
    {
        var spawnPosition = new Vector3(-28.15f, ImportedFloorY, -12.85f);
        var spawnRotation = Quaternion.Euler(0f, 24f, 0f);

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
        var ppe = MakeGroup("PPE Station Rack", anchors, new Vector3(-26.65f, 0f, -12.45f));
        MakeCube("PPE Rack Back", ppe.transform, new Vector3(0f, 1.05f, 0.25f), new Vector3(3.2f, 2.1f, 0.12f), _darkMetal);
        MakeCube("PPE Bench", ppe.transform, new Vector3(0f, 0.32f, -0.45f), new Vector3(3.3f, 0.18f, 0.7f), _cardboard);
        MakeCube("PPE Bench Support Left", ppe.transform, new Vector3(-1.35f, 0.23f, -0.45f), new Vector3(0.14f, 0.46f, 0.14f), _darkMetal);
        MakeCube("PPE Bench Support Right", ppe.transform, new Vector3(1.35f, 0.23f, -0.45f), new Vector3(0.14f, 0.46f, 0.14f), _darkMetal);
        MakeCylinder("PPE Left Post", ppe.transform, new Vector3(-1.45f, 1.15f, -0.05f), new Vector3(0.06f, 1.1f, 0.06f), Vector3.zero, _darkMetal);
        MakeCylinder("PPE Right Post", ppe.transform, new Vector3(1.45f, 1.15f, -0.05f), new Vector3(0.06f, 1.1f, 0.06f), Vector3.zero, _darkMetal);
        MakeLabel("PPE Station Label", ppe.transform, "DPI: casco, occhiali, cuffie, guanti, gilet, scarpe", new Vector3(0f, 2.45f, -0.15f), new Vector3(65f, 0f, 0f), 0.22f, 4.5f);

        ConfigureInteractable(MakeSphere("Casco DPI", ppe.transform, new Vector3(-1.1f, 0.72f, -0.45f), new Vector3(0.45f, 0.24f, 0.36f), _yellow), LabSafetyItemType.Helmet, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Occhiali DPI", ppe.transform, new Vector3(-0.45f, 0.72f, -0.45f), new Vector3(0.55f, 0.08f, 0.12f), _glass), LabSafetyItemType.SafetyGlasses, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeSphere("Cuffie DPI", ppe.transform, new Vector3(0.25f, 0.72f, -0.45f), new Vector3(0.42f, 0.28f, 0.28f), _blue), LabSafetyItemType.HearingProtection, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeSphere("Guanti DPI", ppe.transform, new Vector3(0.9f, 0.72f, -0.45f), new Vector3(0.34f, 0.16f, 0.22f), _red), LabSafetyItemType.Gloves, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Gilet Alta Visibilita", ppe.transform, new Vector3(-0.75f, 1.45f, -0.05f), new Vector3(0.45f, 0.7f, 0.08f), _green), LabSafetyItemType.HighVisibilityVest, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
        ConfigureInteractable(MakeCube("Scarpe Antinfortunistiche", ppe.transform, new Vector3(0.75f, 1.05f, -0.05f), new Vector3(0.75f, 0.22f, 0.22f), _black), LabSafetyItemType.SafetyShoes, LabSafetyItemRole.Ppe, LabSafetyZoneType.PpeStation, null);
    }

    private static void BuildWorkCell(Transform anchors)
    {
        var work = MakeGroup("Work Cell Prep Area", anchors, new Vector3(-25.7f, 0f, -6.9f));
        MakeCube("Work Bench", work.transform, new Vector3(0f, 0.82f, 0f), new Vector3(4.1f, 0.18f, 1.15f), _cardboard);
        MakeCube("Work Bench Frame", work.transform, new Vector3(0f, 0.48f, 0f), new Vector3(4.3f, 0.18f, 1.25f), _darkMetal);
        MakeCube("Work Bench Support A", work.transform, new Vector3(-1.75f, 0.39f, -0.42f), new Vector3(0.12f, 0.78f, 0.12f), _darkMetal);
        MakeCube("Work Bench Support B", work.transform, new Vector3(1.75f, 0.39f, 0.42f), new Vector3(0.12f, 0.78f, 0.12f), _darkMetal);
        MakeCube("Work Tool Shadow Board", work.transform, new Vector3(-1.85f, 1.55f, 0.12f), new Vector3(0.14f, 1.45f, 1.55f), _darkMetal);
        MakeCube("Torque Tool", work.transform, new Vector3(-0.85f, 1.08f, -0.2f), new Vector3(0.95f, 0.08f, 0.16f), _blue);
        MakeCube("Inspection Bin", work.transform, new Vector3(1.15f, 1.05f, 0.1f), new Vector3(0.9f, 0.38f, 0.72f), _green);
        MakeCube("Parts Tote A", work.transform, new Vector3(0.1f, 1.05f, -0.2f), new Vector3(0.7f, 0.32f, 0.52f), _orange);
        MakeLabel("Work Cell Label", work.transform, "Area lavoro: prepara pezzi e utensili prima del nastro", new Vector3(0f, 2.35f, 0.25f), new Vector3(65f, 0f, 0f), 0.2f, 4.4f);
    }

    private static void BuildConveyorLine(Transform anchors)
    {
        var conveyor = MakeGroup("Conveyor Sorting Line", anchors, new Vector3(-21.65f, 0f, -1.5f));
        MakeCube("Conveyor Base", conveyor.transform, new Vector3(0f, 0.55f, 0f), new Vector3(2.15f, 0.45f, 9.4f), _darkMetal);
        MakeCube("Conveyor Belt", conveyor.transform, new Vector3(0f, 0.83f, 0f), new Vector3(1.65f, 0.12f, 9.1f), _belt);
        MakeCube("Conveyor Support A", conveyor.transform, new Vector3(0f, 0.28f, -3.25f), new Vector3(1.7f, 0.56f, 0.14f), _darkMetal);
        MakeCube("Conveyor Support B", conveyor.transform, new Vector3(0f, 0.28f, 3.25f), new Vector3(1.7f, 0.56f, 0.14f), _darkMetal);

        for (var i = 0; i < 8; i++)
        {
            MakeCylinder("Conveyor Roller " + i, conveyor.transform, new Vector3(0f, 1.02f, -3.9f + i * 1.1f), new Vector3(0.07f, 0.75f, 0.07f), new Vector3(0f, 0f, 90f), _darkMetal);
        }

        MakeCube("Sorting Sensor Gate", conveyor.transform, new Vector3(0f, 1.7f, 2.25f), new Vector3(2.35f, 1.45f, 0.16f), _blue);
        MakeCube("Sorting Box A", conveyor.transform, new Vector3(-0.22f, 1.25f, -2.2f), new Vector3(0.85f, 0.55f, 0.68f), _cardboard);
        MakeCube("Sorting Box B", conveyor.transform, new Vector3(0.18f, 1.25f, 1.1f), new Vector3(0.72f, 0.5f, 0.62f), _cardboard);
        MakeLabel("Conveyor Label", conveyor.transform, "Catena e nastro: resta concentrato sul flusso", new Vector3(0f, 2.35f, -3.6f), new Vector3(65f, 0f, 0f), 0.2f, 4.5f);

        for (var i = 0; i < 6; i++)
        {
            MakeCube("Sorting Floor Stripe " + i, conveyor.transform, new Vector3(-1.25f, 0.025f, -4f + i * 1.55f), new Vector3(0.14f, 0.035f, 0.95f), _yellow);
        }
    }

    private static void BuildPackagingArea(Transform anchors)
    {
        var packaging = MakeGroup("Packaging And Quality Check", anchors, new Vector3(-17.7f, 0f, 5.55f));
        MakeCube("Quality Check Table", packaging.transform, new Vector3(0f, 0.78f, 0f), new Vector3(4.3f, 0.18f, 1.4f), _cardboard);
        MakeCube("Quality Table Support A", packaging.transform, new Vector3(-1.65f, 0.39f, -0.48f), new Vector3(0.12f, 0.78f, 0.12f), _darkMetal);
        MakeCube("Quality Table Support B", packaging.transform, new Vector3(1.65f, 0.39f, 0.48f), new Vector3(0.12f, 0.78f, 0.12f), _darkMetal);
        MakeCube("Packaging Tape Roll", packaging.transform, new Vector3(-1.25f, 1.02f, 0.15f), new Vector3(0.42f, 0.12f, 0.42f), _yellow);
        MakeCube("Barcode Scanner", packaging.transform, new Vector3(1.25f, 1.05f, -0.1f), new Vector3(0.42f, 0.22f, 0.32f), _blue);
        MakeCube("Checklist Tablet Mount", packaging.transform, new Vector3(0.15f, 1.12f, -0.45f), new Vector3(0.62f, 0.08f, 0.42f), _darkMetal);
        MakePallet(packaging.transform, new Vector3(-0.35f, 0f, 2.25f), _cardboard);
        MakeCube("Packed Box Stack 1", packaging.transform, new Vector3(-0.35f, 0.82f, 2.25f), new Vector3(0.9f, 0.7f, 0.8f), _cardboard);
        MakeCube("Packed Box Stack 2", packaging.transform, new Vector3(0.65f, 0.62f, 2.22f), new Vector3(0.72f, 0.48f, 0.7f), _cardboard);
        MakeLabel("Packaging Label", packaging.transform, "Controllo e imballaggio: mani libere, niente distrazioni", new Vector3(0f, 2.15f, 0f), new Vector3(65f, 0f, 0f), 0.2f, 4.4f);
    }

    private static void BuildBreakRoom(Transform anchors)
    {
        var breakRoom = MakeGroup("Break Room Enclosed", anchors, new Vector3(-26.55f, 0f, 11.9f));
        MakeCube("Break Room Back Wall", breakRoom.transform, new Vector3(0f, 1.25f, 2.65f), new Vector3(4.7f, 2.5f, 0.14f), _darkMetal);
        MakeCube("Break Room Left Wall", breakRoom.transform, new Vector3(-2.35f, 1.25f, 0f), new Vector3(0.14f, 2.5f, 5.3f), _darkMetal);
        MakeCube("Break Room Right Wall", breakRoom.transform, new Vector3(2.35f, 1.25f, 0f), new Vector3(0.14f, 2.5f, 5.3f), _darkMetal);
        MakeCube("Break Room Front Wall Left", breakRoom.transform, new Vector3(-1.55f, 1.25f, -2.65f), new Vector3(1.6f, 2.5f, 0.14f), _darkMetal);
        MakeCube("Break Room Front Wall Right", breakRoom.transform, new Vector3(1.55f, 1.25f, -2.65f), new Vector3(1.6f, 2.5f, 0.14f), _darkMetal);
        MakeCube("Break Room Door Header", breakRoom.transform, new Vector3(0f, 2.35f, -2.65f), new Vector3(1.25f, 0.3f, 0.16f), _darkMetal);
        MakeCube("Break Room Door Opening", breakRoom.transform, new Vector3(0f, 1.05f, -2.72f), new Vector3(1.05f, 2.1f, 0.04f), _black);
        MakeCube("Break Counter Long", breakRoom.transform, new Vector3(-0.45f, 0.48f, 1.75f), new Vector3(3.5f, 0.18f, 0.62f), _cardboard);
        MakeCube("Coffee Machine Placeholder", breakRoom.transform, new Vector3(-1.55f, 0.92f, 1.75f), new Vector3(0.45f, 0.65f, 0.42f), _darkMetal);
        MakeCube("Snack Vending Machine", breakRoom.transform, new Vector3(1.58f, 1.0f, 1.55f), new Vector3(0.75f, 1.8f, 0.42f), _glass);
        MakeCylinder("Coffee Cup", breakRoom.transform, new Vector3(-0.95f, 0.82f, 1.65f), new Vector3(0.11f, 0.12f, 0.11f), Vector3.zero, _white);
        MakeCylinder("Water Bottle", breakRoom.transform, new Vector3(-0.45f, 0.92f, 1.65f), new Vector3(0.09f, 0.28f, 0.09f), Vector3.zero, _glass);
        ConfigureInteractable(MakeCube("Cibo Zona Ristoro", breakRoom.transform, new Vector3(0.1f, 0.82f, 1.75f), new Vector3(0.52f, 0.14f, 0.36f), _orange), LabSafetyItemType.Food, LabSafetyItemRole.BreakAreaOnly, LabSafetyZoneType.BreakCorner, null);
        MakeCylinder("Break Stool A", breakRoom.transform, new Vector3(0.9f, 0.32f, -0.95f), new Vector3(0.35f, 0.18f, 0.35f), Vector3.zero, _darkMetal);
        MakeCylinder("Break Stool B", breakRoom.transform, new Vector3(-0.2f, 0.32f, -1.1f), new Vector3(0.35f, 0.18f, 0.35f), Vector3.zero, _darkMetal);
        MakeLabel("Break Room Label", breakRoom.transform, "Zona ristoro: cibo e bevande solo qui", new Vector3(0f, 2.85f, 1.5f), new Vector3(65f, 180f, 0f), 0.2f, 4.1f);
    }

    private static void BuildFinalLoadingArea(Transform anchors)
    {
        var loading = MakeGroup("Final Loading Exit", anchors, new Vector3(-21.55f, 0f, 0f));
        MakeCube("Simulated Open Shutter Void", loading.transform, new Vector3(0f, 1.55f, ImportedFloorMaxZ + 0.02f), new Vector3(6.2f, 3.1f, 0.12f), _black);
        MakeCube("Raised Shutter Door Panel", loading.transform, new Vector3(0f, 3.35f, ImportedFloorMaxZ - 0.12f), new Vector3(6.4f, 0.8f, 0.18f), _darkMetal);
        MakeCube("Loading Door Frame Left", loading.transform, new Vector3(-3.25f, 1.6f, ImportedFloorMaxZ - 0.08f), new Vector3(0.18f, 3.2f, 0.22f), _yellow);
        MakeCube("Loading Door Frame Right", loading.transform, new Vector3(3.25f, 1.6f, ImportedFloorMaxZ - 0.08f), new Vector3(0.18f, 3.2f, 0.22f), _yellow);

        var finalConveyor = MakeGroup("Final Conveyor To Loading Door", loading.transform, Vector3.zero);
        MakeCube("Final Conveyor Base", finalConveyor.transform, new Vector3(0f, 0.55f, 12.9f), new Vector3(2.0f, 0.45f, 7.8f), _darkMetal);
        MakeCube("Final Conveyor Belt", finalConveyor.transform, new Vector3(0f, 0.83f, 12.9f), new Vector3(1.55f, 0.12f, 7.55f), _belt);
        MakeCube("Final Conveyor Support A", finalConveyor.transform, new Vector3(0f, 0.28f, 10.1f), new Vector3(1.6f, 0.56f, 0.14f), _darkMetal);
        MakeCube("Final Conveyor Support B", finalConveyor.transform, new Vector3(0f, 0.28f, 15.3f), new Vector3(1.6f, 0.56f, 0.14f), _darkMetal);
        MakeCube("Ready Package A", finalConveyor.transform, new Vector3(-0.3f, 1.18f, 11.1f), new Vector3(0.75f, 0.55f, 0.65f), _cardboard);
        MakeCube("Ready Package B", finalConveyor.transform, new Vector3(0.22f, 1.18f, 13.2f), new Vector3(0.72f, 0.52f, 0.62f), _cardboard);
        MakeCube("Ready Package C", finalConveyor.transform, new Vector3(-0.15f, 1.18f, 15.35f), new Vector3(0.72f, 0.52f, 0.62f), _cardboard);

        var truck = MakeGroup("Loading Truck Placeholder", loading.transform, new Vector3(0f, 0f, ImportedFloorMaxZ + 6.4f));
        MakeCube("Truck Cargo Box", truck.transform, new Vector3(0f, 1.55f, 0f), new Vector3(5.7f, 3.1f, 3.5f), _white);
        MakeCube("Truck Cab", truck.transform, new Vector3(0f, 1.15f, 2.65f), new Vector3(3.2f, 2.3f, 1.8f), _blue);
        MakeCylinder("Truck Wheel FL", truck.transform, new Vector3(-2.05f, 0.45f, 2.95f), new Vector3(0.42f, 0.18f, 0.42f), new Vector3(0f, 0f, 90f), _black);
        MakeCylinder("Truck Wheel FR", truck.transform, new Vector3(2.05f, 0.45f, 2.95f), new Vector3(0.42f, 0.18f, 0.42f), new Vector3(0f, 0f, 90f), _black);
        MakeCylinder("Truck Wheel RL", truck.transform, new Vector3(-2.05f, 0.45f, -1.45f), new Vector3(0.42f, 0.18f, 0.42f), new Vector3(0f, 0f, 90f), _black);
        MakeCylinder("Truck Wheel RR", truck.transform, new Vector3(2.05f, 0.45f, -1.45f), new Vector3(0.42f, 0.18f, 0.42f), new Vector3(0f, 0f, 90f), _black);
        MakeLabel("Loading Exit Label", loading.transform, "Uscita merci: nastro finale verso camion", new Vector3(0f, 3.05f, 13.9f), new Vector3(65f, 0f, 0f), 0.2f, 4.1f);
    }

    private static void BuildInboundPalletArea(Transform anchors)
    {
        var inbound = MakeGroup("Inbound Pallet Staging", anchors, new Vector3(-17.1f, 0f, -11.6f));
        MakePallet(inbound.transform, new Vector3(-0.9f, 0f, 0f), _cardboard);
        MakePallet(inbound.transform, new Vector3(1.1f, 0f, 0.1f), _cardboard);
        MakeCube("Inbound Crate A", inbound.transform, new Vector3(-0.9f, 0.72f, 0f), new Vector3(1.0f, 0.8f, 0.9f), _cardboard);
        MakeCube("Inbound Crate B", inbound.transform, new Vector3(1.1f, 0.62f, 0.1f), new Vector3(0.9f, 0.6f, 0.82f), _cardboard);
        MakeCube("Small Forklift Placeholder", inbound.transform, new Vector3(0.1f, 0.65f, 2.15f), new Vector3(1.4f, 1.3f, 1.7f), _orange);
        MakeCube("Forklift Mast", inbound.transform, new Vector3(0.1f, 1.55f, 1.15f), new Vector3(1.3f, 1.75f, 0.12f), _darkMetal);
        MakeLabel("Inbound Label", inbound.transform, "Area lavoro: merci in ingresso e preparazione", new Vector3(0f, 2.45f, 0.9f), new Vector3(65f, 0f, 0f), 0.2f, 4.2f);
    }

    private static void BuildDistractionObjects(Transform anchors)
    {
        var distractions = MakeGroup("Distraction Objects", anchors, Vector3.zero);
        ConfigureInteractable(MakeCube("Telefono con Notifica", distractions.transform, new Vector3(-21.95f, 1.25f, -3.7f), new Vector3(0.18f, 0.035f, 0.36f), _black), LabSafetyItemType.Phone, LabSafetyItemRole.Distractor, LabSafetyZoneType.Sorting, _notificationClip);
        ConfigureInteractable(MakeCube("Tablet Distrattore", distractions.transform, new Vector3(-18.55f, 1.04f, 5.15f), new Vector3(0.55f, 0.04f, 0.38f), _glass), LabSafetyItemType.Tablet, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeSphere("Pallina Distrattore", distractions.transform, new Vector3(-18.9f, 0.24f, 3.85f), new Vector3(0.34f, 0.34f, 0.34f), _red), LabSafetyItemType.Ball, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeCube("Gameboy Distrattore", distractions.transform, new Vector3(-16.55f, 1.04f, 5.2f), new Vector3(0.42f, 0.08f, 0.55f), _green), LabSafetyItemType.HandheldGame, LabSafetyItemRole.Distractor, LabSafetyZoneType.Packaging, null);
        ConfigureInteractable(MakeCube("Cibo Fuori Area", distractions.transform, new Vector3(-24.7f, 1.04f, -6.75f), new Vector3(0.42f, 0.12f, 0.3f), _orange), LabSafetyItemType.Food, LabSafetyItemRole.BreakAreaOnly, LabSafetyZoneType.Operational, null);
        ConfigureInteractable(MakeCylinder("Birra Distrattore", distractions.transform, new Vector3(-25.2f, 1.08f, -6.7f), new Vector3(0.1f, 0.28f, 0.1f), Vector3.zero, _red), LabSafetyItemType.Beer, LabSafetyItemRole.Prohibited, LabSafetyZoneType.Operational, null);
    }

    private static void BuildWorkflowFloorCues(Transform anchors)
    {
        var route = MakeGroup("Workflow Floor Cues", anchors, Vector3.zero);
        MakeCube("Cue PPE To Work", route.transform, new Vector3(-26.0f, 0.025f, -9.65f), new Vector3(0.65f, 0.035f, 1.25f), _green);
        MakeCube("Cue Work To Conveyor", route.transform, new Vector3(-23.7f, 0.025f, -5.3f), new Vector3(0.65f, 0.035f, 1.25f), _green);
        MakeCube("Cue Conveyor To Packaging", route.transform, new Vector3(-20.2f, 0.025f, 2.9f), new Vector3(0.65f, 0.035f, 1.25f), _green);
        MakeCube("Cue Packaging To Exit", route.transform, new Vector3(-19.0f, 0.025f, 9.2f), new Vector3(0.65f, 0.035f, 1.25f), _green);
        MakeLabel("Workflow Cue Label", route.transform, "Flusso: DPI -> lavoro -> nastro -> controllo -> uscita merci", new Vector3(-21.6f, 2.05f, -8.9f), new Vector3(65f, 0f, 0f), 0.18f, 6.0f);
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

    private static void SetActiveIfPresent(string name, bool active)
    {
        var go = FindSceneObject(name);
        if (go != null)
        {
            go.SetActive(active);
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
