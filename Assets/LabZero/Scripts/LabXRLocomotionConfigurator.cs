using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[DefaultExecutionOrder(-100)]
public class LabXRLocomotionConfigurator : MonoBehaviour
{
    [SerializeField] private bool configureOnEnable = true;
    [SerializeField] private bool configureOnStart = true;
    [SerializeField] private bool useHeadRelativeMovement = true;
    [SerializeField] private bool leftSmoothMotionEnabled = true;
    [SerializeField] private bool rightSmoothMotionEnabled = false;
    [SerializeField] private bool rightSmoothTurnEnabled = true;
    [SerializeField] private float maxStepOffset = 0.22f;
    [SerializeField] private float maxSlopeLimit = 42f;

    private void OnEnable()
    {
        if (configureOnEnable)
        {
            Apply();
        }
    }

    private void Start()
    {
        if (configureOnStart)
        {
            Apply();
        }
    }

    public void Apply()
    {
        ConfigureControllerManagers();
        ConfigureMovementDirection();
        ConfigureCharacterControllers();
    }

    private void ConfigureControllerManagers()
    {
        var managers = Object.FindObjectsByType<ControllerInputActionManager>(FindObjectsInactive.Include);
        foreach (var manager in managers)
        {
            if (manager == null)
            {
                continue;
            }

            var controllerName = manager.gameObject.name.ToLowerInvariant();
            if (controllerName.Contains("left"))
            {
                manager.smoothMotionEnabled = leftSmoothMotionEnabled;
            }
            else if (controllerName.Contains("right"))
            {
                manager.smoothMotionEnabled = rightSmoothMotionEnabled;
                manager.smoothTurnEnabled = rightSmoothTurnEnabled;
            }
        }
    }

    private void ConfigureMovementDirection()
    {
        if (!useHeadRelativeMovement)
        {
            return;
        }

        var moveProviders = Object.FindObjectsByType<DynamicMoveProvider>(FindObjectsInactive.Include);
        foreach (var moveProvider in moveProviders)
        {
            if (moveProvider == null)
            {
                continue;
            }

            moveProvider.leftHandMovementDirection = DynamicMoveProvider.MovementDirection.HeadRelative;
            moveProvider.rightHandMovementDirection = DynamicMoveProvider.MovementDirection.HeadRelative;
        }
    }

    private void ConfigureCharacterControllers()
    {
        var controllers = Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Include);
        foreach (var controller in controllers)
        {
            if (controller == null)
            {
                continue;
            }

            controller.stepOffset = Mathf.Min(controller.stepOffset, maxStepOffset);
            controller.slopeLimit = Mathf.Min(controller.slopeLimit, maxSlopeLimit);
        }
    }
}
