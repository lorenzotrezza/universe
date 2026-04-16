using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PassthroughActivator : MonoBehaviour
{
    [SerializeField] private bool _enableOnStart;
    [SerializeField, ReadOnly] private bool _enabled;

    [SerializeField] private InputActionProperty _passthroughButton;

    private InputAction _resolvedPassthroughAction;

    private void OnEnable()
    {
        _resolvedPassthroughAction = _passthroughButton.action;
        if (_resolvedPassthroughAction == null)
        {
            return;
        }

        _resolvedPassthroughAction.performed += TogglePassthrough;
        _resolvedPassthroughAction.Enable();
    }


    private void OnDisable()
    {
        if (_resolvedPassthroughAction == null)
        {
            return;
        }

        _resolvedPassthroughAction.performed -= TogglePassthrough;
        _resolvedPassthroughAction.Disable();
        _resolvedPassthroughAction = null;
    }

    private void Start()
    {
        TogglePassthrough(_enableOnStart || XRPassthroughUtility.GetPassthroughEnabled());
    }

    public void TogglePassthrough(InputAction.CallbackContext context)
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.DebugOn)
        {
            var enable = XRPassthroughUtility.GetPassthroughEnabled();
            enable = !enable;
            TogglePassthrough(enable);
        }
    }

    public void TogglePassthrough()
    {
        TogglePassthrough(!_enabled);
    }

    public void TogglePassthrough(bool enable)
    {
        XRPassthroughUtility.SetPassthroughEnabled(enable);
        _enabled = enable;
    } 
}
