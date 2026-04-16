using UnityEngine;
using UnityEngine.InputSystem;

public class LeonardoControllerAnimator : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer m_primaryButton_SMR;
    [SerializeField] private SkinnedMeshRenderer m_secondaryButton_SMR;
    [SerializeField] private SkinnedMeshRenderer m_optionButton_SMR;
    [SerializeField] private SkinnedMeshRenderer m_thumbstick_SMR;
    [SerializeField] private SkinnedMeshRenderer m_trigger_SMR;
    [SerializeField] private SkinnedMeshRenderer m_grip_SMR;

    [SerializeField] private InputActionProperty m_primaryButton;
    [SerializeField] private InputActionProperty m_secondaryButton;
    [SerializeField] private InputActionProperty m_optionButton;
    [SerializeField] private InputActionProperty m_thumbstick;
    [SerializeField] private InputActionProperty m_thumbstickButton;
    [SerializeField] private InputActionProperty m_trigger;
    [SerializeField] private InputActionProperty m_grip;

    private int m_thumbstickX = 0;
    private int m_thumbstickY = 1;
    private int m_thumbstickPress = 2;

    private bool m_actionsBound;

    private void OnEnable()
    {
        if (!TryGetActions(out var primaryButton, out var secondaryButton, out var optionButton,
                out var thumbstick, out var thumbstickButton, out var trigger, out var grip))
        {
            return;
        }

        primaryButton.performed += OnPrimaryButtonPressed;
        primaryButton.canceled += OnPrimaryButtonPressed;
        secondaryButton.performed += OnSecondaryButtonPressed;
        secondaryButton.canceled += OnSecondaryButtonPressed;
        optionButton.performed += OnMenuButtonPressed;
        optionButton.canceled += OnMenuButtonPressed;
        thumbstick.performed += OnThumbstickMoved;
        thumbstick.canceled += OnThumbstickMoved;
        thumbstickButton.performed += OnThumbstickPressed;
        thumbstickButton.canceled += OnThumbstickPressed;
        trigger.performed += OnTriggerPressed;
        trigger.canceled += OnTriggerPressed;
        grip.performed += OnGripPressed;
        grip.canceled += OnGripPressed;

        primaryButton.Enable();
        secondaryButton.Enable();
        optionButton.Enable();
        thumbstick.Enable();
        thumbstickButton.Enable();
        trigger.Enable();
        grip.Enable();
        m_actionsBound = true;
    }

    private void OnDisable()
    {
        if (!m_actionsBound ||
            !TryGetActions(out var primaryButton, out var secondaryButton, out var optionButton,
                out var thumbstick, out var thumbstickButton, out var trigger, out var grip))
        {
            return;
        }

        primaryButton.performed -= OnPrimaryButtonPressed;
        primaryButton.canceled -= OnPrimaryButtonPressed;
        secondaryButton.performed -= OnSecondaryButtonPressed;
        secondaryButton.canceled -= OnSecondaryButtonPressed;
        optionButton.performed -= OnMenuButtonPressed;
        optionButton.canceled -= OnMenuButtonPressed;
        thumbstick.performed -= OnThumbstickMoved;
        thumbstick.canceled -= OnThumbstickMoved;
        thumbstickButton.performed -= OnThumbstickPressed;
        thumbstickButton.canceled -= OnThumbstickPressed;
        trigger.performed -= OnTriggerPressed;
        trigger.canceled -= OnTriggerPressed;
        grip.performed -= OnGripPressed;
        grip.canceled -= OnGripPressed;

        primaryButton.Disable();
        secondaryButton.Disable();
        optionButton.Disable();
        thumbstick.Disable();
        thumbstickButton.Disable();
        trigger.Disable();
        grip.Disable();
        m_actionsBound = false;
    }

    private bool TryGetActions(
        out InputAction primaryButton,
        out InputAction secondaryButton,
        out InputAction optionButton,
        out InputAction thumbstick,
        out InputAction thumbstickButton,
        out InputAction trigger,
        out InputAction grip)
    {
        primaryButton = m_primaryButton.action;
        secondaryButton = m_secondaryButton.action;
        optionButton = m_optionButton.action;
        thumbstick = m_thumbstick.action;
        thumbstickButton = m_thumbstickButton.action;
        trigger = m_trigger.action;
        grip = m_grip.action;

        return primaryButton != null &&
            secondaryButton != null &&
            optionButton != null &&
            thumbstick != null &&
            thumbstickButton != null &&
            trigger != null &&
            grip != null;
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext value)
    {
        m_primaryButton_SMR.SetBlendShapeWeight(0, value.ReadValue<float>() * 100);
    }

    private void OnSecondaryButtonPressed(InputAction.CallbackContext value)
    {
        m_secondaryButton_SMR.SetBlendShapeWeight(0, value.ReadValue<float>() * 100);
    }

    private void OnMenuButtonPressed(InputAction.CallbackContext value)
    {
        m_optionButton_SMR.SetBlendShapeWeight(0, value.ReadValue<float>() * 100);
    }

    private void OnThumbstickMoved(InputAction.CallbackContext value)
    {
        m_thumbstick_SMR.SetBlendShapeWeight(m_thumbstickX, value.ReadValue<Vector2>().x * 100);
        m_thumbstick_SMR.SetBlendShapeWeight(m_thumbstickY, value.ReadValue<Vector2>().y * 100);
    }

    private void OnThumbstickPressed(InputAction.CallbackContext value)
    {
        m_trigger_SMR.SetBlendShapeWeight(m_thumbstickPress, value.ReadValue<float>() * 100);
    }

    private void OnTriggerPressed(InputAction.CallbackContext value)
    {
        m_trigger_SMR.SetBlendShapeWeight(0, value.ReadValue<float>() * 100);
    }

    private void OnGripPressed(InputAction.CallbackContext value)
    {
        m_grip_SMR.SetBlendShapeWeight(0, value.ReadValue<float>() * 100);
    }
}
