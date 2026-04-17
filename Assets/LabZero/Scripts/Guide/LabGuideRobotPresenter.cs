using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabGuideRobotPresenter : MonoBehaviour
{
    private const string WarehouseSceneName = "LabWarehouse";
    private const string GuideRootName = "GuideRoot";
    private const string GuideDroneName = "LabGuideDrone";

    [SerializeField] private Transform learnerHead;
    [SerializeField] private Transform speechAnchor;
    [SerializeField] private Transform statusAnchor;
    [SerializeField] private LabGuidePromptBubbleView promptBubble;
    [SerializeField] private LabGuideStatusLineView statusLine;
    [SerializeField] private Vector3 companionOffset = new(0.45f, 0.12f, 0.70f);
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private float hoverAmplitude = 0.08f;
    [SerializeField] private float hoverFrequency = 0.65f;
    [SerializeField] private float minimumForwardRayClearance = 0.6f;

    private Vector3 _followVelocity;
    private Vector3 _hoverBasePosition;
    private Transform _focusTarget;
    private bool _orientationOnly;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureWarehouseGuide();
    }

    private void Awake()
    {
        ResolveReferences();
        _hoverBasePosition = transform.position;
    }

    private void Start()
    {
        if (promptBubble != null)
        {
            promptBubble.ShowPrompt(LabGuidePromptBubbleView.StartupGreeting, LabGuidePromptSeverity.Info);
        }

        if (statusLine != null)
        {
            statusLine.SetStatus("Obiettivo: resta nell'area sicura e ascolta la guida.");
        }
    }

    private void LateUpdate()
    {
        ResolveLearnerHead();
        if (learnerHead == null)
        {
            return;
        }

        if (!_orientationOnly)
        {
            var desiredPosition = CalculateCompanionPosition();
            _hoverBasePosition = Vector3.SmoothDamp(_hoverBasePosition, desiredPosition, ref _followVelocity, smoothTime);
            var hoverOffset = Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            transform.position = _hoverBasePosition + Vector3.up * hoverOffset;
        }

        OrientToFocus();
    }

    public void Bind(
        Transform learner,
        Transform speech,
        Transform status,
        LabGuidePromptBubbleView bubble,
        LabGuideStatusLineView line)
    {
        learnerHead = learner;
        speechAnchor = speech;
        statusAnchor = status;
        promptBubble = bubble;
        statusLine = line;
    }

    public void SetFocusTarget(Transform target, bool orientationOnly)
    {
        _focusTarget = target;
        _orientationOnly = orientationOnly;
    }

    public static GameObject EnsureWarehouseGuide()
    {
        if (SceneManager.GetActiveScene().name != WarehouseSceneName)
        {
            return null;
        }

        var existing = GameObject.Find(GuideDroneName);
        if (existing != null && existing.GetComponent<LabGuideRobotPresenter>() != null)
        {
            return existing;
        }

        var root = GameObject.Find(GuideRootName);
        if (root == null)
        {
            root = new GameObject(GuideRootName);
        }

        var learner = ResolveBestLearnerAnchor();
        var startPosition = ResolveStartPosition(learner);
        return CreateGuideDrone(root.transform, learner, startPosition);
    }

    public static GameObject CreateGuideDrone(Transform parent, Transform learner, Vector3 startPosition)
    {
        var drone = new GameObject(GuideDroneName);
        drone.transform.SetParent(parent, false);
        drone.transform.position = startPosition;
        drone.transform.rotation = Quaternion.identity;

        var presenter = drone.AddComponent<LabGuideRobotPresenter>();
        var speech = CreateAnchor(drone.transform, "SpeechAnchor", new Vector3(0.05f, 0.24f, 0.14f));
        var status = CreateAnchor(drone.transform, "StatusAnchor", new Vector3(0f, -0.18f, 0.18f));

        BuildDroneVisual(drone.transform);
        var bubble = BuildPromptBubble(speech);
        var line = BuildStatusLine(status);

        presenter.Bind(learner, speech, status, bubble, line);
        presenter._hoverBasePosition = startPosition;
        return drone;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureWarehouseGuide();
    }

    private void ResolveReferences()
    {
        ResolveLearnerHead();
        speechAnchor ??= transform.Find("SpeechAnchor");
        statusAnchor ??= transform.Find("StatusAnchor");
        promptBubble ??= GetComponentInChildren<LabGuidePromptBubbleView>(true);
        statusLine ??= GetComponentInChildren<LabGuideStatusLineView>(true);
    }

    private void ResolveLearnerHead()
    {
        if (learnerHead != null)
        {
            return;
        }

        learnerHead = ResolveBestLearnerAnchor();
    }

    private Vector3 CalculateCompanionPosition()
    {
        var desired = learnerHead.TransformPoint(companionOffset);
        var toDesired = desired - learnerHead.position;
        var rayDistance = Vector3.Dot(toDesired, learnerHead.forward);

        if (rayDistance > 0f)
        {
            var nearestOnViewRay = learnerHead.position + learnerHead.forward * rayDistance;
            var clearance = Vector3.Distance(desired, nearestOnViewRay);
            if (clearance < minimumForwardRayClearance)
            {
                desired += learnerHead.right * (minimumForwardRayClearance - clearance);
            }
        }

        return desired;
    }

    private void OrientToFocus()
    {
        var lookTarget = _focusTarget != null
            ? _focusTarget.position
            : learnerHead.position - Vector3.up * 0.35f;
        var direction = lookTarget - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction.normalized, Vector3.up),
            Time.deltaTime * 8f);
    }

    private static Transform ResolveBestLearnerAnchor()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        var previewCamera = GameObject.Find("WarehousePreviewCamera");
        if (previewCamera != null)
        {
            return previewCamera.transform;
        }

        var camera = Object.FindFirstObjectByType<Camera>();
        return camera != null ? camera.transform : null;
    }

    private static Vector3 ResolveStartPosition(Transform learner)
    {
        if (learner != null)
        {
            return learner.TransformPoint(new Vector3(0.65f, -0.05f, 0.95f));
        }

        var spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint != null)
        {
            return spawnPoint.transform.position + Vector3.up * 1.45f + spawnPoint.transform.right * 0.65f;
        }

        return new Vector3(0.65f, 1.45f, 0.95f);
    }

    private static Transform CreateAnchor(Transform parent, string name, Vector3 localPosition)
    {
        var anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = localPosition;
        anchor.transform.localRotation = Quaternion.identity;
        return anchor.transform;
    }

    private static void BuildDroneVisual(Transform parent)
    {
        var bodyMaterial = CreateGuideMaterial("Guide Drone Body", new Color(0.08f, 0.18f, 0.22f, 1f));
        var accentMaterial = CreateGuideMaterial("Guide Drone Accent", new Color(0.10f, 0.72f, 0.92f, 1f));
        var darkMaterial = CreateGuideMaterial("Guide Drone Guard", new Color(0.04f, 0.05f, 0.05f, 1f));

        var body = CreatePrimitive(parent, "Drone Body", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.32f, 0.20f, 0.26f), bodyMaterial);
        var frontLight = CreatePrimitive(parent, "Front Safety Light", PrimitiveType.Sphere, new Vector3(0f, 0.02f, 0.18f), new Vector3(0.10f, 0.06f, 0.04f), accentMaterial);
        var leftGuard = CreatePrimitive(parent, "Left Guard Arm", PrimitiveType.Cylinder, new Vector3(-0.25f, 0f, 0f), new Vector3(0.035f, 0.30f, 0.035f), darkMaterial);
        var rightGuard = CreatePrimitive(parent, "Right Guard Arm", PrimitiveType.Cylinder, new Vector3(0.25f, 0f, 0f), new Vector3(0.035f, 0.30f, 0.035f), darkMaterial);
        var topGuard = CreatePrimitive(parent, "Top Guard Arm", PrimitiveType.Cylinder, new Vector3(0f, 0.13f, 0f), new Vector3(0.03f, 0.34f, 0.03f), darkMaterial);

        leftGuard.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        rightGuard.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        topGuard.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        body.transform.SetAsFirstSibling();
        frontLight.transform.SetAsLastSibling();
    }

    private static LabGuidePromptBubbleView BuildPromptBubble(Transform parent)
    {
        var bubbleGo = new GameObject("GuidePromptBubble");
        bubbleGo.transform.SetParent(parent, false);
        bubbleGo.transform.localPosition = new Vector3(0.46f, 0.12f, 0f);
        bubbleGo.transform.localRotation = Quaternion.Euler(0f, 16f, 0f);
        bubbleGo.transform.localScale = Vector3.one;

        var panelMaterial = CreateGuideMaterial("Guide Bubble Panel", new Color(0.02f, 0.07f, 0.09f, 0.92f));
        CreatePrimitive(bubbleGo.transform, "Bubble Panel", PrimitiveType.Cube, new Vector3(0f, 0f, 0.01f), new Vector3(0.92f, 0.28f, 0.03f), panelMaterial);
        var text = CreateText(bubbleGo.transform, "Bubble Text", new Vector3(-0.42f, 0.04f, -0.015f), 0.055f, 8f);
        var view = bubbleGo.AddComponent<LabGuidePromptBubbleView>();
        view.Bind(text);
        return view;
    }

    private static LabGuideStatusLineView BuildStatusLine(Transform parent)
    {
        var statusGo = new GameObject("GuideStatusLine");
        statusGo.transform.SetParent(parent, false);
        statusGo.transform.localPosition = new Vector3(0.22f, -0.08f, 0f);
        statusGo.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);

        var text = CreateText(statusGo.transform, "Status Text", Vector3.zero, 0.04f, 6.5f);
        text.color = new Color(0.70f, 0.95f, 0.88f, 1f);
        var view = statusGo.AddComponent<LabGuideStatusLineView>();
        view.Bind(text);
        return view;
    }

    private static TextMeshPro CreateText(Transform parent, string name, Vector3 localPosition, float fontSize, float width)
    {
        var textGo = new GameObject(name);
        textGo.transform.SetParent(parent, false);
        textGo.transform.localPosition = localPosition;
        textGo.transform.localRotation = Quaternion.identity;
        textGo.transform.localScale = Vector3.one;

        var text = textGo.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Left;
        text.fontSize = fontSize;
        text.enableWordWrapping = true;
        text.rectTransform.sizeDelta = new Vector2(width, 1f);
        text.text = string.Empty;
        return text;
    }

    private static GameObject CreatePrimitive(
        Transform parent,
        string name,
        PrimitiveType primitiveType,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        var go = GameObject.CreatePrimitive(primitiveType);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        var collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        return go;
    }

    private static Material CreateGuideMaterial(string name, Color color)
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
}
