using UnityEngine;

public class LabWarehouseBootstrap : MonoBehaviour
{
    [SerializeField] private LabSessionSettings settings;
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private GameObject warehouseEnvironmentRoot;
    [SerializeField] private bool equipPhoneOnStart = true;

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

        EquipStartingPhone();
    }

    private void EquipStartingPhone()
    {
        if (!equipPhoneOnStart)
        {
            return;
        }

        var phone = FindPhoneInteractable();
        if (phone == null)
        {
            return;
        }

        var anchor = ResolveEquippedPhoneAnchor();
        if (anchor == null)
        {
            return;
        }

        var distraction = phone.GetComponent<LabEquippedPhoneDistraction>();
        if (distraction == null)
        {
            distraction = phone.gameObject.AddComponent<LabEquippedPhoneDistraction>();
        }

        distraction.Configure(anchor, sessionManager);
    }

    private static LabSafetyInteractable FindPhoneInteractable()
    {
        var interactables = Object.FindObjectsByType<LabSafetyInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            if (interactable != null && interactable.ItemType == LabSafetyItemType.Phone)
            {
                return interactable;
            }
        }

        return null;
    }

    private static Transform ResolveEquippedPhoneAnchor()
    {
        var existing = FindSceneObjectIncludingInactive("EquippedPhoneAnchor");
        if (existing != null)
        {
            return existing.transform;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindFirstObjectByType<Camera>();
        }

        if (camera != null)
        {
            return CreateAnchor(camera.transform, Vector3.zero);
        }

        var rig = FindSceneObjectIncludingInactive("XRRig (Controllers + Hands)");
        if (rig != null)
        {
            return CreateAnchor(rig.transform, new Vector3(0f, 1.35f, 0.55f));
        }

        var spawnPoint = FindSceneObjectIncludingInactive("SpawnPoint");
        return spawnPoint != null ? CreateAnchor(spawnPoint.transform, new Vector3(0f, 1.35f, 0.55f)) : null;
    }

    private static Transform CreateAnchor(Transform parent, Vector3 localPosition)
    {
        var anchor = new GameObject("EquippedPhoneAnchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = localPosition;
        anchor.transform.localRotation = Quaternion.identity;
        return anchor.transform;
    }

    private static GameObject FindSceneObjectIncludingInactive(string objectName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go != null && go.scene.IsValid() && go.name == objectName)
            {
                return go;
            }
        }

        return null;
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
