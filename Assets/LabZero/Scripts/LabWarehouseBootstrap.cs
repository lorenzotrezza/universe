using UnityEngine;

public class LabWarehouseBootstrap : MonoBehaviour
{
    [SerializeField] private LabSessionSettings settings;
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private GameObject warehouseEnvironmentRoot;

    private void Start()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();

        if (settings == null)
        {
            return;
        }

        ApplyPresentationMode();

        if (sessionManager != null)
        {
            sessionManager.Initialize(settings);
            sessionManager.StartRun();
        }
    }

    private void ApplyPresentationMode()
    {
        var isLive = settings.PresentationMode == LabPresentationMode.Live;

        if (warehouseEnvironmentRoot != null)
        {
            warehouseEnvironmentRoot.SetActive(!isLive);
        }

        if (isLive)
        {
            ApplyLiveVisuals();
        }
    }

    private static void ApplyLiveVisuals()
    {
#if UNITY_EDITOR
        ApplyCameraLiveBackground();
#else
#if USING_SNAPDRAGON_SPACES_SDK
        ApplyCameraLiveBackground();
#endif
#endif
    }

    private static void ApplyCameraLiveBackground()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0f);
    }
}
