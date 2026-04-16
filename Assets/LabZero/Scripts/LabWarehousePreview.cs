using UnityEngine;
using UnityEngine.InputSystem;

public class LabWarehousePreview : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float cameraHeight = 0.75f;
    [SerializeField] private bool hideXrRigInEditor = true;

    private Camera _previewCamera;
    private bool _previewReady;

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

        var startPosition = spawnPoint != null
            ? spawnPoint.position + Vector3.up * cameraHeight
            : transform.position + Vector3.up * cameraHeight;
        var startRotation = spawnPoint != null
            ? spawnPoint.rotation
            : Quaternion.identity;

        cameraGo.transform.SetPositionAndRotation(startPosition, startRotation);
        _previewReady = true;
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

        if (keyboard.iKey.isPressed)
        {
            cameraTransform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        }

        if (keyboard.kKey.isPressed)
        {
            cameraTransform.position -= Vector3.up * verticalSpeed * Time.deltaTime;
        }
    }

    private static void HandleShortcuts(Keyboard keyboard)
    {
        if (keyboard.rKey.wasPressedThisFrame)
        {
            LabSceneTransition.RestartWarehouse();
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            LabSceneTransition.ReturnToLobby();
        }
    }
}
