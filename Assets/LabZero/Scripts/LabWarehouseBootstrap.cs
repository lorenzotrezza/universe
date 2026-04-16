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

        if (sessionManager != null)
        {
            sessionManager.Initialize(settings);
            sessionManager.StartRun();
        }
    }
}
