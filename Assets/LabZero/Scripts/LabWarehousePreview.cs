using UnityEngine;
using UnityEngine.InputSystem;

public class LabWarehousePreview : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float cameraHeight = 1.62f;
    [SerializeField] private bool allowVerticalDebugMovement = false;
    [SerializeField] private bool snapSpawnToFloor = true;
    [SerializeField] private float floorProbeHeight = 4f;
    [SerializeField] private float floorProbeDistance = 8f;
    [SerializeField] private bool hideXrRigInEditor = true;
    [SerializeField] private GameObject warehouseEnvironmentRoot;

    private Camera _previewCamera;
    private bool _previewReady;
    private bool _isLiveMock;

    private void Awake()
    {
        if (!Application.isEditor)
        {
            enabled = false;
            return;
        }

        if (hideXrRigInEditor)
        {
            SetInactiveIfFound("XRRig (Controllers + Hands)");
        }

        SetupPreviewCamera();
    }

    private static void SetInactiveIfFound(string objectName)
    {
        var go = GameObject.Find(objectName);
        if (go != null)
        {
            go.SetActive(false);
        }
    }

    private void SetupPreviewCamera()
    {
        if (_previewReady)
        {
            return;
        }

        var cameraGo = new GameObject("WarehousePreviewCamera");
        _previewCamera = cameraGo.AddComponent<Camera>();
        _previewCamera.nearClipPlane = 0.1f;
        _previewCamera.fieldOfView = 70f;
        _previewCamera.depth = 10f;

        var startPosition = GetGroundedSpawnPosition() + Vector3.up * cameraHeight;
        var startRotation = spawnPoint != null
            ? spawnPoint.rotation
            : Quaternion.identity;

        cameraGo.transform.SetPositionAndRotation(startPosition, startRotation);
        _previewReady = true;
    }

    private Vector3 GetGroundedSpawnPosition()
    {
        var basePosition = spawnPoint != null ? spawnPoint.position : transform.position;
        if (!snapSpawnToFloor)
        {
            return basePosition;
        }

        var origin = basePosition + Vector3.up * floorProbeHeight;
        var maxDistance = floorProbeHeight + floorProbeDistance;
        if (Physics.Raycast(origin, Vector3.down, out var hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return basePosition;
    }

    private void Update()
    {
        if (!_previewReady || _previewCamera == null)
        {
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        HandleMovement(keyboard);
        HandleShortcuts(keyboard);
    }

    private void HandleMovement(Keyboard keyboard)
    {
        var cameraTransform = _previewCamera.transform;
        var move = Vector3.zero;

        if (keyboard.wKey.isPressed) move += cameraTransform.forward;
        if (keyboard.sKey.isPressed) move -= cameraTransform.forward;
        if (keyboard.aKey.isPressed) move -= cameraTransform.right;
        if (keyboard.dKey.isPressed) move += cameraTransform.right;

        move.y = 0f;
        if (move.sqrMagnitude > 0.0001f)
        {
            cameraTransform.position += move.normalized * moveSpeed * Time.deltaTime;
        }

        if (keyboard.qKey.isPressed)
        {
            cameraTransform.Rotate(0f, -turnSpeed * Time.deltaTime, 0f, Space.World);
        }

        if (keyboard.eKey.isPressed)
        {
            cameraTransform.Rotate(0f, turnSpeed * Time.deltaTime, 0f, Space.World);
        }

        if (allowVerticalDebugMovement)
        {
            if (keyboard.iKey.isPressed)
            {
                cameraTransform.position += Vector3.up * verticalSpeed * Time.deltaTime;
            }

            if (keyboard.kKey.isPressed)
            {
                cameraTransform.position -= Vector3.up * verticalSpeed * Time.deltaTime;
            }
        }
    }

    private void HandleShortcuts(Keyboard keyboard)
    {
        if (keyboard.rKey.wasPressedThisFrame)
        {
            LabSceneTransition.RestartWarehouse();
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            LabSceneTransition.ReturnToLobby();
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            ToggleLiveMock();
        }
    }

    private void ToggleLiveMock()
    {
        _isLiveMock = !_isLiveMock;

        if (warehouseEnvironmentRoot != null)
        {
            warehouseEnvironmentRoot.SetActive(!_isLiveMock);
        }

        if (_previewCamera == null)
        {
            return;
        }

        _previewCamera.clearFlags = _isLiveMock
            ? CameraClearFlags.SolidColor
            : CameraClearFlags.Skybox;
        _previewCamera.backgroundColor = _isLiveMock
            ? new Color(0.15f, 0.15f, 0.15f, 0f)
            : Color.black;
    }
}
