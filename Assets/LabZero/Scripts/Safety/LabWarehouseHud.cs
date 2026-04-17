using TMPro;
using UnityEngine;

public class LabWarehouseHud : MonoBehaviour
{
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private Transform viewerTransform;
    [SerializeField] private Vector3 overlayLocalPosition = new(-0.38f, 0.22f, 1.2f);

    private GameObject _overlayRoot;
    private TMP_Text _overlayText;

    public bool OverlayVisible => _overlayRoot != null && _overlayRoot.activeSelf;

    private void OnEnable()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        viewerTransform ??= ResolveViewerTransform();

        if (sessionManager != null)
        {
            sessionManager.StateChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (sessionManager != null)
        {
            sessionManager.StateChanged -= Refresh;
        }
    }

    public void Configure(LabSessionManager manager, Transform viewer)
    {
        if (sessionManager != null)
        {
            sessionManager.StateChanged -= Refresh;
        }

        sessionManager = manager;
        viewerTransform = viewer != null ? viewer : ResolveViewerTransform();

        if (sessionManager != null)
        {
            sessionManager.StateChanged += Refresh;
        }

        EnsureOverlayRoot();
        Refresh();
    }

    public void Refresh()
    {
        EnsureOverlayRoot();
        if (_overlayRoot == null || sessionManager == null)
        {
            return;
        }

        var settings = sessionManager.Settings;
        var visible = settings != null && settings.ShowErrorOverlay;
        _overlayRoot.SetActive(visible);

        if (!visible || _overlayText == null)
        {
            return;
        }

        _overlayText.text = $"Errori: {sessionManager.MistakeCount}\nPunteggio: {Mathf.RoundToInt(sessionManager.Score)}\n{sessionManager.LastFeedbackText}";
    }

    private void EnsureOverlayRoot()
    {
        if (_overlayRoot != null)
        {
            return;
        }

        var viewer = viewerTransform != null ? viewerTransform : ResolveViewerTransform();
        if (viewer == null)
        {
            return;
        }

        _overlayRoot = new GameObject("Warehouse Error Overlay");
        _overlayRoot.transform.SetParent(viewer, false);
        _overlayRoot.transform.localPosition = overlayLocalPosition;
        _overlayRoot.transform.localRotation = Quaternion.identity;

        _overlayText = _overlayRoot.AddComponent<TextMeshPro>();
        _overlayText.alignment = TextAlignmentOptions.Left;
        _overlayText.fontSize = 0.055f;
        _overlayText.color = new Color(1f, 0.92f, 0.72f, 1f);
        _overlayText.text = string.Empty;
        _overlayRoot.SetActive(false);
    }

    private static Transform ResolveViewerTransform()
    {
        var camera = Camera.main;
        if (camera != null)
        {
            return camera.transform;
        }

        var anyCamera = Object.FindAnyObjectByType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
    }
}
