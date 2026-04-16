using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LabDebugHotkeys : MonoBehaviour
{
    [SerializeField] private LabTaskManager taskManager;

    [Header("Desktop Movement")]
    [SerializeField] private Transform rigRoot;
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float turnSpeed = 120f;

    [Header("Desktop Preview Setup")]
    [SerializeField] private bool forceDesktopPreviewInEditor = true;

    [Header("Learning Desk Runtime Build")]
    [SerializeField] private bool rebuildLearningDeskOnPlay = true;

    private bool _deskBuilt;
    private bool _previewReady;

    private static readonly Vector3 PreviewStartPosition = new(0f, 1.18f, 0.28f);
    private static readonly Vector3 PreviewLookAt = new(0f, 1.72f, 2.98f);

    private void Awake()
    {
        taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();
        BuildLearningDesk(rebuildLearningDeskOnPlay);

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

        if (taskManager == null)
        {
            return;
        }

        if (WasPressedThisFrame(keyboard.digit1Key, keyboard.numpad1Key))
        {
            taskManager.SelectTheme(LabThemeType.EnglishCommunication);
        }

        if (WasPressedThisFrame(keyboard.digit2Key, keyboard.numpad2Key))
        {
            taskManager.SelectTheme(LabThemeType.BasicMathematics);
        }

        if (WasPressedThisFrame(keyboard.digit3Key, keyboard.numpad3Key))
        {
            taskManager.SelectTheme(LabThemeType.DigitalSkills);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            taskManager.TogglePlayPause();
        }

        if (keyboard.nKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
        {
            taskManager.AdvanceLessonChunk();
        }

        if (keyboard.bKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
        {
            taskManager.RewindLessonChunk();
        }

        if (keyboard.rKey.wasPressedThisFrame || keyboard.tKey.wasPressedThisFrame)
        {
            taskManager.ClearThemeSelection();
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
        SetInactiveIfFound("XR Interaction Manager");
        HideAllLegacyCanvases();

        var desktopCamera = EnsureDesktopCamera();
        if (desktopCamera != null)
        {
            desktopCamera.transform.position = PreviewStartPosition;
            desktopCamera.transform.LookAt(PreviewLookAt);
            rigRoot = desktopCamera.transform;
        }
    }

    private void BuildLearningDesk(bool rebuildFromScratch)
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
                return;
            }
        }

        HideLegacySceneContent();

        var root = new GameObject("LearningDesk_Root").transform;
        CreateRoom(root);
        var deskTop = CreateDeskAndSeat(root);
        var screen = CreateFloatingScreen(root);
        CreateCommandPads(root, deskTop);
        AddDeskLighting(root);

        var presenter = screen.gameObject.AddComponent<LabDeskScreenPresenter>();
        presenter.Configure(taskManager);
    }

    private static Camera EnsureDesktopCamera()
    {
        var cameraGo = GameObject.Find("LearningDesk_Camera");
        if (cameraGo == null)
        {
            cameraGo = new GameObject("LearningDesk_Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            cameraGo.AddComponent<AudioListener>();
            return camera;
        }

        return cameraGo.GetComponent<Camera>();
    }

    private static void HideAllLegacyCanvases()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var canvas in canvases)
        {
            if (canvas == null)
            {
                continue;
            }

            canvas.gameObject.SetActive(false);
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

                var keep = child.name == "Lab Task Manager" || child.name == "Lab Debug Hotkeys";
                child.gameObject.SetActive(keep);
            }
        }

        SetInactiveIfFound("V8 Engine");
        SetInactiveIfFound("Sci-Fi Table");
        SetInactiveIfFound("Floor Shadow Effect");
        SetInactiveIfFound("Directional Light");
        SetInactiveIfFound("Lab Status Canvas");
        HideAllLegacyCanvases();
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
        CreateBlock("Room Floor", root, new Vector3(0f, -0.05f, 1.5f), new Vector3(7f, 0.1f, 7f), new Color(0.13f, 0.14f, 0.16f));
        CreateBlock("Room Ceiling", root, new Vector3(0f, 3.1f, 1.5f), new Vector3(7f, 0.1f, 7f), new Color(0.16f, 0.17f, 0.19f));
        CreateBlock("Room BackWall", root, new Vector3(0f, 1.5f, 4.95f), new Vector3(7f, 3f, 0.1f), new Color(0.18f, 0.19f, 0.22f));
        CreateBlock("Room FrontWall", root, new Vector3(0f, 1.5f, -1.95f), new Vector3(7f, 3f, 0.1f), new Color(0.18f, 0.19f, 0.22f));
        CreateBlock("Room LeftWall", root, new Vector3(-3.45f, 1.5f, 1.5f), new Vector3(0.1f, 3f, 7f), new Color(0.16f, 0.17f, 0.2f));
        CreateBlock("Room RightWall", root, new Vector3(3.45f, 1.5f, 1.5f), new Vector3(0.1f, 3f, 7f), new Color(0.16f, 0.17f, 0.2f));
    }

    private static Transform CreateDeskAndSeat(Transform root)
    {
        var deskTop = CreateBlock("Desk Top", root, new Vector3(0f, 0.82f, 1.4f), new Vector3(2.2f, 0.08f, 1.05f), new Color(0.28f, 0.23f, 0.19f));
        CreateBlock("Desk Leg A", root, new Vector3(-0.95f, 0.4f, 0.98f), new Vector3(0.1f, 0.8f, 0.1f), new Color(0.2f, 0.2f, 0.22f));
        CreateBlock("Desk Leg B", root, new Vector3(0.95f, 0.4f, 0.98f), new Vector3(0.1f, 0.8f, 0.1f), new Color(0.2f, 0.2f, 0.22f));
        CreateBlock("Desk Leg C", root, new Vector3(-0.95f, 0.4f, 1.82f), new Vector3(0.1f, 0.8f, 0.1f), new Color(0.2f, 0.2f, 0.22f));
        CreateBlock("Desk Leg D", root, new Vector3(0.95f, 0.4f, 1.82f), new Vector3(0.1f, 0.8f, 0.1f), new Color(0.2f, 0.2f, 0.22f));

        CreateBlock("Seat Base", root, new Vector3(0f, 0.45f, 0.35f), new Vector3(0.62f, 0.08f, 0.62f), new Color(0.24f, 0.24f, 0.27f));
        CreateBlock("Seat Back", root, new Vector3(0f, 0.78f, 0.1f), new Vector3(0.62f, 0.62f, 0.08f), new Color(0.24f, 0.24f, 0.27f));

        return deskTop;
    }

    private static Transform CreateFloatingScreen(Transform root)
    {
        var frame = CreateBlock("Screen Frame", root, new Vector3(0f, 1.75f, 3.02f), new Vector3(2.95f, 1.75f, 0.08f), new Color(0.2f, 0.22f, 0.26f));
        var panel = CreateBlock("Screen Panel", root, new Vector3(0f, 1.75f, 2.97f), new Vector3(2.78f, 1.6f, 0.02f), new Color(0.08f, 0.1f, 0.14f));

        var videoPlaceholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        videoPlaceholder.name = "Screen Video Placeholder";
        videoPlaceholder.transform.SetParent(panel);
        videoPlaceholder.transform.localPosition = new Vector3(0f, 0.04f, -0.9f);
        videoPlaceholder.transform.localRotation = Quaternion.identity;
        videoPlaceholder.transform.localScale = new Vector3(0.78f, 0.55f, 0.15f);
        videoPlaceholder.GetComponent<Renderer>().sharedMaterial = CreateMaterial("screen_video_placeholder_mat", new Color(0.11f, 0.14f, 0.2f));

        var title = CreateText("Screen Course Title", panel, new Vector3(0f, 0.58f, -2.25f), 6.6f, "Select a Course", 7.2f, 0.9f);
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.8f, 0.92f, 1f);

        var module = CreateText("Screen Module Title", panel, new Vector3(0f, -0.06f, -2.25f), 4.4f, "Module Placeholder", 7.4f, 1.2f);
        module.alignment = TextAlignmentOptions.Center;
        module.color = Color.white;

        var url = CreateText("Screen Video Url", panel, new Vector3(0f, -0.36f, -2.25f), 2.7f, "video-placeholder.local/module", 7.4f, 0.8f);
        url.alignment = TextAlignmentOptions.Center;
        url.color = new Color(0.62f, 0.95f, 0.88f);

        var hint = CreateText("Screen Controls Hint", panel, new Vector3(0f, -0.64f, -2.25f), 2.25f, "Buttons on desk or keys: 1/2/3, SPACE, N, B", 8f, 0.7f);
        hint.alignment = TextAlignmentOptions.Center;
        hint.color = new Color(0.86f, 0.87f, 0.9f);

        return panel;
    }

    private void CreateCommandPads(Transform root, Transform deskTop)
    {
        CreateCommandPad(root, deskTop, "Pad Course 1", "English", new Vector3(-0.55f, 0.87f, 1.12f), new Color(0.21f, 0.65f, 1f), LabDeskCommandType.SelectEnglish);
        CreateCommandPad(root, deskTop, "Pad Course 2", "Math", new Vector3(0f, 0.87f, 1.12f), new Color(0.32f, 0.83f, 0.45f), LabDeskCommandType.SelectMath);
        CreateCommandPad(root, deskTop, "Pad Course 3", "Digital", new Vector3(0.55f, 0.87f, 1.12f), new Color(0.96f, 0.72f, 0.2f), LabDeskCommandType.SelectDigital);

        CreateCommandPad(root, deskTop, "Pad Prev", "Prev", new Vector3(-0.34f, 0.87f, 1.55f), new Color(0.55f, 0.55f, 0.7f), LabDeskCommandType.PreviousModule);
        CreateCommandPad(root, deskTop, "Pad PlayPause", "Play/Pause", new Vector3(0f, 0.87f, 1.55f), new Color(0.82f, 0.35f, 0.92f), LabDeskCommandType.TogglePlayPause);
        CreateCommandPad(root, deskTop, "Pad Next", "Next", new Vector3(0.34f, 0.87f, 1.55f), new Color(0.96f, 0.5f, 0.5f), LabDeskCommandType.NextModule);
    }

    private void CreateCommandPad(Transform root, Transform deskTop, string name, string labelText, Vector3 position, Color color, LabDeskCommandType command)
    {
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = name;
        pad.transform.SetParent(root);
        pad.transform.position = position;
        pad.transform.localScale = new Vector3(0.28f, 0.03f, 0.2f);

        var renderer = pad.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial(name + "_mat", color);

        var label = CreateText(name + "_Label", pad.transform, new Vector3(0f, 0.04f, 0f), 2.2f, labelText);
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        var commandPad = pad.AddComponent<LabDeskCommandPad>();
        commandPad.Configure(taskManager, command, renderer, label, color, Color.white);
    }

    private static void AddDeskLighting(Transform root)
    {
        var lamp = new GameObject("Desk Light");
        lamp.transform.SetParent(root);
        lamp.transform.position = new Vector3(0f, 2.7f, 1.8f);
        var light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 9f;
        light.intensity = 2.4f;
        light.color = new Color(1f, 0.95f, 0.88f);
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

        var material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
    }

    private static TextMeshPro CreateText(
        string name,
        Transform parent,
        Vector3 localPosition,
        float fontSize,
        string text,
        float rectWidth = 6f,
        float rectHeight = 1f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.12f;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
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

        var speed = keyboard.leftShiftKey.isPressed ? moveSpeed * 2f : moveSpeed;
        var forward = Vector3.ProjectOnPlane(rigRoot.forward, Vector3.up).normalized;
        var right = Vector3.ProjectOnPlane(rigRoot.right, Vector3.up).normalized;
        var move = Vector3.zero;

        if (keyboard.wKey.isPressed) move += forward;
        if (keyboard.sKey.isPressed) move -= forward;
        if (keyboard.aKey.isPressed) move -= right;
        if (keyboard.dKey.isPressed) move += right;
        if (keyboard.iKey.isPressed || keyboard.pageUpKey.isPressed) move += Vector3.up;
        if (keyboard.kKey.isPressed || keyboard.pageDownKey.isPressed) move += Vector3.down;

        if (move.sqrMagnitude > 0.0001f)
        {
            rigRoot.position += move.normalized * speed * Time.deltaTime;
        }

        var turn = 0f;
        if (keyboard.qKey.isPressed || keyboard.leftArrowKey.isPressed) turn -= 1f;
        if (keyboard.eKey.isPressed || keyboard.rightArrowKey.isPressed) turn += 1f;
        if (Mathf.Abs(turn) > 0.001f)
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
