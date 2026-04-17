using UnityEngine;

public class LabWarehouseBootstrap : MonoBehaviour
{
    [SerializeField] private LabSessionSettings settings;
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private GameObject warehouseEnvironmentRoot;
    [SerializeField] private bool addPhoneToHotbarOnStart = true;
    [SerializeField] private LabHotbarInventory hotbarInventory;
    [SerializeField] private LabWarehouseHud warehouseHud;
    [SerializeField] private LabWarehouseInteractionInputRouter interactionInputRouter;

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

        EnsureInteractionSystems();
        EnsurePortablePackageInteractables();
        AddStartingPhoneToHotbar();
    }

    private void EnsureInteractionSystems()
    {
        var viewer = ResolveViewerTransform();

        hotbarInventory ??= Object.FindAnyObjectByType<LabHotbarInventory>();
        if (hotbarInventory == null)
        {
            hotbarInventory = gameObject.AddComponent<LabHotbarInventory>();
        }

        hotbarInventory.Configure(sessionManager, viewer);

        warehouseHud ??= Object.FindAnyObjectByType<LabWarehouseHud>();
        if (warehouseHud == null)
        {
            warehouseHud = gameObject.AddComponent<LabWarehouseHud>();
        }

        warehouseHud.Configure(sessionManager, viewer);

        interactionInputRouter ??= Object.FindAnyObjectByType<LabWarehouseInteractionInputRouter>();
        if (interactionInputRouter == null)
        {
            interactionInputRouter = gameObject.AddComponent<LabWarehouseInteractionInputRouter>();
        }

        interactionInputRouter.Configure(hotbarInventory, viewer);
    }

    private void AddStartingPhoneToHotbar()
    {
        if (!addPhoneToHotbarOnStart)
        {
            return;
        }

        var phone = FindPhoneInteractable();
        if (phone == null)
        {
            return;
        }

        var distraction = phone.GetComponent<LabEquippedPhoneDistraction>();
        if (distraction == null)
        {
            distraction = phone.gameObject.AddComponent<LabEquippedPhoneDistraction>();
        }

        distraction.ConfigureForHotbar(hotbarInventory, sessionManager);
        hotbarInventory?.TryEquip(phone);
    }

    private void EnsurePortablePackageInteractables()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go == null || !go.scene.IsValid() || !IsPortablePackageName(go.name))
            {
                continue;
            }

            var collider = go.GetComponentInChildren<Collider>(true);
            if (collider == null)
            {
                collider = go.AddComponent<BoxCollider>();
            }

            var interactable = go.GetComponent<LabSafetyInteractable>();
            if (interactable == null)
            {
                interactable = go.AddComponent<LabSafetyInteractable>();
            }

            interactable.Configure(sessionManager, LabSafetyItemType.Package, LabSafetyItemRole.Neutral, LabSafetyZoneType.Sorting);
        }
    }

    private static bool IsPortablePackageName(string objectName)
    {
        return objectName.StartsWith("Ready Package", System.StringComparison.OrdinalIgnoreCase)
            || objectName.StartsWith("Sorting Box", System.StringComparison.OrdinalIgnoreCase);
    }

    private static LabSafetyInteractable FindPhoneInteractable()
    {
        var interactables = Object.FindObjectsByType<LabSafetyInteractable>(FindObjectsInactive.Include);
        foreach (var interactable in interactables)
        {
            if (interactable != null && interactable.ItemType == LabSafetyItemType.Phone)
            {
                return interactable;
            }
        }

        return null;
    }

    private static Transform ResolveViewerTransform()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindAnyObjectByType<Camera>();
        }

        if (camera != null)
        {
            return camera.transform;
        }

        var rig = FindSceneObjectIncludingInactive("XRRig (Controllers + Hands)");
        if (rig != null)
        {
            return rig.transform;
        }

        var spawnPoint = FindSceneObjectIncludingInactive("SpawnPoint");
        return spawnPoint != null ? spawnPoint.transform : null;
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
