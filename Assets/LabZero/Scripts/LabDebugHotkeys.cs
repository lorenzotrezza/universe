using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LabDebugHotkeys : MonoBehaviour
{
    // Material-inspired palette: #101820, #1F2A36, #F2A900.
    private static readonly Color32 ColorSurfaceDark = new(0x10, 0x18, 0x20, 0xFF);
    private static readonly Color32 ColorSurfaceRaised = new(0x1F, 0x2A, 0x36, 0xFF);
    private static readonly Color32 ColorAccentAmber = new(0xF2, 0xA9, 0x00, 0xFF);
    private static readonly Color32 ColorActionBlue = new(0x29, 0x6F, 0xC2, 0xFF);
    private static readonly Color32 ColorActionTeal = new(0x00, 0x8E, 0x89, 0xFF);
    private static readonly Color32 ColorActionGreen = new(0x2E, 0x7D, 0x32, 0xFF);
    private static readonly Color32 ColorActionOrange = new(0xEF, 0x6C, 0x00, 0xFF);

    private static readonly Vector3 PreviewStartPosition = new(0f, 1.24f, 0.20f);
    private static readonly Vector3 PreviewLookAt = new(0f, 1.45f, 2.55f);

    // UI-SPEC asks for 20-24mm rounded treatment; we approximate with layered plates.
    private const float RoundedPlateRadiusMetersMin = 0.02f;
    private const float RoundedPlateRadiusMetersMax = 0.024f;
    private const float ScreenWidthMeters = 2.8f;
    private const float ScreenHeightMeters = 1.6f;
    private const float ScreenTextScale = 0.082f;
    private const float PadGapHorizontal = 0.08f;
    private const float PadGapVertical = 0.08f;

    [SerializeField] private LabTaskManager taskManager;

    [Header("Desktop Movement")]
    [SerializeField] private Transform rigRoot;
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private bool allowDesktopTranslationInLobby = false;

    [Header("Desktop Preview Setup")]
    [SerializeField] private bool forceDesktopPreviewInEditor = true;

    [Header("Lobby Runtime Build")]
    [SerializeField] private bool rebuildLearningDeskOnPlay = false;

    private bool _deskBuilt;
    private bool _previewReady;

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        BuildLobbyDesk(rebuildLearningDeskOnPlay);

#if UNITY_EDITOR
        if (forceDesktopPreviewInEditor)
        {
            SetupDesktopPreview();
        }
#endif

        rigRoot ??= FindDesktopRigRoot();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.fKey.wasPressedThisFrame)
        {
            RecenterDesktopCamera();
        }

        HandleDesktopMovement(keyboard);
        HandleLobbyKeys(keyboard);
    }

    private void HandleLobbyKeys(Keyboard keyboard)
    {
        if (taskManager == null)
        {
            return;
        }

        if (WasPressedThisFrame(keyboard.minusKey, keyboard.numpadMinusKey))
        {
            taskManager.AdjustTimer(-1);
        }

        if (WasPressedThisFrame(keyboard.equalsKey, keyboard.numpadPlusKey))
        {
            taskManager.AdjustTimer(1);
        }

        if (keyboard.oKey.wasPressedThisFrame)
        {
            taskManager.ToggleErrorOverlay();
        }

        if (keyboard.hKey.wasPressedThisFrame)
        {
            taskManager.ToggleHelpers();
        }

        if (keyboard.mKey.wasPressedThisFrame)
        {
            taskManager.ToggleRunMode();
        }

        if (keyboard.enterKey.wasPressedThisFrame)
        {
            taskManager.StartConfiguredRun();
            LabSceneTransition.LoadWarehouse();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            taskManager.ResetLobbyConfiguration();
        }
    }

    private void SetupDesktopPreview()
    {
        if (_previewReady)
        {
            return;
        }

        _previewReady = true;

        SetInactiveIfFound("XRRig (Controllers + Hands)");
        SetInactiveIfFound("App Manager");
        HideLegacyCanvasesExceptLobby();

        var desktopCamera = EnsureDesktopCamera();
        if (desktopCamera != null)
        {
            desktopCamera.transform.position = PreviewStartPosition;
            desktopCamera.transform.LookAt(PreviewLookAt);
            rigRoot = desktopCamera.transform;
        }
    }

    private void BuildLobbyDesk(bool rebuildFromScratch)
    {
        if (_deskBuilt)
        {
            return;
        }

        _deskBuilt = true;
        var existing = GameObject.Find("LearningDesk_Root");
        if (existing != null)
        {
            if (rebuildFromScratch)
            {
                Destroy(existing);
            }
            else
            {
                ConfigureExistingDesk(existing);
                return;
            }
        }

        HideLegacySceneContent();

        var root = new GameObject("LearningDesk_Root").transform;
        CreateRoom(root);
        var deskTop = CreateDeskAndSeat(root);
        var briefingScreen = CreateBriefingScreen(root);
        CreateLobbyPads(root, deskTop);
        AddDeskLighting(root);

        var presenter = briefingScreen.gameObject.AddComponent<LabDeskScreenPresenter>();
        presenter.Configure(taskManager);
    }

    private void ConfigureExistingDesk(GameObject existing)
    {
        HideLegacySceneContent();

        var presenter = existing.GetComponentInChildren<LabDeskScreenPresenter>(true);
        if (presenter != null)
        {
            presenter.Configure(taskManager);
        }
    }

    private static Camera EnsureDesktopCamera()
    {
        var cameraGo = GameObject.Find("LearningDesk_Camera");
        if (cameraGo == null)
        {
            cameraGo = new GameObject("LearningDesk_Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 64f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            cameraGo.AddComponent<AudioListener>();
            return camera;
        }

        var existingCamera = cameraGo.GetComponent<Camera>();
        if (existingCamera != null)
        {
            existingCamera.fieldOfView = 64f;
            existingCamera.nearClipPlane = 0.01f;
            existingCamera.farClipPlane = 100f;
        }

        return existingCamera;
    }

    private static bool IsUnderLearningDeskRoot(Transform current)
    {
        while (current != null)
        {
            if (current.name == "LearningDesk_Root")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void HideLegacyCanvasesExceptLobby()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas == null)
            {
                continue;
            }

            var keep = canvas.name == "Lab Status Canvas" || IsUnderLearningDeskRoot(canvas.transform);
            if (!keep)
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    private void HideLegacySceneContent()
    {
        var labRoot = GameObject.Find("LabZero");
        if (labRoot != null)
        {
            foreach (Transform child in labRoot.transform)
            {
                if (child == null)
                {
                    continue;
                }

                var keep = child.name == "Lab Task Manager" || child.name == "Lab Debug Hotkeys" || child.name == "Lab Status Canvas";
                child.gameObject.SetActive(keep);
            }
        }

        SetInactiveIfFound("V8 Engine");
        SetInactiveIfFound("Sci-Fi Table");
        SetInactiveIfFound("Floor Shadow Effect");
        SetInactiveIfFound("Directional Light");
        HideLegacyCanvasesExceptLobby();
    }

    private static void SetInactiveIfFound(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
        {
            go.SetActive(false);
        }
    }

    private static void CreateRoom(Transform root)
    {
        CreateBlock("Room Floor", root, new Vector3(0f, -0.05f, 1.5f), new Vector3(7f, 0.1f, 7f), ColorSurfaceDark);
        CreateBlock("Room Ceiling", root, new Vector3(0f, 3.1f, 1.5f), new Vector3(7f, 0.1f, 7f), ColorSurfaceRaised);
        CreateBlock("Room BackWall", root, new Vector3(0f, 1.5f, 4.95f), new Vector3(7f, 3f, 0.1f), ColorSurfaceRaised);
        CreateBlock("Room FrontWall", root, new Vector3(0f, 1.5f, -1.95f), new Vector3(7f, 3f, 0.1f), ColorSurfaceRaised);
        CreateBlock("Room LeftWall", root, new Vector3(-3.45f, 1.5f, 1.5f), new Vector3(0.1f, 3f, 7f), ColorSurfaceRaised);
        CreateBlock("Room RightWall", root, new Vector3(3.45f, 1.5f, 1.5f), new Vector3(0.1f, 3f, 7f), ColorSurfaceRaised);
    }

    private static Transform CreateDeskAndSeat(Transform root)
    {
        var deskTop = CreateBlock("Desk Top", root, new Vector3(0f, 0.82f, 1.4f), new Vector3(2.4f, 0.08f, 1.2f), ColorSurfaceRaised);
        CreateBlock("Desk Shadow", root, new Vector3(0f, 0.78f, 1.4f), new Vector3(2.46f, 0.02f, 1.24f), ColorSurfaceDark);
        CreateBlock("Seat Base", root, new Vector3(0f, 0.45f, 0.35f), new Vector3(0.62f, 0.08f, 0.62f), ColorSurfaceRaised);
        CreateBlock("Seat Back", root, new Vector3(0f, 0.78f, 0.1f), new Vector3(0.62f, 0.62f, 0.08f), ColorSurfaceRaised);

        return deskTop;
    }

    private Transform CreateBriefingScreen(Transform root)
    {
        var frame = CreateBlock(
            "Screen Frame",
            root,
            new Vector3(0f, 1.75f, 3.02f),
            new Vector3(ScreenWidthMeters + 0.16f, ScreenHeightMeters + 0.14f, 0.08f),
            ColorSurfaceRaised);

        var panel = CreateBlock(
            "Screen Panel",
            root,
            new Vector3(0f, 1.75f, 2.97f),
            new Vector3(ScreenWidthMeters, ScreenHeightMeters, 0.02f),
            ColorSurfaceDark);

        CreateText("Screen Scenario Title", panel, new Vector3(0f, 0.42f, -2.0f), 4.8f, "Briefing Sicurezza Magazzino", 11f, 0.75f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Objective", panel, new Vector3(0f, 0.27f, -2.0f), 2.8f, "Configura la sessione prima di entrare nell'area operativa.", 10f, 0.65f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Settings Heading", panel, new Vector3(0f, 0.11f, -2.0f), 2.8f, "Impostazioni sessione", 8f, 0.6f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Timer Row", panel, new Vector3(0f, -0.03f, -2.0f), 2.4f, "Timer: 7 min", 8f, 0.5f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Overlay Row", panel, new Vector3(0f, -0.14f, -2.0f), 2.4f, "Overlay errori: Nascosta", 8f, 0.5f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Helpers Row", panel, new Vector3(0f, -0.25f, -2.0f), 2.4f, "Aiuti: Attivi", 8f, 0.5f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Mode Row", panel, new Vector3(0f, -0.36f, -2.0f), 2.4f, "Modalita: Simulazione", 8f, 0.5f, TextAlignmentOptions.Center, null, ScreenTextScale);
        CreateText("Screen Cta Line", panel, new Vector3(0f, -0.43f, -2.0f), 2.2f, "Quando sei pronto, premi Avvia Addestramento.", 10f, 0.55f, TextAlignmentOptions.Center, null, ScreenTextScale);

        var timerChip = CreateBlock("Timer Value Chip", panel, new Vector3(0.31f, -0.03f, -1.95f), new Vector3(0.20f, 0.075f, 0.08f), ColorSurfaceRaised);
        CreateText("Timer Value Chip Label", timerChip, Vector3.zero, 2.4f, "7 min", 2f, 0.6f);

        var errorOverlayPanel = CreateBlock("Error Overlay Panel", panel, new Vector3(0.30f, -0.27f, -1.9f), new Vector3(0.32f, 0.24f, 0.1f), ColorSurfaceRaised);
        CreateText("Error Overlay Title", errorOverlayPanel, new Vector3(0f, 0.16f, -0.7f), 2.3f, "Errori", 2.4f, 0.6f, TextAlignmentOptions.Center, null, 0.11f);
        CreateText("Error Overlay State", errorOverlayPanel, new Vector3(0f, -0.08f, -0.7f), 1.9f, "Visibilita overlay: Nascosta", 3.2f, 0.6f, TextAlignmentOptions.Center, null, 0.11f);
        errorOverlayPanel.gameObject.SetActive(taskManager != null && taskManager.ShowErrorOverlay);

        frame.GetComponent<Renderer>().sharedMaterial = CreateMaterial("screen_frame_mat", ColorSurfaceRaised);
        panel.GetComponent<Renderer>().sharedMaterial = CreateMaterial("screen_panel_mat", ColorSurfaceDark);

        return panel;
    }

    private void CreateLobbyPads(Transform root, Transform deskTop)
    {
        var standardScale = new Vector3(0.36f, 0.045f, 0.22f);
        var ctaScale = new Vector3(0.72f, 0.05f, 0.24f);
        var deskBounds = deskTop.GetComponent<Renderer>().bounds;
        var surfaceY = deskBounds.max.y + (standardScale.y * 0.5f) + 0.012f;
        var firstRowZ = deskBounds.min.z + 0.26f;
        var secondRowZ = firstRowZ + standardScale.z + PadGapVertical;
        var thirdRowZ = secondRowZ + standardScale.z + PadGapVertical;

        var left = -standardScale.x - (PadGapHorizontal * 0.5f);
        var center = 0f;
        var right = standardScale.x + (PadGapHorizontal * 0.5f);

        CreateLobbyPad(root, "Pad Timer Minus", "Timer -", new Vector3(left, surfaceY, firstRowZ), standardScale, ColorActionBlue, LabDeskCommandType.TimerDown);
        CreateLobbyPad(root, "Pad Timer Plus", "Timer +", new Vector3(center, surfaceY, firstRowZ), standardScale, ColorActionBlue, LabDeskCommandType.TimerUp);
        CreateLobbyPad(root, "Pad Overlay Errori", "Errori", new Vector3(right, surfaceY, firstRowZ), standardScale, ColorActionOrange, LabDeskCommandType.ToggleErrorOverlay);

        CreateLobbyPad(root, "Pad Aiuti", "Aiuti", new Vector3(left, surfaceY, secondRowZ), standardScale, ColorActionTeal, LabDeskCommandType.ToggleHelpers);
        CreateLobbyPad(root, "Pad Modalita", "Modo", new Vector3(center, surfaceY, secondRowZ), standardScale, ColorActionGreen, LabDeskCommandType.ToggleMode);
        CreateLobbyPad(root, "Pad Reset Lobby", "Reset", new Vector3(right, surfaceY, secondRowZ), standardScale, ColorSurfaceRaised, LabDeskCommandType.ResetLobby);

        var resetPadPosition = new Vector3(0f, surfaceY, thirdRowZ);
        CreateLobbyPad(root, "Pad Start Training", "Avvia", resetPadPosition, ctaScale, ColorAccentAmber, LabDeskCommandType.StartTraining);
    }

    private void CreateLobbyPad(
        Transform root,
        string name,
        string labelText,
        Vector3 position,
        Vector3 scale,
        Color color,
        LabDeskCommandType command)
    {
        var shadowScale = new Vector3(scale.x + RoundedPlateRadiusMetersMax, 0.018f, scale.z + RoundedPlateRadiusMetersMin);
        CreateRoundedPadTop(
            name + " Shadow",
            root,
            position + new Vector3(0f, -0.018f, 0.012f),
            shadowScale,
            ColorSurfaceDark);

        var topPlate = CreateRoundedPadTop(name + " Top", root, position, scale, color);

        var renderer = topPlate.GetComponent<Renderer>();
        var label = CreateText(
            name + "_Label",
            root,
            position + new Vector3(0f, (scale.y * 0.5f) + 0.070f, -0.012f),
            2.9f,
            labelText,
            4.4f,
            0.8f,
            TextAlignmentOptions.Center,
            Quaternion.identity,
            0.080f);
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.transform.SetParent(topPlate, true);

        var commandPad = topPlate.gameObject.AddComponent<LabDeskCommandPad>();
        commandPad.Configure(taskManager, command, renderer, label, color, ColorAccentAmber);
    }

    private static Transform CreateRoundedPadTop(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        go.transform.localScale = new Vector3(scale.y, scale.x * 0.5f, scale.z);
        go.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_mat", color);
        return go.transform;
    }

    private static void AddDeskLighting(Transform root)
    {
        var lamp = new GameObject("Desk Light");
        lamp.transform.SetParent(root);
        lamp.transform.position = new Vector3(0f, 2.55f, 0.85f);
        var light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 6f;
        light.intensity = 1.65f;
        light.color = ColorAccentAmber;
    }

    private static Transform CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_mat", color);
        return go.transform;
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
        material.SetFloat("_Smoothness", 0.74f);
        return material;
    }

    private static TextMeshPro CreateText(
        string name,
        Transform parent,
        Vector3 localPosition,
        float fontSize,
        string text,
        float rectWidth = 6f,
        float rectHeight = 1f,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        Quaternion? localRotation = null,
        float localScale = 0.12f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation ?? Quaternion.identity;
        go.transform.localScale = Vector3.one * localScale;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Truncate;
        tmp.rectTransform.sizeDelta = new Vector2(rectWidth, rectHeight);
        return tmp;
    }

    private void HandleDesktopMovement(Keyboard keyboard)
    {
        if (rigRoot == null)
        {
            return;
        }

        if (allowDesktopTranslationInLobby)
        {
            var speed = keyboard.leftShiftKey.isPressed ? moveSpeed * 2f : moveSpeed;
            var forward = Vector3.ProjectOnPlane(rigRoot.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(rigRoot.right, Vector3.up).normalized;
            var move = Vector3.zero;

            if (keyboard.wKey.isPressed) move += forward;
            if (keyboard.sKey.isPressed) move -= forward;
            if (keyboard.aKey.isPressed) move -= right;
            if (keyboard.dKey.isPressed) move += right;
            if (keyboard.iKey.isPressed) move += Vector3.up;
            if (keyboard.kKey.isPressed) move += Vector3.down;

            if (move.sqrMagnitude > 0.0001f)
            {
                rigRoot.position += move.normalized * speed * Time.deltaTime;
            }
        }

        var turn = 0f;
        if (keyboard.qKey.isPressed || keyboard.leftArrowKey.isPressed) turn -= 1f;
        if (keyboard.eKey.isPressed || keyboard.rightArrowKey.isPressed) turn += 1f;
        if (turn > 0.001f || turn < -0.001f)
        {
            rigRoot.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);
        }
    }

    private static bool WasPressedThisFrame(params KeyControl[] keys)
    {
        foreach (var key in keys)
        {
            if (key != null && key.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void RecenterDesktopCamera()
    {
        if (rigRoot == null)
        {
            rigRoot = FindDesktopRigRoot();
        }

        if (rigRoot == null)
        {
            return;
        }

        rigRoot.position = PreviewStartPosition;
        rigRoot.LookAt(PreviewLookAt);
    }

    private static Transform FindDesktopRigRoot()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        var cam = GameObject.Find("LearningDesk_Camera");
        return cam != null ? cam.transform : null;
    }
}
